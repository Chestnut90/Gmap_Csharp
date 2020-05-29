using console.DataBase.Schema;
using console.ViewModel;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace console
{
    class Program
    {

        public class PointLatLng
        {
            public PointLatLng(double lat, double lng)
            {
                this.Lat = lat;
                this.Lng = lng;
            }
            public double Lat;
            public double Lng;

            public override string ToString()
            {
                return $"{this.Lat}, {this.Lng}";
            }
        }

        static void Main(string[] args)
        {
            var matrix_a = Matrix<double>.Build.DenseOfArray(new double[2, 1] { { 1.0}, { 4.0 } });

            Debug.WriteLine(matrix_a.ToMatrixString());

            //PointLatLng top = new PointLatLng(10, 10);
            //PointLatLng lb = new PointLatLng(0, 0);
            //PointLatLng rb = new PointLatLng(0, 20);

            //var calc = Calc.CalcCenterCircle(top, lb, rb);
            //Debug.WriteLine(calc);

            PointLatLng lt = new PointLatLng(10, 0);
            PointLatLng rb = new PointLatLng(0, 20);
            Calc.CalcTwo(lt, rb, 10);
        }

        public class Calc
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
                Debug.WriteLine(matrixResult.ToMatrixString());

                return new PointLatLng(matrixResult.At(1, 0), matrixResult.At(0, 0));
            }

            /// <summary>
            /// calculate distance center to point
            /// </summary>
            /// <param name="center"></param>
            /// <param name="from"></param>
            /// <returns></returns>
            public static double CalcRealDistanceFromCenter(PointLatLng center, PointLatLng point)
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

            public static PointLatLng CalcTwo(PointLatLng leftTop, PointLatLng rightBottom, double distance)
            {
                // 1. get vertexies.
                PointLatLng top = new PointLatLng(leftTop.Lat, (leftTop.Lng + rightBottom.Lng) / 2);
                PointLatLng leftBottom = new PointLatLng(rightBottom.Lat, leftTop.Lng);

                // 2. get center.
                PointLatLng center = CalcCenterCircle(top, leftBottom, rightBottom);
                Debug.WriteLine(center);

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

                return null;
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
}
