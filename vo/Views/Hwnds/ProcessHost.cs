using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using vo.Views.ProcessWindow;

namespace vo.Hwnds.Views
{
    class ProcessHost : HwndHost
    {
        public ProcessHost(string processName)
        {
            this.ProcessName = processName;
            this.Building();
        }

        public string ProcessName { get; set; }

        private Process process;
        private IntPtr processWindowHandle;
        private IntPtr _hwndHost;
        public IntPtr HwndHost { get; private set; }
        private int _hostHeight = 500;
        private int _hostWidth = 500;

        private void Building()
        {
            if (!(process is null))
            {
                process.Kill();
            }

            try
            {
                process = new Process()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = this.ProcessName,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                    }
                };
                process.Start();

                Thread.Sleep(5000);
                process.WaitForInputIdle();
                Debug.WriteLine("Sleep end.");

                processWindowHandle = process.MainWindowHandle;
                //processWindowHandle = GetWindow(process.MainWindowHandle, GW_OWNER);


            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {

            HwndHost = IntPtr.Zero;
            _hwndHost = IntPtr.Zero;

            _hwndHost = CreateWindowEx(0, "static", "",
                WsChild | WsVisible,
                0, 0,
                _hostHeight, _hostWidth,
                hwndParent.Handle,
                (IntPtr)HostId,
                IntPtr.Zero,
                0);

            HwndHost = CreateWindowEx(0, "listbox", "",
                WsChild | WsVisible | LbsNotify
                | WsVscroll | WsBorder,
                0, 0,
                _hostHeight, _hostWidth,
                _hwndHost,
                (IntPtr)ListboxId,
                IntPtr.Zero,
                0);

            return new HandleRef(this, _hwndHost);
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            handled = false;
            return IntPtr.Zero;
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {


        }

        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            DestroyWindow(hwnd.Handle);
        }

        internal const int
            WsChild = 0x40000000,
            WsVisible = 0x10000000,
            LbsNotify = 0x00000001,
            HostId = 0x00000002,
            ListboxId = 0x00000001,
            WsVscroll = 0x00200000,
            WsBorder = 0x00800000;

        [DllImport("user32.dll", EntryPoint = "CreateWindowEx", CharSet = CharSet.Unicode)]
        internal static extern IntPtr CreateWindowEx(int dwExStyle,
            string lpszClassName,
            string lpszWindowName,
            int style,
            int x, int y,
            int width, int height,
            IntPtr hwndParent,
            IntPtr hMenu,
            IntPtr hInst,
            [MarshalAs(UnmanagedType.AsAny)] object pvParam);

        [DllImport("user32.dll", EntryPoint = "DestroyWindow", CharSet = CharSet.Unicode)]
        internal static extern bool DestroyWindow(IntPtr hwnd);

        // ref from : https://www.pinvoke.net/default.aspx/user32/GetWindow.html
        uint GW_HWNDFIRST = 0,
            GW_HWNDLAST = 1,
            GW_HWNDNEXT = 2,
            GW_HWNDPREV = 3,
            GW_OWNER = 4,
            GW_CHILD = 5,
            GW_ENABLEDPOPUP = 6;

        [DllImport("user32.dll", SetLastError = true)]
        internal static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);
    }
}
