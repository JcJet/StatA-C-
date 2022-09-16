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
using DevExpress.Xpf.Editors.Helpers;
using StatA.Strategies;
using StatA.Classes;

namespace StatA.Windows
{
    /// <summary>
    /// Логика взаимодействия для TimeFrameStrategySettingsDialog.xaml
    /// </summary>
    public partial class TimeFrameStrategySettingsDialog : Window
    {
        ArbitrageTimeFrameStrategy Strategy;
        public TimeFrameStrategySettingsDialog(ArbitrageTimeFrameStrategy strategy)
        {
            InitializeComponent();
            Strategy = strategy;
            FillFields(strategy);
        }

        void FillFields(ArbitrageTimeFrameStrategy strategy)
        {
            NameText.Text = strategy.Name;
            monthsInput.Value = strategy.HistoryMonths;
            sdInput.Value = strategy.SdThresh;
            pvalInput.Value = strategy.StatThresh;
            zonesInput.Value = strategy.ZoneCount;
            rangeCalcInput.SelectedIndex = strategy.RangeCalculation == ArbitrageTimeFrameStrategy.RangeCalculationType.History ? 0 : 1;
            stationarityCalcInput.SelectedIndex = strategy.StationarityCalculation == ArbitrageTimeFrameStrategy.StationarityCalculationType.Spread ? 0 : 1;
            spreadDrawInput.SelectedIndex = strategy.SpreadDraw == ArbitrageTimeFrameStrategy.StationarityCalculationType.Spread ? 0 : 1;
            channelInput.Value = strategy.RangeChannel;
            historySourceInput.SelectedIndex = (int)strategy.HistorySource;
            timeFrameInput.Text = strategy.TimeFrame.ToString("hh\\:mm\\:ss");
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            TimeSpan timeframe = new TimeSpan();
            if (!TimeSpan.TryParse(timeFrameInput.Text, out timeframe))
            {
                MessageBox.Show("Неверный формат таймфрейма");
                return;
            }
            Strategy.Name = NameText.Text;
            Strategy.HistoryMonths = Convert.ToInt32(monthsInput.Value);
            Strategy.SdThresh = sdInput.Value.TryConvertToDecimal();
            Strategy.StatThresh = pvalInput.Value.TryConvertToDecimal();
            Strategy.ZoneCount = Convert.ToByte(zonesInput.Value);
            Strategy.RangeCalculation = rangeCalcInput.SelectedIndex == 0 ? ArbitrageTimeFrameStrategy.RangeCalculationType.History : ArbitrageTimeFrameStrategy.RangeCalculationType.CurrentSpread;
            Strategy.StationarityCalculation = stationarityCalcInput.SelectedIndex == 0 ? ArbitrageTimeFrameStrategy.StationarityCalculationType.Spread : ArbitrageTimeFrameStrategy.StationarityCalculationType.SpreadMA;
            Strategy.SpreadDraw = spreadDrawInput.SelectedIndex == 0 ? ArbitrageTimeFrameStrategy.StationarityCalculationType.Spread : ArbitrageTimeFrameStrategy.StationarityCalculationType.SpreadMA;
            Strategy.RangeChannel = channelInput.Value.TryConvertToDecimal();
            Strategy.HistorySource = (HistoryLoader.HistorySourceType) historySourceInput.SelectedIndex;
            Strategy.TimeFrame = timeframe;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
