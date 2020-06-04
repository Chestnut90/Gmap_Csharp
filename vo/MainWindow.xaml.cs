using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
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

            //ProcessTestWindow window = new ProcessTestWindow();
            //window.Show();

        }

        #region User32 dll functions
        [DllImport("user32.dll", EntryPoint = "GetWindowThreadProcessId", SetLastError = true,
             CharSet = CharSet.Unicode, ExactSpelling = true,
             CallingConvention = CallingConvention.StdCall)]
        private static extern long GetWindowThreadProcessId(long hWnd, long lpdwProcessId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern long SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongA", SetLastError = true)]
        private static extern long GetWindowLong(IntPtr hwnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongA", SetLastError = true)]
        public static extern int SetWindowLongA([System.Runtime.InteropServices.InAttribute()] System.IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern long SetWindowPos(IntPtr hwnd, long hWndInsertAfter, long x, long y, long cx, long cy, long wFlags);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool MoveWindow(IntPtr hwnd, int x, int y, int cx, int cy, bool repaint);

        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);
        #endregion

        #region User32 dll params
        private const int SWP_NOOWNERZORDER = 0x200;
        private const int SWP_NOREDRAW = 0x8;
        private const int SWP_NOZORDER = 0x4;
        private const int SWP_SHOWWINDOW = 0x0040;
        private const int WS_EX_MDICHILD = 0x40;
        private const int SWP_FRAMECHANGED = 0x20;
        private const int SWP_NOACTIVATE = 0x10;
        private const int SWP_ASYNCWINDOWPOS = 0x4000;
        private const int SWP_NOMOVE = 0x2;
        private const int SWP_NOSIZE = 0x1;
        private const int GWL_STYLE = (-16);
        private const int WS_VISIBLE = 0x10000000;
        private const int WS_CHILD = 0x40000000;
        #endregion

        Process proc;
        string sFilePath = @"D:\Dev\02.AntiDrone\02. src\RFScaner\RFScaner\bin\x64\Debug\RFScaner.exe";
        IntPtr handle;
        UcProcessContainer container;

        private void GetWindow_Click1(object sender, RoutedEventArgs e)
        {
            if(!(proc is null))
            {
                proc.Kill();
            }

            try
            {
                proc = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = sFilePath,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Minimized,
                    }
                };
                proc.Start();

                Thread.Sleep(5000);
                proc.WaitForInputIdle();

                handle = proc.MainWindowHandle;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // process window handling
            SetWindowLongA(handle, GWL_STYLE, WS_VISIBLE);


            //var h = new WindowInteropHelper(Window.GetWindow(window.Stack));


            return;
            container = new UcProcessContainer();

            var helper = new WindowInteropHelper(Window.GetWindow(container.ViewBox));
            SetParent(handle, helper.Handle);

            // Remove border and whatnot
            SetWindowLongA(handle, GWL_STYLE, WS_VISIBLE);

            this.MainStack.Children.Add(container);
        }

        private void GetWindow_Click(object sender, RoutedEventArgs e)
        {

            //ProcessHost host = new ProcessHost(sFilePath);

            //UcBorder border = new UcBorder();
            //border.Border.Child = host;


            //this.MainStack.Children.Add(border);

            WindowBorder border = new WindowBorder(sFilePath, 1920, 1080);

        }

        private void GetWindow2_Click(object sender, RoutedEventArgs e)
        {
            var items = this.MainStack.Children;

            Size s = new Size(500, 1000);
            (items[0] as UIElement).RenderSize = s;

        }
    }
}
