using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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
using vo.Views.Hwnds;

namespace vo.Views.ProcessWindow
{
    /// <summary>
    /// UcBorder.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UcBorder : UserControl
    {
        public UcBorder(string processName, int width = 0, int height = 0)
        {
            InitializeComponent();
            this.ProcessFileName = processName;
            this.Width = width;
            this.Height = height;

            this.RunProcess();
            this.Building();
        }

        public string ProcessFileName { get; private set; }
        public int ProcessWidth { get; set; }
        public int ProcessHeight { get; set; }
        private Process process;
        private IntPtr processWindowHandle;

        public IntPtr RunProcess(string fileName = null)
        {
            if (fileName is null)
            {
                fileName = this.ProcessFileName;
            }

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
                        FileName = fileName,
                        UseShellExecute = false,
                        WindowStyle = ProcessWindowStyle.Minimized,
                        CreateNoWindow = false,
                    }
                };
                process.Start();

                Thread.Sleep(5000);
                process.WaitForInputIdle();

                processWindowHandle = process.MainWindowHandle;
                return processWindowHandle;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Building(IntPtr handle = default(IntPtr))
        {
            if (handle.Equals(default(IntPtr)))
            {
                handle = this.processWindowHandle;
            }

            //this.Show();
            //this.ShowActivated = true;

            this.BuildWithChild(handle);


            return;
        }

        #region How to Build process window.
        private void BuildWithOwner(IntPtr sourceHandle)
        {
            var helper = new WindowInteropHelper(Window.GetWindow(this.Border));
            helper.Owner = sourceHandle;
        }

        private void BuildWithChild(IntPtr sourceHandle)
        {
            User32Dll.SetWindowLongA(sourceHandle, User32Dll.GWL_STYLE, User32Dll.WS_VISIBLE);

            var helper = new WindowInteropHelper(Window.GetWindow(this));
            User32Dll.SetParent(sourceHandle, helper.Handle);
            User32Dll.MoveWindow(sourceHandle, 0, 0, (int)this.ActualWidth, (int)this.ActualHeight, true);
        }

        private void BuildWithHwndSource(IntPtr sourceHandle)
        {
            HwndSource procHwndSource = HwndSource.FromHwnd(sourceHandle);

            var h = new WindowInteropHelper(Window.GetWindow(this));
            //SetParent(handle, h.Handle);

            HwndSourceParameters parameters = new HwndSourceParameters();


            parameters.WindowStyle = User32Dll.WS_VISIBLE | User32Dll.WS_CHILD;
            parameters.SetPosition(0, 0);
            parameters.SetSize((int)this.ProcessWidth, (int)this.ProcessHeight);
            parameters.ParentWindow = h.Handle;
            //parameters.UsesPerPixelOpacity = true;
            HwndSource src = new HwndSource(parameters);

            //src.CompositionTarget.BackgroundColor = Colors.Transparent;
            src.SizeToContent = SizeToContent.Manual;
            src.CompositionTarget.BackgroundColor = Colors.White;
            //src.RootVisual = (Visual)this.Stack.Child;

            //this = (Window)HwndSource.FromHwnd(handle).RootVisual;

            //int x = (int)this.Left;
            //int y = (int)this.Top;

            //User32Dll.MoveWindow(handle, x, y, (int)this.ProcessWidth - 3, (int)this.ProcessHeight - 2, true);

        }
        #endregion

        /// <summary>
        /// View Event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UserControl_LayoutUpdated(object sender, EventArgs e)
        {
            User32Dll.MoveWindow(processWindowHandle, 0, 0, (int)this.ActualWidth, (int)this.ActualHeight, true);
        }
    }
}
