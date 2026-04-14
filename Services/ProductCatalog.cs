using PSRevitAddin.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;

namespace PSRevitAddin.Services
{
    /// <summary>
    /// xlsx 파일을 System.IO.Compression + System.Xml 로 직접 읽고 수정한다.
    /// (외부 NuGet 패키지 없음)
    /// </summary>
    public class ProductCatalog
    {
        private readonly string _excelPath;
        private const string NS = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";

        public ProductCatalog(string excelPath) => _excelPath = excelPath;

        // ── Namespace ────────────────────────────────────────────────────────

        private static XmlNamespaceManager MakeNs(XmlDocument doc)
        {
            var m = new XmlNamespaceManager(doc.NameTable);
            m.AddNamespace("x", NS);
            return m;
        }

        // ── Shared strings ───────────────────────────────────────────────────

        private static List<string> LoadSharedStrings(ZipArchive zip)
        {
            var list = new List<string>();
            var entry = zip.GetEntry("xl/sharedStrings.xml");
            if (entry == null) return list;

            var doc = new XmlDocument();
            using (var s = entry.Open()) doc.Load(s);
            var ns = MakeNs(doc);

            var nodes = doc.SelectNodes("//x:si", ns);
            if (nodes == null) return list;

            foreach (XmlNode si in nodes)
            {
                var sb = new StringBuilder();
                var tNodes = si.SelectNodes(".//x:t", ns);
                if (tNodes != null)
                    foreach (XmlNode t in tNodes)
                        sb.Append(t.InnerText);
                list.Add(sb.ToString().Trim());
            }
            return list;
        }

        // ── Column helpers ───────────────────────────────────────────────────

        private static int ColToIndex(string letters)
        {
            int v = 0;
            foreach (char c in letters) v = v * 26 + (c - 'A' + 1);
            return v;
        }

        private static string IndexToCol(int col)
        {
            var sb = new StringBuilder();
            while (col > 0) { col--; sb.Insert(0, (char)('A' + col % 26)); col /= 26; }
            return sb.ToString();
        }

        private static (int row, int col) ParseRef(string r)
        {
            int i = 0;
            while (i < r.Length && char.IsLetter(r[i])) i++;
            return (int.Parse(r.Substring(i)), ColToIndex(r.Substring(0, i)));
        }

        // ── Cell value extraction ────────────────────────────────────────────

        private static string CellStr(XmlNode? cell, List<string> ss, XmlNamespaceManager ns)
        {
            if (cell == null) return "";
            string t = cell.Attributes?["t"]?.Value ?? "";
            string raw = cell.SelectSingleNode("x:v", ns)?.InnerText?.Trim() ?? "";

            if (t == "s")
                return int.TryParse(raw, out int idx) && idx < ss.Count ? ss[idx] : "";
            if (t == "inlineStr")
                return cell.SelectSingleNode(".//x:t", ns)?.InnerText?.Trim() ?? "";
            return raw;
        }

        private static double CellDbl(XmlNode? cell, List<string> ss, XmlNamespaceManager ns)
            => double.TryParse(CellStr(cell, ss, ns), out double d) ? d : 0;

        private static decimal CellDec(XmlNode? cell, List<string> ss, XmlNamespaceManager ns)
            => decimal.TryParse(CellStr(cell, ss, ns), out decimal d) ? d : 0;

        // ── Row / cell lookups ───────────────────────────────────────────────

        private static XmlNode? FindCell(XmlNode row, int col, XmlNamespaceManager ns)
        {
            var cells = row.SelectNodes("x:c", ns);
            if (cells == null) return null;
            foreach (XmlNode c in cells)
            {
                string r = c.Attributes?["r"]?.Value ?? "";
                if (!string.IsNullOrEmpty(r) && ParseRef(r).col == col)
                    return c;
            }
            return null;
        }

        private static SortedDictionary<int, XmlNode> GetRowMap(XmlDocument doc, XmlNamespaceManager ns)
        {
            var map = new SortedDictionary<int, XmlNode>();
            var nodes = doc.SelectNodes("//x:sheetData/x:row", ns);
            if (nodes == null) return map;
            foreach (XmlNode row in nodes)
                if (int.TryParse(row.Attributes?["r"]?.Value, out int rn))
                    map[rn] = row;
            return map;
        }

        // ── Cell write helper (inline string) ───────────────────────────────

        private static void SetCellInlineStr(XmlNode row, int rowNum, int col,
            string value, XmlDocument doc, XmlNamespaceManager ns)
        {
            // Find or create the cell element
            var existing = FindCell(row, col, ns);
            XmlElement cell;
            if (existing != null)
            {
                cell = (XmlElement)existing;
                cell.InnerXml = "";
                var toRemove = new List<XmlAttribute>();
                foreach (XmlAttribute a in cell.Attributes) if (a.Name != "r") toRemove.Add(a);
                foreach (var a in toRemove) cell.RemoveAttributeNode(a);
            }
            else
            {
                cell = doc.CreateElement("c", NS);
                cell.SetAttribute("r", $"{IndexToCol(col)}{rowNum}");

                // Insert in column order
                XmlNode? insertBefore = null;
                var cells = row.SelectNodes("x:c", ns);
                if (cells != null)
                    foreach (XmlNode c in cells)
                    {
                        string r = c.Attributes?["r"]?.Value ?? "";
                        if (!string.IsNullOrEmpty(r) && ParseRef(r).col > col)
                        { insertBefore = c; break; }
                    }

                if (insertBefore != null) row.InsertBefore(cell, insertBefore);
                else row.AppendChild(cell);
            }

            cell.SetAttribute("t", "inlineStr");
            var isEl = doc.CreateElement("is", NS);
            var tEl  = doc.CreateElement("t",  NS);
            tEl.InnerText = value;
            isEl.AppendChild(tEl);
            cell.AppendChild(isEl);
        }

        // ── CleanAndSave ─────────────────────────────────────────────────────

        public void CleanAndSave()
        {
            // 1. Read entire xlsx (= zip) into memory
            byte[] original = File.ReadAllBytes(_excelPath);

            XmlDocument sheetDoc;
            List<string> sharedStrings;
            var otherEntries = new Dictionary<string, byte[]>();

            using (var zip = new ZipArchive(new MemoryStream(original), ZipArchiveMode.Read))
            {
                sharedStrings = LoadSharedStrings(zip);

                var sheetEntry = zip.GetEntry("xl/worksheets/sheet1.xml")
                    ?? throw new InvalidOperationException("sheet1.xml을 찾을 수 없습니다.");
                sheetDoc = new XmlDocument();
                using (var s = sheetEntry.Open()) sheetDoc.Load(s);

                // Preserve every other entry as-is
                foreach (var entry in zip.Entries)
                {
                    if (entry.FullName == "xl/worksheets/sheet1.xml") continue;
                    using var ms = new MemoryStream();
                    using (var s = entry.Open()) s.CopyTo(ms);
                    otherEntries[entry.FullName] = ms.ToArray();
                }
            }

            var ns   = MakeNs(sheetDoc);
            var rows = GetRowMap(sheetDoc, ns);
            var sheetData = sheetDoc.SelectSingleNode("//x:sheetData", ns);

            // 2. ① 회사명 빈 셀 채우기 (위→아래, 삭제 전에 먼저)
            string lastVendor = "";
            foreach (var kv in rows)
            {
                if (kv.Key == 1) continue;
                string vendor = CellStr(FindCell(kv.Value, 2, ns), sharedStrings, ns);
                if (!string.IsNullOrEmpty(vendor))
                    lastVendor = vendor;
                else if (!string.IsNullOrEmpty(lastVendor))
                    SetCellInlineStr(kv.Value, kv.Key, 2, lastVendor, sheetDoc, ns);
            }

            // 3. ② 불필요한 행 수집 후 삭제
            var toDelete = new List<XmlNode>();
            foreach (var kv in rows)
            {
                if (kv.Key == 1) continue;
                string productName = CellStr(FindCell(kv.Value, 3, ns), sharedStrings, ns);
                double maxWidth    = CellDbl(FindCell(kv.Value, 6, ns), sharedStrings, ns);
                if (string.IsNullOrEmpty(productName) || maxWidth <= 500)
                    toDelete.Add(kv.Value);
            }
            foreach (var row in toDelete)
                sheetData?.RemoveChild(row);

            // 4. 수정된 sheet XML → byte[]
            byte[] sheetBytes;
            using (var ms = new MemoryStream()) { sheetDoc.Save(ms); sheetBytes = ms.ToArray(); }

            // 5. 새 zip 파일 빌드 후 원본 덮어쓰기
            using var outMs = new MemoryStream();
            using (var outZip = new ZipArchive(outMs, ZipArchiveMode.Create, leaveOpen: true))
            {
                var e = outZip.CreateEntry("xl/worksheets/sheet1.xml", CompressionLevel.Optimal);
                using (var s = e.Open()) s.Write(sheetBytes, 0, sheetBytes.Length);

                foreach (var kv in otherEntries)
                {
                    var entry = outZip.CreateEntry(kv.Key, CompressionLevel.Optimal);
                    using var s = entry.Open();
                    s.Write(kv.Value, 0, kv.Value.Length);
                }
            }
            File.WriteAllBytes(_excelPath, outMs.ToArray());
        }

        // ── GetAllProducts ────────────────────────────────────────────────────

        public List<VendorProduct> GetAllProducts()
        {
            var list = new List<VendorProduct>();

            using var zip = new ZipArchive(File.OpenRead(_excelPath), ZipArchiveMode.Read);
            var ss = LoadSharedStrings(zip);

            var sheetEntry = zip.GetEntry("xl/worksheets/sheet1.xml");
            if (sheetEntry == null) return list;

            var doc = new XmlDocument();
            using (var s = sheetEntry.Open()) doc.Load(s);
            var ns = MakeNs(doc);

            var rows = GetRowMap(doc, ns);
            string lastVendorName = "";

            foreach (var kv in rows)
            {
                if (kv.Key == 1) continue;
                var row = kv.Value;

                string productName = CellStr(FindCell(row, 3, ns), ss, ns);
                if (string.IsNullOrEmpty(productName)) continue;

                string vendorName = CellStr(FindCell(row, 2, ns), ss, ns);
                if (!string.IsNullOrEmpty(vendorName)) lastVendorName = vendorName;

                double maxWidth = CellDbl(FindCell(row, 6, ns), ss, ns);
                if (maxWidth <= 500) continue;

                list.Add(new VendorProduct
                {
                    SymbolCode    = CellStr(FindCell(row,  1, ns), ss, ns),
                    VendorName    = lastVendorName,
                    ProductName   = productName,
                    ModelNumber   = CellStr(FindCell(row,  4, ns), ss, ns),
                    MinWidthMm    = CellDbl(FindCell(row,  5, ns), ss, ns),
                    MaxWidthMm    = maxWidth,
                    MinHeightMm   = CellDbl(FindCell(row,  7, ns), ss, ns),
                    MaxHeightMm   = CellDbl(FindCell(row,  8, ns), ss, ns),
                    IsFireRated   = CellStr(FindCell(row,  9, ns), ss, ns).ToUpper() == "Y",
                    IsInsulated   = CellStr(FindCell(row, 10, ns), ss, ns).ToUpper() == "Y",
                    GlassType     = ParseGlass(CellStr(FindCell(row,   11, ns), ss, ns)),
                    FrameType     = ParseFrame(CellStr(FindCell(row,   12, ns), ss, ns)),
                    OpeningMethod = ParseOpening(CellStr(FindCell(row, 13, ns), ss, ns)),
                    FamilyPath    = CellStr(FindCell(row, 14, ns), ss, ns),
                    UnitPrice     = CellDec(FindCell(row, 15, ns), ss, ns),
                });
            }
            return list;
        }

        // ── Enum parsers ──────────────────────────────────────────────────────

        private static OpeningMethod ParseOpening(string val) => val switch
        {
            "고정창"       => OpeningMethod.Fixed,
            "프로젝트"     => OpeningMethod.ProjectOut,
            "여닫이"       => OpeningMethod.CasementSwing,
            "슬라이딩"     => OpeningMethod.Sliding,
            "턴앤틸트"     => OpeningMethod.TurnTilt,
            "리프트슬라이딩" => OpeningMethod.LiftSliding,
            "패러럴슬라이딩" => OpeningMethod.ParallelSliding,
            _ => OpeningMethod.Fixed
        };

        private static FrameType ParseFrame(string val) => val switch
        {
            "알루미늄" => FrameType.Aluminum,
            "AL+PVC" => FrameType.AlPvc,
            "PVC"    => FrameType.Pvc,
            "복합"    => FrameType.Combination,
            "커튼월"  => FrameType.CurtainWall,
            "한식창"  => FrameType.Traditional,
            _ => FrameType.Aluminum
        };

        private static GlassType ParseGlass(string val) => val switch
        {
            "진공유리" => GlassType.Vacuum,
            "삼중유리" => GlassType.Triple,
            "복층유리" => GlassType.Double,
            "강화유리" => GlassType.Tempered,
            "로이유리" => GlassType.LowE,
            "반사유리" => GlassType.Reflective,
            _ => GlassType.Standard
        };
    }
}
