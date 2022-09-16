﻿#pragma checksum "..\..\..\Windows\StrategiesWindow.xaml" "{8829d00f-11b8-4213-878b-770e8597ac16}" "1945F7637FAA2EE6A952F6DF89C883486A78E0838706C7FFD803381DEF511FFC"
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
using StatA;
using StatA.Windows;
using StockSharp.Algo.Expressions.Xaml;
using StockSharp.Licensing.Xaml;
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
    /// StrategiesWindow
    /// </summary>
    public partial class StrategiesWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 19 "..\..\..\Windows\StrategiesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal StockSharp.Xaml.StrategiesDashboard StrategiesDashboard1;
        
        #line default
        #line hidden
        
        
        #line 21 "..\..\..\Windows\StrategiesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button AddStrategyBtn;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\Windows\StrategiesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button RemoveStrategyBtn;
        
        #line default
        #line hidden
        
        
        #line 23 "..\..\..\Windows\StrategiesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button StrategySettingsBtn;
        
        #line default
        #line hidden
        
        
        #line 24 "..\..\..\Windows\StrategiesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button StartButton;
        
        #line default
        #line hidden
        
        
        #line 25 "..\..\..\Windows\StrategiesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button TestingButton;
        
        #line default
        #line hidden
        
        
        #line 26 "..\..\..\Windows\StrategiesWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button StatisticsButton;
        
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
            System.Uri resourceLocater = new System.Uri("/StatA;component/windows/strategieswindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Windows\StrategiesWindow.xaml"
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
            this.StrategiesDashboard1 = ((StockSharp.Xaml.StrategiesDashboard)(target));
            return;
            case 2:
            this.AddStrategyBtn = ((System.Windows.Controls.Button)(target));
            
            #line 21 "..\..\..\Windows\StrategiesWindow.xaml"
            this.AddStrategyBtn.Click += new System.Windows.RoutedEventHandler(this.AddStrategyBtn_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.RemoveStrategyBtn = ((System.Windows.Controls.Button)(target));
            return;
            case 4:
            this.StrategySettingsBtn = ((System.Windows.Controls.Button)(target));
            
            #line 23 "..\..\..\Windows\StrategiesWindow.xaml"
            this.StrategySettingsBtn.Click += new System.Windows.RoutedEventHandler(this.StrategySettingsBtn_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            this.StartButton = ((System.Windows.Controls.Button)(target));
            
            #line 24 "..\..\..\Windows\StrategiesWindow.xaml"
            this.StartButton.Click += new System.Windows.RoutedEventHandler(this.StartButton_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.TestingButton = ((System.Windows.Controls.Button)(target));
            return;
            case 7:
            this.StatisticsButton = ((System.Windows.Controls.Button)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

