﻿#region S# License
/******************************************************************************************
NOTICE!!!  This program and source code is owned and licensed by
StockSharp, LLC, www.stocksharp.com
Viewing or use of this code requires your acceptance of the license
agreement found at https://github.com/StockSharp/StockSharp/blob/master/LICENSE
Removal of this comment is a violation of the license agreement.

Project: SampleMultiConnection.SampleMultiConnectionPublic
File: SecuritiesWindow.xaml.cs
Created: 2015, 11, 11, 2:32 PM

Copyright 2010 by StockSharp, LLC
*******************************************************************************************/
#endregion S# License

using System;
using System.Linq;
using System.Windows;
using Ecng.Collections;
using Ecng.Xaml;
using MoreLinq;
using StockSharp.BusinessEntities;
using StockSharp.Localization;
using StockSharp.Xaml;

namespace StatA.Windows
{
    public partial class SecuritiesWindow
	{
		private readonly SynchronizedDictionary<Security, QuotesWindow> _quotesWindows = new SynchronizedDictionary<Security, QuotesWindow>();
		private bool _initialized;

		public SecuritiesWindow()
		{
			InitializeComponent();
		}

		protected override void OnClosed(EventArgs e)
		{
			_quotesWindows.SyncDo(d => d.Values.ForEach(w =>
			{
				w.DeleteHideable();
				w.Close();
			}));

			var connector = StaticData.Connector;

			if (connector != null)
			{
				if (_initialized)
					connector.MarketDepthChanged -= TraderOnMarketDepthChanged;
			}

			base.OnClosed(e);
		}

		private void NewOrderClick(object sender, RoutedEventArgs e)
		{
			var connector = StaticData.Connector;

			var newOrder = new OrderWindow
			{
				Order = new Order { Security = SecurityPicker.SelectedSecurity },
				SecurityProvider = connector,
				MarketDataProvider = connector,
				Portfolios = new PortfolioDataSource(connector),
			};

			if (newOrder.ShowModal(this))
				connector.RegisterOrder(newOrder.Order);
		}

		private void SecurityPicker_OnSecuritySelected(Security security)
		{
			TicksButton.IsEnabled = ScoringButton.IsEnabled = Quotes.IsEnabled = NewOrder.IsEnabled = Depth.IsEnabled = security != null;
		    if (security != null)
		    {
		        TicksButton.Content = StaticData.Connector.RegisteredTrades.Contains(security) ? "Тики ☑" : "Тики ☐";
                ScoringButton.Content = StaticData.Scoring.Contains(security) ? "Скоринг ☑" : "Скоринг ☐";
            }
		}

		private void DepthClick(object sender, RoutedEventArgs e)
		{
			var connector = StaticData.Connector;

			var window = _quotesWindows.SafeAdd(SecurityPicker.SelectedSecurity, security =>
			{
				// subscribe on order book flow
				connector.RegisterMarketDepth(security);

				// create order book window
				var wnd = new QuotesWindow { Title = security.Id + " " + LocalizedStrings.MarketDepth };
				wnd.MakeHideable();
				return wnd;
			});

			if (window.Visibility == Visibility.Visible)
				window.Hide();
			else
				window.Show();

			if (!_initialized)
			{
				TraderOnMarketDepthChanged(connector.GetMarketDepth(SecurityPicker.SelectedSecurity));
				connector.MarketDepthChanged += TraderOnMarketDepthChanged;
				_initialized = true;
			}
		}

		private void QuotesClick(object sender, RoutedEventArgs e)
		{
			var security = SecurityPicker.SelectedSecurity;

			var connector = StaticData.Connector;

			if (connector.RegisteredSecurities.Contains(security))
				connector.UnRegisterSecurity(security);
			else
				connector.RegisterSecurity(security);
		}

		private void TraderOnMarketDepthChanged(MarketDepth depth)
		{
			var wnd = _quotesWindows.TryGetValue(depth.Security);

			if (wnd != null)
				wnd.DepthCtrl.UpdateDepth(depth);
		}

		private void FindClick(object sender, RoutedEventArgs e)
		{
			var wnd = new SecurityLookupWindow { Criteria = new Security { Code = "IS" } };

			if (!wnd.ShowModal(this))
				return;

			StaticData.Connector.LookupSecurities(wnd.Criteria);
		}

        private void TicksButton_Click(object sender, RoutedEventArgs e)
        {
            var security = SecurityPicker.SelectedSecurity;

            var connector = StaticData.Connector;

            if (connector.RegisteredTrades.Contains(security))
            {
                connector.UnRegisterTrades(security);
                TicksButton.Content = "Тики ☐";
            }
            else
            {
                connector.RegisterTrades(security);
                TicksButton.Content = "Тики ☑";
            }
        }

        private void ScoringButton_Click(object sender, RoutedEventArgs e)
        {
            var security = SecurityPicker.SelectedSecurity;
            if (StaticData.Scoring.Contains(security))
            {
                StaticData.Scoring.Remove(security);
                ScoringButton.Content = "Скоринг ☐";
            }
            else
            {
                StaticData.Scoring.Add(security);
                ScoringButton.Content = "Скоринг ☑";
            }
            StaticData.scoringWindow.UpdateScoring();
        }


    }
}