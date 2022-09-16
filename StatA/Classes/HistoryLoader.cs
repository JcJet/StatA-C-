using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Ecng.ComponentModel;
using StockSharp.Algo;
using StockSharp.Algo.Candles;
using StockSharp.Algo.History;
using StockSharp.BusinessEntities;
using StockSharp.Messages;
using StockSharp.Algo.History.Russian.Finam;
using StockSharp.Algo.Storages;
using Ecng.Configuration;
using StockSharp.Algo.History.Russian;

namespace StatA.Classes
{
    public static class HistoryLoader
    {
        public enum HistorySourceType
        {
            Google, //В основном США
            Quandl, //В основном США
            Xignite, //В основном США
            Yahoo, //В основном США
            Alor, //В основном Россия
            Finam, //В основном Россия
            Mfd, //В основном Россия
            Ux //В основном Украина
        }
        static IEnumerable<TimeFrameCandle> TryAllSources(Security sec, DateTime from, DateTime to)
        {
            //При загрузке данных из веб-источников, некоторые исключения означают невозможность загрузки, не связанную с локальной системой.
            List<Type> AcceptableExceptions = new List<Type>();
            AcceptableExceptions.Add(typeof (WebException));
            AcceptableExceptions.Add(typeof(InvalidOperationException));
            AcceptableExceptions.Add(typeof(NullReferenceException));
            var sourceG = new GoogleHistorySource();
            var historyTimeFrame = GoogleHistorySource.TimeFrames.First(span =>
            {
                return span >= TimeSpan.FromDays(1);
            });
            IEnumerable<TimeFrameCandleMessage> messages = new List<TimeFrameCandleMessage>();
            try
            {
                messages = sourceG.GetCandles(sec, historyTimeFrame, from, to);
            }
            catch (Exception e)
            {

                if (AcceptableExceptions.Contains(e.GetType()))
                        Debug.WriteLine(
                        "Инструмент не поддерживается источником Google, попытка использовать другие источники...");
                else throw(e);
            }
            if (messages.Count() == 0)
            {
                var sourceQ = new QuandlHistorySource(null, null);
                historyTimeFrame = QuandlHistorySource.TimeFrames.First(span =>
                {
                    return span >= TimeSpan.FromDays(1);
                });
                try
                {
                    messages = sourceQ.GetCandles(sec, historyTimeFrame, from, to);
                }
                catch (Exception e)
                {
                    if (AcceptableExceptions.Contains(e.GetType()))
                        Debug.WriteLine(
                        "Инструмент не поддерживается источником Quandl, попытка использовать другие источники...");
                    //else throw (e);
                }
            }
            if (messages.Count() == 0)
            {
                var sourceX = new XigniteHistorySource(null);
                historyTimeFrame = GoogleHistorySource.TimeFrames.First(span =>
                {
                    return span >= TimeSpan.FromDays(1);
                });
                try
                {
                    messages = sourceX.GetCandles(sec, historyTimeFrame, from, to);
                }
                catch (Exception e)
                {
                    if (AcceptableExceptions.Contains(e.GetType()))
                        Debug.WriteLine("Инструмент не поддерживается источником Xignite, попытка использовать другие источники...");
                    //else throw (e);
                }
            }
            if (messages.Count() == 0)
            {
                var sourceY = new YahooHistorySource(null);
                historyTimeFrame = YahooHistorySource.TimeFrames.First(span =>
                {
                    return span >= TimeSpan.FromDays(1);
                });
                try
                {
                    messages = sourceY.GetCandles(sec, historyTimeFrame, from, to);
                }
                catch (Exception e)
                {
                    if (AcceptableExceptions.Contains(e.GetType()))
                        Debug.WriteLine("Инструмент не поддерживается источником Yahoo, попытка использовать другие источники...");
                    //else throw (e);
                }
            }
            if (messages.Count() == 0)
            {
                var sourceA = new AlorHistorySource(null,null);
                historyTimeFrame = AlorHistorySource.TimeFrames.First(span =>
                {
                    return span >= TimeSpan.FromDays(1);
                });
                try
                {
                    messages = sourceA.GetCandles(sec, historyTimeFrame, from, to);
                }
                catch (Exception e)
                {
                    if (AcceptableExceptions.Contains(e.GetType()))
                        Debug.WriteLine("Инструмент не поддерживается источником Alor, попытка использовать другие источники...");
                    //else throw (e);
                }
            }
            if (messages.Count() == 0)
            {
                var sourceF = new FinamHistorySource(null, null);
                historyTimeFrame = FinamHistorySource.TimeFrames.First(span =>
                {
                    return span >= TimeSpan.FromDays(1);
                });
                var nativeIdStorage = new InMemoryNativeIdStorage();
                //var secStorage = new FinamSecurityInfo();
                //secStorage.Code = sec.Code;
                //secStorage.FinamMarketId = sourceF.SecurityIdGenerator.Split(sec.Id).NativeAsInt;
                //secStorage.FinamSecurityId = Convert.ToInt64(sourceF.SecurityIdGenerator.Split(sec.Id).SecurityCode);
                try
                {
                    messages = sourceF.GetCandles(sec, historyTimeFrame, from, to);
                }
                catch (Exception e)
                {
                    if (AcceptableExceptions.Contains(e.GetType()))
                        Debug.WriteLine("Инструмент не поддерживается источником Finam, попытка использовать другие источники...");
                    //else throw (e);
                    //TODO: Finam не работает
                }
            }
            if (messages.Count() == 0)
            {
                var sourceM = new MfdHistorySource(null, null); //TODO: Что означают эти аргументы?
                historyTimeFrame = MfdHistorySource.TimeFrames.First(span =>
                {
                    return span >= TimeSpan.FromDays(1);
                });
                var nativeIdStorage = new InMemoryNativeIdStorage();
                try
                {
                    messages = sourceM.GetCandles(sec, historyTimeFrame, from, to);
                }
                catch (Exception e)
                {
                    if (AcceptableExceptions.Contains(e.GetType()))
                        Debug.WriteLine("Инструмент не поддерживается источником MFD, попытка использовать другие источники...");
                    //else throw (e);
                    //TODO: MFD не работает
                }
            }
            if (messages.Count() == 0)
            {
                var sourceU = new UxHistorySource(null, null);
                historyTimeFrame = UxHistorySource.TimeFrames.First(span =>
                {
                    return span >= TimeSpan.FromDays(1);
                });
                try
                {
                    messages = sourceU.GetCandles(sec, historyTimeFrame, from, to);
                }
                catch (Exception e)
                {
                    if (AcceptableExceptions.Contains(e.GetType()))
                        Debug.WriteLine("Инструмент не поддерживается источником Ux, попытка использовать другие источники...");
                    else throw (e);
                }
            }
            return messages.ToCandles<TimeFrameCandle>(sec);
        }
        public static Dictionary<DateTimeOffset, Candle> LoadHistory(Security sec, DateTime from, DateTime to, HistorySourceType preferedSource = HistorySourceType.Google)
        {
            IEnumerable<TimeFrameCandleMessage> candleMessages = new List<TimeFrameCandleMessage>();
            var candles = new Dictionary<DateTimeOffset, Candle>();
            switch (preferedSource)
            {
                case HistorySourceType.Google:
                    {
                        var source = new GoogleHistorySource();
                        /*У каждого источника доступны только определенные варианты таймфреймов загружаемых свечек. Варианты таймфреймов перечислены в статическом свойстве источника.
                        Перед загрузкой исторических данных, необходимо обратиться к этому свойству и выбрать один из представленных таймфреймов. В данном случае, для загрузки
                        исторических данных за несколько месяцев и вычисления канала, достаточно взять свечи дневного таймфрейма, либо более длительного, если по каким-либо причином
                        дневные не доступны. Меньшие таймреймы могут не только замедлить загрузку, но и быть недоступными в силу ограничений на максимальное число запрашиваемых свечей
                        у поставщика данных */
                        var historyTimeFrame = GoogleHistorySource.TimeFrames.First(span =>
                        {
                            return span >= TimeSpan.FromDays(1);
                        });
                        List<Dictionary<DateTimeOffset, Candle>> candleLists = new List<Dictionary<DateTimeOffset, Candle>>();
                        if (sec.GetType() != typeof (WeightedIndexSecurity))
                        {
                            try
                            {
                                candleMessages = source.GetCandles(sec, historyTimeFrame, from, to);
                            }
                            catch { }
                            var newCandles = candleMessages.ToCandles<TimeFrameCandle>(sec);
                            if (newCandles.Count() == 0)
                                newCandles = TryAllSources(sec, from, to);
                            var candleDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var newCandle in newCandles)
                            {
                                candleDic[newCandle.OpenTime] = newCandle;
                            }
                            return candleDic;
                        }
                        foreach (var innerSec in ((WeightedIndexSecurity)sec).InnerSecurityIds)
                        {
                            var newSec = new Security();
                            newSec.Code = innerSec.SecurityCode;
                            newSec.Id = innerSec.ToStringId();
                            //загрузка истории по каждой составляющей корзины
                            IEnumerable<TimeFrameCandleMessage> messages = new List<TimeFrameCandleMessage>();
                            try
                            {
                                messages = source.GetCandles(newSec, historyTimeFrame, from, to);
                            }
                            /* Поставщик данных может быть недоступен, либо на нем может не быть данных о запрашиваемом инструменте, в этом случае генерируется исключение */
                            catch (WebException)
                            { }
                            var innerCandles = messages.ToCandles<TimeFrameCandle>(newSec);
                            //Если исторические данные не были получены, то следует попытаться получить их из всех остальных доступных источников
                            if (innerCandles.Count() == 0)
                                innerCandles = TryAllSources(newSec, from, to);
                            var innerDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var innerCandle in innerCandles)
                            {
                                innerDic[innerCandle.OpenTime] = innerCandle;
                            }
                            candleLists.Add(innerDic);
                        }
                        //Формирования индексного инструмента из загруженных данных о его составляющих
                        candles = CreateIndexCandles(candleLists, historyTimeFrame, sec);
                        //candleMessages = source.GetCandles(sec, _timeFrame, from, to);
                        break;
                    }
                case HistorySourceType.Quandl:
                    {
                        var source = new QuandlHistorySource(null, null);
                        var historyTimeFrame = QuandlHistorySource.TimeFrames.First(span =>
                        {
                            return span >= TimeSpan.FromDays(1);
                        });
                        if (sec.GetType() != typeof(WeightedIndexSecurity))
                        {
                            try
                            {
                                candleMessages = source.GetCandles(sec, historyTimeFrame, from, to);
                            }
                            catch (WebException) { }
                            var newCandles = candleMessages.ToCandles<TimeFrameCandle>(sec);
                            if (newCandles.Count() == 0)
                                newCandles = TryAllSources(sec, from, to);
                            var candleDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var newCandle in newCandles)
                            {
                                candleDic[newCandle.OpenTime] = newCandle;
                            }
                            return candleDic;
                        }
                        List<Dictionary<DateTimeOffset, Candle>> candleLists = new List<Dictionary<DateTimeOffset, Candle>>();
                        foreach (var innerSec in ((WeightedIndexSecurity)sec).InnerSecurityIds)
                        {
                            var newSec = new Security();
                            newSec.Code = innerSec.SecurityCode;
                            newSec.Id = innerSec.ToStringId();
                            //загрузка истории по каждой составляющей корзины
                            var messages = source.GetCandles(newSec, historyTimeFrame, from, to);
                            var innerCandles = messages.ToCandles<TimeFrameCandle>(newSec);
                            if (innerCandles.Count() == 0)
                                innerCandles = TryAllSources(newSec, from, to);
                            var innerDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var innerCandle in innerCandles)
                            {
                                innerDic[innerCandle.OpenTime] = innerCandle;
                            }
                            candleLists.Add(innerDic);
                        }

                        candles = CreateIndexCandles(candleLists, historyTimeFrame, sec);
                        break;
                    }
                case HistorySourceType.Xignite:
                    {
                        var source = new XigniteHistorySource(null);
                        var historyTimeFrame = GoogleHistorySource.TimeFrames.First(span => //т.к. у Xignite нет свойства TimeFrames
                        {
                            return span >= TimeSpan.FromDays(1);
                        });
                        if (sec.GetType() != typeof(WeightedIndexSecurity))
                        {
                            try
                            {
                                candleMessages = source.GetCandles(sec, historyTimeFrame, from, to);
                            }
                            catch (WebException) { }
                            var newCandles = candleMessages.ToCandles<TimeFrameCandle>(sec);
                            if (newCandles.Count() == 0)
                                newCandles = TryAllSources(sec, from, to);
                            var candleDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var newCandle in newCandles)
                            {
                                candleDic[newCandle.OpenTime] = newCandle;
                            }
                            return candleDic;
                        }
                        List<Dictionary<DateTimeOffset, Candle>> candleLists = new List<Dictionary<DateTimeOffset, Candle>>();
                        foreach (var innerSec in ((WeightedIndexSecurity)sec).InnerSecurityIds)
                        {
                            var newSec = new Security();
                            newSec.Code = innerSec.SecurityCode;
                            newSec.Id = innerSec.ToStringId();
                            //загрузка истории по каждой составляющей корзины
                            var messages = source.GetCandles(newSec, historyTimeFrame, from, to);
                            var innerCandles = messages.ToCandles<TimeFrameCandle>(newSec);
                            if (innerCandles.Count() == 0)
                                innerCandles = TryAllSources(newSec, from, to);
                            var innerDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var innerCandle in innerCandles)
                            {
                                innerDic[innerCandle.OpenTime] = innerCandle;
                            }
                            candleLists.Add(innerDic);
                        }

                        candles = CreateIndexCandles(candleLists, historyTimeFrame, sec);
                        break;
                    }
                case HistorySourceType.Yahoo:
                    {
                        var source = new YahooHistorySource(null);
                        var historyTimeFrame = YahooHistorySource.TimeFrames.First(span =>
                        {
                            return span >= TimeSpan.FromDays(1);
                        });
                        if (sec.GetType() != typeof(WeightedIndexSecurity))
                        {
                            try
                            {
                                candleMessages = source.GetCandles(sec, historyTimeFrame, from, to);
                            }
                            catch (WebException) { }
                            var newCandles = candleMessages.ToCandles<TimeFrameCandle>(sec);
                            if (newCandles.Count() == 0)
                                newCandles = TryAllSources(sec, from, to);
                            var candleDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var newCandle in newCandles)
                            {
                                candleDic[newCandle.OpenTime] = newCandle;
                            }
                            return candleDic;
                        }
                        List<Dictionary<DateTimeOffset, Candle>> candleLists = new List<Dictionary<DateTimeOffset, Candle>>();
                        foreach (var innerSec in ((WeightedIndexSecurity)sec).InnerSecurityIds)
                        {
                            var newSec = new Security();
                            newSec.Code = innerSec.SecurityCode;
                            newSec.Id = innerSec.ToStringId();
                            //загрузка истории по каждой составляющей корзины
                            var messages = source.GetCandles(newSec, historyTimeFrame, from, to);
                            var innerCandles = messages.ToCandles<TimeFrameCandle>(newSec);
                            if (innerCandles.Count() == 0)
                                innerCandles = TryAllSources(newSec, from, to);
                            var innerDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var innerCandle in innerCandles)
                            {
                                innerDic[innerCandle.OpenTime] = innerCandle;
                            }
                            candleLists.Add(innerDic);
                        }

                        candles = CreateIndexCandles(candleLists, historyTimeFrame, sec);
                        break;
                    }
                case HistorySourceType.Alor:
                    {
                        var source = new AlorHistorySource(null,null);
                        var historyTimeFrame = AlorHistorySource.TimeFrames.First(span =>
                        {
                            return span >= TimeSpan.FromDays(1);
                        });
                        if (sec.GetType() != typeof(WeightedIndexSecurity))
                        {
                            try
                            {
                                candleMessages = source.GetCandles(sec, historyTimeFrame, from, to);
                            }
                            catch (WebException) { }
                            var newCandles = candleMessages.ToCandles<TimeFrameCandle>(sec);
                            if (newCandles.Count() == 0)
                                newCandles = TryAllSources(sec, from, to);
                            var candleDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var newCandle in newCandles)
                            {
                                candleDic[newCandle.OpenTime] = newCandle;
                            }
                            return candleDic;
                        }
                        List<Dictionary<DateTimeOffset, Candle>> candleLists = new List<Dictionary<DateTimeOffset, Candle>>();
                        foreach (var innerSec in ((WeightedIndexSecurity)sec).InnerSecurityIds)
                        {
                            var newSec = new Security();
                            newSec.Code = innerSec.SecurityCode;
                            newSec.Id = innerSec.ToStringId();
                            //загрузка истории по каждой составляющей корзины
                            var messages = source.GetCandles(newSec, historyTimeFrame, from, to);
                            var innerCandles = messages.ToCandles<TimeFrameCandle>(newSec);
                            if (innerCandles.Count() == 0)
                                innerCandles = TryAllSources(newSec, from, to);
                            var innerDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var innerCandle in innerCandles)
                            {
                                innerDic[innerCandle.OpenTime] = innerCandle;
                            }
                            candleLists.Add(innerDic);
                        }

                        candles = CreateIndexCandles(candleLists, historyTimeFrame, sec);
                        break;
                    }
                case HistorySourceType.Finam:
                    {
                        var source = new FinamHistorySource(null, null);
                        var historyTimeFrame = FinamHistorySource.TimeFrames.First(span =>
                        {
                            return span >= TimeSpan.FromDays(1);
                        });
                        var nativeIdStorage = new InMemoryNativeIdStorage();
                        if (sec.GetType() != typeof(WeightedIndexSecurity))
                        {
                            try
                            {
                                candleMessages = source.GetCandles(sec, historyTimeFrame, from, to);
                            }
                            catch (WebException) { }
                            var newCandles = candleMessages.ToCandles<TimeFrameCandle>(sec);
                            if (newCandles.Count() == 0)
                                newCandles = TryAllSources(sec, from, to);
                            var candleDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var newCandle in newCandles)
                            {
                                candleDic[newCandle.OpenTime] = newCandle;
                            }
                            return candleDic;
                        }
                        List<Dictionary<DateTimeOffset, Candle>> candleLists = new List<Dictionary<DateTimeOffset, Candle>>();
                        foreach (var innerSec in ((WeightedIndexSecurity)sec).InnerSecurityIds)
                        {
                            var newSec = new Security();
                            newSec.Code = innerSec.SecurityCode;
                            newSec.Id = innerSec.ToStringId();
                            //загрузка истории по каждой составляющей корзины

                            var messages = source.GetCandles(newSec, historyTimeFrame, from, to);
                            var innerCandles = messages.ToCandles<TimeFrameCandle>(newSec);
                            if (innerCandles.Count() == 0)
                                innerCandles = TryAllSources(newSec, from, to);
                            var innerDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var innerCandle in innerCandles)
                            {
                                innerDic[innerCandle.OpenTime] = innerCandle;
                            }
                            candleLists.Add(innerDic);
                        }

                        candles = CreateIndexCandles(candleLists, historyTimeFrame, sec);
                        break;
                    }
                case HistorySourceType.Mfd:
                    {
                        var source = new MfdHistorySource(null, null);
                        var historyTimeFrame = MfdHistorySource.TimeFrames.First(span =>
                        {
                            return span >= TimeSpan.FromDays(1);
                        });
                        var nativeIdStorage = new InMemoryNativeIdStorage();
                        if (sec.GetType() != typeof(WeightedIndexSecurity))
                        {
                            try
                            {
                                candleMessages = source.GetCandles(sec, historyTimeFrame, from, to);
                            }
                            catch (WebException) { }
                            var newCandles = candleMessages.ToCandles<TimeFrameCandle>(sec);
                            if (newCandles.Count() == 0)
                                newCandles = TryAllSources(sec, from, to);
                            var candleDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var newCandle in newCandles)
                            {
                                candleDic[newCandle.OpenTime] = newCandle;
                            }
                            return candleDic;
                        }
                        List<Dictionary<DateTimeOffset, Candle>> candleLists = new List<Dictionary<DateTimeOffset, Candle>>();
                        foreach (var innerSec in ((WeightedIndexSecurity)sec).InnerSecurityIds)
                        {
                            var newSec = new Security();
                            newSec.Code = innerSec.SecurityCode;
                            newSec.Id = innerSec.ToStringId();
                            //загрузка истории по каждой составляющей корзины
                            var messages = source.GetCandles(newSec, historyTimeFrame, from, to);
                            var innerCandles = messages.ToCandles<TimeFrameCandle>(newSec);
                            if (innerCandles.Count() == 0)
                                innerCandles = TryAllSources(newSec, from, to);
                            var innerDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var innerCandle in innerCandles)
                            {
                                innerDic[innerCandle.OpenTime] = innerCandle;
                            }
                            candleLists.Add(innerDic);
                        }

                        candles = CreateIndexCandles(candleLists, historyTimeFrame, sec);
                        break;
                    }
                case HistorySourceType.Ux:
                    {
                        var source = new UxHistorySource(null, null);
                        var historyTimeFrame = UxHistorySource.TimeFrames.First(span =>
                        {
                            return span >= TimeSpan.FromDays(1);
                        });
                        if (sec.GetType() != typeof(WeightedIndexSecurity))
                        {
                            try
                            {
                                candleMessages = source.GetCandles(sec, historyTimeFrame, from, to);
                            }
                            catch (WebException) { }
                            var newCandles = candleMessages.ToCandles<TimeFrameCandle>(sec);
                            if (newCandles.Count() == 0)
                                newCandles = TryAllSources(sec, from, to);
                            var candleDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var newCandle in newCandles)
                            {
                                candleDic[newCandle.OpenTime] = newCandle;
                            }
                            return candleDic;
                        }
                        List<Dictionary<DateTimeOffset, Candle>> candleLists = new List<Dictionary<DateTimeOffset, Candle>>();
                        foreach (var innerSec in ((WeightedIndexSecurity)sec).InnerSecurityIds)
                        {
                            var newSec = new Security();
                            newSec.Code = innerSec.SecurityCode;
                            newSec.Id = innerSec.ToStringId();
                            //загрузка истории по каждой составляющей корзины
                            var messages = source.GetCandles(newSec, historyTimeFrame, from, to);
                            var innerCandles = messages.ToCandles<TimeFrameCandle>(newSec);
                            if (innerCandles.Count() == 0)
                                innerCandles = TryAllSources(newSec, from, to);
                            var innerDic = new Dictionary<DateTimeOffset, Candle>();
                            foreach (var innerCandle in innerCandles)
                            {
                                innerDic[innerCandle.OpenTime] = innerCandle;
                            }
                            candleLists.Add(innerDic);
                        }

                        candles = CreateIndexCandles(candleLists, historyTimeFrame, sec);
                        break;
                    }


            }
            return candles;
        }

        static Dictionary<DateTimeOffset, Candle> CreateIndexCandles(List<Dictionary<DateTimeOffset, Candle>> candleLists, TimeSpan historyTimeFrame, Security sec)
        {
            var candles = new Dictionary<DateTimeOffset, Candle>();
            foreach (var curTime in candleLists[0])
            {
                var openPrices = new List<decimal>();
                var highPrices = new List<decimal>();
                var lowPrices = new List<decimal>();
                var closePrices = new List<decimal>();
                foreach (var secPrice in candleLists)
                {
                    if (secPrice.ContainsKey(curTime.Key))
                    {
                        openPrices.Add(secPrice[curTime.Key].OpenPrice);
                        highPrices.Add(secPrice[curTime.Key].HighPrice);
                        lowPrices.Add(secPrice[curTime.Key].LowPrice);
                        closePrices.Add(secPrice[curTime.Key].ClosePrice);
                    }
                }
                if (openPrices.Count != candleLists.Count) continue; //расчет цены корзины, только если в этот период есть свечи для всех ее составляющих (иначе будут некорректные рез-ты)

                var cndl = new TimeFrameCandle();
                cndl.TimeFrame = historyTimeFrame;
                cndl.Arg = historyTimeFrame;
                cndl.Security = sec;
                cndl.OpenTime = curTime.Key;
                cndl.OpenPrice = ((WeightedIndexSecurity)sec).Calculate(openPrices.ToArray(),true);
                cndl.HighPrice = ((WeightedIndexSecurity)sec).Calculate(highPrices.ToArray(), true);
                cndl.LowPrice = ((WeightedIndexSecurity)sec).Calculate(lowPrices.ToArray(), true);
                cndl.ClosePrice = ((WeightedIndexSecurity)sec).Calculate(closePrices.ToArray(), true);
                candles[curTime.Key] = cndl;
            }
            return candles;
        }

    }
}
