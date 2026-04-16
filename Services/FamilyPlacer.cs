using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using PSRevitAddin.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public void PlaceWalls(CadParseResult parsedData)
        {
            using (Transaction trans = new Transaction(_doc, "벽체 자동 생성"))
            {
                trans.Start();

                Level defaultLevel = _doc.ActiveView.GenLevel;

                WallType wallType200mm = new FilteredElementCollector(_doc)
                    .OfClass(typeof(WallType))
                    .Cast<WallType>()
                    .FirstOrDefault(w => w.Name.Contains("200mm") || w.Name == "일반 - 200mm");

                ElementId wallTypeId = wallType200mm != null
                    ? wallType200mm.Id
                    : _doc.GetDefaultElementTypeId(ElementTypeGroup.WallType);

                foreach (var line in parsedData.WallCenterlines)
                {
                    Wall.Create(_doc, line, wallTypeId, defaultLevel.Id, 4000.0 / 304.8, 0, false, false);
                }

                trans.Commit();
            }
        }

        public void PlaceWindows(CadParseResult parsedData, FamilySymbol defaultWindowSymbol)
        {
            using (Transaction trans = new Transaction(_doc, "창호 자동 배치"))
            {
                trans.Start();

                // ==========================================
                // 창호 생성 및 배치
                // ==========================================
                var allWindowSymbols = new FilteredElementCollector(_doc)
                    .OfCategory(BuiltInCategory.OST_Windows)
                    .OfClass(typeof(FamilySymbol))
                    .Cast<FamilySymbol>()
                    .ToList();

                var walls = new FilteredElementCollector(_doc, _doc.ActiveView.Id)
                    .OfClass(typeof(Wall))
                    .Cast<Wall>()
                    .ToList();

                // 항상 "WINDOW-어셈블" 패밀리를 기준으로 사용 (없으면 defaultWindowSymbol)
                FamilySymbol windowAssembleBase = allWindowSymbols.FirstOrDefault(s =>
                    s.Family.Name == "WINDOW-어셈블" || s.Name == "WINDOW-어셈블")
                    ?? defaultWindowSymbol;

                foreach (var winData in parsedData.WindowDataList)
                {
                    // 항상 "WINDOW-어셈블" 패밀리를 기준으로 유형 복제
                    FamilySymbol baseSymbol = windowAssembleBase;

                    if (baseSymbol == null) continue;

                    // ---------------------------------------------------------
                    // 🌟 [핵심] 치수에 맞게 새로운 유형 복제 (Duplicate) 🌟
                    // ---------------------------------------------------------

                    // 유형 이름: 유형마크만 사용 (없으면 치수로 대체)
                    double widthMm = winData.Width * 304.8;
                    double heightMm = winData.Height * 304.8;
                    double sillMm = winData.SillHeight * 304.8;
                    string newTypeName = !string.IsNullOrEmpty(winData.Mark)
                        ? winData.Mark
                        : (sillMm > 0 ? $"W{widthMm:0}_H{heightMm:0}_S{sillMm:0}" : $"W{widthMm:0}_H{heightMm:0}");

                    // 같은 Family 내에서 방금 지은 이름의 유형이 이미 있는지 확인
                    FamilySymbol targetSymbol = null;
                    foreach (ElementId symbolId in baseSymbol.Family.GetFamilySymbolIds())
                    {
                        FamilySymbol existingSymbol = _doc.GetElement(symbolId) as FamilySymbol;
                        if (existingSymbol != null && existingSymbol.Name == newTypeName)
                        {
                            targetSymbol = existingSymbol;
                            break;
                        }
                    }

                    // 없으면 새로 복제하고 치수 기입
                    if (targetSymbol == null)
                    {
                        targetSymbol = baseSymbol.Duplicate(newTypeName) as FamilySymbol;

                        // 복제된 새 유형에 치수 및 마크 입력 (유형 속성)
                        SetSymbolParameter(targetSymbol, new[] { "Width", "폭", "너비", "Nominal Width", "최소 폭" }, winData.Width);
                        SetSymbolParameter(targetSymbol, new[] { "Height", "높이", "Nominal Height", "최소 높이" }, winData.Height);
                        SetSymbolParameter(targetSymbol, new[] { "Sill Height", "씰 높이", "씰높이", "하부 높이" }, winData.SillHeight);
                        SetSymbolParameterString(targetSymbol, new[] { "Type Mark", "유형마크", "Mark" }, winData.Mark);
                    }

                    // 새로 만든 심볼(또는 이미 있던 심볼)을 활성화
                    if (!targetSymbol.IsActive) targetSymbol.Activate();
                    // ---------------------------------------------------------


                    // ---------------------------------------------------------
                    // 3. 교차점 찾기 및 3D 배치
                    // ---------------------------------------------------------
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

                    // 창호 실제 배치 (복제된 targetSymbol 사용)
                    if (hostwall != null && insertPoint != null)
                    {
                        Level winLevel = _doc.GetElement(hostwall.LevelId) as Level;

                        // 삽입점을 벽 중심선에 투영하되, 벽 끝에서 일정 거리 안쪽으로 클램핑
                        XYZ projectedPoint = insertPoint;
                        LocationCurve hostWallCurve = hostwall.Location as LocationCurve;
                        if (hostWallCurve != null)
                        {
                            Curve wc = hostWallCurve.Curve;
                            IntersectionResult proj = wc.Project(insertPoint);
                            // 벽 파라미터 t: 0=시작, 1=끝. 끝에서 5% 안쪽으로 클램핑
                            double t = proj.Parameter;
                            double tNorm = (t - wc.GetEndParameter(0)) / (wc.GetEndParameter(1) - wc.GetEndParameter(0));

                            // 디버그: 벽 범위 벗어난 창호 확인
                            if (tNorm < 0.05 || tNorm > 0.95)
                            {
                                Autodesk.Revit.UI.TaskDialog.Show("범위 이탈 창호",
                                    $"창호 '{winData.Mark}' 삽입점이 벽 끝 근처입니다.\n" +
                                    $"tNorm = {tNorm:F3}\n" +
                                    $"삽입점: ({insertPoint.X:F2}, {insertPoint.Y:F2})\n" +
                                    $"벽 시작: ({wc.GetEndPoint(0).X:F2}, {wc.GetEndPoint(0).Y:F2})\n" +
                                    $"벽 끝: ({wc.GetEndPoint(1).X:F2}, {wc.GetEndPoint(1).Y:F2})");
                            }

                            tNorm = Math.Max(0.05, Math.Min(0.95, tNorm));
                            double tClamped = wc.GetEndParameter(0) + tNorm * (wc.GetEndParameter(1) - wc.GetEndParameter(0));
                            XYZ safePoint = wc.Evaluate(tClamped, false);
                            projectedPoint = new XYZ(safePoint.X, safePoint.Y, winLevel.Elevation);
                        }
                        else
                        {
                            projectedPoint = new XYZ(insertPoint.X, insertPoint.Y, winLevel.Elevation);
                        }

                        FamilyInstance windowInst = _doc.Create.NewFamilyInstance(
                            projectedPoint,
                            targetSymbol,
                            hostwall,
                            winLevel,
                            Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                        _doc.Regenerate();

                        // 씰높이 인스턴스 파라미터
                        Parameter sillParam = windowInst.LookupParameter("Sill Height") 
                            ?? windowInst.LookupParameter("씰 높이")
                            ?? windowInst.LookupParameter("씰높이");
                        if (sillParam != null && !sillParam.IsReadOnly && winData.SillHeight > 0)
                            sillParam.Set(winData.SillHeight);

                        // Mark 파라미터
                        Parameter instMarkParam = windowInst.LookupParameter("Mark");
                        if (instMarkParam != null && !instMarkParam.IsReadOnly && !string.IsNullOrEmpty(winData.Mark))
                            instMarkParam.Set(winData.Mark);
                    }
                }

                trans.Commit();
            }
        }

        // ==========================================
        // 유틸리티 및 헬퍼 메서드 모음
        // ==========================================

        /// <summary>
        /// 여러 파라미터 이름 중 일치하는 것을 찾아 숫자(피트) 값을 입력합니다.
        /// </summary>
        private void SetSymbolParameter(FamilySymbol symbol, string[] paramNames, double value)
        {
            if (value <= 0) return;
            foreach (var paramName in paramNames)
            {
                Parameter param = symbol.LookupParameter(paramName);
                if (param != null && !param.IsReadOnly)
                {
                    param.Set(value);
                    break; // 성공하면 루프 종료
                }
            }
        }

        /// <summary>
        /// 여러 파라미터 이름 중 일치하는 것을 찾아 문자열 값을 입력합니다.
        /// </summary>
        private void SetSymbolParameterString(FamilySymbol symbol, string[] paramNames, string value)
        {
            if (string.IsNullOrEmpty(value)) return;
            foreach (var paramName in paramNames)
            {
                Parameter param = symbol.LookupParameter(paramName);
                if (param != null && !param.IsReadOnly)
                {
                    param.Set(value);
                    break; // 성공하면 루프 종료
                }
            }
        }

        /// <summary>
        /// 교차점이 실제 선분 범위 내에 있는지 검증합니다.
        /// </summary>
        private bool IsPointOnSegment(XYZ point, Curve segment)
        {
            const double tolerance = 0.001; // 피트 단위 공차

            XYZ p1 = segment.GetEndPoint(0);
            XYZ p2 = segment.GetEndPoint(1);
            XYZ v = p2 - p1;  // 선분 방향 벡터
            XYZ w = point - p1; // 시작점에서 교차점으로의 벡터

            double vLen = v.GetLength();
            if (vLen < tolerance) return false;

            // 외적으로 점이 직선(무한선) 위에 있는지 확인
            XYZ cross = v.CrossProduct(w);
            if (cross.GetLength() > tolerance) return false;

            // 내적으로 점이 선분의 양 끝점(p1, p2) 사이에 있는지 확인
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