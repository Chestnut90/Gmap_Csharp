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
using vo.Gmap.Common;
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
        }

        /// <summary>
        /// collection changed event for boundary points.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void PointLatLngs_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
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

            if (this.IsAlarm)
            {
                if (localPath.Count == 4)
                {
                    DrawEllipse((this.AlarmGeometry as EllipseGeometry), localPath[2], localPath[3]);

                }
            }
            else
            {
                DrawEllipse((this.AlarmGeometry as EllipseGeometry), default(Point), default(Point));
            }

            DrawEllipse((this.OriginGeometry as EllipseGeometry), localPath[0], localPath[1]);

            return shape;
        }

        /// <summary>
        /// Draw Ellipse
        /// </summary>
        /// <param name="ellipse"></param>
        /// <param name="leftTop"></param>
        /// <param name="rightBottom"></param>
        private void DrawEllipse(EllipseGeometry ellipse, Point leftTop, Point rightBottom)
        {
            if (leftTop.Equals(default(Point)) & rightBottom.Equals(default(Point)))
            {
                ellipse.Center = default(Point);
                ellipse.RadiusX = 0;
                ellipse.RadiusY = 0;
                return;
            }

            Point center = new Point((leftTop.X + rightBottom.X) / 2, (leftTop.Y + rightBottom.Y) / 2);
            double radiusX = Math.Abs(leftTop.X - center.X);
            double radiusY = Math.Abs(rightBottom.Y - center.Y);

            ellipse.Center = center;
            ellipse.RadiusX = radiusX;
            ellipse.RadiusY = radiusY;
        }

        /// <summary>
        /// Add two PointLatLng as Right of middle and Up of middle by ordered.
        /// </summary>
        /// <param name="addDistance"></param>
        private (PointLatLng, PointLatLng) CalcBoundaryPointLatLngs(double distance)
        {
            var points = this.PointSwap(this.PointLatLngs[0], this.PointLatLngs[1]);

            PointLatLng point1 = points.Item1;
            PointLatLng point2 = points.Item2;

            // new left top
            var leftTopUpValue = LatLngCommon.CalcDistanceAndBearing(point1, distance, 0.0);      // 1. origin left top to upside distance
            var leftTopLeftValue = LatLngCommon.CalcDistanceAndBearing(point1, distance, 270.0);  // 2. origin left top to left side distance.
            PointLatLng newLeftTop = new PointLatLng(leftTopUpValue.Item1.Lat, leftTopLeftValue.Item1.Lng);

            // new right bottom
            var rightBottomDownValue = LatLngCommon.CalcDistanceAndBearing(point2, distance, 180.0);      // 1. origin right bottom to downside distance
            var rightBottomRightValue = LatLngCommon.CalcDistanceAndBearing(point2, distance, 90.0);      // 2. origin right bottom to right side distance.
            PointLatLng newRightBottom = new PointLatLng(rightBottomDownValue.Item1.Lat, rightBottomRightValue.Item1.Lng);

            return (newLeftTop, newRightBottom);
        }

        // TODO
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
            (this.Shape as Path).Fill = this.tiling();
            return this.Shape;
        }
    }
}
