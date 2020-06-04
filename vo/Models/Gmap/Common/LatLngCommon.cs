﻿using GMap.NET;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vo.Models.Gmap.Common
{
    class LatLngCommon
    {
        #region PointLatLng functions
        /// <summary>
        /// Calculate latitude and longitude from specific Point(Lat, Lng) have distance and bearing angle.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="bearing"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        public static (PointLatLng, double) CalcDistanceAndBearing(PointLatLng point1, double distance, double bearing)
        {
            double newLatitute;
            double newLongitude;
            double newBearing;

            CalcDistanceAndBearing(point1.Lat, point1.Lng, bearing, distance, out newLatitute, out newLongitude, out newBearing);

            return (new PointLatLng(newLatitute, newLongitude), newBearing);
        }

        /// <summary>
        /// Calculate distance between two points
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static double CalcDistance(PointLatLng point1, PointLatLng point2)
        {
            return CalcDistance(point1.Lat, point1.Lng, point2.Lat, point2.Lng);
        }

        /// <summary>
        /// Calculate bearing between two points
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static double CalcBearing(PointLatLng point1, PointLatLng point2)
        {
            // 현재 위치 : 위도나 경도는 지구 중심을 기반으로 하는 각도이기 때문에 라디안 각도로 변환한다.
            double Cur_Lat_radian = point1.Lat * (Math.PI / 180.0);
            double Cur_Lon_radian = point1.Lng * (Math.PI / 180.0);

            // 목표 위치 : 위도나 경도는 지구 중심을 기반으로 하는 각도이기 때문에 라디안 각도로 변환한다.
            double Dest_Lat_radian = point2.Lat * (Math.PI / 180.0);
            double Dest_Lon_radian = point2.Lng * (Math.PI / 180.0);

            // radian distance
            double radian_distance = Math.Acos(
                Math.Sin(Cur_Lat_radian) * Math.Sin(Dest_Lat_radian) + 
                Math.Cos(Cur_Lat_radian) * Math.Cos(Dest_Lat_radian) * Math.Cos(Cur_Lon_radian - Dest_Lon_radian));

            // 목적지 이동 방향을 구한다.(현재 좌표에서 다음 좌표로 이동하기 위해서는 방향을 설정해야 한다. 라디안값이다.
            double radian_bearing = Math.Acos(
                (Math.Sin(Dest_Lat_radian) - Math.Sin(Cur_Lat_radian) * Math.Cos(radian_distance)) / 
                (Math.Cos(Cur_Lat_radian) * Math.Sin(radian_distance)));       
            if (radian_bearing is double.NaN)
            {
                radian_bearing = 0.0;
            }
            // acos의 인수로 주어지는 x는 360분법의 각도가 아닌 radian(호도)값이다.       
            double true_bearing = 0;
            if (Math.Sin(Dest_Lon_radian - Cur_Lon_radian) < 0)
            {
                true_bearing = radian_bearing * (180.0 / Math.PI);
                true_bearing = 360.0 - true_bearing;
            }
            else
            {
                true_bearing = radian_bearing * (180.0 / Math.PI);
            }

            return /*(short)*/true_bearing;
        }
        #endregion

        #region boundary for triangle
        /// <summary>
        /// Find center point of inner circle with triangle.
        /// </summary>
        /// <param name="top"></param>
        /// <param name="leftBottom"></param>
        /// <param name="rightBottom"></param>
        /// <returns></returns>
        public static PointLatLng CalcCenterInnerCircle(PointLatLng top, PointLatLng leftBottom, PointLatLng rightBottom)
        {
            // top - right bottom
            //double slopeTopRight = CalDistance(top.Lat, top.Lng, rightBottom.Lat, rightBottom.Lng);
            double slopeTopRight = Math.Sqrt(Math.Pow(top.Lng - rightBottom.Lng, 2) + Math.Pow(top.Lat - rightBottom.Lat, 2));

            // top - left bottom
            //double slopeTopLeft = CalDistance(top.Lat, top.Lng, leftBottom.Lat, leftBottom.Lng);
            double slopeTopLeft = Math.Sqrt(Math.Pow(top.Lng - leftBottom.Lng, 2) + Math.Pow(top.Lat - leftBottom.Lat, 2));

            // left - right bottoms
            //double slopeLeftRight = CalDistance(leftBottom.Lat, leftBottom.Lng, rightBottom.Lat, rightBottom.Lng);
            double slopeLeftRight = Math.Sqrt(Math.Pow(leftBottom.Lng - rightBottom.Lng, 2) + Math.Pow(leftBottom.Lat - rightBottom.Lat, 2));

            //double x = (top.Lng * slopeLeftRight + leftBottom.Lng * slopeTopRight + rightBottom.Lng * slopeTopLeft) / (slopeTopLeft + slopeLeftRight + slopeTopRight);
            //double y = (top.Lat * slopeLeftRight + leftBottom.Lat * slopeTopRight + rightBottom.Lat * slopeTopLeft) / (slopeTopLeft + slopeLeftRight + slopeTopRight);

            var topVector = Vector<double>.Build.DenseOfArray(new double[] { top.Lng, top.Lat });
            var leftBottomVector = Vector<double>.Build.DenseOfArray(new double[] { leftBottom.Lng, leftBottom.Lat });
            var rightBottomVector = Vector<double>.Build.DenseOfArray(new double[] { rightBottom.Lng, rightBottom.Lat });

            topVector = topVector.Multiply(slopeLeftRight);
            leftBottomVector = leftBottomVector.Multiply(slopeTopRight);
            rightBottomVector = rightBottomVector.Multiply(slopeTopLeft);

            var center = topVector.Add(leftBottomVector);
            center = center.Add(rightBottomVector);
            center = center.Multiply(1.0 / (slopeTopRight + slopeTopLeft + slopeLeftRight));

            return new PointLatLng(center.At(1), center.At(0));
        }

        /// <summary>
        /// Calculate center 
        /// </summary>
        /// <param name="top"></param>
        /// <param name="leftBottom"></param>
        /// <param name="rightBottom"></param>
        /// <returns></returns>
        public static PointLatLng CalcCenterOuterCircle(PointLatLng top, PointLatLng leftBottom, PointLatLng rightBottom)
        {
            // Middle point of left and right side
            PointLatLng leftMid = new PointLatLng((top.Lat + leftBottom.Lat) / 2, (top.Lng + leftBottom.Lng) / 2);
            PointLatLng rightMid = new PointLatLng((top.Lat + rightBottom.Lat) / 2, (top.Lng + rightBottom.Lng) / 2);

            var matrixA = Matrix<double>.Build.DenseOfArray(new double[2, 2]
            {
                { (leftBottom.Lng - top.Lng), (leftBottom.Lat - top.Lat)},
                { (rightBottom.Lng - top.Lng), (rightBottom.Lat - top.Lat)}
            });

            var matrixB = Matrix<double>.Build.DenseOfArray(new double[2, 1]
            {
                { (Math.Pow(leftBottom.Lng, 2) - Math.Pow(top.Lng, 2) + Math.Pow(leftBottom.Lat, 2) - Math.Pow(top.Lat, 2)) },
                { (Math.Pow(rightBottom.Lng, 2) - Math.Pow(top.Lng, 2) + Math.Pow(rightBottom.Lat, 2) - Math.Pow(top.Lat, 2)) }
            });
            matrixB = matrixB.Multiply(0.5);

            var matrixResult = matrixA.PseudoInverse().Multiply(matrixB);

            return new PointLatLng(matrixResult.At(1, 0), matrixResult.At(0, 0));
        }

        /// <summary>
        /// Calculate new point from center
        /// leftBottom - top - rightBottom 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="top"></param>
        /// <param name="leftBottom"></param>
        /// <param name="rightBottom"></param>
        /// <param name="distance"></param>
        /// <returns></returns>
        private static (PointLatLng, double) CalcVertexWithCenter(PointLatLng center, PointLatLng top, PointLatLng leftBottom, PointLatLng rightBottom, double distance)
        {
            //double dis_IA = Math.Sqrt(Math.Pow(center.Lat - top.Lat, 2) + Math.Pow(center.Lng - top.Lng, 2));
            double real_dis_IA = CalcDistance(center, top);
            var vector_AC = Vector<double>.Build.DenseOfArray(new double[] { (rightBottom.Lng - top.Lng), (rightBottom.Lat - top.Lat) });
            var vector_AB = Vector<double>.Build.DenseOfArray(new double[] { (leftBottom.Lng - top.Lng), (leftBottom.Lat - top.Lat) });
            double dot_product1 = vector_AB.DotProduct(vector_AC);
            double dis_AC = Math.Sqrt(Math.Pow(vector_AC.At(0), 2) + Math.Pow(vector_AC.At(1), 2));
            double dis_AB = Math.Sqrt(Math.Pow(vector_AB.At(0), 2) + Math.Pow(vector_AB.At(1), 2));
            double angle_A = Math.Acos(dot_product1 / (dis_AB * dis_AC));// * 180.0 / Math.PI;
            //double innerRadius = Math.Sin(angle_A / 2) * dis_IA;
            double real_innerRadius = Math.Sin(angle_A / 2) * real_dis_IA;
            //double dis_ItoNewTop = dis_IA * (innerRadius + distance) / innerRadius;
            double real_dis_ItoNewTop = real_dis_IA * (real_innerRadius + distance) / real_innerRadius;
            //double bearing_IA = CalcBearing(I.Lat, A.Lat, I.Lng, A.Lng);// TODO;
            //double bearing_IA = CalcBearing2(I, A); // degree
            double bearing_IA = CalcBearing(center, top);
            return CalcDistanceAndBearing(center, real_dis_ItoNewTop, bearing_IA);
        }

        public static (PointLatLng, PointLatLng) CalcOuterRectangleWithInnerCircle(PointLatLng leftTop, PointLatLng rightBottom, double distance)
        {
            // top - A
            // left bottom - B
            // right bottom - C

            // 1. get vertexies.
            PointLatLng A = new PointLatLng(leftTop.Lat, (leftTop.Lng + rightBottom.Lng) / 2);
            PointLatLng B = new PointLatLng(rightBottom.Lat, leftTop.Lng);
            PointLatLng C = rightBottom;

            // 2. get center.
            PointLatLng I = CalcCenterInnerCircle(A, B, C);

            // Debug
            Debug.WriteLine($"Top : {A.Lat}, {A.Lng}");
            Debug.WriteLine($"Left bottom : {B.Lat}, {B.Lng}");
            Debug.WriteLine($"Right bottom : {C.Lat}, {C.Lng}");
            Debug.WriteLine($"inner center : {I.Lat}, {I.Lng}");


            var newTop = CalcVertexWithCenter(I, A, B, C, distance);
            var newRightBottom = CalcVertexWithCenter(I, C, A, B, distance);

            double left_lng = 2 * newTop.Item1.Lng - newRightBottom.Item1.Lng;
            PointLatLng newLeftTop = new PointLatLng(newTop.Item1.Lat, left_lng);

            // Debug.
            Debug.WriteLine($"New Top : {newTop.Item1.Lat}, {newTop.Item1.Lng}");
            Debug.WriteLine($"New Right Bottom : {newRightBottom.Item1.Lat}, {newRightBottom.Item1.Lng}");
            Debug.WriteLine($"New Left Bottom : {newLeftTop.Lat}, {newLeftTop.Lng}");
            return (newLeftTop, newRightBottom.Item1);
        }

        // TODO
        public static (PointLatLng, PointLatLng) CalcOuterRectangleWithInnerCircle2(PointLatLng leftTop, PointLatLng rightBottom, double distance)
        {
            // top - A
            // left bottom - B
            // right bottom - C

            // 1. get vertexies.
            PointLatLng A = new PointLatLng(leftTop.Lat, (leftTop.Lng + rightBottom.Lng) / 2);
            PointLatLng B = new PointLatLng(rightBottom.Lat, leftTop.Lng);
            PointLatLng C = rightBottom;

            // 2. get center.
            PointLatLng I = CalcCenterInnerCircle(A, B, C);

            // Debug
            Debug.WriteLine($"Top : {A.Lat}, {A.Lng}");
            Debug.WriteLine($"Left bottom : {B.Lat}, {B.Lng}");
            Debug.WriteLine($"Right bottom : {C.Lat}, {C.Lng}");
            Debug.WriteLine($"inner center : {I.Lat}, {I.Lng}");

            // 3. new top
            //double dis_IA = CalcDistance(I, A); // 중심과 Top의 길이.
            double dis_IA = Math.Sqrt(Math.Pow(I.Lat - A.Lat, 2) + Math.Pow(I.Lng - A.Lng, 2));
            double real_dis_IA = CalcDistance(I, A);
            var vector_AC = Vector<double>.Build.DenseOfArray(new double[] { (C.Lng - A.Lng), (C.Lat - A.Lat) });
            var vector_AB = Vector<double>.Build.DenseOfArray(new double[] { (B.Lng - A.Lng), (B.Lat - A.Lat) });
            double dot_product1 = vector_AB.DotProduct(vector_AC);
            double dis_AC = Math.Sqrt(Math.Pow(vector_AC.At(0), 2) + Math.Pow(vector_AC.At(1), 2));
            double dis_AB = Math.Sqrt(Math.Pow(vector_AB.At(0), 2) + Math.Pow(vector_AB.At(1), 2));
            double angle_A = Math.Acos(dot_product1 / (dis_AB * dis_AC));// * 180.0 / Math.PI;
            double innerRadius = Math.Sin(angle_A / 2) * dis_IA;
            double real_innerRadius = Math.Sin(angle_A / 2) * real_dis_IA;
            double dis_ItoNewTop = dis_IA * (innerRadius + distance) / innerRadius;
            double real_dis_ItoNewTop = real_dis_IA * (real_innerRadius + distance) / real_innerRadius;
            //double bearing_IA = CalcBearing(I.Lat, A.Lat, I.Lng, A.Lng);// TODO;
            //double bearing_IA = CalcBearing2(I, A); // degree
            double bearing_IA = CalcBearing(I, A);
            var newTop = CalcDistanceAndBearing(I, real_dis_ItoNewTop, bearing_IA);


            // 4. new right Bottom
            //double dis_IC = CalcDistance(I, C);
            double dis_IC = Math.Sqrt(Math.Pow(I.Lat - C.Lat, 2) + Math.Pow(I.Lng - C.Lng, 2));
            double real_dis_IC = CalcDistance(I, C);
            var vector_CA = Vector<double>.Build.DenseOfArray(new double[] { (A.Lng - C.Lng), (A.Lat - C.Lat) });
            var vector_CB = Vector<double>.Build.DenseOfArray(new double[] { (B.Lng - C.Lng), (B.Lat - C.Lat) });
            double dot_product2 = vector_CA.DotProduct(vector_CB);
            double dis_CA = Math.Sqrt(Math.Pow(vector_CA.At(0), 2) + Math.Pow(vector_CA.At(1), 2));
            double dis_CB = Math.Sqrt(Math.Pow(vector_CB.At(0), 2) + Math.Pow(vector_CB.At(1), 2));
            double angle_C = Math.Acos(dot_product2 / (dis_CA * dis_CB));// * 180.0 / Math.PI;
            double innerRaidus2 = Math.Sin(angle_C / 2) * dis_IC;// check
            double dis_ItoNewRightBottom = dis_IC * (innerRadius + distance) / innerRadius;
            double real_dis_ItoNewRightBottom = real_dis_IC * (real_innerRadius + distance) / real_innerRadius;
            double bearing_ttt = CalcBearing(I, C);

            // dot product
            var vector_IA = Vector<double>.Build.DenseOfArray(new double[]
            {
                (A.Lng - I.Lng), (A.Lat - I.Lat)
            });
            var vector_IC = Vector<double>.Build.DenseOfArray(new double[]
            {
                (C.Lng - I.Lng), (C.Lat - I.Lat)
            });
            double dot = vector_IA.DotProduct(vector_IC);
            double theta = Math.Acos(dot / (dis_IA * dis_IC)) * 180.0 / Math.PI;

            var newRightBottom = CalcDistanceAndBearing(I, real_dis_ItoNewRightBottom, bearing_ttt);

            double left_lng = 2 * newTop.Item1.Lng - newRightBottom.Item1.Lng;
            PointLatLng newLeftTop = new PointLatLng(newTop.Item1.Lat, left_lng);

            // Debug.
            Debug.WriteLine($"New Top : {newTop.Item1.Lat}, {newTop.Item1.Lng}");
            Debug.WriteLine($"New Right bottom : {newRightBottom.Item1.Lat}, {newRightBottom.Item1.Lng}");
            Debug.WriteLine($"New Left top : {newLeftTop.Lat}, {newLeftTop.Lng}");
            return (newLeftTop, newRightBottom.Item1);
        }

        // TODO
        public static (PointLatLng, PointLatLng) CalcOuterRectangle(PointLatLng leftTop, PointLatLng rightBottom, double distance)
        {
            // 1. find triangle points - top / leftbottom / rightbottom
            PointLatLng top = new PointLatLng(leftTop.Lat, (leftTop.Lng + rightBottom.Lng) / 2);
            PointLatLng leftBottom = new PointLatLng(rightBottom.Lat, leftTop.Lng);
            // PointLatLng rightBottom = rightBottom;

            // 2. calc center of outer circle and real radius.
            PointLatLng center = CalcCenterOuterCircle(top, leftBottom, rightBottom);
            double radius = CalcDistance(center, top);
            double raidus2 = CalcDistance(center, rightBottom);
            double radius3 = CalcDistance(center, leftBottom);

            // 3. middle point of each slopes
            PointLatLng leftMiddle = new PointLatLng((top.Lat + leftBottom.Lat) / 2, (top.Lng + leftBottom.Lng) / 2);
            PointLatLng rightMiddle = new PointLatLng((top.Lat + rightBottom.Lat) / 2, (top.Lng + rightBottom.Lng) / 2);

            // 4. distance center to middles.
            double dis_leftmiddle = CalcDistance(center, leftMiddle);
            double dis_rightmiddle = CalcDistance(center, rightMiddle);

            // 5. bearing center to top and to right bottom.
            double bearing_top = CalcBearing(center, top);
            double bearing_rightBottom = CalcBearing(center, rightBottom);

            // 6. calculate new top and right bottom
            double newRaidus = (dis_rightmiddle + distance) * radius / dis_rightmiddle;
            var newTopValue = CalcDistanceAndBearing(center, newRaidus, bearing_top);
            var newRightBottomValue = CalcDistanceAndBearing(center, newRaidus, bearing_rightBottom);

            // 7. to left top and right bottom
            PointLatLng newLeftTop = new PointLatLng(newTopValue.Item1.Lat, 2 * newTopValue.Item1.Lng - newRightBottomValue.Item1.Lng);
            PointLatLng newRightBottom = newRightBottomValue.Item1;

            Debug.WriteLine($"TOP            : {top.Lat}, {top.Lng}");
            Debug.WriteLine($"LEFT BOTTOM    : {leftBottom.Lat}, {leftBottom.Lng}");
            Debug.WriteLine($"RIGHT BOTTOM   : {rightBottom.Lat}, {rightBottom.Lng}");
            Debug.WriteLine($"CENTER         : {center.Lat}, {center.Lng}");
            Debug.WriteLine($"N LEFT TOP     : {newLeftTop.Lat}, {newLeftTop.Lng}");
            Debug.WriteLine($"N RIGHT BOTTOM : {newRightBottom.Lat}, {newRightBottom.Lng}");

            return (newLeftTop, newRightBottom);
        }
        
        #endregion

        #region boundary for polygon
        
        public static List<PointLatLng> CalcBoundaryPointsAsPolygon(List<PointLatLng> origin, double distance)
        {
            List<PointLatLng> list = new List<PointLatLng>();

            // weight lat and lng

            double weight_lat = origin.Average(point => point.Lat);
            double weight_lng = origin.Average(point => point.Lng);

            PointLatLng weight_point = new PointLatLng(weight_lat, weight_lng);
            Debug.WriteLine($"Weight : {weight_lat}, {weight_lng}");

            foreach(var point in origin)
            {
                double bearing = CalcBearing(weight_point, point);
                double real_distance = CalcDistance(weight_point, point);
                var result = CalcDistanceAndBearing(weight_point, real_distance + distance, bearing);
                list.Add(result.Item1);

                Debug.WriteLine($"Point1 : {point.Lat}, {point.Lng} to {result.Item1.Lat}, {result.Item1.Lng}, and bearing : {bearing}");

            }

            return list;
        }

        #endregion

        #region base
        /// <summary>
        /// 두 위경도 좌표 및 방위각을 통한 거리 계산
        /// </summary>
        /// <param name="dLat"></param>
        /// <param name="dLon"></param>
        /// <param name="dBrg"></param>
        /// <param name="dDist"></param>
        /// <param name="dDestLat"></param>
        /// <param name="dDestLon"></param>
        /// <param name="dABrg"></param>
        public static void CalcDistanceAndBearing(double dLat, double dLon, double dBrg, double dDist, out double dDestLat, out double dDestLon, out double dABrg)
        {
            double m_dMetres = 1.0 / 298.257223563;     // WGS84
            double m_dWGS84 = 6378137.0;                   // meters

            double dPiD4 = 0.0, dTwo_pi = 0.0, dCalcUnit = 0.0;
            double dTanU1 = 0.0, dU1 = 0.0, dSigma1 = 0.0, dSinalpha = 0.0, dCosalpha_sq = 0.0;
            double dU2 = 0.0, dCalcValueA = 0.0, dCalcValueB = 0.0, dCalcValueC = 0.0;
            double dSigma = 0.0, dLast_sigma = 0.0;
            double dTwo_sigma_m = 0.0, dDelta_sigma = 0.0;
            double dLambda = 0.0, dOmega = 0.0;

            dPiD4 = System.Math.Atan(1.0);
            dTwo_pi = dPiD4 * 8.0;
            dLat = dLat * dPiD4 / 45.0;
            dLon = dLon * dPiD4 / 45.0;
            dBrg = dBrg * dPiD4 / 45.0;
            if (dBrg < 0.0)
            {
                dBrg = dBrg + dTwo_pi;
            }
            if (dBrg > dTwo_pi)
            {
                dBrg = dBrg - dTwo_pi;
            }

            dCalcUnit = m_dWGS84 * (1.0 - m_dMetres);
            dTanU1 = (1 - m_dMetres) * System.Math.Tan(dLat);
            dU1 = System.Math.Atan(dTanU1);
            dSigma1 = System.Math.Atan2(dTanU1, System.Math.Cos(dBrg));
            dSinalpha = System.Math.Cos(dU1) * System.Math.Sin(dBrg);
            dCosalpha_sq = 1.0 - dSinalpha * dSinalpha;

            dU2 = dCosalpha_sq * (m_dWGS84 * m_dWGS84 - dCalcUnit * dCalcUnit) / (dCalcUnit * dCalcUnit);
            dCalcValueA = 1.0 + (dU2 / 16384) * (4096 + dU2 * (-768 + dU2 * (320 - 175 * dU2)));
            dCalcValueB = (dU2 / 1024) * (256 + dU2 * (-128 + dU2 * (74 - 47 * dU2)));

            // Starting with the approximation
            dSigma = (dDist / (dCalcUnit * dCalcValueA));
            dLast_sigma = 2.0 * dSigma + 2.0;  // something impossible

            // Iterate the following three equations 
            // until there is no significant change in sigma 
            dTwo_sigma_m = 0;
            while (System.Math.Abs((dLast_sigma - dSigma) / dSigma) > 1.0E-12)
            {
                dTwo_sigma_m = 2 * dSigma1 + dSigma;

                dDelta_sigma = dCalcValueB * System.Math.Sin(dSigma) * (System.Math.Cos(dTwo_sigma_m) + (dCalcValueB / 4) * (System.Math.Cos(dSigma) * (-1 + 2 * System.Math.Pow(System.Math.Cos(dTwo_sigma_m), 2)
                    - (dCalcValueB / 6) * System.Math.Cos(dTwo_sigma_m) * (-3 + 4 * System.Math.Pow(System.Math.Sin(dSigma), 2)) * (-3 + 4 * System.Math.Pow(System.Math.Cos(dTwo_sigma_m), 2)))));

                dLast_sigma = dSigma;
                dSigma = (dDist / (dCalcUnit * dCalcValueA)) + dDelta_sigma;
            }

            dDestLat = System.Math.Atan2((System.Math.Sin(dU1) * System.Math.Cos(dSigma) + System.Math.Cos(dU1) * System.Math.Sin(dSigma) * System.Math.Cos(dBrg)),
                ((1 - m_dMetres) * System.Math.Sqrt(System.Math.Pow(dSinalpha, 2) + System.Math.Pow(System.Math.Sin(dU1) * System.Math.Sin(dSigma) - System.Math.Cos(dU1) * System.Math.Cos(dSigma) * System.Math.Cos(dBrg), 2))));

            dLambda = System.Math.Atan2((System.Math.Sin(dSigma) * System.Math.Sin(dBrg)), (System.Math.Cos(dU1) * System.Math.Cos(dSigma) - System.Math.Sin(dU1) * System.Math.Sin(dSigma) * System.Math.Cos(dBrg)));

            dCalcValueC = (m_dMetres / 16) * dCosalpha_sq * (4 + m_dMetres * (4 - 3 * dCosalpha_sq));

            dOmega = dLambda - (1 - dCalcValueC) * m_dMetres * dSinalpha * (dSigma + dCalcValueC * System.Math.Sin(dSigma) * (System.Math.Cos(dTwo_sigma_m) + dCalcValueC * System.Math.Cos(dSigma) * (-1 + 2 * System.Math.Pow(System.Math.Cos(dTwo_sigma_m), 2))));

            dDestLon = dLon + dOmega;
            dABrg = System.Math.Atan2(dSinalpha, (-System.Math.Sin(dU1) * System.Math.Sin(dSigma) + System.Math.Cos(dU1) * System.Math.Cos(dSigma) * System.Math.Cos(dBrg)));

            dABrg = dABrg + dTwo_pi / 2.0;
            if (dABrg < 0.0)
            {
                dABrg = dABrg + dTwo_pi;
            }

            if (dABrg > dTwo_pi)
            {
                dABrg = dABrg - dTwo_pi;
            }

            dDestLat = dDestLat * 45.0 / dPiD4;
            dDestLon = dDestLon * 45.0 / dPiD4;
            dABrg = dABrg * 45.0 / dPiD4;
        }

        /// <summary>
        /// 두 위경도 좌표간 거리 계산 - harvesine formula
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lon1"></param>
        /// <param name="lat2"></param>
        /// <param name="lon2"></param>
        /// <returns></returns>
        public static double CalcDistance(double lat1, double lon1, double lat2, double lon2)
        {

            double theta, dist;
            theta = lon1 - lon2;
            dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1))
                  * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
            dist = Math.Acos(dist);
            dist = rad2deg(dist);

            dist = dist * 60 * 1.1515;
            dist = dist * 1.609344;    // 단위 mile 에서 km 변환.  
            dist = dist * 1000.0;      // 단위  km 에서 m 로 변환  

            return dist;
        }

        /// <summary>
        /// 주어진 도(degree) 값을 라디언으로 변환   
        /// </summary>
        /// <param name="deg"></param>
        /// <returns></returns>
        private static double deg2rad(double deg)
        {
            return (double)(deg * Math.PI / (double)180d);
        }

        /// <summary>
        /// 주어진 라디언(radian) 값을 도(degree) 값으로 변환   
        /// </summary>
        /// <param name="rad"></param>
        /// <returns></returns>
        private static double rad2deg(double rad)
        {
            return (double)(rad * (double)180d / Math.PI);
        }

        /// <summary>
        /// 방위각 계산 from U_VHF - Error
        /// </summary>
        /// <param name="lat1"></param>
        /// <param name="lat2"></param>
        /// <param name="lon1"></param>
        /// <param name="lon2"></param>
        /// <returns></returns>
        public static double CalcBearing(double lat1, double lat2, double lon1, double lon2)
        {
            double y = Math.Sin(lat2 - lat1) * Math.Cos(lon2);
            double x = (Math.Cos(lon1) * Math.Sin(lon2)) - (Math.Sin(lon1) * Math.Cos(lon2) * Math.Cos(lat2 - lat1));
            double theta = Math.Atan2(y, x);
            double bearing = (theta * 180 / Math.PI + 360) % 360;
            return bearing;
        }

        #endregion
    }
}