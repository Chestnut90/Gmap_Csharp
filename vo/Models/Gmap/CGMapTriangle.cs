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
       
        protected override List<PointLatLng> CalcBoundaryPoints(List<PointLatLng> points, double distance)
        {
            var result = LatLngCommon.CalcOuterRectangleWithInnerCircle(points[0], points[1], distance);
            return new List<PointLatLng>() { result.Item1, result.Item2 };
        }
    }
}
