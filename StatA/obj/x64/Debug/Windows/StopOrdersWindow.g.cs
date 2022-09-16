﻿#pragma checksum "..\..\..\..\Windows\StopOrdersWindow.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "373BF5E2FAE45922721119379AF95E7F"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

using DevExpress.Xpf.DXBinding;
using StockSharp.Algo.Expressions.Xaml;
using StockSharp.Licensing.Xaml;
using StockSharp.Localization;
using StockSharp.Xaml;
using StockSharp.Xaml.Charting;
using StockSharp.Xaml.Diagram;
using StockSharp.Xaml.GridControl;
using StockSharp.Xaml.PropertyGrid;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
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


namespace StatA.Windows {
    
    
    /// <summary>
    /// StopOrderWindow
    /// </summary>
    public partial class StopOrderWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        /// <summary>
        /// OrderGrid Name Field
        /// </summary>
        
        #line 8 "..\..\..\..\Windows\StopOrdersWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public StockSharp.Xaml.OrderConditionalGrid OrderGrid;
        
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
            System.Uri resourceLocater = new System.Uri("/StatA;component/windows/stoporderswindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Windows\StopOrdersWindow.xaml"
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
            this.OrderGrid = ((StockSharp.Xaml.OrderConditionalGrid)(target));
            
            #line 8 "..\..\..\..\Windows\StopOrdersWindow.xaml"
            this.OrderGrid.OrderCanceling += new System.Action<StockSharp.BusinessEntities.Order>(this.OrderGrid_OnOrderCanceling);
            
            #line default
            #line hidden
            
            #line 8 "..\..\..\..\Windows\StopOrdersWindow.xaml"
            this.OrderGrid.OrderReRegistering += new System.Action<StockSharp.BusinessEntities.Order>(this.OrderGrid_OnOrderReRegistering);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

