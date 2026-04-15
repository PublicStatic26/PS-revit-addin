using PSRevitAddin.Models;

namespace PSRevitAddin.Services
{
    /// <summary>
    /// ⑤ 제품 선택 검증
    ///
    /// 사용 흐름:
    ///   1. 사용자가 comboBox7에서 창호 유형(WD1 등)을 선택한다.
    ///   2. 카드 목록에서 제품을 클릭해 선택한다.
    ///   3. Finish 버튼 클릭 시 이 클래스의 Validate()를 호출한다.
    ///   4. 검증 통과 시 ParameterUpdater로 Revit 파라미터를 적용한다.
    /// </summary>
    public class ProductSelector
    {
        /// <summary>
        /// 선택한 제품이 해당 창호 치수에 적용 가능한지 검증한다.
        /// </summary>
        /// <param name="product">카드에서 선택한 제품</param>
        /// <param name="window">comboBox7에서 선택한 창호 유형 (TypeCode + 치수)</param>
        /// <returns>검증 결과. IsValid가 false이면 Message에 실패 사유가 담긴다.</returns>
        public SelectionResult Validate(VendorProduct product, WindowUnit window)
        {
            // 너비 범위 확인
            if (window.WidthMm < product.MinWidthMm || window.WidthMm > product.MaxWidthMm)
            {
                return SelectionResult.Fail(
                    $"너비 불일치: {window.TypeCode} 너비 {window.WidthMm:0}mm 는 " +
                    $"{product.ProductName} 허용 범위({product.MinWidthMm:0}~{product.MaxWidthMm:0}mm) 밖입니다.");
            }

            // 높이 범위 확인
            if (window.HeightMm < product.MinHeightMm || window.HeightMm > product.MaxHeightMm)
            {
                return SelectionResult.Fail(
                    $"높이 불일치: {window.TypeCode} 높이 {window.HeightMm:0}mm 는 " +
                    $"{product.ProductName} 허용 범위({product.MinHeightMm:0}~{product.MaxHeightMm:0}mm) 밖입니다.");
            }

            return SelectionResult.Ok();
        }
    }

    /// <summary>
    /// Validate() 결과를 담는 클래스.
    /// IsValid가 true면 적용 가능, false면 Message에 실패 사유가 있다.
    /// </summary>
    public class SelectionResult
    {
        public bool IsValid { get; private set; }
        public string Message { get; private set; }

        private SelectionResult(bool isValid, string message)
        {
            IsValid = isValid;
            Message = message;
        }

        public static SelectionResult Ok()
        {
            return new SelectionResult(true, string.Empty);
        }

        public static SelectionResult Fail(string message)
        {
            return new SelectionResult(false, message);
        }
    }
}