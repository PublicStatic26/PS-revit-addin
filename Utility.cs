using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace PSRevitAddin
{
    /// <summary>
    /// 공통 유틸리티: 단위 변환, 문자열 파싱, 좌표 변환
    /// </summary>
    internal static class Utility
    {
        #region 단위 변환
        // Revit 내부 단위는 피트(feet). CAD/UI는 밀리미터(mm) 기준.

        public static double FeetToMm(double feet)
        {
            return feet * 304.8;
        }

        public static double MmToFeet(double mm)
        {
            return mm / 304.8;
        }

        #endregion

        #region 문자열 파싱

        /// <summary>
        /// 블록명에서 유형마크를 추출한다. 예: "WD1_1200x1500" → "WD1"
        /// </summary>
        public static string ParseTypeCode(string blockName)
        {
            if (string.IsNullOrWhiteSpace(blockName)) return string.Empty;

            int underscoreIndex = blockName.IndexOf('_');
            return underscoreIndex > 0
                ? blockName.Substring(0, underscoreIndex)
                : blockName;
        }

        /// <summary>
        /// 치수 문자열에서 너비(W)를 파싱한다. 예: "1200x1500" → 1200
        /// </summary>
        public static double ParseWidth(string dimensionStr)
        {
            return ParseDimension(dimensionStr, index: 0);
        }

        /// <summary>
        /// 치수 문자열에서 높이(H)를 파싱한다. 예: "1200x1500" → 1500
        /// </summary>
        public static double ParseHeight(string dimensionStr)
        {
            return ParseDimension(dimensionStr, index: 1);
        }

        private static double ParseDimension(string dimensionStr, int index)
        {
            if (string.IsNullOrWhiteSpace(dimensionStr)) return 0;

            string[] parts = dimensionStr.Split('x', 'X');
            if (parts.Length <= index) return 0;

            return double.TryParse(parts[index].Trim(), out double value) ? value : 0;
        }

        #endregion

        #region 좌표 변환

        /// <summary>
        /// XYZ 좌표를 mm 단위로 변환한다. (Revit 내부 단위 feet → mm)
        /// </summary>
        public static (double X, double Y, double Z) ToMm(XYZ point)
        {
            return (FeetToMm(point.X), FeetToMm(point.Y), FeetToMm(point.Z));
        }

        /// <summary>
        /// mm 좌표를 Revit XYZ(feet)로 변환한다.
        /// </summary>
        public static XYZ ToRevitPoint(double xMm, double yMm, double zMm = 0)
        {
            return new XYZ(MmToFeet(xMm), MmToFeet(yMm), MmToFeet(zMm));
        }

        #endregion

        #region 요소 검색

        /// <summary>
        /// 주어진 위치에서 가장 가까운 벽을 찾는다.
        /// </summary>
        public static Wall FindNearestWall(List<Wall> walls, XYZ location)
        {
            if (walls == null || walls.Count == 0)
                return null;

            Wall nearestWall = null;
            double minDistance = double.MaxValue;

            foreach (Wall wall in walls)
            {
                // 벽의 위치 정보 (LocationCurve)에서 곡선 추출
                LocationCurve wallLocation = wall.Location as LocationCurve;
                if (wallLocation == null) continue;

                Curve curve = wallLocation.Curve;
                if (curve == null) continue;

                // 곡선 위의 가장 가까운 거리 계산
                // Revit에서는 곡선 위의 정확한 가장 가까운 점을 직접 구하기 위해
                // 끝점들 중 더 가까운 점을 기준으로 거리를 계산
                XYZ p1 = curve.GetEndPoint(0);
                XYZ p2 = curve.GetEndPoint(1);

                double dist1 = location.DistanceTo(p1);
                double dist2 = location.DistanceTo(p2);
                double minDist = Math.Min(dist1, dist2);

                if (minDist < minDistance)
                {
                    minDistance = minDist;
                    nearestWall = wall;
                }
            }

            return nearestWall;
        }
        #endregion
    }
}

     