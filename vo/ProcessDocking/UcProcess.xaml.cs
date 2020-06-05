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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace vo.ProcessDocking
{
    /// <summary>
    /// UcProcess.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcProcess : UserControl
    {

        //string sFilePath;
        Rect SWPrograms;

        [System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Sequential)]
        public struct HWND__
        {
            public int unused;
        }

        public UcProcess(string exeName)
        {
            InitializeComponent();

            this.ExeName = exeName;
            this.Loaded += new RoutedEventHandler(OnVisibleChanged);
            this.SizeChanged += new SizeChangedEventHandler(OnSizeChanged);
            this.SizeChanged += new SizeChangedEventHandler(OnResize);

            SWPrograms = new Rect();

        }
        ~UcProcess()
        {
            this.Dispose();
        }

        private bool _iscreated = false;
        private bool _isdisposed = false;
        IntPtr _appWin;
        private Process _childp;

        public string ExeName { get; private set; }

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

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

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


        protected void OnSizeChanged(object s, SizeChangedEventArgs e)
        {
            this.InvalidateVisual();
        }

        protected void OnVisibleChanged(object s, RoutedEventArgs e)
        {
            // If control needs to be initialized/created
            if (_iscreated == false)
            {

                // Mark that control is created
                _iscreated = true;

                // Initialize handle value to invalid
                _appWin = IntPtr.Zero;

                try
                {
                    var procInfo = new System.Diagnostics.ProcessStartInfo(this.ExeName);
                    procInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(this.ExeName);
                    // Start the process
                    _childp = System.Diagnostics.Process.Start(procInfo);

                    Thread.Sleep(5000);

                    // Wait for process to be created and enter idle condition
                    _childp.WaitForInputIdle();

                    // Get the main handle
                    _appWin = _childp.MainWindowHandle;
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.Message + "Error");
                }


                // Put it into this form
                var helper = new WindowInteropHelper(Window.GetWindow(this.StackPanel));
                SetParent(_appWin, helper.Handle);

                // Remove border and whatnot
                SetWindowLongA(_appWin, GWL_STYLE, WS_VISIBLE);


                GetWindowRect(_appWin, ref SWPrograms);

                this.Width = SWPrograms.Right - SWPrograms.Left;
                this.Height = SWPrograms.Bottom - SWPrograms.Top;
                Window parent = Window.GetWindow(this.VisualParent);

                // Move the window to overlay it on this window
                //MoveWindow(_appWin, 5, 5, (int)SWPrograms.Right - SWPrograms.Left, (int)SWPrograms.Bottom - SWPrograms.Top , true);
                MoveWindow(_appWin, (int)parent.Left, (int)parent.Top, (int)this.ActualWidth - 3, (int)this.ActualHeight - 2, true);


                //RFScannerViewBox.Children.Add(UcRFScan);

            }
        }

        protected void OnResize(object s, SizeChangedEventArgs e)
        {
            if (this._appWin != IntPtr.Zero)
            {
                Window parent = Window.GetWindow(this.VisualParent);
                //MoveWindow(_appWin, 0, 0, (int)this.ActualWidth, (int)this.ActualHeight, true);
                //MoveWindow(_appWin, 5, 5, (int)SWPrograms.Right - SWPrograms.Left, (int)SWPrograms.Bottom - SWPrograms.Top, true);
                MoveWindow(_appWin, (int)parent.Left, (int)parent.Top, (int)this.ActualWidth - 3, (int)this.ActualHeight - 2, true);
            }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_isdisposed)
            {
                if (disposing)
                {
                    if (_iscreated && _appWin != IntPtr.Zero && !_childp.HasExited)
                    {
                        // Stop the application
                        _childp.Kill();

                        // Clear internal handle
                        _appWin = IntPtr.Zero;
                    }
                }
                _isdisposed = true;
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
