using GMap.NET;
using System;
using System.Collections.Generic;
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
            this.MarkerType = MarkerType.POLYGON;
            this.LatLngPoints = new List<PointLatLng>();
            this.LatLngPoints.Add(pos);
        }

        private List<PointLatLng> LatLngPoints { get; set; }

        protected PointCollection GetPointsFromLatLngCollection()
        {
            PointCollection points = new PointCollection();
            //List<Point> localPoints = new List<Point>();

            GPoint offset = this.Map.FromLatLngToLocal(this.Position);
            foreach (var i in this.LatLngPoints)
            {
                var p = this.Map.FromLatLngToLocal(i);
                points.Add(new Point(p.X - offset.X, p.Y - offset.Y));
            }

            return points;
        }

        protected override void Alarm_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected override UIElement SetShape()
        {
            this.Shape = new Polygon();
            (this.Shape as Polygon).Stroke = Brushes.Red;
            (this.Shape as Polygon).StrokeThickness = 1.5;
            (this.Shape as Polygon).Fill = Brushes.AliceBlue;
            return this.Shape;
        }

    }
}
