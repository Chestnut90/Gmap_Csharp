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
    class CGMapEllipse : CGMapMarker
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="tag"></param>
        /// <param name="zIndex"></param>
        public CGMapEllipse(PointLatLng pos, string tag, int zIndex) : base(pos, tag, zIndex)
        {
            this.MarkerType = MarkerType.ELLIPSE;
            this.PointLatLngs = new ObservableCollection<PointLatLng>() { pos, pos };// GMapControl에서 shape null 방지.
            this.PointLatLngs.CollectionChanged += PointLatLngs_CollectionChanged;
        }

        /// <summary>
        /// collection changed event for boundary points.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PointLatLngs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            int count = this.PointLatLngs.Count;

            int selectedIndex = 0;
            if (!(e == null))
            {
                selectedIndex = e.NewStartingIndex;
            }

            if (!this.IsAlarm)
            {
                return;
            }

            if (selectedIndex == 2 | selectedIndex == 3)
            {
                return;
            }

            var boundaryPoints = this.CalcBoundaryPointLatLngs(this.AlarmDistance);

            if (count <= 2)
            {
                this.PointLatLngs.Add(boundaryPoints.Item1);
                this.PointLatLngs.Add(boundaryPoints.Item2);
            }
            else
            {
                this.PointLatLngs[2] = boundaryPoints.Item1;
                this.PointLatLngs[3] = boundaryPoints.Item2;
            }
        }

        public void SetNextPoint(PointLatLng point)
        {
            this.PointLatLngs[1] = point;
        }

        /// <summary>
        /// Drawing function from Ellipse.
        /// </summary>
        /// <param name="localPath"></param>
        /// <param name="addBlurEffect"></param>
        /// <returns></returns>
        public override Path CreatePath(List<Point> localPath, bool addBlurEffect)
        {
            Path shape = (this.Shape as Path);

            Point center = new Point((localPath[0].X + localPath[1].X) / 2, (localPath[0].Y + localPath[1].Y) / 2);

            if (this.IsAlarm)
            {
                if (localPath.Count == 4)
                {
                    double width = Math.Abs(localPath[2].X - center.X);
                    double height = Math.Abs(localPath[3].Y - center.Y);
                    DrawEllipse((this.AlarmGeometry as EllipseGeometry), center, width, height);

                }
            }
            else
            {
                DrawEllipse((this.AlarmGeometry as EllipseGeometry), default(Point));
            }

            double radiusX = Math.Abs(localPath[0].X - localPath[1].X) / 2;
            double radiusY = Math.Abs(localPath[0].Y - localPath[1].Y) / 2;
            DrawEllipse((this.OriginGeometry as EllipseGeometry), center, radiusX, radiusY);

            return shape;
        }

        /// <summary>
        /// draw ellipse with geometry
        /// </summary>
        /// <param name="ellipseGeometry"></param>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <param name="isInit"></param>
        private void DrawEllipse(EllipseGeometry ellipseGeometry, Point center, double radiusX = 0.0, double radiusY = 0.0)
        {

            if (center.Equals(default(Point)))
            {
                ellipseGeometry.Center = default(Point);
                ellipseGeometry.RadiusX = 0;
                ellipseGeometry.RadiusY = 0;
                return;
            }

            ellipseGeometry.Center = center;
            ellipseGeometry.RadiusX = radiusX;
            ellipseGeometry.RadiusY = radiusY;

            // TODO : Drawing.
            //GeometryDrawing drawing = new GeometryDrawing();
            //drawing.Geometry = (this.OriginGeometry as EllipseGeometry);
            //drawing.Brush = Brushes.Red;
        }

        protected override void Alarm_Click(object sender, RoutedEventArgs e)
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
                this.PointLatLngs_CollectionChanged(null, null);
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

        /// <summary>
        /// Add two PointLatLng as Right of middle and Up of middle by ordered.
        /// </summary>
        /// <param name="addDistance"></param>
        private (PointLatLng, PointLatLng) CalcBoundaryPointLatLngs(double addDistance)
        {
            PointLatLng point1 = this.PointLatLngs[0];
            PointLatLng point2 = this.PointLatLngs[1];

            // 1. calc 4 vertexies 
            PointLatLng center = new PointLatLng((point1.Lat + point2.Lat) / 2, (point1.Lng + point2.Lng) / 2);
            PointLatLng midUp = new PointLatLng(point1.Lat, (point1.Lng + point2.Lng) / 2);
            PointLatLng midRight = new PointLatLng((point1.Lat + point2.Lat) / 2, point2.Lng);

            // 2. real radius x and y as meter.
            double realRadiusX = CalDistance(center.Lat, center.Lng, midRight.Lat, midRight.Lng);
            double realRadiusY = CalDistance(center.Lat, center.Lng, midUp.Lat, midUp.Lng);

            // 3. calculate up and right coordinates with each radius added distance.
            var rightValue = this.GetLatLonBear(center, realRadiusX + addDistance, 90.0);
            var upValue = this.GetLatLonBear(center, realRadiusY + addDistance, 0.0);
            return (rightValue.Item1, upValue.Item1);
            this.Points.Add(rightValue.Item1);
            this.Points.Add(upValue.Item1);
        }

        /// <summary>
        /// default setting.
        /// </summary>
        /// <returns></returns>
        protected override UIElement SetShape()
        {
            this.Shape = new Path();
            this.GeometryGroup = new GeometryGroup();
            this.OriginGeometry = new EllipseGeometry();
            this.AlarmGeometry = new EllipseGeometry();
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
        ///  TODO:
        /// </summary>
        /// <param name="point"></param>
        /// <param name="distance"></param>
        /// <param name="bearing"></param>
        /// <returns></returns>
        private (PointLatLng, double) GetLatLonBear(PointLatLng point, double distance, double bearing)
        {
            double newLatitute;
            double newLongitude;
            double newBearing;

            this.GetCalcPoint(point.Lat, point.Lng, bearing, distance, out newLatitute, out newLongitude, out newBearing);

            return (new PointLatLng(newLatitute, newLongitude), newBearing);
        }
    }
}
