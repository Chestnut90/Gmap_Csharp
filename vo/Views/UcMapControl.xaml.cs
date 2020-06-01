using DevExpress.Mvvm;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using vo.Gmap;

namespace vo.Views
{
    public enum DrawState
    {
        None,
        Ellipse,
        Triangle,
        Rectangle,
        Polygon
    }

    /// <summary>
    /// UcMapControl.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcMapControl : UserControl
    {
        public UcMapControl()
        {
            InitializeComponent();
            drawState = DrawState.None;
            Messenger.Default.Register<GMapMessage<GMapMarker>>(this, GMapMessage_Receiver);
        }

        private void NoJapanSea()
        {
            PointLatLng ptSeaJapanPos = new PointLatLng();
            ptSeaJapanPos.Lat = 39.8;
            ptSeaJapanPos.Lng = 134.3;

            List<PointLatLng> points = new List<PointLatLng>();
            double lat = 0.15, lng = 0.4;
            points.Add(new PointLatLng(ptSeaJapanPos.Lat - lat, ptSeaJapanPos.Lng - lng));
            points.Add(new PointLatLng(ptSeaJapanPos.Lat - lat, ptSeaJapanPos.Lng + lng));
            points.Add(new PointLatLng(ptSeaJapanPos.Lat + lat, ptSeaJapanPos.Lng + lng));
            points.Add(new PointLatLng(ptSeaJapanPos.Lat + lat, ptSeaJapanPos.Lng - lng));

            GMapPolygon polygon = new GMapPolygon(points);
            gMapControl.Markers.Add(polygon);
            (polygon.Shape as Path).Fill = new SolidColorBrush(Color.FromArgb(255, 75, 98, 165));
            (polygon.Shape as Path).Opacity = 1;
            (polygon.Shape as Path).Stroke = Brushes.Transparent;
            polygon.Tag = "NoJapanSea";

            Label text = new Label();
            text.Content = "East Sea";
            text.VerticalContentAlignment = VerticalAlignment.Center;
            text.HorizontalContentAlignment = HorizontalAlignment.Center;
            text.Width = 200;
            text.Height = 80;
            GMapMarker marker = new GMapMarker(ptSeaJapanPos);
            marker.Tag = "NoJapanSea";
            marker.Shape = text;
            marker.Offset = new Point(-text.Width / 2, -text.Height / 2);
            text.Foreground = Brushes.White;
            //text.Background = Brushes.Transparent;
            gMapControl.Markers.Add(marker);
        }

        private void GMapControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.NoJapanSea();
            gMapControl.EmptyMapBackground = Brushes.Navy;
            gMapControl.MouseWheelZoomEnabled = true;
            gMapControl.MouseWheelZoomType = MouseWheelZoomType.MousePositionWithoutCenter;
            gMapControl.DragButton = MouseButton.Left;
            //this.MousePopUp =  gMapControl.ContextMenu;
            gMapControl.MapProvider = GMapProviders.GoogleHybridMap;  //Map Data Provider Set
            //gMapControl.WebProxy = null;

            GMaps.Instance.Mode = AccessMode.ServerAndCache;
            string strPath = AppDomain.CurrentDomain.BaseDirectory;
            gMapControl.CacheLocation = strPath;
            //GMaps.Instance.Mode = AccessMode.ServerOnly; // TODO :

            gMapControl.MaxZoom = 19;                   //Limit Maximum Zoom Value
            gMapControl.MinZoom = 7;                    //Limit Minimum Zoom Value
            gMapControl.Zoom = 7;                       //Default Zoom Value

            // 지도 초기 위치
            gMapControl.Position = new PointLatLng(36.4158195890877, 127.320395708084);  //초기위치 국과연 운동장

            // 지도 중앙 false
            gMapControl.ShowCenter = false;
            gMapControl.IgnoreMarkerOnMouseWheel = true;
        }

        private DrawState drawState;        // Draw State.
        private bool isDrawing = false;     // if true then drawing state.
        private Point start;                // start point of drawing object.
        private Point end;                  // end point of drawing obejct.
        private GMapMarker drawingObject;   // Object to drawing. // default must be null.
        private int zIndex;                 // altitude of drawing object.
        private string tag;                 // tag -> id for drawing object.
        private void InvokeResizing()
        {
            // for invoking resize event.
            this.gMapControl.InitializeForBackgroundRendering((int)this.gMapControl.ActualWidth, (int)this.gMapControl.ActualHeight);
            //this.gMapControl.RenderSize = this.gMapControl.RenderSize;
        }

        /// <summary>
        /// Context menu item click
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Draw_Clicks(object sender, RoutedEventArgs e)
        {
            InputTextBox itb = new InputTextBox();

            bool? result = itb.ShowDialog();

            switch (result)
            {
                case true:

                    string[] output = itb.GetInputObjects();
                    if (output.Length != 2)
                    {
                        return;
                    }
                    tag = Convert.ToString(output[0]);
                    zIndex = Convert.ToInt32(output[1]);

                    break;
                default:
                    return;
            }

            var menuItem = (MenuItem)sender;
            switch (menuItem.Header)
            {
                case "타원":
                    this.drawState = DrawState.Ellipse;
                    break;
                case "삼각형":
                    this.drawState = DrawState.Triangle;
                    break;
                case "사각형":
                    this.drawState = DrawState.Rectangle;
                    break;
                case "다각형":
                    this.drawState = DrawState.Polygon;
                    break;
                default:
                    this.drawState = DrawState.None;
                    this.gMapControl.Cursor = Cursors.AppStarting;
                    return;
            }

            this.gMapControl.Cursor = Cursors.Hand;
            // map move -> to disable.

        }

        private void SetStartDrawing(DrawState drawState)
        {
            this.drawState = drawState;
            this.isDrawing = true;
        }

        private void SetEndDrawing()
        {
            this.drawingObject = null;
            this.drawState = DrawState.None;
            this.isDrawing = false;
        }

        private (int, int) GetLocalPoint(MouseButtonEventArgs e)
        {
            int x = (int)e.GetPosition(this).X;
            int y = (int)e.GetPosition(this).Y;

            return (x, y);
        }

        private void GMapControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (this.drawState.Equals(DrawState.None))
            {
                return;
            }

            if (isDrawing)
            {
                var localPoint = e.GetPosition(this);
                PointLatLng pointLatLng = this.gMapControl.FromLocalToLatLng((int)localPoint.X, (int)localPoint.Y);
                double width = Math.Abs(localPoint.X - start.X);
                double height = Math.Abs(localPoint.Y - start.Y);

                switch (drawState)
                {
                    case DrawState.Ellipse:

                        (drawingObject as CGMapEllipse).SetNextPoint(pointLatLng);
                        break;
                    case DrawState.Rectangle:
                        (drawingObject as CGMapRectangle).SetNextPoint(pointLatLng);
                        break;
                    case DrawState.Polygon:
                        // None.
                        break;
                    case DrawState.Triangle:
                        (drawingObject as CGMapTriangle).SetNextPoint(pointLatLng);
                        break;
                    default:
                        return;
                }

                this.InvokeResizing();
            }

        }

        private void GMapControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var localPoint = GetLocalPoint(e);
            PointLatLng pointLatLng = this.gMapControl.FromLocalToLatLng(localPoint.Item1, localPoint.Item2);

            switch (this.drawState)
            {
                case DrawState.Ellipse:

                    if (!this.isDrawing)
                    {
                        this.start = new Point(localPoint.Item1, localPoint.Item2);
                        drawingObject = new CGMapEllipse(pointLatLng, this.tag, this.zIndex);
                        this.isDrawing = true;
                        this.gMapControl.Markers.Add(drawingObject);
                        return;
                    }
                    (drawingObject as CGMapEllipse).Points[1] = pointLatLng;

                    break;
                case DrawState.Rectangle:

                    if (!this.isDrawing)
                    {
                        this.start = new Point(localPoint.Item1, localPoint.Item2);
                        drawingObject = new CGMapRectangle(pointLatLng, this.tag, this.zIndex);
                        this.isDrawing = true;
                        this.gMapControl.Markers.Add(drawingObject);
                        return;
                    }
                    //(drawingObject as CGMapRectange).Point2 = pointLatLng;

                    break;
                case DrawState.Polygon:
                    if (!this.isDrawing)
                    {
                        this.start = new Point(localPoint.Item1, localPoint.Item2);
                        drawingObject = new CGMapPolygon(pointLatLng, this.tag, this.zIndex);
                        this.isDrawing = true;
                        this.gMapControl.Markers.Add(drawingObject);
                        return;
                    }
                    //(drawingObject as CGMapPolygon).Point2 = pointLatLng;
                    //(drawingObject as CGMapPolygon).SetShape(pointLatLng);
                    break;
                case DrawState.Triangle:
                    if (!this.isDrawing)
                    {
                        this.start = new Point(localPoint.Item1, localPoint.Item2);
                        drawingObject = new CGMapTriangle(pointLatLng, this.tag, this.zIndex);
                        this.isDrawing = true;
                        this.gMapControl.Markers.Add(drawingObject);
                        return;
                    }
                    //(drawingObject as CGMapTriangle).Point2 = pointLatLng;
                    break;
                case DrawState.None:
                    return;
                default:
                    return;
            }
        }

        private void GMapControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.drawState.Equals(DrawState.None))
            {
                this.SetEndDrawing();
            }
        }

        #region Message Operation
        private void GMapMessage_Receiver(GMapMessage<GMapMarker> param)
        {
            if (!param.Receiver.Equals("GMapControl"))
            {
                return;
            }

            switch (param.Action)
            {
                case Gmap.Action.MODIFICATION:
                    this.GMapMarker_Modification(param.Data, param.MarkerType);
                    break;
                case Gmap.Action.DELETE:
                    // TODO : delete marker.
                    break;
                case Gmap.Action.ALARM:

                    this.GMapMarker_AlarmSetting(param.Data, param.MarkerType);

                    break;
                default:
                    return;
            }
        }

        private void GMapMarker_AlarmSetting(GMapMarker marker, MarkerType markerType)
        {
            switch (markerType)
            {
                case MarkerType.ELLIPSE:
                    this.drawingObject = (marker as CGMapEllipse);
                    break;
                case MarkerType.RECTANGE:
                    this.drawingObject = (marker as CGMapRectangle);
                    break;
                case MarkerType.TRIANGLE:
                    this.drawingObject = (marker as CGMapTriangle);
                    break;
                default:
                    return;
            }


            this.InvokeResizing();
            this.drawingObject = null;
        }

        /// <summary>
        /// Modification message handling.
        /// </summary>
        /// <param name="marker"></param>
        /// <param name="markerType"></param>
        private void GMapMarker_Modification(GMapMarker marker, MarkerType markerType)
        {
            switch (markerType)
            {
                case MarkerType.ELLIPSE:
                    this.drawingObject = ((CGMapEllipse)marker);
                    this.SetStartDrawing(DrawState.Ellipse);
                    break;
                case MarkerType.TRIANGLE:
                    this.drawingObject = ((CGMapTriangle)marker);
                    this.SetStartDrawing(DrawState.Triangle);
                    break;
                case MarkerType.RECTANGE:
                    this.drawingObject = ((CGMapRectangle)marker);
                    this.SetStartDrawing(DrawState.Rectangle);
                    break;
                case MarkerType.POLYGON:
                    this.drawingObject = ((CGMapPolygon)marker);
                    this.SetStartDrawing(DrawState.Polygon);
                    break;
                default:
                    return;
            }
        }


        #endregion

        private void MenuDefenseSystem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuDrone_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuAlaramArea_Click(object sender, RoutedEventArgs e)
        {

        }

        private void MenuJamming_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
