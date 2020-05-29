using DevExpress.Mvvm;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using vo.Views;

namespace vo.Gmap
{
    class CGMapMarker : GMapMarker, IShapable
    {
        #region To calculate real distance with lat, lon

        //public double HaversineDistance(PointLatLng pos1, PointLatLng pos2, DistanceUnit unit)
        //{
        //    double R = (unit == DistanceUnit.Miles) ? 3960 : 6371;
        //    var lat = (pos2.Lat - pos1.Lat).ToRadians();
        //    var lng = (pos2.Lng - pos1.Lng).ToRadians();
        //    var h1 = Math.Sin(lat / 2) * Math.Sin(lat / 2) +
        //                  Math.Cos(pos1.Lat.ToRadians()) * Math.Cos(pos2.Lat.ToRadians()) *
        //                  Math.Sin(lng / 2) * Math.Sin(lng / 2);
        //    var h2 = 2 * Math.Asin(Math.Min(1, Math.Sqrt(h1)));
        //    return R * h2;
        //}

        public (PointLatLng, double) CalcPointLatLngBearing(PointLatLng point, double angle, double distance)
        {
            double latitude;
            double longitude;
            double bearing;
            this.GetCalcPoint(point.Lat, point.Lng, angle, distance, out latitude, out longitude, out bearing);
            return (new PointLatLng(latitude, longitude), bearing);
        }

        public void GetCalcPoint(double dLat, double dLon, double dBrg, double dDist, out double dDestLat, out double dDestLon, out double dABrg)
        {
            double m_dMetres = 1.0 / 298.257223563;		// WGS84
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

        public double GetBearing(double lat1, double lat2, double lon1, double lon2)
        {
            double y = Math.Sin(lat2 - lat1) * Math.Cos(lon2);
            double x = (Math.Cos(lon1) * Math.Sin(lon2)) - (Math.Sin(lon1) * Math.Cos(lon2) * Math.Cos(lat2 - lat1));
            double theta = Math.Atan2(y, x);
            double bearing = (theta * 180 / Math.PI + 360) % 360;
            return bearing;
        }

        //public (double, double) GetLatLon(double lat, double lon, )

        public enum DistanceUnit { Miles, Kilometers };

        public double CalDistance(double lat1, double lon1, double lat2, double lon2)
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
        private double deg2rad(double deg)
        {
            return (double)(deg * Math.PI / (double)180d);
        }

        // 주어진 라디언(radian) 값을 도(degree) 값으로 변환  
        private double rad2deg(double rad)
        {
            return (double)(rad * (double)180d / Math.PI);
        }
        #endregion

        public CGMapMarker(PointLatLng pos, string tag, int zIndex) : base(pos)
        {
            this.Tag = tag;
            this.ZIndex = ZIndex;
            this.Shape = this.SetShape();   // virtual and overrided 
            this.SetToolTip(this.Shape);
            this.SetContextMenu(this.Shape);
            this.IsAlarm = false;
        }

        protected GeometryGroup GeometryGroup { get; set; }
        protected Geometry OriginGeometry { get; set; }
        protected Geometry AlarmGeometry { get; set; }

        /// <summary>
        /// Type of Marker
        /// </summary>
        protected MarkerType MarkerType { get; set; }

        /// <summary>
        /// 경고 구역 설정
        /// </summary>
        protected bool IsAlarm { get; set; }

        /// <summary>
        /// 경고 구역 설정 거리
        /// </summary>
        protected double AlarmDistance { get; set; }

        protected ObservableCollection<PointLatLng> PointLatLngs { get; set; }

        /// <summary>
        /// Define Shape of Derived class.
        /// </summary>
        /// <returns></returns>
        protected virtual UIElement SetShape()
        {
            this.Shape = new Path();
            this.GeometryGroup = new GeometryGroup();
            this.OriginGeometry = new PathGeometry();
            this.AlarmGeometry = new PathGeometry();
            this.GeometryGroup.Children.Add(OriginGeometry);
            this.GeometryGroup.Children.Add(AlarmGeometry);
            this.GeometryGroup.FillRule = FillRule.Nonzero;

            (this.Shape as Path).Data = GeometryGroup;
            (this.Shape as Path).Stroke = Brushes.Red;
            (this.Shape as Path).StrokeThickness = 1.5;
            (this.Shape as Path).Fill = Brushes.AliceBlue;
            return this.Shape;
        }

        /// <summary>
        /// Context menu
        /// </summary>
        /// <param name="element"></param>
        protected void SetContextMenu(UIElement element)
        {
            ContextMenu menu = new ContextMenu();

            menu.Items.Add(this.Modify());
            menu.Items.Add(this.Delete());
            menu.Items.Add(this.Alarm());

            ContextMenuService.SetContextMenu(element, menu);
        }

        #region MenuItems on Context Menu

        /// <summary>
        /// MenuItem of context menu -> modify
        /// </summary>
        /// <returns></returns>
        protected MenuItem Modify()
        {
            MenuItem modify = new MenuItem();
            modify.Header = "수정";
            modify.Click += Modify_Click;
            return modify;
        }

        /// <summary>
        /// MenuItem of context menu -> delete
        /// </summary>
        /// <returns></returns>
        protected MenuItem Delete()
        {
            MenuItem delete = new MenuItem();
            delete.Header = "삭제";
            delete.Click += Delete_Click;
            return delete;
        }

        /// <summary>
        /// MenuItem of context menu -> alarm
        /// </summary>
        /// <returns></returns>
        protected MenuItem Alarm()
        {
            MenuItem alarm = new MenuItem();
            alarm.Header = "경고구역 설정";
            alarm.Click += Alarm_Click;
            return alarm;
        }

        #endregion

        #region MenuItem events

        /// <summary>
        /// Modify button event -> send message to MapControl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Modify_Click(object sender, RoutedEventArgs e)
        {
            // modifying
            GMapMessage<GMapMarker> message = new GMapMessage<GMapMarker>();
            message.Sender = Convert.ToString(this.Tag);
            message.Receiver = "GMapControl";
            message.Action = Action.MODIFICATION;
            message.MarkerType = this.MarkerType;
            message.Data = this;

            Messenger.Default.Send(message);
        }

        /// <summary>
        /// Delete menuItem event -> send message to MapControl.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Delete_Click(object sender, RoutedEventArgs e)
        {
            GMapMessage<GMapMarker> message = new GMapMessage<GMapMarker>();
            message.Sender = Convert.ToString(this.Tag);
            message.Receiver = "GMapControl";
            message.Action = Action.DELETE;
            message.Data = this;

            Messenger.Default.Send(message);
        }

        // TODO : virutal.
        /// <summary>
        /// Alarm menuItem event -> set alarm boundary to shape.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Alarm_Click(object sender, RoutedEventArgs e)
        {
            MenuItem alarmMenu = (sender as MenuItem);

            if (!this.IsAlarm)
            {
                InputTextBox itb = new InputTextBox();
                switch (itb.ShowDialog())
                {
                    case true:
                        string[] value = itb.GetInputObjects();
                        if (value.Length != 1)
                        {
                            return;
                        }
                        this.AlarmDistance = Convert.ToDouble(value[0]);
                        break;
                    default:
                        return;
                }
            }

            if (!this.IsAlarm)
            {
                alarmMenu.Header = "경고구역 해제";
                this.IsAlarm = true;
            }
            else
            {
                alarmMenu.Header = "경고구역 설정";
                this.IsAlarm = false;
            }

            GMapMessage<GMapMarker> message = new GMapMessage<GMapMarker>();
            message.Sender = Convert.ToString(this.Tag);
            message.Receiver = "GMapControl";
            message.Action = Action.ALARM;
            message.Data = this;
            message.MarkerType = this.MarkerType;

            Messenger.Default.Send(message);
        }

        #endregion

        #region Tooltip on Shape.

        /// <summary>
        ///  tooltip of this shape.
        /// </summary>
        /// <param name="element"></param>
        protected void SetToolTip(UIElement element)
        {
            TextBlock tooltipbox = new TextBlock();
            tooltipbox.Text = Convert.ToString(this.Tag); // Replace to ID.
            tooltipbox.Foreground = Brushes.Black;
            tooltipbox.Background = Brushes.White;
            tooltipbox.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            tooltipbox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            ToolTipService.SetToolTip(element, tooltipbox);
        }

        #endregion

        #region Implementation Ishapable 
        public List<PointLatLng> Points
        {
            get => this.PointLatLngs.ToList();
            set { throw new NotImplementedException("Use PointLatlngs property instead this. this property used for only get."); }
        }

        public virtual Path CreatePath(List<Point> localPath, bool addBlurEffect)
        {
            throw new NotImplementedException();
        }
        #endregion

        // TODO : below

        /// <summary>
        /// calculate center point using input two points.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        protected Point CalcCenterPoint(Point point1, Point point2)
        {
            return new Point((point1.X + point2.X) / 2, (point2.Y + point2.Y) / 2);
        }


    }
}
