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

        /// <summary>
        /// Draw Rectangle
        /// </summary>
        /// <param name="rectangleGeometry"></param>
        /// <param name="leftTop"></param>
        /// <param name="rightBottom"></param>
        private void DrawRectangle(RectangleGeometry rectangleGeometry, Point leftTop, Point rightBottom)
        {
            //GeometryDrawing gd = new GeometryDrawing();
            //gd.Brush = this.tiling();
            //gd.Geometry = rectangleGeometry;

            //DrawingGroup
            rectangleGeometry.Rect = new Rect(leftTop, rightBottom);
        }

        private DrawingBrush tiling()
        {

            //
            // Create a Drawing. This will be the DrawingBrushes' content.
            //
            PolyLineSegment polyLineSegmetn = new PolyLineSegment();
            polyLineSegmetn.Points.Add(new Point(0, 0));
            polyLineSegmetn.Points.Add(new Point(5, 5));
            polyLineSegmetn.Points.Add(new Point(5, 0));
            polyLineSegmetn.Points.Add(new Point(0, 5));

            PathFigure triangleFigure = new PathFigure();
            triangleFigure.IsClosed = false;
            triangleFigure.StartPoint = new Point(0, 0);
            triangleFigure.Segments.Add(polyLineSegmetn);

            PathGeometry triangleGeometry = new PathGeometry();
            triangleGeometry.Figures.Add(triangleFigure);

            GeometryDrawing triangleDrawing = new GeometryDrawing();
            triangleDrawing.Geometry = triangleGeometry;
            //triangleDrawing.Brush = new SolidColorBrush(Color.FromArgb(255, 204, 204, 255));

            Pen trianglePen = new Pen(Brushes.Black, 0.5);
            triangleDrawing.Pen = trianglePen;
            trianglePen.MiterLimit = 0;
            //triangleDrawing.Freeze();

            //
            // Create another TileBrush, this time with tiling.
            //
            DrawingBrush tileBrushWithTiling = new DrawingBrush();
            tileBrushWithTiling.Drawing = triangleDrawing;
            tileBrushWithTiling.TileMode = TileMode.Tile;

            // Specify the brush's Viewport. Otherwise,
            // a single tile will be produced that fills
            // the entire output area and its TileMode will
            // have no effect.
            // This setting uses realtive values to
            // creates four tiles.
            tileBrushWithTiling.Viewport = new Rect(0, 0, 5, 5);
            tileBrushWithTiling.ViewportUnits = BrushMappingMode.Absolute;

            return tileBrushWithTiling;
            //tilingExampleRectangle.Fill = tileBrushWithTiling;
            
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
            PointLatLng point1 = this.PointLatLngs[0];
            PointLatLng point2 = this.PointLatLngs[1];

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
    }
}
