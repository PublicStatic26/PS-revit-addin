using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PSRevitAddin.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace PSRevitAddin.Services
{
    /// <summary>
    /// ③ Excel에서 제품 목록 조회
    /// </summary>
    public class ProductCatalog
    {
        public List<VendorProduct> GetAll()
        {
            return new List<VendorProduct>
            {
                // ─── LG하우시스 ───────────────────────────────
                new VendorProduct
                {
                    VendorName    = "LG하우시스",
                    ProductName   = "슈퍼세이브6",
                    ModelNumber   = "SS6-1200",
                    MinWidthMm    = 600,  MaxWidthMm  = 1800,
                    MinHeightMm   = 900,  MaxHeightMm = 2400,
                    IsFireRated   = false,
                    IsInsulated   = true,
                    GlassType     = GlassType.Triple,
                    FrameType     = FrameType.Aluminum,
                    OpeningMethod = OpeningMethod.Sliding,
                    UnitPrice     = 850000
                },
                new VendorProduct
                {
                    VendorName    = "LG하우시스",
                    ProductName   = "슈퍼세이브5",
                    ModelNumber   = "SS5-900",
                    MinWidthMm    = 500,  MaxWidthMm  = 1500,
                    MinHeightMm   = 800,  MaxHeightMm = 2100,
                    IsFireRated   = true,
                    IsInsulated   = true,
                    GlassType     = GlassType.LowE,
                    FrameType     = FrameType.AlPvc,
                    OpeningMethod = OpeningMethod.CasementSwing,
                    UnitPrice     = 720000
                },

                // ─── KCC글라스 ───────────────────────────────
                new VendorProduct
                {
                    VendorName    = "KCC글라스",
                    ProductName   = "스위트홈 플러스",
                    ModelNumber   = "SH-1500",
                    MinWidthMm    = 700,  MaxWidthMm  = 2000,
                    MinHeightMm   = 1000, MaxHeightMm = 2500,
                    IsFireRated   = false,
                    IsInsulated   = true,
                    GlassType     = GlassType.Triple,
                    FrameType     = FrameType.Aluminum,
                    OpeningMethod = OpeningMethod.Sliding,
                    UnitPrice     = 790000
                },
                new VendorProduct
                {
                    VendorName    = "KCC글라스",
                    ProductName   = "스위트홈 방화",
                    ModelNumber   = "SH-F900",
                    MinWidthMm    = 500,  MaxWidthMm  = 1200,
                    MinHeightMm   = 800,  MaxHeightMm = 2100,
                    IsFireRated   = true,
                    IsInsulated   = false,
                    GlassType     = GlassType.Tempered,
                    FrameType     = FrameType.Aluminum,
                    OpeningMethod = OpeningMethod.CasementSwing,
                    UnitPrice     = 680000
                },

                // ─── 현대L&C ─────────────────────────────────
                new VendorProduct
                {
                    VendorName    = "현대L&C",
                    ProductName   = "하이샤시 프리미엄",
                    ModelNumber   = "HS-P1200",
                    MinWidthMm    = 600,  MaxWidthMm  = 1800,
                    MinHeightMm   = 900,  MaxHeightMm = 2400,
                    IsFireRated   = false,
                    IsInsulated   = true,
                    GlassType     = GlassType.Triple,
                    FrameType     = FrameType.AlPvc,
                    OpeningMethod = OpeningMethod.TurnTilt,
                    UnitPrice     = 920000
                },
                new VendorProduct
                {
                    VendorName    = "현대L&C",
                    ProductName   = "하이샤시 스탠다드",
                    ModelNumber   = "HS-S900",
                    MinWidthMm    = 500,  MaxWidthMm  = 1500,
                    MinHeightMm   = 800,  MaxHeightMm = 2100,
                    IsFireRated   = true,
                    IsInsulated   = true,
                    GlassType     = GlassType.LowE,
                    FrameType     = FrameType.Pvc,
                    OpeningMethod = OpeningMethod.Fixed,
                    UnitPrice     = 650000
                },
            };
        }
    }
}

