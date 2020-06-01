using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vo.Gmap
{
    public enum Action
    {
        MODIFICATION,
        DELETE,
        ALARM
    }

    public enum MarkerType
    {
        ELLIPSE,
        TRIANGLE,
        RECTANGE,
        POLYGON
    }

    public class GMapMessage<T>
    {
        public GMapMessage() { }

        public GMapMessage(string sender, string receiver, Action action, T data, MarkerType markerType)
        {
            this.Sender = sender;
            this.Receiver = receiver;
            this.Action = action;
            this.Data = data;
            this.MarkerType = MarkerType;
        }

        public Action Action { get; set; }
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public T Data { get; set; }
        public MarkerType MarkerType { get; set; }
    }
}
