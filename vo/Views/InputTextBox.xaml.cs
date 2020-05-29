using DevExpress.Mvvm;
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
using System.Windows.Shapes;

namespace vo.Views
{
    /// <summary>
    /// InputTextBox.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class InputTextBox : Window
    {
        public InputTextBox()
        {
            InitializeComponent();
            this.SetMessenger();
        }


        private void SetMessenger()
        {
        }

        public string[] GetInputObjects()
        {
            string text = this.TextInput.Text.Trim();

            string[] data = text.Split(' ');

            return data;
        }

        private void ButtonOk_Click(object sender, RoutedEventArgs e)
        {
            TMessage<object> message = new TMessage<object>();
            message.Sender = "InputTextBox";
            message.Receiver = "DataBaseTest";
            message.Data = this.GetInputObjects();

            Messenger.Default.Send(message);
            this.DialogResult = true;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
