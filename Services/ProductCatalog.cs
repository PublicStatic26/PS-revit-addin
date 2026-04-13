using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PSRevitAddin.Models;
using System;
using System.Collections.Generic;
using System.IO;

using ClosedXML.Excel;
using System.Linq;

namespace PSRevitAddin.Services
{
    /// <summary>
    /// ③ Excel에서 제품 목록 조회
    /// </summary>
    public class ProductCatalog
    {
        // Excel 파일 경로를 저장할 변수
        private string _excelPath;

        // 생성자 - 경로를 받아서 저장
        public ProductCatalog(string excelPath)
        {
            _excelPath = excelPath;
        }


        public List<VendorProduct> GetAllProducts()
        {
            var list = new List<VendorProduct>();

            // Excel 파일 열기
            using (var wb = new XLWorkbook(_excelPath))
            {
                //첫 번째 시트
                var ws = wb.Worksheet(1);

                //데이터 있는 행만 가져온다
                foreach (var row in ws.RowsUsed().Skip(1))
                {
                    var product = new VendorProduct
                    {

                        SymbolCode = row.Cell(1).GetValue<string>(),

                        VendorName = row.Cell(2).GetValue<string>(),

                        ProductName = row.Cell(3).GetValue<string>(),

                        ModelNumber = row.Cell(4).GetValue<string>(),

                        OpeningMethod = ParseOpening(row.Cell(5).GetValue<string>()),

                        FrameType = ParseFrame(row.Cell(6).GetValue<string>()),

                        GlassType = ParseGlass(row.Cell(12).GetValue<string>()),

                        MaxWidthMm = row.Cell(13).GetValue<double>(),

                        MaxHeightMm = row.Cell(14).GetValue<double>(),

                        MinWidthMm = row.Cell(15).GetValue<double>(),

                        MinHeightMm = row.Cell(16).GetValue<double>(),

                        IsFireRated = row.Cell(18).GetValue<string>() == "Y",

                        IsInsulated = row.Cell(20).GetValue<double>() > 0,

                        UnitPrice = row.Cell(21).GetValue<decimal>(),
                    };
                    list.Add(product);
                }
            }
            return list;
        }
        private OpeningMethod ParseOpening(string val) => val switch
        {
            "고정창" => OpeningMethod.Fixed,
            "슬라이딩" => OpeningMethod.Sliding,
            "프로젝트" => OpeningMethod.ProjectOut,
            "여닫이" => OpeningMethod.CasementSwing,
            _ => OpeningMethod.Fixed
        };

        private FrameType ParseFrame(string val) => val switch
        {
            "알루미늄" => FrameType.Aluminum,
            "PVC" => FrameType.Pvc,
            "AL+PVC" => FrameType.AlPvc,
            "복합" => FrameType.Combination,
            "커튼월" => FrameType.CurtainWall,
            _ => FrameType.Aluminum
        };

        private GlassType ParseGlass(string val) => val switch
        {
            "로이유리" => GlassType.LowE,
            "복층유리" => GlassType.Double,
            "삼중유리" => GlassType.Triple,
            "진공유리" => GlassType.Vacuum,
            "강화유리" => GlassType.Tempered,
            _ => GlassType.Standard
        };
    }
}
