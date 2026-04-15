using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PSRevitAddin.Models;
using System;
using System.Collections.Generic;

namespace PSRevitAddin.Services
{
    /// <summary>
    /// ② Revit 기본 창호 객체 생성
    /// </summary>
    public class FamilyPlacer
    {
        private Document _doc;

        public FamilyPlacer(Document doc)
        {
            _doc = doc;
        }

        public int PlaceWindows(List<CadWindowData> dataList)
        {
            int successCount = 0;
            var allWindowSymbols = new FilteredElementCollector(_doc).OfCategory(BuiltInCategory.OST_Windows).OfClass(typeof(Wall)).Cast<FamilySymbol>().ToList();
            var walls = new FilteredElementCollector(_doc, _doc.ActiveView.Id).OfClass(typeof(Wall)).Cast<Wall>().ToList();
            using (Transaction trans = new Transaction(_doc, "cad 배치"))
            {
                trans.Start();
                foreach (var data in dataList)
                {
                    FamilySymbol symbol = allWindowSymbols.FirstOrDefault(s => s.Name == data.Mark);
                    if (symbol == null) continue;
                    if (!symbol.IsActive) symbol.Activate();

                    Wall hostwall = Utility.FindNearestWall(walls, data.Location);
                    if (hostwall != null)
                    {
                        Level level = _doc.GetElement(hostwall.LevelId) as Level;
                        _doc.Create.NewFamilyInstance(data.Location, symbol, hostwall, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        successCount++;
                    }
                }

                trans.Commit();
            }
            return successCount;
        }
    }
}
