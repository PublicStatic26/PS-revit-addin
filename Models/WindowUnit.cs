using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;

namespace PSRevitAddin.Models
{
    /// <summary>
    /// 창호 정보 (유형마크, 사이즈, 좌표, 선택 상태)
    ///
    /// 데이터 흐름:
    ///   ① CadParser     → 생성 (TypeCode, Width/Height, Location)
    ///   ② FamilyPlacer  → RevitElementId 기록
    ///   ⑤ ProductSelector → SelectedProduct 설정
    ///   ⑥ ParameterUpdater → 읽기 전용 (Revit 반영)
    ///   ⑦ ScheduleManager → 읽기 전용 (수량 집계)
    /// </summary>
    public class WindowUnit
    {
        // ─── ① CAD에서 추출 ───────────────────────────────────────────

        /// <summary>유형마크. 예: "WD1"</summary>
        public string TypeCode { get; set; } = string.Empty;

        /// <summary>창호/문 구분</summary>
        public WindowType WindowType { get; set; }

        /// <summary>너비 (mm)</summary>
        public double WidthMm { get; set; }

        /// <summary>높이 (mm)</summary>
        public double HeightMm { get; set; }

        /// <summary>CAD에서 추출한 배치 좌표 (Revit 내부 단위 feet)</summary>
        public XYZ Location { get; set; } = XYZ.Zero;

        // ─── ② Revit 배치 후 기록 ────────────────────────────────────

        /// <summary>
        /// Revit에 배치된 창호 Element ID.
        /// ②에서 배치 직후 기록, ⑥에서 파라미터 교체 대상 추적에 사용.
        /// 배치 전에는 null.
        /// </summary>
        public ElementId? RevitElementId { get; set; }

        // ─── ⑤ 제품 선택 후 기록 ─────────────────────────────────────

        /// <summary>
        /// 사용자가 선택한 최종 제품.
        /// 선택 전에는 null.
        /// </summary>
        public VendorProduct? SelectedProduct { get; set; }

        /// <summary>제품 선택 완료 여부</summary>
        public bool IsProductSelected
        {
            get { return SelectedProduct != null; }
        }
    }
}