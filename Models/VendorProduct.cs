using System;
using System.Collections.Generic;

namespace PSRevitAddin.Models
{
    /// <summary>
    /// 제품 정보 (회사, 단가, 성능)
    ///
    /// 데이터 흐름:
    ///   ③ ProductCatalog  → Excel에서 읽어 생성
    ///   ④ ProductFilter   → 조건(방화/단열/유리) AND 필터링
    ///   ⑤ ProductSelector → 사용자가 최종 선택 → WindowUnit.SelectedProduct에 할당
    ///   ⑥ ParameterUpdater → FamilyPath 사용, 파라미터 값 Revit에 입력
    ///   ⑦ ScheduleManager → UnitPrice 사용, 수량×단가 집계
    /// </summary>
    internal class VendorProduct
    {
        // ─── 제조사 / 제품 식별 ───────────────────────────────────────

        /// <summary>제조사명. 예: "LG하우시스"</summary>
        public string VendorName { get; set; } = string.Empty;

        /// <summary>제품명. 예: "슈퍼세이브5 Premium"</summary>
        public string ProductName { get; set; } = string.Empty;

        /// <summary>모델번호. 예: "SS5-1200"</summary>
        public string ModelNumber { get; set; } = string.Empty;

        // ─── 적용 가능 사이즈 범위 ───────────────────────────────────

        /// <summary>적용 가능 최소 너비 (mm)</summary>
        public double MinWidthMm { get; set; }

        /// <summary>적용 가능 최대 너비 (mm)</summary>
        public double MaxWidthMm { get; set; }

        /// <summary>적용 가능 최소 높이 (mm)</summary>
        public double MinHeightMm { get; set; }

        /// <summary>적용 가능 최대 높이 (mm)</summary>
        public double MaxHeightMm { get; set; }

        // ─── ④ 필터 조건 ─────────────────────────────────────────────

        /// <summary>방화 인증 여부</summary>
        public bool IsFireRated { get; set; }

        /// <summary>단열 성능 적용 여부</summary>
        public bool IsInsulated { get; set; }

        /// <summary>유리 종류</summary>
        public GlassType GlassType { get; set; }

        /// <summary>프레임 재질</summary>
        public FrameType FrameType { get; set; }

        /// <summary>개폐 방식</summary>
        public OpeningMethod OpeningMethod { get; set; }

        // ─── ⑥ 패밀리 교체 ───────────────────────────────────────────

        /// <summary>
        /// Revit 패밀리 파일 경로 (.rfa).
        /// ⑥ ParameterUpdater에서 기존 기본 객체를 이 패밀리로 교체.
        /// </summary>
        public string FamilyPath { get; set; } = string.Empty;

        // ─── ⑦ 견적 집계 ─────────────────────────────────────────────

        /// <summary>단가 (원)</summary>
        public decimal UnitPrice { get; set; }
    }
}