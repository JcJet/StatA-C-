using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Ecng.Configuration;
using Ecng.Serialization;
using Ecng.Xaml;
using NPOI.HPSF.Wellknown;
using StockSharp.BusinessEntities;
using StatA.Classes;
using StatA.Windows;
using StockSharp.Algo;
using StockSharp.Algo.Storages;
using StockSharp.Algo.Strategies;
using StockSharp.Messages;
using StockSharp.Micex.Native.Tables;

namespace StatA
{
    //Статический класс для хранения "глобальных" переменных
    public static class StaticData
    {
        //Корзины инструментов для парного трейдинга или арбитража
        public static Pair CurrPair { get; set; }
        //Список инструментов для скоринга
        public static List<Security> Scoring { get; set; }
        //Синтетический инструмент - спред между корзинами
        public static WeightedIndexSecurity Spread { get; set; }
        public static SecuritiesWindow securitiesWindow { get; set; }
        public static OrdersWindow ordersWindow { get; set; }
        public static StopOrderWindow stopOrdersWindow { get; set; }
        public static PortfoliosWindow portfoliosWindow { get; set; }
        public static MyTradesWindow myTradesWindow { get; set; }
        public static TradesWindow tradesWindow { get; set; }
        public static ChartWindow chartWindow { get; set; }
        public static ScoringWindow scoringWindow { get; set; }
        public static StorageWindow storageWindow { get; set; }
        public static StrategiesWindow strategiesWindow { get; set; }
        public static Connector Connector { get; set; }
        public static StorageRegistry Storage { get; set; }
        public static List<Strategy> Strategies { get; set; }
        public static Dictionary<Strategy, UnifiedChartWindow> ChartWindows { get; set; }
        public static void Init()
        {
            CurrPair = new Pair();
            Scoring = new List<Security>();
            Spread = new WeightedIndexSecurity();
            SecurityId id = new SecurityId() {SecurityCode = "Init1", BoardCode = "Init1"};
            Spread.Weights.Add(id, 1);
            id = new SecurityId() { SecurityCode = "Init2", BoardCode = "Init2" };
            Spread.Weights.Add(id, -1);
            //Spread.Weights.Add(CurrPair.FirstBasket.ToSecurityId(), 1);
            //Spread.Weights.Add(CurrPair.SecondBasket.ToSecurityId(), -1);
            Strategies = new List<Strategy>();
            securitiesWindow = new SecuritiesWindow();
            ordersWindow = new OrdersWindow();
            stopOrdersWindow = new StopOrderWindow();
            portfoliosWindow = new PortfoliosWindow();
            myTradesWindow = new MyTradesWindow();
            tradesWindow = new TradesWindow();
            chartWindow = new ChartWindow();
            scoringWindow = new ScoringWindow();
            storageWindow = new StorageWindow();
            strategiesWindow = new StrategiesWindow();

            ordersWindow.MakeHideable();
            myTradesWindow.MakeHideable();
            tradesWindow.MakeHideable();
            securitiesWindow.MakeHideable();
            stopOrdersWindow.MakeHideable();
            portfoliosWindow.MakeHideable();
            chartWindow.MakeHideable();
            scoringWindow.MakeHideable();
            storageWindow.MakeHideable();
            strategiesWindow.MakeHideable();
            Storage = new StorageRegistry();
            ChartWindows = new Dictionary<Strategy, UnifiedChartWindow>();
        }

        public static void DeInit()
        {
            ordersWindow.DeleteHideable();
            myTradesWindow.DeleteHideable();
            tradesWindow.DeleteHideable();
            securitiesWindow.DeleteHideable();
            stopOrdersWindow.DeleteHideable();
            portfoliosWindow.DeleteHideable();
            chartWindow.DeleteHideable();
            scoringWindow.DeleteHideable();
            storageWindow.DeleteHideable();
            strategiesWindow.DeleteHideable();
            foreach (var window in ChartWindows)
            {
                window.Value.DeleteHideable();
                window.Value.Close();
            }
            securitiesWindow.Close();
            tradesWindow.Close();
            myTradesWindow.Close();
            stopOrdersWindow.Close();
            ordersWindow.Close();
            portfoliosWindow.Close();
            chartWindow.Close();
            scoringWindow.Close();
            storageWindow.Close();
            strategiesWindow.Close();

            Connector.Dispose();
            ConfigManager.GetService<IEntityRegistry>().DelayAction.OnFlush();
        }
    }
}
