using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PSRevitAddin.Models;
using System.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Windows;

namespace PSRevitAddin.Services
{
    /// <summary>
    /// ① CAD에서 유형마크 + 좌표 추출
    /// </summary>

    public class CadWindowData
    {
        public string Mark { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double SillHeight { get; set; }  // 씰높이 (feet)
        public XYZ Location { get; set; }
        public Curve CenterLine { get; set; }
    }

    public class CadParseResult
    {
        public List<Curve> WallCenterlines { get; set; } = new List<Curve>();
        public List<CadWindowData> WindowDataList { get; set; } = new List<CadWindowData>();
    }
    
    
    public class CadParser
    {
        private Document _doc;

        public CadParser(Document doc)
        {
            _doc = doc;
        }


        public CadParseResult ParseAllCadData()
        {
            CadParseResult result = new CadParseResult();

            // 1. 벽체 중심선 추출
            var wallLines = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfClass(typeof(CurveElement))
                .Cast<CurveElement>()
                .Where(x => x.LineStyle.Name.Contains("라인") )
                .Select(x => x.GeometryCurve)
                .ToList();
            result.WallCenterlines = wallLines;

            // 2. 창호 데이터 추출
            result.WindowDataList = ExtractWindowData();

            return result;
        }

        public List<CadWindowData> ExtractWindowData()
        {
            List<CadWindowData> results = new List<CadWindowData>();

            var detailLines = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfClass(typeof(CurveElement))
                .Cast<CurveElement>()
                .Where(x => x.LineStyle.Name.Contains("창생성"))
                .ToList();
            
            var textNotes = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                .OfClass(typeof(TextNote))
                .Cast<TextNote>()
                .Where(x => x.TextNoteType.Name.Contains("최종도면-돋움체-6"))
                .ToList();

            // 반경 내 텍스트 검색 기준 (Revit 내부 단위: feet, 3m = 약 9.84ft)
            double searchRadius = 9.84;

            foreach (var lineElement in detailLines)
            {
                Curve curve = lineElement.GeometryCurve;
                XYZ midpoint = (curve.GetEndPoint(0) + curve.GetEndPoint(1)) / 2;

                // 반경 내 모든 텍스트 수집 (가까운 순)
                var nearbyTexts = textNotes
                    .Where(t => {
                        double dx = t.Coord.X - midpoint.X;
                        double dy = t.Coord.Y - midpoint.Y;
                        return Math.Sqrt(dx * dx + dy * dy) < searchRadius;
                    })
                    .OrderBy(t => {
                        double dx = t.Coord.X - midpoint.X;
                        double dy = t.Coord.Y - midpoint.Y;
                        return dx * dx + dy * dy;
                    })
                    .ToList();

                if (nearbyTexts.Count == 0) continue;

                var data = new CadWindowData();
                data.Location = midpoint;
                data.CenterLine = curve;

                string mark = "";
                double widthMm = 0, heightMm = 0, sillHeightMm = 0;

                foreach (var textNote in nearbyTexts)
                {
                    string t = textNote.Text.Trim();

                    if (t.Contains("_"))
                    {
                        // 형식: "BPW_1500x1200"
                        mark = Utility.ParseTypeCode(t);
                        string dimpart = t.Split('_')[1];
                        widthMm = Utility.ParseWidth(dimpart);
                        heightMm = Utility.ParseHeight(dimpart);
                        break; // 완전한 정보 획득
                    }
                    else if ((t.Contains("x") || t.Contains("X")) && widthMm == 0)
                    {
                        // 형식: "21x22x6" (폭x높이x씰높이) 또는 "1500x1200"
                        string[] parts = t.Split('x', 'X');
                        double w = parts.Length > 0 && double.TryParse(parts[0].Trim(), out double pw) ? pw : 0;
                        double h = parts.Length > 1 && double.TryParse(parts[1].Trim(), out double ph) ? ph : 0;
                        double s = parts.Length > 2 && double.TryParse(parts[2].Trim(), out double ps) ? ps : 0;
                        // 100 미만 숫자는 100mm 단위 표기 → mm 환산 (22 → 2200mm)
                        widthMm = (w > 0 && w < 100) ? w * 100 : w;
                        heightMm = (h > 0 && h < 100) ? h * 100 : h;
                        sillHeightMm = (s > 0 && s < 100) ? s * 100 : s;
                    }
                    else if (!string.IsNullOrWhiteSpace(t) && string.IsNullOrEmpty(mark))
                    {
                        // 형식: "BPW" (마크만)
                        mark = t;
                    }
                }

                data.Mark = mark;
                data.Width = Utility.MmToFeet(widthMm);
                data.Height = Utility.MmToFeet(heightMm);
                data.SillHeight = Utility.MmToFeet(sillHeightMm);

                results.Add(data);
            }

            return results;
        }
    }

    public class TextNodeInfo
    {
        public string Text { get; set; }    
        public XYZ Location { get; set; }
    }
}