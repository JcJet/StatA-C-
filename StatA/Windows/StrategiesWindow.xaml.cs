using StatA.Strategies;
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
using Ecng.Xaml;
using StockSharp.Xaml;

namespace StatA.Windows
{
    /// <summary>
    /// Логика взаимодействия для StrategiesWindow.xaml
    /// </summary>
    public partial class StrategiesWindow : Window
    {
        public StrategiesWindow()
        {
            InitializeComponent();
        }

        void StrategiesRefresh()
        {
            StrategiesDashboard1.BeginDataUpdate();
            StrategiesDashboard1.Items.Clear();
            foreach (var strategy in StaticData.Strategies)
            {
                StrategiesDashboardItem item = new StrategiesDashboardItem(strategy.Name, strategy,strategy.Name);
                StrategiesDashboard1.Items.Add(item);
            }
            StrategiesDashboard1.EndDataUpdate();
        }
        private void AddStrategyBtn_Click(object sender, RoutedEventArgs e)
        {
            var strategy = new ArbitrageStrategy();
            strategy.ShowSettings();
            StaticData.Strategies.Add(strategy);
            StrategiesRefresh();
        }

        private void RemoveStrategyBtn_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in StrategiesDashboard1.SelectedItems)
            {
                StrategiesDashboard1.BeginDataUpdate();
                StrategiesDashboard1.Items.Remove((StrategiesDashboardItem) item);
                StrategiesDashboard1.EndDataUpdate();
            }
        }

        private void StrategySettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            if (StrategiesDashboard1.SelectedItems.Count != 1)
                MessageBox.Show("Для изменения параметров стратегии, необходимо выбрать одну");
            else
            {
                ((IConfigurable) ((StrategiesDashboardItem) StrategiesDashboard1.SelectedItem).Strategy).ShowSettings();
                StrategiesRefresh();
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (StrategiesDashboard1.SelectedItems.Count != 1)
            {
                MessageBox.Show("Для запуска стратегии, необходимо выбрать одну");
                return;
            }
            var strategy = ((StrategiesDashboardItem) StrategiesDashboard1.SelectedItem).Strategy;
            if (!StaticData.ChartWindows.ContainsKey(strategy))
            {
                var wnd = new UnifiedChartWindow((ArbitrageStrategy)strategy);
                wnd.WindowState = WindowState.Maximized;
                wnd.MakeHideable();
                StaticData.ChartWindows[strategy] = wnd;
            }
            StaticData.ChartWindows[strategy].ShowOrHide();
        }
    }
}
