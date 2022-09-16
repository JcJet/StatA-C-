﻿#pragma checksum "..\..\..\Windows\SecuritiesWindow1.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "1B24A6915068AC24932B6FE1C219D541"
//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

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
    /// SecuritiesWindow
    /// </summary>
    public partial class SecuritiesWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        /// <summary>
        /// SecurityPicker Name Field
        /// </summary>
        
        #line 13 "..\..\..\Windows\SecuritiesWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        public StockSharp.Xaml.SecurityPicker SecurityPicker;
        
        #line default
        #line hidden
        
        
        #line 22 "..\..\..\Windows\SecuritiesWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Find;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\Windows\SecuritiesWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button TicksButton;
        
        #line default
        #line hidden
        
        
        #line 28 "..\..\..\Windows\SecuritiesWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Quotes;
        
        #line default
        #line hidden
        
        
        #line 29 "..\..\..\Windows\SecuritiesWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button Depth;
        
        #line default
        #line hidden
        
        
        #line 32 "..\..\..\Windows\SecuritiesWindow1.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button NewOrder;
        
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
            System.Uri resourceLocater = new System.Uri("/StatA;component/windows/securitieswindow1.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\Windows\SecuritiesWindow1.xaml"
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
            this.SecurityPicker = ((StockSharp.Xaml.SecurityPicker)(target));
            
            #line 13 "..\..\..\Windows\SecuritiesWindow1.xaml"
            this.SecurityPicker.SecuritySelected += new System.Action<StockSharp.BusinessEntities.Security>(this.SecurityPicker_OnSecuritySelected);
            
            #line default
            #line hidden
            return;
            case 2:
            this.Find = ((System.Windows.Controls.Button)(target));
            
            #line 22 "..\..\..\Windows\SecuritiesWindow1.xaml"
            this.Find.Click += new System.Windows.RoutedEventHandler(this.FindClick);
            
            #line default
            #line hidden
            return;
            case 3:
            this.TicksButton = ((System.Windows.Controls.Button)(target));
            
            #line 27 "..\..\..\Windows\SecuritiesWindow1.xaml"
            this.TicksButton.Click += new System.Windows.RoutedEventHandler(this.TicksButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.Quotes = ((System.Windows.Controls.Button)(target));
            
            #line 28 "..\..\..\Windows\SecuritiesWindow1.xaml"
            this.Quotes.Click += new System.Windows.RoutedEventHandler(this.QuotesClick);
            
            #line default
            #line hidden
            return;
            case 5:
            this.Depth = ((System.Windows.Controls.Button)(target));
            
            #line 29 "..\..\..\Windows\SecuritiesWindow1.xaml"
            this.Depth.Click += new System.Windows.RoutedEventHandler(this.DepthClick);
            
            #line default
            #line hidden
            return;
            case 6:
            this.NewOrder = ((System.Windows.Controls.Button)(target));
            
            #line 32 "..\..\..\Windows\SecuritiesWindow1.xaml"
            this.NewOrder.Click += new System.Windows.RoutedEventHandler(this.NewOrderClick);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

