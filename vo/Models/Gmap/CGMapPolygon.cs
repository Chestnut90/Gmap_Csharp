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
using vo.Models.Gmap.Common;

namespace vo.Models.Gmap
{
    class CGMapPolygon : CGMapMarker
    {
        public CGMapPolygon(PointLatLng pos, string tag, int zIndex) : base(pos, tag, zIndex)
        {
            this.MarkerType = Common.MarkerType.POLYGON;
        }

        public override void SetNextPoint(PointLatLng point)
        {
            this.PointLatLngs.Add(point);
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

        protected override List<PointLatLng> CalcBoundaryPoints(List<PointLatLng> points, double distance)
        {
            return LatLngCommon.CalcBoundaryPointsAsPolygon(points, distance);
        }
    }
}
