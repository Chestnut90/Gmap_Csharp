﻿#pragma checksum "..\..\..\..\Views\UcMapControl.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "B589E0BFD0060F7EAC2048A3DEB82149442EDD24CA11C3606311A11CDBEE6E54"
//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

using DevExpress.Xpf.DXBinding;
using GMap.NET.WindowsPresentation;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;
using vo.Views;


namespace vo.Views {
    
    
    /// <summary>
    /// UcMapControl
    /// </summary>
    public partial class UcMapControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 11 "..\..\..\..\Views\UcMapControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal GMap.NET.WindowsPresentation.GMapControl gMapControl;
        
        #line default
        #line hidden
        
        
        #line 13 "..\..\..\..\Views\UcMapControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ContextMenu popup;
        
        #line default
        #line hidden
        
        
        #line 14 "..\..\..\..\Views\UcMapControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem MenuDefenseSystem;
        
        #line default
        #line hidden
        
        
        #line 15 "..\..\..\..\Views\UcMapControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem MenuDrone;
        
        #line default
        #line hidden
        
        
        #line 17 "..\..\..\..\Views\UcMapControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem MenuPolygon;
        
        #line default
        #line hidden
        
        
        #line 18 "..\..\..\..\Views\UcMapControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem MenuEllipse;
        
        #line default
        #line hidden
        
        
        #line 19 "..\..\..\..\Views\UcMapControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem MenuRectangle;
        
        #line default
        #line hidden
        
        
        #line 20 "..\..\..\..\Views\UcMapControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem MenuTriangle;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\..\Views\UcMapControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem MenuAlaramArea;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\..\Views\UcMapControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.MenuItem MenuJamming;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\..\Views\UcMapControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock TextBlockLatLng;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/vo;component/views/ucmapcontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Views\UcMapControl.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "4.0.0.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.gMapControl = ((GMap.NET.WindowsPresentation.GMapControl)(target));
            
            #line 11 "..\..\..\..\Views\UcMapControl.xaml"
            this.gMapControl.Loaded += new System.Windows.RoutedEventHandler(this.GMapControl_Loaded);
            
            #line default
            #line hidden
            
            #line 11 "..\..\..\..\Views\UcMapControl.xaml"
            this.gMapControl.MouseLeftButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.GMapControl_MouseLeftButtonDown);
            
            #line default
            #line hidden
            
            #line 11 "..\..\..\..\Views\UcMapControl.xaml"
            this.gMapControl.MouseRightButtonDown += new System.Windows.Input.MouseButtonEventHandler(this.GMapControl_MouseRightButtonDown);
            
            #line default
            #line hidden
            
            #line 11 "..\..\..\..\Views\UcMapControl.xaml"
            this.gMapControl.MouseMove += new System.Windows.Input.MouseEventHandler(this.GMapControl_MouseMove);
            
            #line default
            #line hidden
            
            #line 11 "..\..\..\..\Views\UcMapControl.xaml"
            this.gMapControl.MouseWheel += new System.Windows.Input.MouseWheelEventHandler(this.GMapControl_MouseWheel);
            
            #line default
            #line hidden
            return;
            case 2:
            this.popup = ((System.Windows.Controls.ContextMenu)(target));
            return;
            case 3:
            this.MenuDefenseSystem = ((System.Windows.Controls.MenuItem)(target));
            
            #line 14 "..\..\..\..\Views\UcMapControl.xaml"
            this.MenuDefenseSystem.Click += new System.Windows.RoutedEventHandler(this.MenuDefenseSystem_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.MenuDrone = ((System.Windows.Controls.MenuItem)(target));
            
            #line 15 "..\..\..\..\Views\UcMapControl.xaml"
            this.MenuDrone.Click += new System.Windows.RoutedEventHandler(this.MenuDrone_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.MenuPolygon = ((System.Windows.Controls.MenuItem)(target));
            
            #line 17 "..\..\..\..\Views\UcMapControl.xaml"
            this.MenuPolygon.Click += new System.Windows.RoutedEventHandler(this.Draw_Clicks);
            
            #line default
            #line hidden
            return;
            case 6:
            this.MenuEllipse = ((System.Windows.Controls.MenuItem)(target));
            
            #line 18 "..\..\..\..\Views\UcMapControl.xaml"
            this.MenuEllipse.Click += new System.Windows.RoutedEventHandler(this.Draw_Clicks);
            
            #line default
            #line hidden
            return;
            case 7:
            this.MenuRectangle = ((System.Windows.Controls.MenuItem)(target));
            
            #line 19 "..\..\..\..\Views\UcMapControl.xaml"
            this.MenuRectangle.Click += new System.Windows.RoutedEventHandler(this.Draw_Clicks);
            
            #line default
            #line hidden
            return;
            case 8:
            this.MenuTriangle = ((System.Windows.Controls.MenuItem)(target));
            
            #line 20 "..\..\..\..\Views\UcMapControl.xaml"
            this.MenuTriangle.Click += new System.Windows.RoutedEventHandler(this.Draw_Clicks);
            
            #line default
            #line hidden
            return;
            case 9:
            this.MenuAlaramArea = ((System.Windows.Controls.MenuItem)(target));
            
            #line 22 "..\..\..\..\Views\UcMapControl.xaml"
            this.MenuAlaramArea.Click += new System.Windows.RoutedEventHandler(this.MenuAlaramArea_Click);
            
            #line default
            #line hidden
            return;
            case 10:
            this.MenuJamming = ((System.Windows.Controls.MenuItem)(target));
            
            #line 23 "..\..\..\..\Views\UcMapControl.xaml"
            this.MenuJamming.Click += new System.Windows.RoutedEventHandler(this.MenuJamming_Click);
            
            #line default
            #line hidden
            return;
            case 11:
            this.TextBlockLatLng = ((System.Windows.Controls.TextBlock)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

