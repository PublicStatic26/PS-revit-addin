using Autodesk.Revit.DB;
using System.Collections.Generic;
using System.Linq;

namespace PSRevitAddin.Services
{
    /// <summary>
    /// ⑦ 일람표 수량×단가 자동 집계
    /// </summary>
    public class ScheduleManager
    {
        /// <summary>
        /// WINDOW-어셈블 패밀리의 모든 유형에서 파라미터를 읽어 창호일람표 행 목록을 반환한다.
        /// </summary>
        public List<WindowScheduleRow> GetWindowSchedule(Document doc)
        {
            var rows = new List<WindowScheduleRow>();

            var symbols = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .Cast<FamilySymbol>()
                .Where(s => s.FamilyName == "WINDOW-어셈블")
                .OrderBy(s => s.Name)
                .ToList();

            foreach (var symbol in symbols)
            {
                var row = new WindowScheduleRow
                {
                    SymbolCode    = symbol.Name,
                    VendorName    = symbol.LookupParameter("회사명")?.AsString() ?? "",
                    ProductName   = symbol.LookupParameter("제품명")?.AsString() ?? "",
                    ModelNumber   = symbol.LookupParameter("모델번호")?.AsString() ?? "",
                    WidthMm       = (symbol.LookupParameter("폭")?.AsDouble() ?? 0) * 304.8,
                    HeightMm      = (symbol.LookupParameter("높이")?.AsDouble() ?? 0) * 304.8,
                    OpeningMethod = symbol.LookupParameter("개폐방식")?.AsString() ?? "",
                    IsFireRated   = (symbol.LookupParameter("방화")?.AsInteger() ?? 0) == 1,
                    IsInsulated   = (symbol.LookupParameter("단열")?.AsInteger() ?? 0) == 1,
                    GlassMaterial = GetMaterialName(doc, symbol.LookupParameter("유리 재료")),
                    FrameMaterial = GetMaterialName(doc, symbol.LookupParameter("프레임 재료")),
                    UnitPrice     = symbol.LookupParameter("단가")?.AsDouble() ?? 0,
                    SillHeightMm  = GetSillHeightMm(doc, symbol),
                };
                rows.Add(row);
            }

            return rows;
        }

        /// <summary>
        /// 씰 높이(Sill Height)는 인스턴스 파라미터이므로
        /// 심볼(유형)에서 먼저 시도하고, 없으면 해당 유형을 사용하는 첫 번째 인스턴스에서 읽는다.
        /// </summary>
        private double GetSillHeightMm(Document doc, FamilySymbol symbol)
        {
            // 1) 유형 파라미터에서 먼저 시도
            var symbolParam = symbol.LookupParameter("Sill Height")
                           ?? symbol.LookupParameter("씰 높이")
                           ?? symbol.LookupParameter("씰높이");
            if (symbolParam != null)
                return symbolParam.AsDouble() * 304.8;

            // 2) 해당 유형을 사용하는 첫 번째 인스턴스에서 읽기
            var instance = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>()
                .FirstOrDefault(fi => fi.Symbol.Id == symbol.Id);

            if (instance == null) return 0;

            var instParam = instance.LookupParameter("Sill Height")
                         ?? instance.LookupParameter("씰 높이")
                         ?? instance.LookupParameter("씰높이");
            return instParam != null ? instParam.AsDouble() * 304.8 : 0;
        }

        private string GetMaterialName(Document doc, Parameter param)
        {
            if (param == null) return "";
            var matId = param.AsElementId();
            if (matId == null || matId == ElementId.InvalidElementId) return "";
            return (doc.GetElement(matId) as Material)?.Name ?? "";
        }
    }

    public class WindowScheduleRow
    {
        public string SymbolCode    { get; set; } = "";
        public string VendorName    { get; set; } = "";
        public string ProductName   { get; set; } = "";
        public string ModelNumber   { get; set; } = "";
        public double WidthMm       { get; set; }
        public double HeightMm      { get; set; }
        public string OpeningMethod { get; set; } = "";
        public bool   IsFireRated   { get; set; }
        public bool   IsInsulated   { get; set; }
        public string GlassMaterial { get; set; } = "";
        public string FrameMaterial { get; set; } = "";
        public double UnitPrice     { get; set; }
        public double SillHeightMm  { get; set; }
    }
}
