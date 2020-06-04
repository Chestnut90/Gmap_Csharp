using DevExpress.Mvvm;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
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
    class CGMapTriangle : CGMapMarker
    {
        /// <summary>
        /// Constuctor
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="tag"></param>
        /// <param name="zIndex"></param>
        public CGMapTriangle(PointLatLng pos, string tag, int zIndex) : base(pos, tag, zIndex)
        {
            this.MarkerType = MarkerType.TRIANGLE;
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
        
        /// <summary>
        /// Add two PointLatLng as left top and right bottom by ordered.
        /// </summary>
        /// <param name="addDistance"></param>
        private (PointLatLng, PointLatLng) CalcBoundaryPointLatLngs(double addDistance)
        {
            PointLatLng point1 = this.PointLatLngs[0];
            PointLatLng point2 = this.PointLatLngs[1];
            return LatLngCommon.CalcOuterRectangleWithInnerCircle(point1, point2, addDistance);
        }

        public override Path CreatePath(List<Point> localPath, bool addBlurEffect)
        {
            Path shape = (this.Shape as Path);

            Point center = new Point((localPath[0].X + localPath[1].X) / 2, (localPath[0].Y + localPath[1].Y) / 2);

            if (this.IsAlarm)
            {
                if (localPath.Count == 4)
                {
                    DrawTriangle((this.AlarmGeometry as PathGeometry), localPath[2], localPath[3]);
                }
            }
            else
            {
                DrawTriangle((this.AlarmGeometry as PathGeometry), default(Point), default(Point));
            }

            DrawTriangle((this.OriginGeometry as PathGeometry), localPath[0], localPath[1]);
            return shape;
        }

        /// <summary>
        /// Draw Triangle
        /// </summary>
        /// <param name="pathGeometry"></param>
        /// <param name="leftTop"></param>
        /// <param name="rightBottom"></param>
        private void DrawTriangle(PathGeometry pathGeometry, Point leftTop, Point rightBottom)
        {
            Point top = new Point((leftTop.X + rightBottom.X) / 2, leftTop.Y);
            Point leftBottom = new Point(leftTop.X, rightBottom.Y);

            PathFigureCollection figures = pathGeometry.Figures;
            PathFigure triagnelFigure = new PathFigure();
            triagnelFigure.StartPoint = top;
            triagnelFigure.Segments = new PathSegmentCollection()
            { new LineSegment(leftBottom, true), new LineSegment(rightBottom, true) };
            triagnelFigure.IsClosed = true;
            triagnelFigure.IsFilled = true;

            if (figures.Count < 1)
            {
                figures.Add(triagnelFigure);
            }
            else
            {
                figures[0] = triagnelFigure;
            }
        }

        public void SetNextPoint(PointLatLng point)
        {
            this.PointLatLngs[1] = point;
        }

        protected override UIElement SetShape()
        {
            this.Shape = new Path();
            this.GeometryGroup = new GeometryGroup();
            this.OriginGeometry = new PathGeometry();
            this.AlarmGeometry = new PathGeometry();
            this.GeometryGroup.Children.Add(OriginGeometry);
            this.GeometryGroup.Children.Add(AlarmGeometry);
            this.GeometryGroup.FillRule = FillRule.Nonzero;

            (this.Shape as Path).Data = this.GeometryGroup;
            (this.Shape as Path).Stroke = Brushes.Red;
            (this.Shape as Path).StrokeThickness = 1.5;
            (this.Shape as Path).Fill = this.tiling();
            return this.Shape;
        }

    }
}
