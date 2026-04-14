using Autodesk.Revit.DB;
using PSRevitAddin.Models;
using System.Collections.Generic;
using System.Linq;

namespace PSRevitAddin.Services
{
    /// <summary>
    /// ⑥ 실제 패밀리 교체 + 파라미터 입력
    /// </summary>
    public class ParameterUpdater
    {
        private readonly Document _doc;

        // ── Enum → 한국어 재료명 변환 ──────────────────────────────────────

        private static readonly Dictionary<GlassType, string> GlassNames = new Dictionary<GlassType, string>
        {
            { GlassType.LowE,     "로이유리" },
            { GlassType.Double,   "복층유리" },
            { GlassType.Triple,   "삼중유리" },
            { GlassType.Vacuum,   "진공유리" },
            { GlassType.Tempered, "강화유리" },
            { GlassType.Standard, "일반유리" },
        };

        private static readonly Dictionary<FrameType, string> FrameNames = new Dictionary<FrameType, string>
        {
            { FrameType.Aluminum,    "알루미늄" },
            { FrameType.Pvc,         "PVC" },
            { FrameType.AlPvc,       "AL+PVC" },
            { FrameType.Combination, "복합" },
            { FrameType.CurtainWall, "커튼월" },
        };

        private static readonly Dictionary<OpeningMethod, string> OpeningNames = new Dictionary<OpeningMethod, string>
        {
            { OpeningMethod.Fixed,         "고정창" },
            { OpeningMethod.Sliding,       "슬라이딩" },
            { OpeningMethod.ProjectOut,    "프로젝트" },
            { OpeningMethod.CasementSwing, "여닫이" },
        };

        public ParameterUpdater(Document doc)
        {
            _doc = doc;
        }

        // ── 재료 찾기 or 생성 ────────────────────────────────────────────────

        private Material GetOrCreateMaterial(string name)
        {
            var mat = new FilteredElementCollector(_doc)
                .OfClass(typeof(Material))
                .Cast<Material>()
                .FirstOrDefault(m => m.Name == name);

            if (mat == null)
            {
                var matId = Material.Create(_doc, name);
                mat = _doc.GetElement(matId) as Material;
            }
            return mat;
        }

        // ── 메인 ────────────────────────────────────────────────────────────

        // 창호를 생성하기 전에 개폐방식에 따라서 필터를 건다. 선택된 방식에 따라서 생성이 된다.
        public void UpdateFamilyType(List<VendorProduct> products)
        {
            using (var trans = new Transaction(_doc, "창호 타입 생성"))
            {
                trans.Start();

                foreach (var product in products)
                {
                    // 개폐방식별 패밀리 선택
                    var familyName = product.OpeningMethod switch
                    {
                        OpeningMethod.CasementSwing => "WINDOW_여닫이창",
                        OpeningMethod.Fixed => "WINDOW_고정창",
                        OpeningMethod.Sliding => "WINDOW_슬라이딩창",
                        OpeningMethod.ProjectOut => "WINDOW_프로젝트창",
                        _                        => "WINDOW_고정창"

                    };

                    // 해당 패밀리 심볼 찾기
                    var collector = new FilteredElementCollector(_doc)
                        .OfClass(typeof(FamilySymbol))
                        .Cast<FamilySymbol>()
                        .Where(s => s.FamilyName == familyName)
                        .ToList();

                    string typeName = product.SymbolCode;

                    // 기존 타입 찾기
                    var existing = collector.FirstOrDefault(s => s.Name == typeName);
                    FamilySymbol symbol;

                    if (existing == null)
                    {
                        var baseSymbol = collector.FirstOrDefault();
                        if (baseSymbol == null) continue;

                        symbol = baseSymbol.Duplicate(typeName) as FamilySymbol;
                        if (symbol != null) collector.Add(symbol);
                    }
                    else
                    {
                        symbol = existing;
                    }

                    if (symbol == null) continue;

                    // ── 유형 마크 ────────────────────────────────────────────
                    symbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK)
                          ?.Set(product.SymbolCode);

                    // ── 문자 파라미터 ────────────────────────────────────────
                    symbol.LookupParameter("회사명")?.Set(product.VendorName);
                    symbol.LookupParameter("제품명")?.Set(product.ProductName);
                    symbol.LookupParameter("모델번호")?.Set(product.ModelNumber);
                    symbol.LookupParameter("개폐방식")?.Set(OpeningNames[product.OpeningMethod]);

                    // ── 예/아니오 파라미터 ───────────────────────────────────
                    symbol.LookupParameter("방화")?.Set(product.IsFireRated ? 1 : 0);
                    symbol.LookupParameter("단열")?.Set(product.IsInsulated ? 1 : 0);

                    // ── 치수 파라미터 (feet → mm 변환) ──────────────────────
                    symbol.LookupParameter("폭")?.Set(product.MaxWidthMm / 304.8);
                    symbol.LookupParameter("높이")?.Set(product.MaxHeightMm / 304.8);
                    symbol.LookupParameter("최소 폭")?.Set(product.MinWidthMm / 304.8);
                    symbol.LookupParameter("최소 높이")?.Set(product.MinHeightMm / 304.8);

                    // ── 재료 파라미터 ────────────────────────────────────────
                    var glassMat = GetOrCreateMaterial(GlassNames[product.GlassType]);
                    symbol.LookupParameter("유리 재료")?.Set(glassMat?.Id);

                    var frameMat = GetOrCreateMaterial(FrameNames[product.FrameType]);
                    symbol.LookupParameter("프레임 재료")?.Set(frameMat?.Id);

                    // ── 단가 ─────────────────────────────────────────────────
                    symbol.LookupParameter("단가")?.Set((double)product.UnitPrice);
                }

                trans.Commit();
            }
        }
    }
}