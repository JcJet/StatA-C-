using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using StatA.Classes;
using StatA.Strategies;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using StockSharp.Algo.Storages;
using StockSharp.Messages;
using StockSharp.Xaml.Charting;
using StockSharp.Algo.Strategies;

namespace StatA.Windows
{
    /// <summary>
    /// Логика взаимодействия для UnifiedChartWindow.xaml
    /// </summary>
    public partial class UnifiedChartWindow : Window
    {
        public ChartArea candlesArea, indicatorsArea, indicatorsArea2;
        public ChartCandleElement chartCandleElement1, chartCandleElement2;
        private ChartTradeElement chartTradeElement;
        private ChartIndicatorElement chartVolumeElement;
        private ChartIndicatorElement chartStationarityElement;
        private ChartIndicatorElement chartSpreadElement;
        private ChartIndicatorElement chartMovingAverageElement;
        private ChartIndicatorElement[] chartStandardDeviationElement;
        public ChartWindow.WindowSettings settings;
        private IMarketDataStorage<CandleMessage> candleStorage;
        private VolumeIndicator volumeIndicator;
        //private ArbitrageIndicator arbitrageIndicator; //ComplexIndicatorValue - ошибка в метоже Draw() графика... Также как и ADX (TODO: разобраться, как разместить комплексный индикатор)
        private Stationarity stationarityIndicator;
        private SpreadIndicator spreadIndicator;
        private SimpleMovingAverage movingAverage;
        private StandardDeviation standardDeviation;
        //private AverageDirectionalIndex adxIndicator;
        private CandleManager candleManager;
        private ArbitrageStrategy SelectedStrategy;
        public UnifiedChartWindow(ArbitrageStrategy strategy)
        {
            //Инициализация настроек окна
            settings = new ChartWindow.WindowSettings();
            //settings.timeFrame = strategy.TimeFrame;
            InitializeComponent();
            SelectedStrategy = strategy;
            ChartInit();
        }
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var ctfW = new CustomTFWindow();
            ctfW.ShowDialog();
        }

        void ChartInit() //организацию элементов - в стратегию
        {
            Chart1.ClearAreas();
            Chart1.IsAutoRange = true;
            //Инициализация индикаторов
            volumeIndicator = new VolumeIndicator();
            stationarityIndicator = new Stationarity();
            spreadIndicator = new SpreadIndicator();
            movingAverage = new SimpleMovingAverage();
            movingAverage.Length = 25; //Рекомендация Robotcraft: 20-25
            standardDeviation = new StandardDeviation();
            //Инициализация элементов графиков
            chartVolumeElement = new ChartIndicatorElement();
            chartVolumeElement.DrawStyle = ChartIndicatorDrawStyles.Histogram;
            chartStationarityElement = new ChartIndicatorElement();
            chartStationarityElement.DrawStyle = ChartIndicatorDrawStyles.Histogram;
            chartSpreadElement = new ChartIndicatorElement() {Color = Colors.Black};
            chartMovingAverageElement = new ChartIndicatorElement();
            chartMovingAverageElement.Color = Colors.Cyan;
            chartStandardDeviationElement = new ChartIndicatorElement[]
            {
                    new ChartIndicatorElement() {Color = Colors.Orange}, //SD
                    new ChartIndicatorElement() {Color = Colors.Orange}, //-SD
                    new ChartIndicatorElement() {Color = Colors.Red}, //2*SD
                    new ChartIndicatorElement() {Color = Colors.Red}, //-2*SD
            };
            chartCandleElement1 = new ChartCandleElement();
            chartCandleElement2 = new ChartCandleElement();
            //chartTradeElement = new ChartTradeElement();
            //Инициализация области графика
            candlesArea = new ChartArea(); 
            indicatorsArea = new ChartArea();
            indicatorsArea.Height = 400;
            indicatorsArea2 = new ChartArea();
            indicatorsArea2.Height = 150;
            //Добавление областей к графику
            Chart1.Areas.Add(candlesArea);
            Chart1.Areas.Add(indicatorsArea);
            Chart1.Areas.Add(indicatorsArea2);
            //Добавление элементов к областям графика
            candlesArea.Elements.Add(chartCandleElement1);
            candlesArea.Elements.Add(chartCandleElement2);
            indicatorsArea.Elements.Add(chartSpreadElement);
            indicatorsArea.Elements.Add(chartMovingAverageElement);
            indicatorsArea.Elements.Add(chartStandardDeviationElement[0]);
            indicatorsArea.Elements.Add(chartStandardDeviationElement[1]);
            indicatorsArea.Elements.Add(chartStandardDeviationElement[2]);
            indicatorsArea.Elements.Add(chartStandardDeviationElement[3]);
            indicatorsArea2.Elements.Add(chartStationarityElement);

        }

        void TimeFrameChange(TimeSpan timeframe)
        {
            settings.timeFrame = timeframe;
            if (timeframe.TotalMinutes == 1)
                M1_RB.IsChecked = true;
            if (timeframe.TotalMinutes == 2)
                M2_RB.IsChecked = true;
            if (timeframe.TotalMinutes == 3)
                M3_RB.IsChecked = true;
            if (timeframe.TotalMinutes == 4)
                M4_RB.IsChecked = true;
            if (timeframe.TotalMinutes == 5)
                M5_RB.IsChecked = true;
            if (timeframe.TotalMinutes == 10)
                M10_RB.IsChecked = true;
            if (timeframe.TotalMinutes == 15)
                M15_RB.IsChecked = true;
            if (timeframe.TotalMinutes == 30)
                M30_RB.IsChecked = true;
            if (timeframe.TotalMinutes == 60)
                H1_RB.IsChecked = true;
            if (timeframe.TotalHours == 4)
                H4_RB.IsChecked = true;
            if (timeframe.TotalHours == 24)
                D_RB.IsChecked = true;
            if (timeframe.TotalDays == 7)
                W_RB.IsChecked = true;
            if (timeframe.TotalDays == 30)
                MN_RB.IsChecked = true;
            else
            {
                M1_RB.IsChecked = false;
                M2_RB.IsChecked = false;
                M3_RB.IsChecked = false;
                M3_RB.IsChecked = false;
                M4_RB.IsChecked = false;
                M5_RB.IsChecked = false;
                M10_RB.IsChecked = false;
                M15_RB.IsChecked = false;
                M30_RB.IsChecked = false;
                H1_RB.IsChecked = false;
                H4_RB.IsChecked = false;
                D_RB.IsChecked = false;
                W_RB.IsChecked = false;
                MN_RB.IsChecked = false;
                timeFrameTB.Text = settings.timeFrame.ToString("hh\\:mm\\:ss");
            }

        }
        private void RB_Checked(object sender, RoutedEventArgs e)
        {
            #region if-else таймфреймов
            if (e.Source == Tick_RB)
            {
                //TODO
                //ticks?   
            }
            else if (e.Source == M1_RB)
            {
                settings.timeFrame = new TimeSpan(0, 0, 1, 0);
            }
            else if (e.Source == M2_RB)
            {
                settings.timeFrame = new TimeSpan(0, 0, 2, 0);
            }
            else if (e.Source == M3_RB)
            {
                settings.timeFrame = new TimeSpan(0, 0, 3, 0);
            }
            else if (e.Source == M4_RB)
            {
                settings.timeFrame = new TimeSpan(0, 0, 4, 0);
            }
            else if (e.Source == M5_RB)
            {
                settings.timeFrame = new TimeSpan(0, 0, 5, 0);
            }
            else if (e.Source == M10_RB)
            {
                settings.timeFrame = new TimeSpan(0, 0, 10, 0);
            }
            else if (e.Source == M15_RB)
            {
                settings.timeFrame = new TimeSpan(0, 0, 15, 0);
            }
            else if (e.Source == M30_RB)
            {
                settings.timeFrame = new TimeSpan(0, 0, 30, 0);
            }
            else if (e.Source == H1_RB)
            {
                settings.timeFrame = new TimeSpan(0, 1, 0, 0);
            }
            else if (e.Source == M4_RB)
            {
                settings.timeFrame = new TimeSpan(0, 4, 0, 0);
            }
            else if (e.Source == D_RB)
            {
                settings.timeFrame = new TimeSpan(1, 0, 0, 0);
            }
            else if (e.Source == W_RB)
            {
                settings.timeFrame = new TimeSpan(7, 0, 0, 0);
            }
            else if (e.Source == MN_RB)
            {
                settings.timeFrame = new TimeSpan(30, 0, 0, 0);
            }
            #endregion
        }

        private void ChartRB_Checked(object sender, RoutedEventArgs e)
        {
            if (e.Source == BasketOneCB)
            {
                settings.chartMode = ChartWindow.WindowSettings.ChartMode.FirstBasket;
            }
            if (e.Source == BasketTwoCB)
            {
                settings.chartMode = ChartWindow.WindowSettings.ChartMode.SecondBasket;
            }
            if (e.Source == ArbitrageCB)
            {
                settings.chartMode = ChartWindow.WindowSettings.ChartMode.Arbitrage;
            }
        }

        private void startStrategyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            candleManager = new CandleManager(StaticData.Connector);
            //TODO: задание инструментов для каждого экземпляра стратегии
            var sec1 = StaticData.CurrPair.FirstBasket;
            var sec2 = StaticData.CurrPair.SecondBasket;
            SelectedStrategy.InitStrategy(this, Chart1, chartCandleElement1, chartCandleElement2, chartTradeElement,
                spreadIndicator,
                chartSpreadElement, movingAverage, chartMovingAverageElement, standardDeviation,
                chartStandardDeviationElement, stationarityIndicator, chartStationarityElement, candleManager, sec1,
                sec2, settings.timeFrame);
            SelectedStrategy.Start();
            //var series = StaticData.Spread.TimeFrame(settings.timeFrame);
            //candleManager.Start(series);
        }

        private void stopStrategyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedStrategy.Stop();
        }

        private void configureStrategyMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectedStrategy.ShowSettings();
        }

        private void timeFrameTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            //SetCandleSource();
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            //SetCandleSource();
        }

        private void dbgGetCandles_Click(object sender, RoutedEventArgs e)
        {
            //var candleManagerTmp = new CandleManager(StaticData.Connector);
            //var series = StaticData.CurrPair.FirstBasket.TimeFrame(settings.timeFrame);
            //var candles = candleManagerTmp.GetCandles<TimeFrameCandle>(series, 100);
            //foreach (var candle in candles)
            //{
            //    Debug.WriteLine(candle.ToString());
            //}
            //candleManagerTmp.Dispose();
            var candles = candleStorage.Load(DateTime.Today);
            foreach (var candle in candles)
            {
                Debug.WriteLine(candle.ToString());
            }
        }

    }
}
