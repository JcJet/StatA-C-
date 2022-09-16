using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using DevExpress.Xpf.Editors.Helpers;
using DevExpress.Xpf.Grid;
using Ecng.Collections;
using Ecng.Common;
using OEC;
using StatA.Classes;
using StockSharp.Algo.Candles;
using StockSharp.BusinessEntities;
using StockSharp.Algo.Storages;
using StockSharp.Algo;
using StockSharp.Algo.Indicators;
using StockSharp.Messages;
using StatA.Classes;
using System.IO;
using CQG;

namespace StatA.Windows
{
    /// <summary>
    /// Логика взаимодействия для ScoringWindow.xaml
    /// </summary>
    public partial class ScoringWindow : Window
    {
        Dictionary<string, ScoringStats> ScoringResult;
        BackgroundWorker backgroundWorker;
        struct ScoringStats
        {
            public string Pair { get; set; }
            public Tuple<long, long> CandleCount { get; set; }
            public decimal Stationarity { get; set; }
        }
        public ScoringWindow()
        {

            InitializeComponent();
            SecSourceInput.SelectedIndex = 1;
            var gc = new DataGridTextColumn();
            ScoringResult = new Dictionary<string, ScoringStats>();
            gc.Header = "Weight";
            Basket1Grid.Columns.Add(gc);
            gc = new DataGridTextColumn();
            gc.Header = "Weight";
            Basket2Grid.Columns.Add(gc);
            UpdateBaskets();
            UpdateScoring();
            scoringGrid.ItemsSource = ScoringResult;
            backgroundWorker = (BackgroundWorker)FindResource("backgroundWorker");
        }

        private void ScoringSecuritiesList_SecuritySelected(Security obj)
        {
            var security = obj;
            AddToBasket1Button.IsEnabled = AddToBasket2Button.IsEnabled = security != null;
            if (security != null)
            {
                AddToBasket1Button.Content = StaticData.CurrPair.FirstBasket.InnerSecurityIds.Contains(security.ToSecurityId()) ? "Добавить ☑" : "Добавить ☐";
                AddToBasket2Button.Content = StaticData.CurrPair.SecondBasket.InnerSecurityIds.Contains(security.ToSecurityId()) ? "Добавить ☑" : "Добавить ☐";
                
            }
        }

        private void AddToBasket1Button_Click(object sender, RoutedEventArgs e)
        {
            var security = ScoringSecuritiesList.SelectedSecurity;

            if (StaticData.CurrPair.FirstBasket.InnerSecurityIds.Contains(security.ToSecurityId()))
            {
                StaticData.CurrPair.FirstBasket.Weights.Remove(security.ToSecurityId());
                AddToBasket1Button.Content = "Добавить ☐";
            }
            else
            {
                StaticData.CurrPair.FirstBasket.Weights.Add(security.ToSecurityId(), Basket1WeightInput.Value.TryConvertToDecimal());
                AddToBasket1Button.Content = "Добавить ☑";
            }
            UpdateBaskets();
        }

        private void AddToBasket2Button_Click(object sender, RoutedEventArgs e)
        {
            var security = ScoringSecuritiesList.SelectedSecurity;

            if (StaticData.CurrPair.SecondBasket.InnerSecurityIds.Contains(security.ToSecurityId()))
            {
                StaticData.CurrPair.SecondBasket.Weights.Remove(security.ToSecurityId());
                AddToBasket2Button.Content = "Добавить ☐";
            }
            else
            {
                StaticData.CurrPair.SecondBasket.Weights.Add(security.ToSecurityId(), Basket1WeightInput.Value.TryConvertToDecimal());
                AddToBasket2Button.Content = "Добавить ☑";
            }
            UpdateBaskets();
        }

        void UpdateBaskets()
        {
            Basket1Grid.Items.Clear();
            Basket2Grid.Items.Clear();
            foreach (var sec in StaticData.CurrPair.FirstBasket.InnerSecurityIds)
            {
                Basket1Grid.Items.Add(sec);

            }
            foreach (var sec in StaticData.CurrPair.SecondBasket.InnerSecurityIds)
            {
                Basket2Grid.Items.Add(sec);
            }

        }
        public void UpdateScoring()
        {
            ScoringSecuritiesList.Securities.Clear();
            foreach (var sec in StaticData.Scoring)
            {
                ScoringSecuritiesList.Securities.Add(sec);
            }
        }

        private void Basket1DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            StaticData.CurrPair.FirstBasket.Weights.Remove((SecurityId)Basket1Grid.SelectedItem);
            UpdateBaskets();
        }

        private void Basket2DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            StaticData.CurrPair.SecondBasket.Weights.Remove((SecurityId)Basket2Grid.SelectedItem);
            UpdateBaskets();
        }

        private void Basket1WeightInput_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Basket1Grid.SelectedItem == null) return;
            var selectedSec = (SecurityId)Basket1Grid.SelectedItem;
            if ((Basket1Grid.SelectedItems.Count == 1) && (Basket1Grid.IsFocused))
                StaticData.CurrPair.FirstBasket.Weights[selectedSec] = Basket1WeightInput.Value.TryConvertToDecimal();
        }

        private void Basket2WeightInput_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Basket2Grid.SelectedItem == null) return;
            var selectedSec = (SecurityId)Basket2Grid.SelectedItem;
            if ((Basket2Grid.SelectedItems.Count == 1) && (Basket2Grid.IsFocused))
                StaticData.CurrPair.SecondBasket.Weights[selectedSec] = Basket2WeightInput.Value.TryConvertToDecimal();
        }

        private void Basket1Grid_SelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            Basket1DeleteButton.IsEnabled = Basket1Grid.SelectedItems.Count == 1;
            if (Basket1Grid.SelectedItems.Count == 1)
            {
                var selectedSecurity = (SecurityId)Basket1Grid.SelectedItem;
                Basket1WeightInput.Value = StaticData.CurrPair.FirstBasket.Weights[selectedSecurity];
            }
        }

        private void Basket2Grid_SelectedItemChanged(object sender, SelectionChangedEventArgs e)
        {
            Basket2DeleteButton.IsEnabled = Basket2Grid.SelectedItems.Count == 1;
            if (Basket2Grid.SelectedItems.Count == 1)
            {
                var selectedSecurity = (SecurityId)Basket2Grid.SelectedItem;
                Basket2WeightInput.Value = StaticData.CurrPair.SecondBasket.Weights[selectedSecurity];
            }
        }

        private enum SecuritiesSelection
        {
            ScoringList,
            All
        }

        private class CalcScoringInput
        {
            public TimeSpan Timeframe { get; set; }
            public SecuritiesSelection SecurityScope { get; set; }

            public CalcScoringInput(TimeSpan timeframe, SecuritiesSelection securityScope)
            {
                Timeframe = timeframe;
                SecurityScope = securityScope;
            }
        }
        private Dictionary<string, ScoringStats> CalcScoring(TimeSpan timeframe, SecuritiesSelection securityScope)
        {
            Dictionary<DateTimeOffset, Candle> Candles1, Candles2, CandlesArb;
            DateTimeOffset Candles1_LastTime, Candles2_LastTime, CandlesArb_LastTime;
            Dictionary<string, ScoringStats> result = new Dictionary<string, ScoringStats>();
            int iteration = 0;
            //ScoringResult.Clear();
            //progressBar.Value = 0;
            //timeframe = TimeSpan.FromDays(1); //TODO: для более длинных периодов необходима будет загрузка исторических данных
            //Перебор всех возможных пар
            List<Security> securities = new List<Security>();
            var to = StaticData.Connector.CurrentTime.DateTime;
            var from = to - TimeSpan.FromDays(360);
            var storage = new StorageRegistry();
            List<Security> SecList = new List<Security>();
            if (securityScope == SecuritiesSelection.All)
                SecList = StaticData.securitiesWindow.SecurityPicker.Securities.ToList();
            if (securityScope == SecuritiesSelection.ScoringList)
                SecList = StaticData.Scoring;
            iteration = 0;
            foreach (var sec in SecList)
            {
                if (sec.State == SecurityStates.Stoped) continue;// || (sec.State.IsNull())) continue; //Не имеют ли некоторые торгуемые инструменты State = null?
                var candles = HistoryLoader.LoadHistory(sec, from, to);
                //File.AppendAllText("SecLog.txt", String.Format("{0} {1} {2} \n\r", sec.State.IsNull(), sec.Id, candles.Count));
                if (candles.Count == 0) continue;
                //candles[candles.Keys.Min()].ClosePrice = 0;
                var candleStorage = storage.GetCandleStorage(typeof(TimeFrameCandle), sec, timeframe);
                foreach (var candle in candles)
                {
                    /*Корректировка шага цены, указанного в инструменте. При загрузке исторических данных, возможно получение свечей со значениями цены, не кратными шагу инструмента.
                    Для дальнейшей обработки необходимо привести в соответствие цены и шаг цены. Хотя это и является проблемой в получаемых данных.
                    */
                    var remainder = candle.Value.OpenPrice % sec.PriceStep.Value;
                    if (remainder > 0)
                        sec.PriceStep *= 0.1M;
                    remainder = candle.Value.HighPrice % sec.PriceStep.Value;
                    if (remainder > 0)
                        sec.PriceStep *= 0.1M;
                    remainder = candle.Value.LowPrice % sec.PriceStep.Value;
                    if (remainder > 0)
                        sec.PriceStep *= 0.1M;
                    remainder = candle.Value.ClosePrice % sec.PriceStep.Value;
                    if (remainder > 0)
                        sec.PriceStep *= 0.1M;
                }
                candleStorage.Save(candles.Values);
                candleStorage.Serializer.DoDispose();
                if (backgroundWorker.CancellationPending)
                    return null;
                if (backgroundWorker.WorkerReportsProgress)
                    backgroundWorker.ReportProgress(Convert.ToInt32((iteration/(SecList.Count/100d))/2));
                iteration++;
            }

            //
            iteration = 0;
            foreach (var sec1 in SecList)
            {
                foreach (var sec2 in SecList)
                {
                    if (sec1.Code == sec2.Code)
                    {
                        //progressBar.Value = +1 / (StaticData.Scoring.Count / 100.0);
                        continue;
                    }
                    //Инициализация переменных
                    Candles1 = new Dictionary<DateTimeOffset, Candle>();
                    Candles2 = new Dictionary<DateTimeOffset, Candle>();
                    CandlesArb = new Dictionary<DateTimeOffset, Candle>();
                    Candles1_LastTime = new DateTimeOffset();
                    Candles2_LastTime = new DateTimeOffset();
                    CandlesArb_LastTime = new DateTimeOffset();
                    var spreadIndicator = new SpreadIndicator();
                    var stationarityIndicator = new Stationarity();
                    var secPair = new WeightedIndexSecurity();
                    secPair.Weights.Add(sec1.ToSecurityId(), 1);
                    secPair.Weights.Add(sec2.ToSecurityId(), -1);
                    var candleStorage = storage.GetCandleStorage(typeof(TimeFrameCandle), sec1, timeframe);
                    foreach (var candle in candleStorage.Load(from, to))
                    {
                        Candles1[candle.OpenTime] = candle;
                    }
                    candleStorage.DoDispose();
                    candleStorage = storage.GetCandleStorage(typeof(TimeFrameCandle), sec2, timeframe);
                    foreach (var candle in candleStorage.Load(from, to))
                    {
                        Candles2[candle.OpenTime] = candle;
                    }
                    candleStorage.DoDispose();
                    foreach (var timeKey in Candles1.Keys)
                    {
                        decimal price1, price2;
                        price1 = Candles1[timeKey].ClosePrice;
                        if (!Candles2.ContainsKey(timeKey))
                        {
                            if (Candles2_LastTime == null) continue;
                            else price2 = Candles2[Candles2_LastTime].ClosePrice;
                        }
                        else
                        {
                            price2 = Candles2[timeKey].ClosePrice;
                            Candles2_LastTime = timeKey;
                        }
                        var pair = new Tuple<decimal, decimal>(price1, Candles2[timeKey].ClosePrice);
                        var spread = spreadIndicator.Process(pair);
                        stationarityIndicator.Process(spread).GetValue<decimal>();
                        if (backgroundWorker.CancellationPending)
                            return null;

                    }
                    ScoringStats stats = new ScoringStats();
                    stats.Pair = secPair.Code;
                    stats.Stationarity = stationarityIndicator.GetValue(0);
                    stats.CandleCount = new Tuple<long, long>(Candles1.Count, Candles2.Count);
                    result[secPair.Code] = stats;
                    //progressBar.Value = +1 / (StaticData.Scoring.Count / 100.0);
                }
                if (backgroundWorker.CancellationPending)
                    return null;
                if (backgroundWorker.WorkerReportsProgress)
                    backgroundWorker.ReportProgress(Convert.ToInt32((iteration/(SecList.Count/100d))/2));
                iteration++;
            }
            return result;
        }

        private void CalcScoringBtn_Click(object sender, RoutedEventArgs e)
        {
            if (CalcScoringBtn.Tag.ToString() == "Start")
            {
                ChangeScoringButton(true);
                CalcScoringInput input = new CalcScoringInput(TimeSpan.FromDays(1), SecSourceInput.SelectedIndex == 0 ? SecuritiesSelection.All : SecuritiesSelection.ScoringList);
                backgroundWorker.RunWorkerAsync(input);
            }
            else if (CalcScoringBtn.Tag.ToString() == "Stop")
            {
                ChangeScoringButton(false);
                backgroundWorker.CancelAsync();
            }
            ScoringResult = CalcScoring(TimeSpan.FromDays(1), SecSourceInput.SelectedIndex == 0 ? SecuritiesSelection.All : SecuritiesSelection.ScoringList);
            //Отображение результата скоринга
            //scoringGrid.ItemsSource = null;
            //scoringGrid.Items.Clear();
            //foreach (var item in ScoringResult)
            //{
            //    scoringGrid.Items.Add(new { Pair = item.Key, Stationarity = item.Value.Stationarity, Count = item.Value.CandleCount.Item1 +"-"+ item.Value.CandleCount.Item2});
            //}

        }

        private void ChangeScoringButton(bool active)
        {
            if (active)
            {
                CalcScoringBtn.Tag = "Stop";
                CalcScoringBtn.Content = "Стоп";
            }
            else
            {
                CalcScoringBtn.Tag = "Start";
                CalcScoringBtn.Content = "Обновить скоринг";
            }

        }
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            CalcScoringInput input = (CalcScoringInput) e.Argument;
            var result = CalcScoring(input.Timeframe, input.SecurityScope);
            if (backgroundWorker.CancellationPending)
                e.Cancel = true;
            e.Result = result;
        }

        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                MessageBox.Show(e.Error.Message, "Ошибка вычисления скоринга", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            else if (!e.Cancelled)
            {
                ScoringResult = (Dictionary<string, ScoringStats>)e.Result;
            }
            ChangeScoringButton(false);
            progressBar.Value = 0;
        }

        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
            if (e.ProgressPercentage <= 50)
                progressText.Content = "Загрузка исторических данных...";
            else
                progressText.Content = "Вычисление статистик...";
        }
    }
}
