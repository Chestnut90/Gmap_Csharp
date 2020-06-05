using DevExpress.Mvvm;
using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public abstract class CGMapMarker : GMapMarker, IShapable
    {
        public CGMapMarker(PointLatLng pos, string tag, int zIndex) : base(pos)
        {
            this.Tag = tag;
            this.ZIndex = ZIndex;
            this.Shape = this.SetShape();   // virtual and overrided 
            this.SetToolTip(this.Shape);
            this.SetContextMenu(this.Shape);
            this.IsAlarm = false;

            this.PointLatLngs = new ObservableCollection<PointLatLng>() { pos, pos };// GMapControl에서 shape null 방지.
            this.PointLatLngs.CollectionChanged += PointLatLngs_CollectionChanged;

        }

        #region Geometry

        protected GeometryGroup GeometryGroup { get; set; }
        protected Geometry OriginGeometry { get; set; }
        protected Geometry AlarmGeometry { get; set; }

        /// <summary>
        /// Define Shape of Derived class.
        /// </summary>
        /// <returns></returns>
        protected virtual UIElement SetShape()
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
            (this.Shape as Path).Fill = this.Tiling();
            return this.Shape;
        }

        #endregion

        protected ObservableCollection<PointLatLng> PointLatLngs { get; set; }

        public virtual void SetNextPoint(PointLatLng point)
        {
            this.PointLatLngs[1] = point;
        }

        /// <summary>
        /// need to override
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void PointLatLngs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (!this.IsAlarm)
            {
                this.BoundaryPoints = new List<PointLatLng>();
                return;
            }

            // set boundaryPoints
            this.BoundaryPoints = this.CalcBoundaryPoints(this.PointLatLngs.ToList(), this.AlarmDistance); 
        }

        /// <summary>
        /// Type of Marker
        /// </summary>
        protected MarkerType MarkerType { get; set; }

        /// <summary>
        /// 경고 구역 설정
        /// </summary>
        protected bool IsAlarm { get; set; }

        /// <summary>
        /// 경고 구역 설정 거리
        /// </summary>
        protected double AlarmDistance { get; set; }

        protected List<PointLatLng> BoundaryPoints { get; set; }

        protected abstract List<PointLatLng> CalcBoundaryPoints(List<PointLatLng> points, double distance);

        /// <summary>
        /// Context menu
        /// </summary>
        /// <param name="element"></param>
        protected void SetContextMenu(UIElement element)
        {
            ContextMenu menu = new ContextMenu();

            menu.Items.Add(this.Modify());
            menu.Items.Add(this.Delete());
            menu.Items.Add(this.Alarm());

            ContextMenuService.SetContextMenu(element, menu);
        }

        #region MenuItems on Context Menu

        /// <summary>
        /// MenuItem of context menu -> modify
        /// </summary>
        /// <returns></returns>
        protected MenuItem Modify()
        {
            MenuItem modify = new MenuItem();
            modify.Header = "수정";
            modify.Click += Modify_Click;
            return modify;
        }

        /// <summary>
        /// MenuItem of context menu -> delete
        /// </summary>
        /// <returns></returns>
        protected MenuItem Delete()
        {
            MenuItem delete = new MenuItem();
            delete.Header = "삭제";
            delete.Click += Delete_Click;
            return delete;
        }

        /// <summary>
        /// MenuItem of context menu -> alarm
        /// </summary>
        /// <returns></returns>
        protected MenuItem Alarm()
        {
            MenuItem alarm = new MenuItem();
            alarm.Header = "경고구역 설정";
            alarm.Click += Alarm_Click;
            return alarm;
        }

        #endregion

        #region MenuItem events

        /// <summary>
        /// Modify button event -> send message to MapControl
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Modify_Click(object sender, RoutedEventArgs e)
        {
            // modifying
            GMapMessage<GMapMarker> message = new GMapMessage<GMapMarker>();
            message.Sender = Convert.ToString(this.Tag);
            message.Receiver = "GMapControl";
            message.Action = Common.Action.MODIFICATION;
            message.MarkerType = this.MarkerType;
            message.Data = this;

            Messenger.Default.Send(message);
        }

        /// <summary>
        /// Delete menuItem event -> send message to MapControl.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Delete_Click(object sender, RoutedEventArgs e)
        {
            GMapMessage<GMapMarker> message = new GMapMessage<GMapMarker>();
            message.Sender = Convert.ToString(this.Tag);
            message.Receiver = "GMapControl";
            message.Action = Common.Action.DELETE;
            message.Data = this;

            Messenger.Default.Send(message);
        }

        /// <summary>
        /// Alarm menuItem event -> set alarm boundary to shape.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void Alarm_Click(object sender, RoutedEventArgs e)
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

                // invoke collection changed event to make boundary points and add points.
                //this.PointLatLngs_CollectionChanged(null, null);
            }
            else
            {
                alarmMenu.Header = "경고구역 설정";
                this.IsAlarm = false;
            }
            this.PointLatLngs_CollectionChanged(null, null);
            GMapMessage<GMapMarker> message = new GMapMessage<GMapMarker>();
            message.Sender = Convert.ToString(this.Tag);
            message.Receiver = "GMapControl";
            message.Action = Common.Action.ALARM;
            message.Data = this;
            message.MarkerType = this.MarkerType;

            Messenger.Default.Send(message);
        }

        #endregion

        #region Tooltip on Shape.

        /// <summary>
        ///  tooltip of this shape.
        /// </summary>
        /// <param name="element"></param>
        protected void SetToolTip(UIElement element)
        {
            TextBlock tooltipbox = new TextBlock();
            tooltipbox.Text = Convert.ToString(this.Tag); // Replace to ID.
            tooltipbox.Foreground = Brushes.Black;
            tooltipbox.Background = Brushes.White;
            tooltipbox.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            tooltipbox.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            ToolTipService.SetToolTip(element, tooltipbox);
        }

        #endregion

        #region Implementation Ishapable 
        public virtual List<PointLatLng> Points
        {
            get
            {
                if(this.BoundaryPoints is null)
                {
                    this.BoundaryPoints = new List<PointLatLng>();
                }

                IEnumerable<PointLatLng> t = this.PointLatLngs.Concat(this.BoundaryPoints);
                return t.ToList();
                //return new List<PointLatLng>(this.PointLatLngs).Concat(this.BoundaryPoints);
            }
            set { throw new NotImplementedException("Use PointLatlngs property instead this. this property used for only get."); }
        }

        public virtual Path CreatePath(List<Point> localPath, bool addBlurEffect)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Common Functions
        protected DrawingBrush Tiling()
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

        protected (PointLatLng, PointLatLng) PointSwap(PointLatLng point1, PointLatLng point2)
        {
            double lat1 = point1.Lat;
            double lat2 = point2.Lat;
            double lng1 = point1.Lng;
            double lng2 = point2.Lng;

            if (lat1 < lat2)
            {
                double temp = lat1;
                lat1 = lat2;
                lat2 = temp;
            }

            if (lng1 > lng2)
            {
                double temp = lng1;
                lng1 = lng2;
                lng2 = temp;
            }

            PointLatLng leftTop = new PointLatLng(lat1, lng1);
            PointLatLng rightBottom = new PointLatLng(lat2, lng2);
            return (leftTop, rightBottom);
        }
        #endregion

    }
}
