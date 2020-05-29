using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vo
{
    public class TMessage<ViewModel>
    {
        public TMessage(string sender, string receiver, string action, ViewModel data)
        {
            this.Sender = sender;
            this.Receiver = Receiver;
            this.Action = action;
            this.Data = data;
        }

        public TMessage()
        {

        }

        public string Sender { get; set; }
        public string Receiver { get; set; }
        public string Action { get; set; }
        public ViewModel Data { get; set; }

    }
}
