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

namespace vo.ProcessDocking
{
    /// <summary>
    /// DockingTargetWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DockingTargetWindow : Window
    {
        public DockingTargetWindow()
        {
            InitializeComponent();
            this.Building();
        }

        string sFilePath = @"D:\Dev\02.AntiDrone\02. src\RFScaner\RFScaner\bin\x64\Debug\RFScaner.exe";
        string notepad = @"C:\Windows\System32\notepad.exe";

        private void Building()
        {

            this.viewer1.Content = new UcProcess(notepad);


        }
    }
}
