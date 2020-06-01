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
using vo.Gmap.Common;
using vo.Views;

namespace vo.Gmap
{
    class CGMapMarker : GMapMarker, IShapable
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

        /// <summary>
        /// need to override
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected virtual void PointLatLngs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        protected GeometryGroup GeometryGroup { get; set; }
        protected Geometry OriginGeometry { get; set; }
        protected Geometry AlarmGeometry { get; set; }

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

        protected ObservableCollection<PointLatLng> PointLatLngs { get; set; }

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
            (this.Shape as Path).Fill = Brushes.AliceBlue;
            return this.Shape;
        }

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
                this.PointLatLngs_CollectionChanged(null, null);
            }
            else
            {
                alarmMenu.Header = "경고구역 설정";
                this.IsAlarm = false;
            }

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
        public List<PointLatLng> Points
        {
            get => this.PointLatLngs.ToList();
            set { throw new NotImplementedException("Use PointLatlngs property instead this. this property used for only get."); }
        }

        public virtual Path CreatePath(List<Point> localPath, bool addBlurEffect)
        {
            throw new NotImplementedException();
        }
        #endregion

        // TODO : below

        /// <summary>
        /// calculate center point using input two points.
        /// </summary>
        /// <param name="point1"></param>
        /// <param name="point2"></param>
        /// <returns></returns>
        protected Point CalcCenterPoint(Point point1, Point point2)
        {
            return new Point((point1.X + point2.X) / 2, (point2.Y + point2.Y) / 2);
        }


    }
}
