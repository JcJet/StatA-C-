﻿#region S# License
/******************************************************************************************
NOTICE!!!  This program and source code is owned and licensed by
StockSharp, LLC, www.stocksharp.com
Viewing or use of this code requires your acceptance of the license
agreement found at https://github.com/StockSharp/StockSharp/blob/master/LICENSE
Removal of this comment is a violation of the license agreement.

Project: SampleMultiConnection.SampleMultiConnectionPublic
File: OrdersWindow.xaml.cs
Created: 2015, 11, 11, 2:32 PM

Copyright 2010 by StockSharp, LLC
*******************************************************************************************/
#endregion S# License

using System.Collections.Generic;
using MoreLinq;
using StockSharp.BusinessEntities;

namespace StatA.Windows
{
    public partial class OrdersWindow
	{
		public OrdersWindow()
		{
			InitializeComponent();
		}

		private void OrderGrid_OnOrderCanceling(IEnumerable<Order> orders)
		{
            //orders.ForEach(MainWindow.Instance.Connector.CancelOrder);
            orders.ForEach(StaticData.Connector.CancelOrder);
            

        }
        private void OrderGrid_OnOrderCanceling(Order order)
        {
            //orders.ForEach(MainWindow.Instance.Connector.CancelOrder);
            StaticData.Connector.CancelOrder(order);


        }
    }
}