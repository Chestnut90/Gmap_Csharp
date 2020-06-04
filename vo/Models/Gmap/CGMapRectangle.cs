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
using vo.Models.Gmap.Common;
using vo.Views;

namespace vo.Models.Gmap
{
    class CGMapRectangle : CGMapMarker
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="tag"></param>
        /// <param name="zIndex"></param>
        public CGMapRectangle(PointLatLng pos, string tag, int zIndex) : base(pos, tag, zIndex)
        {
            this.MarkerType = MarkerType.RECTANGE;
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
        /// Drawing function from IShapable
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

        /// <summary>
        /// Draw Rectangle
        /// </summary>
        /// <param name="rectangleGeometry"></param>
        /// <param name="leftTop"></param>
        /// <param name="rightBottom"></param>
        private void DrawRectangle(RectangleGeometry rectangleGeometry, Point leftTop, Point rightBottom)
        {
            rectangleGeometry.Rect = new Rect(leftTop, rightBottom);

            //DrawingGroup
            GeometryDrawing drawing = new GeometryDrawing();
            drawing.Geometry = rectangleGeometry;

            Pen pen;
            if (rectangleGeometry.Equals(this.AlarmGeometry))
            {
                pen = new Pen(Brushes.Yellow, 10);
            }
            else
            {
                pen = new Pen(Brushes.Green, 10);
            }

            drawing.Pen = pen;

        }

        private void Test()
        {
            //
            // Create the Geometry to draw.
            //
            GeometryGroup ellipses = new GeometryGroup();
            ellipses.Children.Add(
                new EllipseGeometry(new Point(50, 50), 45, 20)
                );
            ellipses.Children.Add(
                new EllipseGeometry(new Point(50, 50), 20, 45)
                );

            //
            // Create a GeometryDrawing.
            //
            GeometryDrawing aGeometryDrawing = new GeometryDrawing();
            aGeometryDrawing.Geometry = ellipses;

            // Paint the drawing with a gradient.
            aGeometryDrawing.Brush =
                new LinearGradientBrush(
                    Colors.Blue,
                    Color.FromRgb(204, 204, 255),
                    new Point(0, 0),
                    new Point(1, 1));

            // Outline the drawing with a solid color.
            aGeometryDrawing.Pen = new Pen(Brushes.Black, 10);

            //
            // Use a DrawingImage and an Image control
            // to display the drawing.
            //
            DrawingImage geometryImage = new DrawingImage(aGeometryDrawing);

            // Freeze the DrawingImage for performance benefits.
            geometryImage.Freeze();

            Image anImage = new Image();
            anImage.Source = geometryImage;
            anImage.Stretch = Stretch.None;
            anImage.HorizontalAlignment = HorizontalAlignment.Left;

            //
            // Place the image inside a border and
            // add it to the page.
            //
            Border exampleBorder = new Border();
            exampleBorder.Child = anImage;
            exampleBorder.BorderBrush = Brushes.Gray;
            exampleBorder.BorderThickness = new Thickness(1);
            exampleBorder.HorizontalAlignment = HorizontalAlignment.Left;
            exampleBorder.VerticalAlignment = VerticalAlignment.Top;
            exampleBorder.Margin = new Thickness(10);

            //this.Margin = new Thickness(20);
            //this.Background = Brushes.White;
            //this.Content = exampleBorder;
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
            //(this.Shape as Path).Fill = Brushes.AliceBlue;
            (this.Shape as Path).Fill = this.tiling();
            return this.Shape;
        }

        // TODO : Point swap
        /// <summary>
        ///  Calculate boundary point left top and right bottom
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        private (PointLatLng, PointLatLng) CalcBoundaryPointLatLngs(double distance)
        {
            var points = this.Swap(this.PointLatLngs[0], this.PointLatLngs[1]);
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

        private (PointLatLng, PointLatLng) Swap(PointLatLng p1, PointLatLng p2)
        {
            // 1Quadrant (1사분면)
            if (p1.Lng < p2.Lng & p1.Lat < p2.Lat)
            {
                PointLatLng lt = new PointLatLng(p2.Lat, p1.Lng);
                PointLatLng rb = new PointLatLng(p1.Lat, p2.Lng);
                return (lt, rb);
            }
            else if (p1.Lat > p2.Lat & p1.Lng < p2.Lng)
            {
                PointLatLng lt = new PointLatLng(p1.Lat, p2.Lng);
                PointLatLng rb = new PointLatLng(p2.Lat, p1.Lng);
                return (lt, rb);
            }
            return (p1, p2);
        }
    }
}
