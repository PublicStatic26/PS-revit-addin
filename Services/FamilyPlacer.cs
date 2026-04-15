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
        public void ExecutePlacement(CadParseResult parsedData, FamilySymbol defaultWindowSymbol)
        {
            using (Transaction trans = new Transaction(_doc, "벽체 및 창호 자동 생성"))
            {
                trans.Start();

                // ==========================================
                // 1. 벽체 생성
                // ==========================================
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

                // [필수] 벽 생성 후 교차점 계산을 위해 DB 갱신
                _doc.Regenerate();

                // ==========================================
                // 2. 창호 생성 및 배치
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

                foreach (var winData in parsedData.WindowDataList)
                {
                    // 마크와 일치하는 '원본 거푸집' 패밀리 찾기 (없으면 매개변수로 받은 기본 창호 사용)
                    FamilySymbol baseSymbol = allWindowSymbols.FirstOrDefault(s =>
                        s.Name == winData.Mark ||
                        (s.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK) != null &&
                         s.get_Parameter(BuiltInParameter.ALL_MODEL_TYPE_MARK).AsString() == winData.Mark))
                        ?? defaultWindowSymbol;

                    if (baseSymbol == null) continue;

                    // ---------------------------------------------------------
                    // 🌟 [핵심] 치수에 맞게 새로운 유형 복제 (Duplicate) 🌟
                    // ---------------------------------------------------------

                    // mm 단위로 변환해서 직관적인 새 유형 이름 만들기 (예: "W1500_H2000")
                    double widthMm = winData.Width * 304.8;
                    double heightMm = winData.Height * 304.8;
                    string newTypeName = $"W{widthMm:0}_H{heightMm:0}";

                    if (!string.IsNullOrEmpty(winData.Mark))
                    {
                        newTypeName = $"{winData.Mark}_{newTypeName}"; // 예: "BPW_W1500_H2000"
                    }

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

                        // 씰높이만큼 Z축 오프셋 적용
                        XYZ finalInsertPoint = new XYZ(insertPoint.X, insertPoint.Y, insertPoint.Z + winData.SillHeight);

                        FamilyInstance windowInst = _doc.Create.NewFamilyInstance(
                            finalInsertPoint,
                            targetSymbol, // 원본이 아닌 '복제본' 지정!
                            hostwall,
                            winLevel,
                            Autodesk.Revit.DB.Structure.StructuralType.NonStructural);

                        // 인스턴스 파라미터에도 씰높이 저장
                        Parameter sillParam = windowInst.LookupParameter("Sill Height") 
                            ?? windowInst.LookupParameter("씰 높이")
                            ?? windowInst.LookupParameter("씰높이");
                        if (sillParam != null && !sillParam.IsReadOnly && winData.SillHeight > 0)
                            sillParam.Set(winData.SillHeight);

                        // 인스턴스의 Mark(마크) 매개변수에도 값 넣어주기
                        Parameter instMarkParam = windowInst.LookupParameter("Mark");
                        if (instMarkParam != null && !instMarkParam.IsReadOnly && !string.IsNullOrEmpty(winData.Mark))
                        {
                            instMarkParam.Set(winData.Mark);
                        }
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