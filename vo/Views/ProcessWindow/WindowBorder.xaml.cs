using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace vo.Views.ProcessWindow
{
    /// <summary>
    /// WindowBorder.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class WindowBorder : Window
    {
        public WindowBorder(string processFile, int width, int height)
        {
            InitializeComponent();

            this.ProcessFile = processFile;
            this.ProcessWidth = width;
            this.ProcessHeight = height;
            this.Building();
        }

        public WindowBorder()
        {
            InitializeComponent();
        }

        public string ProcessFile { get; private set; }
        public int ProcessWidth { get; set; }
        public int ProcessHeight { get; set; }
        private Process proc;
        private IntPtr handle;

        private void Building()
        {
            if (!(proc is null))
            {
                proc.Kill();
            }

            try
            {
                proc = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = ProcessFile,
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

            this.Show();
            this.ShowActivated = true;

            var h = new WindowInteropHelper(Window.GetWindow(this.Stack));
            //SetParent(handle, h.Handle);

            HwndSourceParameters parameters = new HwndSourceParameters();

            parameters.WindowStyle = WS_VISIBLE | WS_CHILD;
            parameters.SetPosition(0, 0);
            parameters.SetSize((int)this.ProcessWidth, (int)this.ProcessHeight);
            parameters.ParentWindow = handle;
            //parameters.UsesPerPixelOpacity = true;
            HwndSource src = new HwndSource(parameters);

            //src.CompositionTarget.BackgroundColor = Colors.Transparent;
            src.SizeToContent = SizeToContent.Manual;
            src.CompositionTarget.BackgroundColor = Colors.White;
            //src.RootVisual = (Visual)this.Stack.Child;

            //this = (Window)HwndSource.FromHwnd(handle).RootVisual;

            int x = (int)this.Left;
            int y = (int)this.Top;

            MoveWindow(handle, x, y, (int)this.ProcessWidth - 3, (int)this.ProcessHeight - 2, true);

            return;
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
    }
}
