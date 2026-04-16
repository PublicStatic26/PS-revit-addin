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
            { OpeningMethod.Fixed,           "고정(Fix)창" },
            { OpeningMethod.ProjectOut,      "프로젝트창" },
            { OpeningMethod.CasementSwing,   "여닫이창" },
            { OpeningMethod.Sliding,         "슬라이딩창" },
            { OpeningMethod.TurnTilt,        "턴앤틸트창" },
            { OpeningMethod.LiftSliding,     "리프트슬라이딩창" },
            { OpeningMethod.ParallelSliding, "패러럴슬라이딩창" },
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

        /// <summary>
        /// WINDOW-어셈블 유형(WD-01, WD-02...)을 찾아 제품 파라미터를 적용한다.
        ///
        /// [targetWidthMm / targetHeightMm 선택적 파라미터 안내]
        /// 기본값은 0이며, 두 가지 호출 방식이 있다.
        ///
        /// 방식 A — 파라미터 없이 호출 (기존 방식, 기존 코드 그대로 작동)
        ///   updater.UpdateFamilyType(products);
        ///   → 폭/높이에 제품의 최대치수(MaxWidthMm, MaxHeightMm)가 입력된다.
        ///
        /// 방식 B — CAD 실치수 전달 (⑤ 제품 선택 완료 후 Finish 버튼에서 호출)
        ///   updater.UpdateFamilyType(products, window.WidthMm, window.HeightMm);
        ///   → 폭/높이에 CAD 도면에서 읽어온 실제 창호 치수가 입력된다.
        ///   → 제품의 최소/최대 치수는 별도 파라미터(최소 폭, 최소 높이)에 그대로 기록된다.
        /// </summary>
        public void UpdateFamilyType(List<VendorProduct> products,
            double targetWidthMm = 0, double targetHeightMm = 0)
        {
            using (var trans = new Transaction(_doc, "창호 파라미터 적용"))
            {
                trans.Start();

                foreach (var product in products)
                {
                    // WINDOW-어셈블 패밀리에서 유형마크(WD-01 등)로 심볼 찾기
                    var symbol = new FilteredElementCollector(_doc)
                        .OfClass(typeof(FamilySymbol))
                        .Cast<FamilySymbol>()
                        .FirstOrDefault(s => s.FamilyName == "WINDOW-어셈블"
                                          && s.Name == product.SymbolCode);

                    if (symbol == null) continue;

                    // ── 유형 마크 ────────────────────────────────────────────
                    symbol.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK)
                          ?.Set(product.SymbolCode);

                    // ── 문자 파라미터 ────────────────────────────────────────
                    symbol.LookupParameter("회사명")?.Set(product.VendorName);
                    symbol.LookupParameter("제품명")?.Set(product.ProductName);
                    symbol.LookupParameter("모델번호")?.Set(product.ModelNumber);
                    symbol.LookupParameter("개폐방식")?.Set(OpeningNames.TryGetValue(product.OpeningMethod, out var openingName) ? openingName : product.OpeningMethod.ToString());

                    // ── 예/아니오 파라미터 ───────────────────────────────────
                    symbol.LookupParameter("방화")?.Set(product.IsFireRated ? 1 : 0);
                    symbol.LookupParameter("단열")?.Set(product.IsInsulated ? 1 : 0);

                    // ── 치수 파라미터 (feet → mm 변환) ──────────────────────
                    // 폭/높이: CAD 실치수가 전달되면 그 값 사용, 없으면 제품 최대치수로 대체
                    double w = targetWidthMm > 0 ? targetWidthMm : product.MaxWidthMm;
                    double h = targetHeightMm > 0 ? targetHeightMm : product.MaxHeightMm;
                    symbol.LookupParameter("폭")?.Set(w / 304.8);
                    symbol.LookupParameter("높이")?.Set(h / 304.8);
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