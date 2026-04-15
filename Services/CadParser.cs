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
                .Where(x => x.TextNoteType.Name.Contains("창호정리_2-돋움체-23"))
                .ToList();

            foreach (var lineElement in detailLines)
            {
                Curve curve = lineElement.GeometryCurve;
                XYZ midpoint = (curve.GetEndPoint(0) + curve.GetEndPoint(1)) / 2;

                var nearestText = textNotes.OrderBy(t =>
                {
                    double dx = t.Coord.X - midpoint.X;
                    double dy = t.Coord.Y - midpoint.Y;
                    return (dx * dx) + (dy * dy);
                }).FirstOrDefault();

                if (nearestText != null)
                {
                    var data = new CadWindowData();
                    data.Location = midpoint;
                    data.CenterLine = curve;
                    string rawText = nearestText.Text;
                    
                    if (rawText.Contains("_"))
                    {
                        data.Mark = Utility.ParseTypeCode(rawText);
                        var splitText = rawText.Split('_');
                        if (splitText.Length > 1)
                        {
                            string dimpart = rawText.Split('_')[1];
                            data.Width = Utility.MmToFeet(Utility.ParseWidth(dimpart));
                            data.Height = Utility.MmToFeet(Utility.ParseHeight(dimpart));
                        }
                    }
                    results.Add(data);
                }
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