using GMap.NET;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace vo.Gmap
{
    class CGMapPolygon : CGMapMarker
    {
        public CGMapPolygon(PointLatLng pos, string tag, int zIndex) : base(pos, tag, zIndex)
        {
            this.MarkerType = Common.MarkerType.POLYGON;
        }

        public void SetNextPoint(PointLatLng point)
        {
            this.PointLatLngs.Add(point);
        }

        /// <summary>
        /// Override
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void PointLatLngs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

        }

        public override Path CreatePath(List<Point> localPath, bool addBlurEffect)
        {
            Path shape = (this.Shape as Path);

            if (this.IsAlarm)
            {

            }
            else
            {
                // init
            }

            DrawPolygon((this.OriginGeometry as PathGeometry), localPath);

            return shape;
        }

        private void DrawPolygon(PathGeometry geometry, List<Point> points)
        {
            PathFigureCollection figures = geometry.Figures;
            PathFigure polygonFigure = new PathFigure();
            polygonFigure.StartPoint = points[0];
            PolyLineSegment seg = new PolyLineSegment();
            seg.Points = new PointCollection(points);
            polygonFigure.IsClosed = true;
            polygonFigure.IsFilled = true;

            polygonFigure.Segments.Add(seg);

            if(figures.Count < 1)
            {
                figures.Add(polygonFigure);
            }
            else
            {
                figures[0] = polygonFigure;
            }

        }

        /// <summary>
        /// Define Shape of Derived class.
        /// </summary>
        /// <returns></returns>
        protected override UIElement SetShape()
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

    }
}
