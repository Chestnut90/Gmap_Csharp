using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using vo.Hwnds.Views;
using vo.Views;
using vo.Views.ProcessWindow;

namespace vo
{
    
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            this.GetWindow.Click += this.GetWindow_Click;
        }

        string sFilePath = @"D:\Dev\02.AntiDrone\02. src\RFScaner\RFScaner\bin\x64\Debug\RFScaner.exe";
        string notepad = @"C:\Windows\System32\notepad.exe";

        private void GetWindow_Click(object sender, RoutedEventArgs e)
        {
            //this.ViewBoxStack.Children.Add(new UcTest());

            //WindowBorder border = new WindowBorder(notepad);
            //border.RunProcess();
            //border.Building();

            this.ViewBoxStack.Children.Add(new UcBorder(notepad, 500, 500));

        }

        private void GetWindow_Click_HwndHost(object sender, RoutedEventArgs e)
        {
            ProcessHost prochost = new ProcessHost(notepad);
            this.ViewBoxStack.Children.Add(prochost);
        }

        private void GetWindow2_Click(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
