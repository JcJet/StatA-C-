using System.Collections.Generic;
using Ecng.Common;
using Ecng.Xaml;
using MoreLinq;
using StockSharp.Algo;
using StockSharp.BusinessEntities;
using StockSharp.Localization;
using StockSharp.Xaml;

namespace StatA.Windows
{
    public partial class StopOrderWindow
	{
		public StopOrderWindow()
		{
			InitializeComponent();
		}

		private void OrderGrid_OnOrderCanceling(IEnumerable<Order> orders)
		{
			orders.ForEach(StaticData.Connector.CancelOrder);
		}
        private void OrderGrid_OnOrderCanceling(Order order)
        {
            StaticData.Connector.CancelOrder(order);
        }

        private void OrderGrid_OnOrderReRegistering(Order order)
		{
			var window = new OrderWindow
			{
				Title = LocalizedStrings.Str2976Params.Put(order.TransactionId),
				SecurityProvider = StaticData.Connector,
				MarketDataProvider = StaticData.Connector,
				Portfolios = new PortfolioDataSource(StaticData.Connector),
				Order = order.ReRegisterClone(newVolume: order.Balance),
			};

			if (window.ShowModal(this))
			{
                StaticData.Connector.ReRegisterOrder(order, window.Order);
			}
		}
	}
}
