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
using vo.Gmap.Common;

namespace vo.Gmap
{
    class CGMapPolygon : CGMapMarker
    {
        public CGMapPolygon(PointLatLng pos, string tag, int zIndex) : base(pos, tag, zIndex)
        {
            this.MarkerType = Common.MarkerType.POLYGON;
            BoundaryPoints = new List<PointLatLng>();
        }

        private List<PointLatLng> BoundaryPoints { get; set; }

        public void SetNextPoint(PointLatLng point)
        {
            this.PointLatLngs.Add(point);
        }

        public override List<PointLatLng> Points
        {
            get
            {
                IEnumerable<PointLatLng> t = this.PointLatLngs.Concat(this.BoundaryPoints);
                return t.ToList();
                //return new List<PointLatLng>(this.PointLatLngs).Concat(this.BoundaryPoints);
            }
            set
            {
                base.Points = value;
            }
        }

        /// <summary>
        /// Override
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected override void PointLatLngs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            int count = this.PointLatLngs.Count;

            if (!this.IsAlarm)
            {
                this.BoundaryPoints = new List<PointLatLng>();
                return;
            }

            // set boundaryPoints
            this.BoundaryPoints = LatLngCommon.CalcBoundaryPointsAsPolygon(this.PointLatLngs.ToList(), this.AlarmDistance);
        }

        public override Path CreatePath(List<Point> localPath, bool addBlurEffect)
        {
            Path shape = (this.Shape as Path);

            if (this.IsAlarm)
            {
                // half of localPath list.
                int count = localPath.Count/2;
                int originLast = (localPath.Count - 1) / 2;

                DrawPolygon((this.OriginGeometry as PathGeometry), localPath.GetRange(0, count));
                DrawPolygon((this.AlarmGeometry as PathGeometry), localPath.GetRange(originLast + 1, count));
            }
            else
            {
                DrawPolygon((this.OriginGeometry as PathGeometry), localPath);
                DrawPolygon((this.AlarmGeometry as PathGeometry), null);    // Init
            }

            return shape;
        }



        private void DrawPolygon(PathGeometry geometry, List<Point> points)
        {
            if (points is null)
            {
                geometry.Figures.Clear();
                return;
            }
            if(points.Count == 0)
            {
                geometry.Figures.Clear();
                return;
            }

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
            (this.Shape as Path).Fill = this.tiling();
            return this.Shape;
        }

    }
}
