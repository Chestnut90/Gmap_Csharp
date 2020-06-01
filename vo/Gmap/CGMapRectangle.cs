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
    class CGMapRectangle : CGMapMarker
    {
        public CGMapRectangle(PointLatLng pos, string tag, int zIndex) : base(pos, tag, zIndex)
        {
            this.MarkerType = MarkerType.RECTANGE;
            this.PointLatLngs = new ObservableCollection<PointLatLng>() { pos, pos };// GMapControl에서 shape null 방지.
            this.PointLatLngs.CollectionChanged += PointLatLngs_CollectionChanged;
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

        public override Path CreatePath(List<Point> localPath, bool addBlurEffect)
        {
            Path shape = (this.Shape as Path);

            Point center = new Point((localPath[0].X + localPath[1].X) / 2, (localPath[0].Y + localPath[1].Y) / 2);

            if (this.IsAlarm)
            {
                if (localPath.Count == 4)
                {
                    DrawRectangle((this.AlarmGeometry as RectangleGeometry), localPath[2], localPath[3]);
                }
            }
            else
            {
                DrawRectangle((this.AlarmGeometry as RectangleGeometry), default(Point), default(Point));
            }

            DrawRectangle((this.OriginGeometry as RectangleGeometry), localPath[0], localPath[1]);
            return shape;
        }

        private void DrawRectangle(RectangleGeometry rectangleGeometry, Point leftTop, Point rightBottom)
        {
            rectangleGeometry.Rect = new Rect(leftTop, rightBottom);
        }

        protected override UIElement SetShape()
        {
            this.Shape = new Path();
            this.GeometryGroup = new GeometryGroup();
            this.OriginGeometry = new RectangleGeometry();
            this.AlarmGeometry = new RectangleGeometry();
            this.GeometryGroup.Children.Add(OriginGeometry);
            this.GeometryGroup.Children.Add(AlarmGeometry);
            this.GeometryGroup.FillRule = FillRule.Nonzero;

            (this.Shape as Path).Data = this.GeometryGroup;
            (this.Shape as Path).Stroke = Brushes.Red;
            (this.Shape as Path).StrokeThickness = 1.5;
            (this.Shape as Path).Fill = Brushes.AliceBlue;
            return this.Shape;
        }

        /// <summary>
        /// Add two PointLatLng as left top and right bottom by ordered.
        /// </summary>
        /// <param name="addDistance"></param>
        private (PointLatLng, PointLatLng) CalcBoundaryPointLatLngs(double addDistance)
        {
            // 1. find center 
            PointLatLng point1 = this.PointLatLngs[0];
            PointLatLng point2 = this.PointLatLngs[1];
            PointLatLng center = new PointLatLng((point1.Lat + point2.Lat) / 2, (point1.Lng + point2.Lng) / 2);

            // 2. find middle up and middle right side.
            PointLatLng midUp = new PointLatLng(point1.Lat, (point1.Lng + point2.Lng) / 2);
            PointLatLng midRight = new PointLatLng((point1.Lat + point2.Lat) / 2, point2.Lng);

            // 3. find angle with top
            double topDistanceX = this.CalDistance(center.Lat, center.Lng, midUp.Lat, midUp.Lng);
            double topDistanceY = this.CalDistance(point1.Lat, point1.Lng, midUp.Lat, midUp.Lng);
            double topAngle = Math.Atan2(topDistanceY, topDistanceX) * 180 / Math.PI;
            double topDistanceS = Math.Sqrt(Math.Pow(topDistanceY, 2) + Math.Pow(topDistanceX, 2));
            double leftTopDistance = topDistanceS * (addDistance + topDistanceY) / topDistanceY;

            double bottomDistanceX = this.CalDistance(center.Lat, center.Lng, midRight.Lat, midRight.Lng);
            double bottomDistanceY = this.CalDistance(point2.Lat, point2.Lng, midRight.Lat, midRight.Lng);
            double bottomAngle = Math.Atan2(bottomDistanceY, bottomDistanceX) * 180 / Math.PI;
            double bottomDistanceS = Math.Sqrt(Math.Pow(bottomDistanceX, 2) + Math.Pow(bottomDistanceY, 2));
            double rightBottomDistance = bottomDistanceS * (bottomDistanceX + addDistance) / bottomDistanceX;

            // 4. find boundary rectangle left top and right bottom.
            var leftTop = GetLatLonBear(center, leftTopDistance, 360 - topAngle);
            var rightBottom = GetLatLonBear(center, rightBottomDistance, 90 + bottomAngle);

            return (leftTop.Item1, rightBottom.Item1);
        }

        // TODO : below.
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
