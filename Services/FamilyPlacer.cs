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

        /// <summary>
        /// CAD 파싱 데이터를 받아 벽체와 창호를 자동으로 배치합니다.
        /// </summary>
        public void ExecutePlacement(CadParseResult parsedData, FamilySymbol defaultWindowSymbol)
        {
            using (Transaction trans = new Transaction(_doc, "벽체 및 창호 자동 생성"))
            {
                trans.Start();

                // 1. 벽체 생성
                Level defaultLevel = _doc.ActiveView.GenLevel;
                
                // "일반 - 200mm" 벽 유형을 찾기, 없으면 기본 벽 유형 사용
                WallType wallType200mm = new FilteredElementCollector(_doc)
                    .OfClass(typeof(WallType))
                    .Cast<WallType>()
                    .FirstOrDefault(w => w.Name.Contains("200mm") || w.Name == "일반 - 200mm");
                
                ElementId wallTypeId = wallType200mm != null 
                    ? wallType200mm.Id 
                    : _doc.GetDefaultElementTypeId(ElementTypeGroup.WallType);
                
                // 벽체 생성 (높이 3000mm = 약 9.84 feet)
                foreach (var line in parsedData.WallCenterlines)
                {
                    Wall.Create(_doc, line, wallTypeId, defaultLevel.Id, 3000.0 / 304.8, 0, false, false);
                }

                // [필수] 벽 생성 후 DB 갱신
                _doc.Regenerate();

                // 2. 창호 배치
                var allWindowSymbols = new FilteredElementCollector(_doc)
                    .OfCategory(BuiltInCategory.OST_Windows)
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()
                    .ToList();

                var walls = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                    .OfClass(typeof(Wall))
                    .Cast<Wall>()
                    .ToList();

                foreach (var winData in parsedData.WindowDataList)
                {
                    // 마크와 일치하는 패밀리 찾기 (없으면 기본 창호 사용)
                    FamilySymbol symbol = allWindowSymbols.FirstOrDefault(s => 
                        s.Name == winData.Mark ||
                        (s.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK) != null &&
                         s.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK).AsString() == winData.Mark)) 
                        ?? defaultWindowSymbol;

                    if (symbol == null) continue;
                    if (!symbol.IsActive) symbol.Activate();

                    XYZ insertPoint = null;
                    Wall hostwall = null;

                    foreach (var wall in walls)
                    {
                        LocationCurve wallCurve = wall.Location as LocationCurve;
                        if (wallCurve == null) continue;
                        Curve wallLine = wallCurve.Curve;
                        Curve winLine = winData.CenterLine;
                        IntersectionResultArray results = null;
                        SetComparisonResult result = wallLine.Intersect(winLine, out results);
                        if (result == SetComparisonResult.Overlap && results != null && results.Size > 0)
                        {
                            for (int i = 0; i < results.Size; i++)
                            {
                                XYZ pt = results.get_Item(i).XYZPoint;
                                if (IsPointOnSegment(pt, wallLine) && IsPointOnSegment(pt, winLine))
                                {
                                    insertPoint = pt;
                                    hostwall = wall;
                                    break;
                                }
                            }
                        }
                        if (insertPoint != null) break;
                    }

                    // 교차점이 없으면 가장 가까운 벽 사용
                    if (insertPoint == null || hostwall == null)
                    {
                        hostwall = Utility.FindNearestWall(walls, winData.Location);
                        insertPoint = winData.Location;
                    }

                    if (hostwall != null && insertPoint != null)
                    {
                        Level winLevel = _doc.GetElement(hostwall.LevelId) as Level;
                        FamilyInstance windowInst = _doc.Create.NewFamilyInstance(
                            insertPoint, 
                            symbol, 
                            hostwall, 
                            winLevel, 
                            Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                        // [추가] 유형 특성(타입 파라미터)에 치수 입력
                        try
                        {
                            string[] widthParamNames = { "Width", "폭", "너비", "Nominal Width", "최소 폭" };
                            foreach (var paramName in widthParamNames)
                            {
                                Parameter widthParam = symbol.LookupParameter(paramName);
                                if (widthParam != null && !widthParam.IsReadOnly && winData.Width > 0)
                                {
                                    widthParam.Set(winData.Width);
                                    break;
                                }
                            }
                            string[] heightParamNames = { "Height", "높이", "Nominal Height", "최소 높이" };
                            foreach (var paramName in heightParamNames)
                            {
                                Parameter heightParam = symbol.LookupParameter(paramName);
                                if (heightParam != null && !heightParam.IsReadOnly && winData.Height > 0)
                                {
                                    heightParam.Set(winData.Height);
                                    break;
                                }
                            }
                        }
                        catch { /* 무시 */ }

                        // [추가] CAD에서 읽은 창호 유형(마크)을 FamilySymbol(타입 파라미터)에 저장
                        try
                        {
                            string[] markParamNames = { "Type Mark", "유형마크", "Mark" };
                            foreach (var paramName in markParamNames)
                            {
                                Parameter typeMarkParam = symbol.LookupParameter(paramName);
                                if (typeMarkParam != null && !typeMarkParam.IsReadOnly && !string.IsNullOrEmpty(winData.Mark))
                                {
                                    typeMarkParam.Set(winData.Mark);
                                    break;
                                }
                            }
                        }
                        catch { /* 무시 */ }

                        // 🔧 [추가] CAD에서 읽은 데이터를 Revit 매개변수에 저장
                        try
                        {
                            // 1. Mark (유형마크) - CAD에서 읽은 마크로 설정 (예: "BPW")
                            Parameter markParam = windowInst.LookupParameter("Mark");
                            if (markParam != null && !markParam.IsReadOnly && !string.IsNullOrEmpty(winData.Mark))
                            {
                                markParam.Set(winData.Mark);
                            }
                            
                            // 2. Width - 여러 이름 시도 (Width, 폭, 너비 등)
                            string[] widthParamNames = { "Width", "폭", "너비", "Nominal Width", "최소 폭" };
                            foreach (var paramName in widthParamNames)
                            {
                                Parameter widthParam = windowInst.LookupParameter(paramName);
                                if (widthParam != null && !widthParam.IsReadOnly && winData.Width > 0)
                                {
                                    widthParam.Set(winData.Width);
                                    break;
                                }
                            }
                            
                            // 3. Height - 여러 이름 시도 (Height, 높이 등)
                            string[] heightParamNames = { "Height", "높이", "Nominal Height", "최소 높이" };
                            foreach (var paramName in heightParamNames)
                            {
                                Parameter heightParam = windowInst.LookupParameter(paramName);
                                if (heightParam != null && !heightParam.IsReadOnly && winData.Height > 0)
                                {
                                    heightParam.Set(winData.Height);
                                    break;
                                }
                            }
                        }
                        catch (Exception paramEx)
                        {
                            // 매개변수 설정 실패해도 창호는 배치됨
                        }
                    }
                }

                trans.Commit();
            }
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
                        FamilyInstance windowInst = _doc.Create.NewFamilyInstance(data.Location, symbol, hostwall, level, Autodesk.Revit.DB.Structure.StructuralType.NonStructural);
                        
                        // [추가] 유형 특성(타입 파라미터)에 치수 입력
                        try
                        {
                            string[] widthParamNames = { "Width", "폭", "너비", "Nominal Width", "최소 폭" };
                            foreach (var paramName in widthParamNames)
                            {
                                Parameter widthParam = symbol.LookupParameter(paramName);
                                if (widthParam != null && !widthParam.IsReadOnly && data.Width > 0)
                                {
                                    widthParam.Set(data.Width);
                                    break;
                                }
                            }
                            string[] heightParamNames = { "Height", "높이", "Nominal Height", "최소 높이" };
                            foreach (var paramName in heightParamNames)
                            {
                                Parameter heightParam = symbol.LookupParameter(paramName);
                                if (heightParam != null && !heightParam.IsReadOnly && data.Height > 0)
                                {
                                    heightParam.Set(data.Height);
                                    break;
                                }
                            }
                        }
                        catch { /* 무시 */ }

                        // [추가] CAD에서 읽은 창호 유형(마크)을 FamilySymbol(타입 파라미터)에 저장
                        try
                        {
                            string[] markParamNames = { "Type Mark", "유형마크", "Mark" };
                            foreach (var paramName in markParamNames)
                            {
                                Parameter typeMarkParam = symbol.LookupParameter(paramName);
                                if (typeMarkParam != null && !typeMarkParam.IsReadOnly && !string.IsNullOrEmpty(data.Mark))
                                {
                                    typeMarkParam.Set(data.Mark);
                                    break;
                                }
                            }
                        }
                        catch { /* 무시 */ }

                        // 🔧 [추가] CAD에서 읽은 데이터를 Revit 매개변수에 저장
                        try
                        {
                            // 1. Mark (유형마크) - CAD에서 읽은 마크로 설정 (예: "BPW")
                            Parameter markParam = windowInst.LookupParameter("Mark");
                            if (markParam != null && !markParam.IsReadOnly && !string.IsNullOrEmpty(data.Mark))
                            {
                                markParam.Set(data.Mark);
                            }
                            
                            // 2. Width - 여러 이름 시도 (Width, 폭, 너비 등)
                            string[] widthParamNames = { "Width", "폭", "너비", "Nominal Width", "최소 폭" };
                            foreach (var paramName in widthParamNames)
                            {
                                Parameter widthParam = windowInst.LookupParameter(paramName);
                                if (widthParam != null && !widthParam.IsReadOnly && data.Width > 0)
                                {
                                    widthParam.Set(data.Width);
                                    break;
                                }
                            }
                            
                            // 3. Height - 여러 이름 시도 (Height, 높이 등)
                            string[] heightParamNames = { "Height", "높이", "Nominal Height", "최소 높이" };
                            foreach (var paramName in heightParamNames)
                            {
                                Parameter heightParam = windowInst.LookupParameter(paramName);
                                if (heightParam != null && !heightParam.IsReadOnly && data.Height > 0)
                                {
                                    heightParam.Set(data.Height);
                                    break;
                                }
                            }
                        }
                        catch (Exception paramEx)
                        {
                            // 매개변수 설정 실패해도 창호는 배치됨
                        }
                        
                        successCount++;
                    }
                }

                trans.Commit();
            }
            return successCount;
        }

        /// <summary>
        /// 교차점이 실제 선분 범위 내에 있는지 검증합니다.
        /// </summary>
        private bool IsPointOnSegment(XYZ point, Curve segment)
        {
            const double tolerance = 0.001; // mm 단위

            XYZ p1 = segment.GetEndPoint(0);
            XYZ p2 = segment.GetEndPoint(1);
            XYZ v = p2 - p1;  // 선분 방향 벡터
            XYZ w = point - p1; // 시작점에서 교차점으로의 벡터

            double vLen = v.GetLength();
            if (vLen < tolerance) return false;

            // 외적으로 점이 선 위에 있는지 확인 (직선 방정식)
            XYZ cross = v.CrossProduct(w);
            if (cross.GetLength() > tolerance) return false;

            // 내적으로 점이 선분 범위 내에 있는지 확인
            double t = w.DotProduct(v) / (vLen * vLen);
            return t >= -tolerance && t <= 1 + tolerance;
        }

        /// <summary>
        /// 벽과 창호의 교차점을 선분 범위 내에서 찾습니다.
        /// </summary>
        private bool IsPointInsideWallExtent(XYZ point, Wall wall)
        {
            LocationCurve wallCurve = wall.Location as LocationCurve;
            if (wallCurve == null) return false;

            Curve curve = wallCurve.Curve;
            XYZ p1 = curve.GetEndPoint(0);
            XYZ p2 = curve.GetEndPoint(1);

            // 점에서 선분까지의 최단거리 계산
            double t = ClosestPointOnLineSegment(point, p1, p2);

            // t가 0~1 사이면 점이 선분 범위 내
            return t >= -0.001 && t <= 1.001;
        }

        /// <summary>
        /// 점에서 선분까지의 매개변수 t를 계산 (0=p1, 1=p2)
        /// </summary>
        private double ClosestPointOnLineSegment(XYZ point, XYZ p1, XYZ p2)
        {
            XYZ v = p2 - p1;
            XYZ w = point - p1;
            double vlen2 = v.DotProduct(v);
            
            if (vlen2 < 0.001) return 0;
            
            return w.DotProduct(v) / vlen2;
        }

        /// <summary>
        /// 벽과 창호의 교차점을 찾습니다.
        /// </summary>
        private XYZ FindIntersectionPoint(Curve wallLine, Curve windowLine)
        {
            try
            {
                IntersectionResultArray results = null;
                SetComparisonResult result = wallLine.Intersect(windowLine, out results);

                if (result == SetComparisonResult.Overlap)
                {
                    // 교차 또는 중첩하는 경우
                    if (results != null && results.Size > 0)
                    {
                        // 첫 번째 교차점의 UV 매개변수 가져오기
                        IntersectionResult intResult = results.get_Item(0);
                        return intResult.XYZPoint;
                    }
                }
            }
            catch
            {
                // 교차점 계산 실패는 무시
            }

            return null;
        }
    }
}
