using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StockSharp.Algo.Strategies;
using StockSharp.Xaml.Charting;
using StockSharp.Algo.Indicators;
using StatA.Classes;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.BusinessEntities;
using StockSharp.Logging;
using StockSharp.Localization;
using Ecng.Common;
using System.Diagnostics;
using System.Net;
using Amazon.Runtime.Internal;
using DevExpress.Mvvm.Native;
using DevExpress.Mvvm.POCO;
using StockSharp.Algo.History;
using StockSharp.Algo.History.Russian.Finam;
using StockSharp.Messages;
using Ecng.ComponentModel;
using NPOI.HSSF.Record;
using NPOI.OpenXmlFormats.Dml.Chart;
using SterlingLib;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using Ecng.Reflection;
using StatA.Windows;
using Ecng.Xaml;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using StockSharp.Algo.Strategies.Protective;

namespace StatA.Strategies
{
    public class ArbitrageTimeFrameStrategy : TimeFrameStrategy, IConfigurable
    {
        private UnifiedChartWindow _window;
        private IChart _chart;
        private ChartArea _candlesArea, _indicatorsArea, _indicatorsArea2;
        private ChartCandleElement _candlesElem1, _candlesElem2; //элементы графика, отображающие инструменты(нормированно и линейными графиками в одной области для наглядности)
        private ChartTradeElement _tradesElem;
        private ChartIndicatorElement _statElem;
        private ChartIndicatorElement _spreadElem;
        private ChartIndicatorElement _smaElem;
        private ChartIndicatorElement[] _sdElem;
        private ChartIndicatorElement[] _spreadRangeElem;
        private ICandleManager _candleManager;
        public WeightedIndexSecurity _basket { get; private set; }
        private Security _sec1, _sec2;
        private CandleSeries _series;
        private TimeSpan _timeFrame;
        private Dictionary<DateTimeOffset, Candle> _candles1, _candles2, _candlesArb;
        private DateTimeOffset _candles1_LastTime, _candles2_LastTime, _candlesArb_LastTime;
        private List<MyTrade> _myTrades = new List<MyTrade>();
        public decimal _historicMaximum { get; private set; }
        public decimal _historicMinimum { get; private set; }
        public int BufferSize { get; set; } //буффер свечей (чтобы не было утечки памяти)
        public HistoryLoader.HistorySourceType HistorySource { get; set; } //Источник для загрузки исторических маркет-данных
        public enum RangeCalculationType
        {
            History, CurrentSpread
        }

        public enum StationarityCalculationType
        {
            Spread, SpreadMA
        }



        #region Свободные параметры стратегии
        public int HistoryMonths { get; set; } //число месяцев для вычисления исторического диапазона
        public decimal SdThresh { get; set; } //уровень стандартного отклонения спреда для входа в позицию
        public decimal StatThresh { get; set; } // максимальный p-value теста на стационарность (p-val меньше <=> больше вероятность, что спред стационарен)
        public byte ZoneCount { get; set; } //число зон ско *1, *1.5 *2...
        public RangeCalculationType RangeCalculation { get; set; } //способ расчета середины диапазона, исторический - консервативно, по текущему спреду - более рискованно
        public StationarityCalculationType StationarityCalculation { get; set; } //стационарность графика спреда относительно центра или относительно его МА
        public StationarityCalculationType SpreadDraw { get; set; } //Отображение спреда как есть или с выравниваением по МА
        public decimal RangeChannel { get; set; } //коэфф. изменения границы диапазона, 0-1. Определяет соотношение риск/доходность 
        #endregion
        public SpreadRange _range;
        public SpreadIndicator _spreadInd { get; private set; } //спред между инструментами
        public Stationarity _statInd { get; private set; } //тест Дикки-Фуллера на стационарность, возвращает p-value
        public SimpleMovingAverage _smaInd { get; private set; } //скользящая средняя для спреда
        public StandardDeviation _sdInd { get; private set; } //стандартное отклонение (зоны для скользящей средней)
        public LinearRegSlope _regInd1 { get; private set; } //наклон регрессии для инструмента 1
        public LinearRegSlope _regInd2 { get; private set; } //наклон регрессии для инструмента 2
        int _crossing;
        DateTimeOffset _nextTime;
        public ArbitrageTimeFrameStrategy() : base(new TimeSpan(0, 5, 0))

        {

        }

        public void InitStrategy(UnifiedChartWindow window, IChart chart, ChartCandleElement candlesElem1, ChartCandleElement candlesElem2,
            ChartTradeElement tradesElem,
            SpreadIndicator spreadInd, ChartIndicatorElement spreadElem, SimpleMovingAverage smaInd,
            ChartIndicatorElement smaElem, StandardDeviation sdInd, ChartIndicatorElement[] sdElem, Stationarity statInd,
            ChartIndicatorElement statElem, CandleManager candleManager, Security sec1, Security sec2,
            TimeSpan timeFrame)
        {
            TimeFrame = Interval = timeFrame;
            CommentOrders = true;
            //MyTrades;
            //Orders;
            //StopOrders;
            //Parameters.Add(new StrategyParam<decimal>(this, "", 5));
            //Элементы графика
            _window = window;
            _chart = chart;
            _candlesElem1 = candlesElem1;
            _candlesElem1.DrawStyle = ChartCandleDrawStyles.LineClose;
            _candlesElem1.LineColor = Colors.Red;
            _candlesElem2 = candlesElem2;
            _candlesElem2.DrawStyle = ChartCandleDrawStyles.LineClose;
            _candlesElem2.LineColor = Colors.Blue;
            _tradesElem = tradesElem;
            _statElem = statElem;
            _spreadElem = spreadElem;
            _smaElem = smaElem;
            _sdElem = sdElem;
            //Источник свечей
            _sec1 = sec1;
            _sec2 = sec2;
            _basket = new WeightedIndexSecurity();
            _basket.Weights.Add(_sec1.ToSecurityId(), 1);
            _basket.Weights.Add(_sec2.ToSecurityId(), 1);
            _timeFrame = timeFrame;
            _series = _basket.TimeFrame(timeFrame);
            _candleManager = candleManager; //?
            //Индикаторы
            _spreadInd = spreadInd;
            _statInd = statInd;
            _smaInd = smaInd;
            _sdInd = sdInd;
            _regInd1 = new LinearRegSlope();
            _regInd1.Length = 400; //Регрессия по бОльшему числу свечек, как эквивалент рассчета на бОльшем таймфрейме. (возожно, стоит взять 200*4=800 свечей, если это не ударит по скорости расчета)
            _regInd2 = new LinearRegSlope();
            _regInd2.Length = 400;
            //Переменные
            _candles1 = new Dictionary<DateTimeOffset, Candle>();
            _candles2 = new Dictionary<DateTimeOffset, Candle>();
            _candlesArb = new Dictionary<DateTimeOffset, Candle>();
            //Значения по умолчанию
            HistoryMonths = 12;
            BufferSize = 2000;
            StationarityCalculation = StationarityCalculationType.SpreadMA;
            SpreadDraw = StationarityCalculationType.Spread;
            RangeCalculation = RangeCalculationType.History;
            _spreadRangeElem = new ChartIndicatorElement[5];
            _spreadRangeElem[0] = new ChartIndicatorElement();
            _spreadRangeElem[1] = new ChartIndicatorElement();
            _spreadRangeElem[2] = new ChartIndicatorElement();
            _spreadRangeElem[3] = new ChartIndicatorElement();
            _spreadRangeElem[4] = new ChartIndicatorElement();
            _spreadElem.ChartArea.Elements.Add(_spreadRangeElem[0]);
            _spreadElem.ChartArea.Elements.Add(_spreadRangeElem[1]);
            _spreadElem.ChartArea.Elements.Add(_spreadRangeElem[2]);
            _spreadElem.ChartArea.Elements.Add(_spreadRangeElem[3]);
            _spreadElem.ChartArea.Elements.Add(_spreadRangeElem[4]);

        }
        public ArbitrageTimeFrameStrategy(UnifiedChartWindow window, IChart chart, ChartCandleElement candlesElem1, ChartCandleElement candlesElem2, ChartTradeElement tradesElem,
            SpreadIndicator spreadInd, ChartIndicatorElement spreadElem, SimpleMovingAverage smaInd,
            ChartIndicatorElement smaElem, StandardDeviation sdInd, ChartIndicatorElement[] sdElem, Stationarity statInd,
            ChartIndicatorElement statElem, CandleManager candleManager, Security sec1, Security sec2,
            TimeSpan timeFrame) : base(timeFrame)
        {
            InitStrategy(window, chart, candlesElem1, candlesElem2, tradesElem, spreadInd, spreadElem, smaInd, smaElem, sdInd,
                sdElem, statInd, statElem, candleManager, sec1, sec2, timeFrame);
        }
        protected override void OnStarted()
        {
            /*Для эмуляции поддержки некоторых типов ордеров следует запустить экспорт стаканов, для работы с ордерами через котирования */
            if (StaticData.Connector.RegisteredMarketDepths.Contains(_sec1))
                StaticData.Connector.RegisterMarketDepth(_sec1);
            if (StaticData.Connector.RegisteredMarketDepths.Contains(_sec2))
                StaticData.Connector.RegisterMarketDepth(_sec2);
            //вычисление исторического максимума и минимума (6-24 месяцев), а также середины диапазона + деление диапазона на 4 равных сегмента
            var dateEnd = DateTime.Now;
            var dateStart = dateEnd - TimeSpan.FromDays(30 * HistoryMonths);
            var candles1 = HistoryLoader.LoadHistory(_sec1, dateStart, dateEnd, HistorySource);
            var candles2 = HistoryLoader.LoadHistory(_sec2, dateStart, dateEnd, HistorySource);
            var spreadIndicator = new SpreadIndicator();
            Dictionary<DateTimeOffset, decimal> candleDict1 = new Dictionary<DateTimeOffset, decimal>();
            Dictionary<DateTimeOffset, decimal> candleDict2 = new Dictionary<DateTimeOffset, decimal>();
            _range = new SpreadRange(decimal.MaxValue, decimal.MinValue);
            foreach (var entry1 in candleDict1)
            {
                if (!candleDict2.ContainsKey(entry1.Key)) return;
                Tuple<decimal, decimal> prices;
                prices = new Tuple<decimal, decimal>(entry1.Value, candleDict2[entry1.Key]);
                var spread = spreadIndicator.Process(prices).GetValue<decimal>();
                if (spread < _range.Min)
                    _range.Min = spread;
                if (spread > _range.Max)
                    _range.Max = spread;
            }

            //Подписка на события
            //_candleManager.WhenCandlesFinished(_series).Do(ProcessCandle).Apply(this);
            this.WhenNewMyTrade().Do(trade => _myTrades.Add(trade)).Apply(this);
            //_candleManager.WhenCandles(_series).Do(ProcessCandle).Apply(this);

            //_candleManager.Start(_series);
            //_candleManager.Processing += ProcessCandle;
            //TimeFrameStrategy
            // вычисляем время окончания текущей свечи
            _nextTime = Interval.GetCandleBounds(CurrentTime).Max;
            //base.OnStarted();
            Interval = new TimeSpan(0,0,1);
            MessageBox.Show(Interval.ToString());
            Start();

        }

        private void ProcessCandle(CandleSeries series, Candle candle)
        {
            Debug.WriteLine("Check Candle:" + candle);
            if (ProcessState == ProcessStates.Stopping)
            {
                CancelActiveOrders();
                return;
            }
            this.AddInfoLog(LocalizedStrings.Str3634Params.Put(candle.OpenTime, candle.OpenPrice, candle.HighPrice, candle.LowPrice, candle.ClosePrice, candle.TotalVolume, candle.Security));

            if (candle.Security.Code == _sec1.Code)
            {
                if (candle.OpenTime.DateTime == _candles1_LastTime.DateTime)
                {
                    //Отладка, закоментировать или удалить потом
                    Debug.WriteLine(String.Format("Свеча с тем же временем:{0}; существующая свеча: {1}", candle, _candles1[candle.OpenTime]));
                }
                _candles1[candle.OpenTime] = candle;
                //TODO: Почему приходят новые с тем же временем открытия, тикером, но другими котировками?
                _candles1_LastTime = candle.OpenTime;
            }
            else if (candle.Security.Code == _sec2.Code)
            {
                _candles2[candle.OpenTime] = candle;
                _candles2_LastTime = candle.OpenTime;
            }
            else if (candle.Security.Code == _basket.Code)
            {
                _candlesArb[candle.OpenTime] = candle;
                if (candle.OpenTime.DateTime < _candlesArb_LastTime.DateTime) return;
                if (!((_candles1.ContainsKey(candle.OpenTime) && (_candles2.ContainsKey(candle.OpenTime))))) return;
                _candlesArb_LastTime = candle.OpenTime;
                if (!(_candlesArb_LastTime <= _candles1_LastTime) && (_candlesArb_LastTime <= _candles2_LastTime))
                    throw new Exception("Рассогласование времени получения свечей, результаты вычислений не значимы"); //TODO: случается
                //var volume = volumeIndicator.Process(candle);
                var regSlope1 = _regInd1.Process(candle);
                var regSlope2 = _regInd2.Process(candle);
                var pair = new Tuple<decimal, decimal>(_candles1[candle.OpenTime].ClosePrice,
                    _candles2[candle.OpenTime].ClosePrice);
                var spread = _spreadInd.Process(pair);
                var spreadMA = _smaInd.Process(spread);
                IIndicatorValue spreadStationarity;
                if (StationarityCalculation == StationarityCalculationType.Spread)
                    spreadStationarity = _statInd.Process(spread);
                else
                    spreadStationarity = _statInd.Process(spread.GetValue<decimal>() - spreadMA.GetValue<decimal>());
                var sd = _sdInd.Process(spread);
                var sd0 = new DecimalIndicatorValue(_smaInd, spreadMA.GetValue<decimal>() + sd.GetValue<decimal>());
                var sd1 = new DecimalIndicatorValue(_smaInd, spreadMA.GetValue<decimal>() - sd.GetValue<decimal>());
                var sd2 = new DecimalIndicatorValue(_smaInd, spreadMA.GetValue<decimal>() + 2 * sd.GetValue<decimal>());
                var sd3 = new DecimalIndicatorValue(_smaInd, spreadMA.GetValue<decimal>() - 2 * sd.GetValue<decimal>());
                /*
                Вход в позицию:
                0. стационарность спреда в пределах допустимого
                1. направление позиции совпадает с направлением регрессионной линии (не от центра, а от нейтральной зоны)
                2. спред в зоне 1.5+, позиция зависит от расстояния в СКО, 25%, 50%, 100%
                */
                if (SpreadDraw == StationarityCalculationType.SpreadMA)
                {
                    spread = new DecimalIndicatorValue(_spreadInd,
                        spread.GetValue<decimal>() - spreadMA.GetValue<decimal>());
                    sd0 = new DecimalIndicatorValue(_smaInd, sd0.Value + spreadMA.GetValue<decimal>());
                    sd1 = new DecimalIndicatorValue(_smaInd, sd1.Value + spreadMA.GetValue<decimal>());
                    sd2 = new DecimalIndicatorValue(_smaInd, sd2.Value + spreadMA.GetValue<decimal>());
                    sd3 = new DecimalIndicatorValue(_smaInd, sd3.Value + spreadMA.GetValue<decimal>());
                    spreadMA = new DecimalIndicatorValue(_smaInd, 0);
                }
                var volCoeff = CalcPosition(spreadMA.GetValue<decimal>(), spread.GetValue<decimal>(),
                    spreadStationarity.GetValue<decimal>(), _range, sd.GetValue<decimal>());
                var vol = Volume * volCoeff;
                if (vol.Abs() != Position.Abs())
                {
                    if (vol.Abs() < Position.Abs())
                    {
                        if (volCoeff.Abs() <= 0.25m)
                        {
                            CancelActiveOrders();
                            this.ClosePositionByQuoting();
                            var orderVolume = Position.Abs();
                            var orderDirection = orderVolume.Sign() > 0 ? Sides.Buy : Sides.Sell;
                            var orderPrice = candle.ClosePrice + ((orderDirection == Sides.Buy ? Security.PriceStep : -Security.PriceStep) ?? 1);
                            if (orderVolume > Security.PriceStep)
                                RegisterOrder(this.CreateOrder(orderDirection, orderPrice, orderVolume));
                            //RegisterOrder(this.CreateOrder());
                        }
                    }
                    if (vol.Abs() > Position.Abs())
                    {
                        CancelActiveOrders();
                        //if (Position.Sign() != vol.Sign()) 
                        var orderVolume = vol.Abs() - Position.Abs();
                        var orderDirection = orderVolume.Sign() > 0 ? Sides.Buy : Sides.Sell;
                        var orderPrice = candle.ClosePrice + ((orderDirection == Sides.Buy ? Security.PriceStep : -Security.PriceStep) ?? 1);
                        if (orderVolume > Security.PriceStep)
                            RegisterOrder(this.CreateOrder(orderDirection, orderPrice, orderVolume));
                    }

                }


                var data = new ChartDrawData();
                data
                    .Group(candle.OpenTime)
                    .Add(_candlesElem1, _candles1[candle.OpenTime])
                    .Add(_candlesElem2, NormedCandle(_candles2[candle.OpenTime]))
                    .Add(_spreadElem, spread)
                    .Add(_smaElem, spreadMA)
                    .Add(_sdElem[0], sd0)
                    .Add(_sdElem[1], sd1)
                    .Add(_sdElem[2], sd2)
                    .Add(_sdElem[3], sd3)
                    .Add(_statElem, spreadStationarity)
                    .Add(_spreadRangeElem[0], _range.Max)
                    .Add(_spreadRangeElem[1], _range.Min)
                    .Add(_spreadRangeElem[2], _range.GetProcentile(0.50m))
                    .Add(_spreadRangeElem[3], _range.GetProcentile(0.25m))
                    .Add(_spreadRangeElem[4], _range.GetProcentile(0.75m));
                _chart.Draw(data);

                Debug.WriteLine("Draw candle:" + candle.OpenTime);
                //_window.GuiAsync(() => _window.Chart1.Draw(data));
                //_window.GuiAsync(() => _chart.Draw(data));
                //data.DoDispose();
            }
        }

        Candle NormedCandle(Candle candle)
        {
            //Нормирование свечи второго инструмента по тем же правилам, что и при рассчете спреда, для отображения графиков.
            var cndl = candle.Clone();
            cndl.OpenPrice *= _spreadInd.b;
            cndl.HighPrice *= _spreadInd.b;
            cndl.LowPrice *= _spreadInd.b;
            cndl.ClosePrice *= _spreadInd.b;
            return cndl;
        }

        public void ShowSettings()
        {
            var ssd = new TimeFrameStrategySettingsDialog(this);
            ssd.ShowDialog();
        }

        public decimal CalcPosition(decimal ma, decimal spread, decimal stat, SpreadRange range, decimal sd)
        {
            if (stat > StatThresh) return 0;

            if (spread < range.GetProcentile(0.25m))
            {
                if (spread < ma - sd * 2) return 1;
                else if (spread < ma - sd * 1.5m) return 0.75m;
            }
            if (spread > range.GetProcentile(0.75m))
            {
                if (spread > ma + sd * 2) return -1;
                else if (spread < ma - sd * 1.5m) return 0.75m;
            }
            if (spread > range.GetProcentile(0.25m) && ((spread < range.GetProcentile(0.50m))))
                if (spread < ma - sd * 1.5m) return 0.5m;
            if (spread < range.GetProcentile(0.75m) && ((spread > range.GetProcentile(0.50m))))
                if (spread > ma + sd * 1.5m) return -0.5m;
            return -CrossingHappened(ma, spread) * 0.25m;
        }

        int CrossingHappened(decimal ma, decimal spread)
        {
            var diff = spread - ma;
            int crossing = 0;
            if (diff > 0 + ma / 100) crossing = 1;
            if (diff < 0 - ma / 100) crossing = -1;
            if (crossing == _crossing) return 0;
            else
            {
                _crossing = crossing;
                return crossing;
            }

        }

        protected override ProcessResults OnProcess()
        {
            MessageBox.Show("0");
            // если стратегия в процессе остановки
            if (base.ProcessState == ProcessStates.Stopping)
            {
                CancelActiveOrders();
                return ProcessResults.Stop;
            }
            // событие обработки торговой стратегии вызвалось в первый раз, что раньше, чем окончания текущей 5-минутки.
            if (Connector.CurrentTime < _nextTime)
            {
                // возвращаем ProcessResults.Continue, так как наш алгоритм еще не закончил свою работу, а просто ожидает следующего вызова.
                return ProcessResults.Continue;
            }
            // получаем сформированную свечу
            var candle = _candleManager.GetTimeFrameCandle(_series, _nextTime - TimeFrame);

            // если свечи не существует (не было ни одной сделке в тайм-фрейме), то ждем окончания следующей свечи.
            if (candle == null)
            {
                // если прошло больше 10 секунд с момента окончания свечи, а она так и не появилась,
                // значит сделок в прошедшей 5-минутке не было, и переходим на следующую свечу
                if ((Connector.CurrentTime - _nextTime) > TimeSpan.FromSeconds(10))
                    _nextTime = TimeFrame.GetCandleBounds(Connector.CurrentTime).Max;

                return ProcessResults.Continue;
            }

            _nextTime += TimeFrame;
            ProcessCandle(_series, candle);
            return ProcessResults.Continue;
        }


    }
}
