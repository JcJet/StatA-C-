using System;
using System.Collections.Generic;
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
using System.Diagnostics;
using System.IO;
using Amazon.Runtime.Internal.Util;
using DevExpress.Xpf.Bars;
using Ecng.Common;
using Ecng.Xaml;
using MoreLinq;
using StatA.Classes;
using SterlingLib;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Indicators;
using StockSharp.Algo.Storages;
using StockSharp.Messages;
using StockSharp.Xaml.Charting;

namespace StatA.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChartWindow.xaml
    /// </summary>
    public partial class ChartWindow : Window
    {
        public ChartArea candlesArea, indicatorsArea, indicatorsArea2, IndicatorsArea3;
        public ChartCandleElement chartCandleElement;
        private ChartIndicatorElement chartVolumeElement;
        private ChartIndicatorElement chartStationarityElement;
        private ChartIndicatorElement chartSpreadElement;
        private ChartIndicatorElement chartMovingAverageElement;
        private ChartIndicatorElement[] chartStandardDeviationElement;
        private ChartIndicatorElement chartStochElement;
        public WindowSettings settings;
        private IMarketDataStorage<CandleMessage> candleStorage;
        private VolumeIndicator volumeIndicator;
        private StochasticOscillator stochasticOscillator;
        //private ArbitrageIndicator arbitrageIndicator; //ComplexIndicatorValue - ошибка в метоже Draw() графика... Также как и ADX (TODO: разобраться, как разместить комплексный индикатор)
        private Stationarity stationarityIndicator;
        private SpreadIndicator spreadIndicator;
        private SimpleMovingAverage movingAverage;
        private StandardDeviation standardDeviation;
        //private AverageDirectionalIndex adxIndicator;
        private DateTimeOffset lastCandleTime;
        CandleManager candleManager;
        /*Для сохранения в хранилище проще List<Candle>, но данные переменные применяются в алгоритме, где требуется извлекать свечку по дате из сохраненных ранее списков, циклы
        foreach крайне нежелательны из-за понижения производительности*/
        Dictionary<DateTimeOffset, Candle> Candles1, Candles2, CandlesArb; 
        DateTimeOffset Candles1_LastTime, Candles2_LastTime, CandlesArb_LastTime;
        [Serializable]
        public class WindowSettings
        {
            public TimeSpan timeFrame { get; set; }
            public enum ChartMode
            {
                FirstBasket, SecondBasket, Arbitrage
            }
            public ChartMode chartMode { get; set; }

            public WindowSettings()
            {
                chartMode = ChartMode.FirstBasket;
                timeFrame = new TimeSpan(0,0,5,0);
            }
        }

        public ChartWindow()
        {
            //Инициализация настроек окна
            settings = new WindowSettings();
            InitializeComponent();
            this.DataContext = settings;
            Candles1 = new Dictionary<DateTimeOffset,Candle>();
            Candles2 = new Dictionary<DateTimeOffset, Candle>();
            CandlesArb = new Dictionary<DateTimeOffset, Candle>();
        }

        void SetCandleSource()
        {
            //var candleManager = new CandleManager(StaticData.Connector);
            //foreach (var seriesItem in candleManager.Series)
            //{
            //  candleManager.Stop(seriesItem);
            //}
            if (StaticData.Connector == null) return;
            if (settings.chartMode == WindowSettings.ChartMode.FirstBasket)
            {
                lastCandleTime = DateTimeOffset.MinValue;
                Chart1.ClearAreas();
                Chart1.IsAutoRange = true;
                //Инициализация индикаторов
                volumeIndicator = new VolumeIndicator();
                stochasticOscillator = new StochasticOscillator(); //для проверки реализации индикатора с диапазоном
                //Инициализация элементов графиков
                chartVolumeElement = new ChartIndicatorElement();
                chartVolumeElement.DrawStyle = ChartIndicatorDrawStyles.Histogram;
                chartCandleElement = new ChartCandleElement();
                chartStochElement = new ChartIndicatorElement();
                //Инициализация области графика
                candlesArea = new ChartArea();
                indicatorsArea = new ChartArea();
                indicatorsArea2 = new ChartArea();
                IndicatorsArea3 = new ChartArea();
                //Добавление областей к графику
                Chart1.Areas.Add(candlesArea);
                Chart1.Areas.Add(indicatorsArea);
                Chart1.Areas.Add(indicatorsArea2);
                Chart1.Areas.Add(IndicatorsArea3);
                //Добавление элементов к областям графика
                candlesArea.Elements.Add(chartCandleElement);
                indicatorsArea.Elements.Add(chartVolumeElement);
                IndicatorsArea3.Elements.Add(chartStochElement);
                //Инициализация CandleManager и подписка на событие получения свечи
                candleManager = new CandleManager(StaticData.Connector);
                candleManager.Processing += DrawCandle;
                var series = StaticData.CurrPair.FirstBasket.TimeFrame(settings.timeFrame); //для корзины
                //Запуск получения свечей
                candleManager.Start(series);
            }
            else if (settings.chartMode == WindowSettings.ChartMode.SecondBasket)
            {
                
            }
            else if (settings.chartMode == WindowSettings.ChartMode.Arbitrage)
            {
                Candles1.Clear();
                Candles2.Clear();
                CandlesArb.Clear();
                lastCandleTime = DateTimeOffset.MinValue;
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
                chartSpreadElement = new ChartIndicatorElement();
                chartMovingAverageElement = new ChartIndicatorElement();
                chartMovingAverageElement.Color = Colors.Cyan;
                chartStandardDeviationElement = new ChartIndicatorElement[]
                {
                    new ChartIndicatorElement() {Color = Colors.Orange}, //SD
                    new ChartIndicatorElement() {Color = Colors.Orange}, //-SD
                    new ChartIndicatorElement() {Color = Colors.Red}, //2*SD
                    new ChartIndicatorElement() {Color = Colors.Red}, //-2*SD
                };
                chartCandleElement = new ChartCandleElement();
                //Инициализация области графика
                candlesArea = new ChartArea();
                indicatorsArea = new ChartArea();
                indicatorsArea2 = new ChartArea();
                //Добавление областей к графику
                Chart1.Areas.Add(candlesArea);
                Chart1.Areas.Add(indicatorsArea);
                Chart1.Areas.Add(indicatorsArea2);
                //Добавление элементов к областям графика
                candlesArea.Elements.Add(chartCandleElement);
                indicatorsArea.Elements.Add(chartSpreadElement);
                indicatorsArea.Elements.Add(chartMovingAverageElement);
                indicatorsArea.Elements.Add(chartStandardDeviationElement[0]);
                indicatorsArea.Elements.Add(chartStandardDeviationElement[1]);
                indicatorsArea.Elements.Add(chartStandardDeviationElement[2]);
                indicatorsArea.Elements.Add(chartStandardDeviationElement[3]);
                indicatorsArea2.Elements.Add(chartStationarityElement);
                //Инициализация CandleManager и подписка на событие получения свечи
                candleManager = new CandleManager(StaticData.Connector);
                candleManager.Processing += DrawCandle;
                var series = StaticData.Spread.TimeFrame(settings.timeFrame); //для корзины
                //Запуск получения свечей
                candleManager.Start(series);
            }
        }
        private void DrawCandle(CandleSeries series, Candle candle)
        {
            if (candle.State != CandleStates.Finished)
                return;
            //if (candle.OpenTime < lastCandleTime) return; //т.к. потом снова приходят старые свечи по этому инструменту(почему?), вызывая исключение на графике.
            lastCandleTime = candle.OpenTime;
            File.AppendAllText("CandlesRecieved.txt",
                string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8}", candle.CloseTime, candle.Security, candle.OpenTime,
                    candle.HighPrice, candle.LowPrice, candle.ClosePrice, candle.CloseVolume, candle.State,
                    candle.Security.Code)); //DEBUG
            if (settings.chartMode == WindowSettings.ChartMode.Arbitrage)
            {
                //CalcWeights(StaticData.Spread);
                if (candle.Security.Code == StaticData.CurrPair.FirstBasket.Code)
                {
                    if (candle.OpenTime.DateTime == Candles1_LastTime.DateTime)
                    {
                        //Отладка, закоментировать или удалить потом
                        Debug.WriteLine(String.Format("Свеча с тем же временем:{0}; существующая свеча: {1}", candle, Candles1[candle.OpenTime]));
                    }
                    Candles1[candle.OpenTime] = candle;
                    //TODO: Почему приходят новые с тем же временем открытия, тикером, но другими котировками?
                    Candles1_LastTime = candle.OpenTime;
                }
                else if (candle.Security.Code == StaticData.CurrPair.SecondBasket.Code)
                {
                    Candles2[candle.OpenTime] = candle;
                    Candles2_LastTime = candle.OpenTime;
                }
                else if (candle.Security.Code == StaticData.Spread.Code)
                {
                    CandlesArb[candle.OpenTime] = candle;
                    if (candle.OpenTime.DateTime < CandlesArb_LastTime.DateTime) return; 
                    CandlesArb_LastTime = candle.OpenTime;
                    if (!(CandlesArb_LastTime <= Candles1_LastTime) && (CandlesArb_LastTime <= Candles2_LastTime))
                        throw new Exception("Рассогласование времени получения свечей, результаты вычислений не значимы");
                    //var volume = volumeIndicator.Process(candle);
                    var pair = new Tuple<decimal, decimal>(Candles1[candle.OpenTime].ClosePrice,
                        Candles2[candle.OpenTime].ClosePrice);
                    var spread = spreadIndicator.Process(pair);
                    var spreadMA = movingAverage.Process(spread);
                    var spreadStationarity = stationarityIndicator.Process(spread);
                    var sd = standardDeviation.Process(spread);
                    var sd0 = new DecimalIndicatorValue(movingAverage, spreadMA.GetValue<decimal>() + sd.GetValue<decimal>());
                    var sd1 = new DecimalIndicatorValue(movingAverage, spreadMA.GetValue<decimal>() - sd.GetValue<decimal>());
                    var sd2 = new DecimalIndicatorValue(movingAverage, spreadMA.GetValue<decimal>() + 2*sd.GetValue<decimal>());
                    var sd3 = new DecimalIndicatorValue(movingAverage, spreadMA.GetValue<decimal>() - 2*sd.GetValue<decimal>());
                    var data = new ChartDrawData();
                    data
                        .Group(candle.OpenTime)
                        .Add(chartCandleElement, candle)
                        .Add(chartSpreadElement, spread)
                        .Add(chartMovingAverageElement, spreadMA)
                        .Add(chartStandardDeviationElement[0], sd0)
                        .Add(chartStandardDeviationElement[1], sd1)
                        .Add(chartStandardDeviationElement[2], sd2)
                        .Add(chartStandardDeviationElement[3], sd3)
                        .Add(chartStationarityElement, spreadStationarity);
                    this.GuiAsync(() => StaticData.chartWindow.Chart1.Draw(data));
                    //Chart1.Draw(data);
                    data.DoDispose();
                }
                candle.DoDispose();
            }
            if (settings.chartMode == WindowSettings.ChartMode.FirstBasket)
            {
                if (candle.Security.Code == StaticData.CurrPair.FirstBasket.Code)
                    //т.к приходят отдельно свечи со всех инструментов корзины, помимо взвешенной суммы
                {
                    //TODO: Перепроверить правильность отрисовки графика корзины (на первый взгляд все как надо, но есть сомнения из-за множества предыдущих багов
                    var volume = volumeIndicator.Process(candle);
                    var so = stochasticOscillator.Process(candle);
                    var data = new ChartDrawData();
                    data
                        .Group(candle.OpenTime)
                        .Add(chartCandleElement, candle)
                        .Add(chartVolumeElement, volume);
                    this.GuiAsync(() => StaticData.chartWindow.Chart1.Draw(data));
                    //Chart1.Draw(data);
                    data.DoDispose();
                }
                candle.DoDispose();
            }
        }

        void CalcWeights(WeightedIndexSecurity sec)
        {
            //TODO: coeff - lm()
            //IIndicatorValue val = new Tuple<decimal>();
            //linearRegression;
            decimal coeff = 0.5M;
            StaticData.Spread.Weights[StaticData.CurrPair.SecondBasket.ToSecurityId()] = coeff;
            //Стоит ли перерисовывать график? (вопрос стратегии)
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var ctfW = new CustomTFWindow();
            ctfW.ShowDialog();
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
                settings.chartMode = WindowSettings.ChartMode.FirstBasket;
            }
            if (e.Source == BasketTwoCB)
            {
                settings.chartMode = WindowSettings.ChartMode.SecondBasket;
            }
            if (e.Source == ArbitrageCB)
            {
                settings.chartMode = WindowSettings.ChartMode.Arbitrage;
            }
        }
        private void timeFrameTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            //SetCandleSource();
        }

        private void RefreshBtn_Click(object sender, RoutedEventArgs e)
        {
            SetCandleSource();
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
