using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PSRevitAddin.Models;
using System;
using System.Collections.Generic;

namespace PSRevitAddin.Services
{
    /// <summary>
    /// ④ 방화/단열/유리 조건 AND 필터
    ///
    /// 사용 흐름:
    ///   1. MainForm에서 체크박스/콤보박스 변경 시 조건 속성을 업데이트한다.
    ///   2. Apply()를 호출해 조건에 맞는 제품 목록을 받는다.
    ///   3. 결과가 0개면 호출부(MainForm)에서 사용자에게 알림을 표시한다.
    /// </summary>
    public class ProductFilter
    {
        // ─── 필터 조건 ────────────────────────────────────────────────
        // null = 해당 조건 미적용 (모든 제품 통과)

        /// <summary>
        /// 표시할 제조사 이름 목록.
        /// 비어있으면 모든 제조사 통과, 하나라도 있으면 목록에 있는 제조사만 통과.
        /// </summary>
        public List<string> SelectedVendors { get; set; } = new List<string>();

        /// <summary>창호/문 프레임 재질 조건</summary>
        public FrameType? SelectedFrame { get; set; }

        /// <summary>유리 종류 조건</summary>
        public GlassType? SelectedGlass { get; set; }

        /// <summary>개폐 방식 조건</summary>
        public OpeningMethod? SelectedOpening { get; set; }

        /// <summary>방화 인증 필수 여부 (true면 방화 제품만 통과)</summary>
       
        public bool FilterFireRated { get; set; }
        

        /// <summary>단열 성능 필수 여부 (true면 단열 제품만 통과)</summary>
        public bool FilterInsulated { get; set; }

        // ─── 사이즈 조건 ──────────────────────────────────────────────
        // 0 = 미입력 (사이즈 조건 미적용)

        /// <summary>창호/문 너비 (mm). 제품의 Min~Max 범위 안에 들어야 통과.</summary>
        public double TargetWidthMm { get; set; }

        /// <summary>창호/문 높이 (mm). 제품의 Min~Max 범위 안에 들어야 통과.</summary>
        public double TargetHeightMm { get; set; }

        // ─── 핵심 메서드 ──────────────────────────────────────────────

        /// <summary>
        /// 현재 저장된 조건으로 제품 목록을 AND 필터링한다.
        /// 결과가 0개일 때의 알림 처리는 호출부(MainForm)에서 담당한다.
        /// </summary>
        public List<VendorProduct> Apply(List<VendorProduct> products)
        {
            List<VendorProduct> result = new List<VendorProduct>();

            foreach (VendorProduct product in products)
            {
                // 제조사 조건 (선택된 제조사가 없으면 전체 통과)
                if (SelectedVendors.Count > 0 && !SelectedVendors.Contains(product.VendorName))
                {
                    continue;
                }

                // 방화 조건
                if (FilterFireRated && !product.IsFireRated)
                {
                    continue;
                }

                // 단열 조건
                if (FilterInsulated && !product.IsInsulated)
                {
                    continue;
                }

                // 프레임 조건
                if (SelectedFrame != null && product.FrameType != SelectedFrame)
                {
                    continue;
                }

                // 유리 조건
                if (SelectedGlass != null && product.GlassType != SelectedGlass)
                {
                    continue;
                }

                // 개폐방식 조건
                if (SelectedOpening != null && product.OpeningMethod != SelectedOpening)
                {
                    continue;
                }

                // 너비 사이즈 조건
                if (TargetWidthMm > 0)
                {
                    if (TargetWidthMm < product.MinWidthMm || TargetWidthMm > product.MaxWidthMm)
                    {
                        continue;
                    }
                }

                // 높이 사이즈 조건
                if (TargetHeightMm > 0)
                {
                    if (TargetHeightMm < product.MinHeightMm || TargetHeightMm > product.MaxHeightMm)
                    {
                        continue;
                    }
                }

                result.Add(product);
            }

            return result;
        }

        /// <summary>모든 필터 조건을 초기 상태로 되돌린다.</summary>
        public void Reset()
        {
            SelectedVendors.Clear();
            SelectedFrame = null;
            SelectedGlass = null;
            SelectedOpening = null;
            FilterFireRated = false;
            FilterInsulated = false;
            TargetWidthMm = 0;
            TargetHeightMm = 0;
        }
    }
}
