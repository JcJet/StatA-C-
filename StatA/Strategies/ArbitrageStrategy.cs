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
using StockSharp.Algo.Strategies.Protective;

namespace StatA.Strategies
{
    public class ArbitrageStrategy : Strategy, IConfigurable
    {
        private UnifiedChartWindow _window;
        private IChart _chart;
        private ChartArea _candlesArea, _indicatorsArea, _indicatorsArea2;
        private ChartCandleElement _candlesElem1, _candlesElem2; //элементы графика, отображающие инструменты(нормированно и линейными графиками в одной области для наглядности)
        private ChartTradeElement _tradesElem;
        private ChartOrderElement _orderElem1, _orderElem2, _orderElem3; //эмементы графика для отображения сделок по первой и второй арбитражной группе, а также для индикатора спреда
        List<Order> _orderList1, _orderList2; 
        private ChartIndicatorElement _statElem;
        private ChartIndicatorElement _spreadElem;
        private ChartIndicatorElement _smaElem;
        private ChartIndicatorElement[] _sdElem;
        private ChartIndicatorElement[] _spreadRangeElem;
        decimal curDemoPos;
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
        int _crossing { get; set; }
        IncrementalIdGenerator genID;

        public ArbitrageStrategy()
        {
            //Значения по умолчанию
            HistoryMonths = 12;
            BufferSize = 2000;
            ZoneCount = 2;
            SdThresh = 1;
            StatThresh = 0.5m;
            RangeChannel = 1;
            StationarityCalculation = StationarityCalculationType.SpreadMA;
            SpreadDraw = StationarityCalculationType.Spread;
            RangeCalculation = RangeCalculationType.History;
        }

        public void InitStrategy(UnifiedChartWindow window, IChart chart, ChartCandleElement candlesElem1, ChartCandleElement candlesElem2,
            ChartTradeElement tradesElem,
            SpreadIndicator spreadInd, ChartIndicatorElement spreadElem, SimpleMovingAverage smaInd,
            ChartIndicatorElement smaElem, StandardDeviation sdInd, ChartIndicatorElement[] sdElem, Stationarity statInd,
            ChartIndicatorElement statElem, CandleManager candleManager, Security sec1, Security sec2,
            TimeSpan timeFrame)
        {
            CommentOrders = true;
            genID = new IncrementalIdGenerator();
            //MyTrades;
            //Orders;
            //StopOrders;
            //Parameters.Add(new StrategyParam<decimal>(this,"", 5));
            //Элементы графика
            _window = window;
            _chart = chart;
            _candlesElem1 = candlesElem1;
            _candlesElem2 = candlesElem2;
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
            _basket.Weights.Add(_sec2.ToSecurityId(), -1);
            this.Security = _basket;
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
            //Цвета и легенда
            _chart.Areas.ForEach((a) => a.YAxises.First().AutoRange = true);
            _candlesElem1.DrawStyle = ChartCandleDrawStyles.LineClose;
            _candlesElem1.LineColor = Colors.Red;
            _candlesElem1.Title = _sec1.Code;
            _candlesElem2.DrawStyle = ChartCandleDrawStyles.LineClose;
            _candlesElem2.LineColor = Colors.Blue;
            _candlesElem2.Title = _sec2.Code;
            _statElem.Title = "Нестационарность спреда";
            _spreadElem.Title = "Спред";
            _smaElem.Title = "SMA спреда";
            _sdElem[0].Title = "СКО";
            _sdElem[0].Color = Colors.Orange;
            _sdElem[1].Title = "СКО";
            _sdElem[1].Color = Colors.Orange;
            _sdElem[2].Title = "СКО*2";
            _sdElem[2].Color = Colors.Red;
            _sdElem[3].Title = "СКО*2";
            _sdElem[3].Color = Colors.Red;
            _spreadRangeElem[0].Title = "Максимум";
            _spreadRangeElem[1].Title = "Минимум";
            _spreadRangeElem[2].Title = "Середина диапазона";
            _spreadRangeElem[3].Title = "-25%";
            _spreadRangeElem[4].Title = "+25%";
            _statElem.Color = (Color)ColorConverter.ConvertFromString("#FF446EA2");
            curDemoPos = 0;
            _orderElem1 = new ChartOrderElement();
            _orderElem1.Title = sec1.Code + "(заявки)";
            _orderElem2 = new ChartOrderElement();
            _orderElem2.Title = sec2.Code + "(заявки)";
            _orderElem2.BuyColor = _orderElem2.BuyStrokeColor;
            _orderElem2.SellColor = _orderElem2.SellStrokeColor;
            _orderElem3 = new ChartOrderElement();
            _orderElem3.Title = "Сигнал";
            _candlesElem1.ChartArea.Elements.Add(_orderElem1);
            _candlesElem1.ChartArea.Elements.Add(_orderElem2);
            _spreadElem.ChartArea.Elements.Add(_orderElem3);
            _orderList1 = new List<Order>();
            _orderList2 = new List<Order>();
        }
        public ArbitrageStrategy(UnifiedChartWindow window, IChart chart, ChartCandleElement candlesElem1, ChartCandleElement candlesElem2, ChartTradeElement tradesElem,
            SpreadIndicator spreadInd, ChartIndicatorElement spreadElem, SimpleMovingAverage smaInd,
            ChartIndicatorElement smaElem, StandardDeviation sdInd, ChartIndicatorElement[] sdElem, Stationarity statInd,
            ChartIndicatorElement statElem, CandleManager candleManager, Security sec1, Security sec2,
            TimeSpan timeFrame)
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
            var dateStart = dateEnd - TimeSpan.FromDays(30*HistoryMonths);
            var candles1 = HistoryLoader.LoadHistory(_sec1, dateStart, dateEnd, HistorySource);
            var candles2 = HistoryLoader.LoadHistory(_sec2, dateStart, dateEnd, HistorySource);
            var spreadIndicator = new SpreadIndicator();
            _range = new SpreadRange(decimal.MaxValue, decimal.MinValue);
            foreach (var entry1 in candles1)
            {
                if (!candles2.ContainsKey(entry1.Key)) return;
                Tuple<decimal, decimal> prices;
                prices = new Tuple<decimal, decimal>(entry1.Value.ClosePrice, candles2[entry1.Key].ClosePrice);
                var spread = spreadIndicator.Process(prices).GetValue<decimal>();
                if (spreadIndicator.IsFormed)
                {
                    if (spread < _range.Min)
                        _range.Min = spread;
                    if (spread > _range.Max)
                        _range.Max = spread;
                }
            }
            _range.Max *= RangeChannel;
            _range.Min *= RangeChannel;

            //Подписка на события
            //_candleManager.WhenCandlesFinished(_series).Do(ProcessCandle).Apply(this);
            this.WhenNewMyTrade().Do(trade => _myTrades.Add(trade)).Apply(this);
            //_candleManager.WhenCandles(_series).Do(ProcessCandle).Apply(this);
            _candleManager.Start(_series);
            _candleManager.Processing += ProcessCandle;
            //base.OnStarted();
        }

        private void ProcessCandle(CandleSeries series, Candle candle)
        {
            Debug.WriteLine("Check Candle:" + candle + " ");
            if (ProcessState == ProcessStates.Stopping)
            {
                CancelActiveOrders();
                return;
            }
            this.AddInfoLog(LocalizedStrings.Str3634Params.Put(candle.OpenTime, candle.OpenPrice, candle.HighPrice, candle.LowPrice, candle.ClosePrice, candle.TotalVolume, candle.Security));

            if (candle.Security.Code == _sec1.Code)
            {
                _candles1[candle.OpenTime] = candle;
                //TODO: Почему приходят новые с тем же временем открытия, тикером, но другими котировками?
                _candles1_LastTime = candle.OpenTime;
                CheckTimes();
            }
            else if (candle.Security.Code == _sec2.Code)
            {
                _candles2[candle.OpenTime] = candle;
                _candles2_LastTime = candle.OpenTime;
                CheckTimes();
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

        void CheckTimes()
        {
            if ((_candles1_LastTime >= _candlesArb_LastTime) && (_candles2_LastTime >= _candlesArb_LastTime))
            {
                var minTime = _candles1_LastTime <= _candles2_LastTime ? _candles1_LastTime : _candles2_LastTime;
                if ((_candles1.ContainsKey(minTime) && (_candles2.ContainsKey(minTime))))
                {
                    _candlesArb_LastTime = minTime;
                    Process();
                }
            }
        }
        void Process()
        {
            var candle1 = _candles1[_candlesArb_LastTime];
            var candle2 = _candles2[_candlesArb_LastTime];
            var regSlope1 = _regInd1.Process(candle1);
            var regSlope2 = _regInd2.Process(candle2);
            var pair = new Tuple<decimal, decimal>(candle1.ClosePrice, candle2.ClosePrice);
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
            //var volCoeff = CalcPosition(spreadMA.GetValue<decimal>(), spread.GetValue<decimal>(),
            //    spreadStationarity.GetValue<decimal>(), _range, sd.GetValue<decimal>());
            //var vol = Volume * volCoeff;
            //if (vol.Abs() != Position.Abs())
            //{
            //    if (vol.Abs() < Position.Abs())
            //    {
            //        if (volCoeff.Abs() <= 0.25m)
            //        {
            //            CancelActiveOrders();
            //            this.ClosePositionByQuoting();
            //            var orderVolume = Position.Abs();
            //            var orderDirection = orderVolume.Sign() > 0 ? Sides.Buy : Sides.Sell;
            //            var orderPrice = candle.ClosePrice + ((orderDirection == Sides.Buy ? Security.PriceStep : -Security.PriceStep) ?? 1);
            //            if (orderVolume > Security.PriceStep)
            //                RegisterOrder(this.CreateOrder(orderDirection, orderPrice, orderVolume));
            //            //RegisterOrder(this.CreateOrder());
            //        }
            //    }
            //    if (vol.Abs() > Position.Abs())
            //    {
            //        CancelActiveOrders();
            //        //if (Position.Sign() != vol.Sign()) 
            //        var orderVolume = vol.Abs() - Position.Abs();
            //        var orderDirection = orderVolume.Sign() > 0 ? Sides.Buy : Sides.Sell;
            //        var orderPrice = candle.ClosePrice + ((orderDirection == Sides.Buy ? Security.PriceStep : -Security.PriceStep) ?? 1);
            //        if (orderVolume > Security.PriceStep)
            //            RegisterOrder(this.CreateOrder(orderDirection, orderPrice, orderVolume));
            //    }

            //}
            var rangeMax = new DecimalIndicatorValue(_spreadInd, _range.Max);
            var rangeMin = new DecimalIndicatorValue(_spreadInd, _range.Min);
            var range25 = new DecimalIndicatorValue(_spreadInd, _range.GetProcentile(0.25m));
            var range50 = new DecimalIndicatorValue(_spreadInd, _range.GetProcentile(0.50m));
            var range75 = new DecimalIndicatorValue(_spreadInd, _range.GetProcentile(0.75m));
            var pos = CalcPosition(spreadMA.GetValue<decimal>(), spread.GetValue<decimal>(), spreadStationarity.GetValue<decimal>(), _range, sd.GetValue<decimal>());
            Order order1 = null, order2 = null, order2normed = null, order3 = null;
            if (((pos >= 0.25m) || (pos <= -0.25m)) && ((_spreadInd.IsFormed)&&(_statInd.IsFormed)))
            {
                if (curDemoPos >= 0)
                {
                    if (curDemoPos < pos)
                    {
                        order1 = this.CreateOrder(Sides.Buy, candle1.ClosePrice, Volume * (pos - curDemoPos));
                        order1.TransactionId = genID.GetNextId();
                        _orderList1.Add(order1);
                        order2 = this.CreateOrder(Sides.Sell, candle2.ClosePrice, Volume * (pos - curDemoPos));
                        order2.TransactionId = genID.GetNextId();
                        _orderList2.Add(order2);
                        order3 = order1.ReRegisterClone(spread.GetValue<decimal>());
                        order3.TransactionId = genID.GetNextId();
                        curDemoPos = pos;
                    }
                    else if ((curDemoPos > pos) && (pos <= 0))
                    {
                        order1 = this.CreateOrder(Sides.Sell, candle1.ClosePrice, Volume * (curDemoPos-pos));
                        order1.TransactionId = genID.GetNextId();
                        _orderList1.Add(order1);
                        order2 = this.CreateOrder(Sides.Buy, candle2.ClosePrice, Volume * (curDemoPos - pos));
                        order2.TransactionId = genID.GetNextId();
                        _orderList2.Add(order2);
                        order3 = order1.ReRegisterClone(spread.GetValue<decimal>());
                        order3.TransactionId = genID.GetNextId();
                        curDemoPos = pos;
                    }

                }
                else if (curDemoPos <= 0)
                {
                    if (curDemoPos > pos)
                    {
                        order1 = this.CreateOrder(Sides.Sell, candle1.ClosePrice, Volume * (pos - curDemoPos));
                        order1.TransactionId = genID.GetNextId();
                        _orderList1.Add(order1);
                        order2 = this.CreateOrder(Sides.Buy, candle2.ClosePrice, Volume * (pos - curDemoPos));
                        order2.TransactionId = genID.GetNextId();
                        _orderList2.Add(order2);
                        order3 = order1.ReRegisterClone(spread.GetValue<decimal>());
                        order3.TransactionId = genID.GetNextId();
                        curDemoPos = pos;
                    }
                    else if ((curDemoPos < pos) && (pos >= 0))
                    {
                        order1 = this.CreateOrder(Sides.Buy, candle1.ClosePrice, Volume * (curDemoPos + pos));
                        order1.TransactionId = genID.GetNextId();
                        _orderList1.Add(order1);
                        order2 = this.CreateOrder(Sides.Sell, candle2.ClosePrice, Volume * (curDemoPos + pos));
                        order2.TransactionId = genID.GetNextId();
                        _orderList2.Add(order2);
                        order3 = order1.ReRegisterClone(spread.GetValue<decimal>());
                        order3.TransactionId = genID.GetNextId();
                        curDemoPos = pos;
                    }
                }
            }
            var data = new ChartDrawData();
            if (!order2.IsNull())
            {
                order2normed = order2.ReRegisterClone(order2.Price * _spreadInd.b);
                order2normed.TransactionId = genID.GetNextId();
            }
            data
                .Group(candle1.OpenTime)
                .Add(_candlesElem1, _candles1[candle1.OpenTime])
                .Add(_candlesElem2, NormedCandle(_candles2[candle1.OpenTime]))
                .Add(_spreadElem, spread)
                .Add(_smaElem, spreadMA)
                .Add(_sdElem[0], sd0)
                .Add(_sdElem[1], sd1)
                .Add(_sdElem[2], sd2)
                .Add(_sdElem[3], sd3)
                .Add(_statElem, spreadStationarity)
                .Add(_spreadRangeElem[0], rangeMax)
                .Add(_spreadRangeElem[1], rangeMin)
                .Add(_spreadRangeElem[2], range50)
                .Add(_spreadRangeElem[3], range25)
                .Add(_spreadRangeElem[4], range75)
                .Add(_orderElem1, order1)
                .Add(_orderElem2, order2normed)
                .Add(_orderElem3, order3);
            _chart.Draw(data);
            Debug.WriteLine("Draw candle:" + candle1.OpenTime);
            //_window.GuiAsync(() => _window.Chart1.Draw(data));
            //_window.GuiAsync(() => _chart.Draw(data));
            //data.DoDispose();
        }
        public void ShowSettings()
        {
            var ssd = new StrategySettingsDialog(this);
            ssd.ShowDialog();
        }

        public decimal CalcPosition(decimal ma, decimal spread, decimal stat, SpreadRange range, decimal sd)
        {
            if (stat > StatThresh) return 0;

            //if (spread < range.GetProcentile(0.25m))
            {
                if (spread < ma - sd*2) return 1;
                else if (spread < ma - sd * 1.5m) return 0.75m;
            }
           // if (spread > range.GetProcentile(0.75m))
            {
                if (spread > ma + sd*2) return -1;
                else if (spread < ma - sd * 1.5m) return 0.75m;
            }
            //if (spread > range.GetProcentile(0.25m) && ((spread < range.GetProcentile(0.50m))))
                if (spread < ma - sd * 1.5m) return 0.5m;
            //if (spread < range.GetProcentile(0.75m) && ((spread > range.GetProcentile(0.50m))))
                if (spread > ma + sd * 1.5m) return -0.5m;
            return -CrossingHappened(ma, spread)*0.25m;
        }

        int CrossingHappened(decimal ma, decimal spread)
        {
            var diff = spread - ma;
            int crossing = 0;
            if (diff > 0 + ma/100) crossing = 1;
            if (diff < 0 - ma/100) crossing = -1;
            if (crossing == _crossing) return 0;
            else
            {
                _crossing = crossing;
                return crossing;
            }

        }
    }
}
