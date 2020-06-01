using GMap.NET;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vo.Gmap.Common
{
    class LatLngCommon
    {
        /// <summary>
        /// Calculate center 
        /// </summary>
        /// <param name="top"></param>
        /// <param name="leftBottom"></param>
        /// <param name="rightBottom"></param>
        /// <returns></returns>
        public static PointLatLng CalcCenterCircle(PointLatLng top, PointLatLng leftBottom, PointLatLng rightBottom)
        {

            var matrixA = Matrix<double>.Build.DenseOfArray(new double[2, 2]
            {
                { (leftBottom.Lng - top.Lng), (leftBottom.Lat - top.Lat)},
                { (rightBottom.Lng - leftBottom.Lng), (rightBottom.Lat - leftBottom.Lat)}
            });

            var matrixB = Matrix<double>.Build.DenseOfArray(new double[2, 1]
            {
                { (Math.Pow(leftBottom.Lng, 2) - Math.Pow(top.Lng, 2) + Math.Pow(leftBottom.Lat, 2) - Math.Pow(top.Lat, 2)) },
                { (Math.Pow(rightBottom.Lng, 2) - Math.Pow(leftBottom.Lng, 2) + Math.Pow(rightBottom.Lat, 2) - Math.Pow(leftBottom.Lat, 2)) }
            });
            matrixB = matrixB.Multiply(0.5);

            var matrixResult = matrixA.PseudoInverse().Multiply(matrixB);

            return new PointLatLng(matrixResult.At(1, 0), matrixResult.At(0, 0));
        }

<<<<<<< HEAD
        /// <summary>
        /// calculate distance center to point
        /// </summary>
        /// <param name="center"></param>
        /// <param name="from"></param>
        /// <returns></returns>
        public static double CalcRealDistanceFromCenter(PointLatLng center, PointLatLng point)
=======
        public static (PointLatLng, PointLatLng) CalcFind(PointLatLng leftTop, PointLatLng rightBottom, double distance)
        {

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
            double distance_center_top = Math.Sqrt(Math.Pow(I.Lat - A.Lat, 2) + Math.Pow(I.Lng - A.Lng, 2));

            // right slope point.
            var matrixA = Matrix<double>.Build.DenseOfArray(new double[2, 2]
            {
                {(A.Lng - I.Lng), (A.Lat - I.Lat) },
                {(C.Lng - I.Lng), (C.Lat - I.Lat) }
            });

            var matrixB = Matrix<double>.Build.DenseOfArray(new double[2, 1]
            {
                {((A.Lng - I.Lng)*I.Lng + (A.Lat - I.Lat)*I.Lat) },
                {((C.Lng - I.Lng)*I.Lng + (C.Lat - I.Lat)*I.Lat) }
            });

            var resultMatrix = matrixA.PseudoInverse().Multiply(matrixB);
            PointLatLng slopePoint = new PointLatLng(resultMatrix.At(1, 0), resultMatrix.At(0, 0));

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
            double bearing_IA = CalcBearing(A.Lat, I.Lat, A.Lng, I.Lng);
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
            double bearing_IC = CalcBearing(C.Lat, I.Lat, C.Lng, I.Lng);
            var newRightBottom = CalcDistanceAndBearing(I, real_dis_ItoNewRightBottom, bearing_IC);

            double left_lng = 2 * newTop.Item1.Lng - newRightBottom.Item1.Lng;
            PointLatLng newLeftTop = new PointLatLng(newTop.Item1.Lat, left_lng);

            // Debug.
            Debug.WriteLine($"New Top : {newTop.Item1.Lat}, {newTop.Item1.Lng}");
            Debug.WriteLine($"New Right bottom : {newRightBottom.Item1.Lat}, {newRightBottom.Item1.Lng}");
            Debug.WriteLine($"New Left top : {newLeftTop.Lat}, {newLeftTop.Lng}");
            return (newLeftTop, newRightBottom.Item1);
        }
        
        public static (PointLatLng, PointLatLng) CalcOuterRectangle(PointLatLng leftTop, PointLatLng rightBottom, double distance)
>>>>>>> dev
        {
            return CalDistance(center.Lat, center.Lng, point.Lat, point.Lng);
        }

        /// <summary>
        /// calculate angle Arc Cosine ( center - point1 ) / ( center - point2)
        /// </summary>
        /// <param name="center"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        public static double CalcAngleFromCenter(PointLatLng center, PointLatLng point1, PointLatLng point2)
        {
            double x = CalcRealDistanceFromCenter(center, point1);
            double slope = CalcRealDistanceFromCenter(center, point2);

            return Math.Acos(x / slope);
        }

        public static (PointLatLng, double) CalcPointLatLngBearing(PointLatLng point, double angle, double distance)
        {
            double latitude;
            double longitude;
            double bearing;
            GetCalcPoint(point.Lat, point.Lng, angle, distance, out latitude, out longitude, out bearing);
            return (new PointLatLng(latitude, longitude), bearing);
        }

        public static void GetCalcPoint(double dLat, double dLon, double dBrg, double dDist, out double dDestLat, out double dDestLon, out double dABrg)
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

        public static (PointLatLng, PointLatLng) CalcTwo(PointLatLng leftTop, PointLatLng rightBottom, double distance)
        {
            // 1. get vertexies.
            PointLatLng top = new PointLatLng(leftTop.Lat, (leftTop.Lng + rightBottom.Lng) / 2);
            PointLatLng leftBottom = new PointLatLng(rightBottom.Lat, leftTop.Lng);

            // 2. get center.
            PointLatLng center = CalcCenterCircle(top, leftBottom, rightBottom);

            // 3. calc middle points and distances.
            PointLatLng leftMid = new PointLatLng((top.Lat + leftBottom.Lat) / 2, (top.Lng + leftBottom.Lng) / 2);
            PointLatLng rightMid = new PointLatLng((top.Lat + rightBottom.Lat) / 2, (top.Lng + rightBottom.Lng) / 2);

            double leftDistanceCenter = CalcRealDistanceFromCenter(center, leftMid);
            double rightDistanceCenter = CalcRealDistanceFromCenter(center, rightMid);

            // 4. distance : center to top
            double disCenterToTop = CalcRealDistanceFromCenter(center, top);
            double disCenterToRightBottom = CalcRealDistanceFromCenter(center, rightBottom);

            // 5. distance new Radius and 
            double newRadius = (leftDistanceCenter + distance) * disCenterToTop / leftDistanceCenter;
            //double r = (rightDistanceCenter + distance) * disCenterToRightBottom / rightDistanceCenter; // same with radius

            // 6. larger top and right bottom
            double leftAngle = Math.Acos(rightDistanceCenter / disCenterToTop) * 180.0 / Math.PI;
            var largerTopValue = CalcPointLatLngBearing(center, 0.0, newRadius);
            var largerBottomValue = CalcPointLatLngBearing(center, leftAngle * 2, newRadius);

            PointLatLng largerTop = largerTopValue.Item1;
            PointLatLng largerRightBottom = largerBottomValue.Item1;
            PointLatLng largerLeftTop = new PointLatLng(largerTop.Lat, 2 * largerTop.Lng - largerRightBottom.Lng);

            return (largerLeftTop, largerRightBottom);
        }

        #region base
        private static double CalDistance(double lat1, double lon1, double lat2, double lon2)
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

        // 주어진 도(degree) 값을 라디언으로 변환  
        private static double deg2rad(double deg)
        {
            return (double)(deg * Math.PI / (double)180d);
        }

        // 주어진 라디언(radian) 값을 도(degree) 값으로 변환  
        private static double rad2deg(double rad)
        {
            return (double)(rad * (double)180d / Math.PI);
        }
        #endregion
    }
}
