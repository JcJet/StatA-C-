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
    /// Логика взаимодействия для StrategySettingsDialog.xaml
    /// </summary>
    public partial class StrategySettingsDialog : Window
    {
        ArbitrageStrategy Strategy;
        public StrategySettingsDialog(ArbitrageStrategy strategy)
        {
            InitializeComponent();
            Strategy = strategy;
            FillFields(strategy);
        }

        void FillFields(ArbitrageStrategy strategy)
        {
            NameText.Text = strategy.Name;
            monthsInput.Value = strategy.HistoryMonths;
            sdInput.Value = strategy.SdThresh;
            pvalInput.Value = strategy.StatThresh;
            zonesInput.Value = strategy.ZoneCount;
            rangeCalcInput.SelectedIndex = strategy.RangeCalculation == ArbitrageStrategy.RangeCalculationType.History ? 0 : 1;
            stationarityCalcInput.SelectedIndex = strategy.StationarityCalculation == ArbitrageStrategy.StationarityCalculationType.Spread ? 0 : 1;
            spreadDrawInput.SelectedIndex = strategy.SpreadDraw == ArbitrageStrategy.StationarityCalculationType.Spread ? 0 : 1;
            channelInput.Value = strategy.RangeChannel;
            historySourceInput.SelectedIndex = (int) strategy.HistorySource;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Strategy.Name = NameText.Text;
            Strategy.HistoryMonths = Convert.ToInt32(monthsInput.Value);
            Strategy.SdThresh = sdInput.Value.TryConvertToDecimal();
            Strategy.StatThresh = pvalInput.Value.TryConvertToDecimal();
            Strategy.ZoneCount = Convert.ToByte(zonesInput.Value);
            Strategy.RangeCalculation = rangeCalcInput.SelectedIndex == 0 ? ArbitrageStrategy.RangeCalculationType.History : ArbitrageStrategy.RangeCalculationType.CurrentSpread;
            Strategy.StationarityCalculation = stationarityCalcInput.SelectedIndex == 0 ? ArbitrageStrategy.StationarityCalculationType.Spread : ArbitrageStrategy.StationarityCalculationType.SpreadMA;
            Strategy.SpreadDraw = spreadDrawInput.SelectedIndex == 0 ? ArbitrageStrategy.StationarityCalculationType.Spread : ArbitrageStrategy.StationarityCalculationType.SpreadMA;
            Strategy.RangeChannel = channelInput.Value.TryConvertToDecimal();
            Strategy.HistorySource = (HistoryLoader.HistorySourceType) historySourceInput.SelectedIndex;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
