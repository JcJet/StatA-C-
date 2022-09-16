using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Ecng.Common;
using Ecng.Configuration;
using Ecng.Serialization;
using Ecng.Xaml;

using StockSharp.Algo;
using StockSharp.Algo.Storages;
using StockSharp.Algo.Storages.Csv;
using StockSharp.BusinessEntities;
using StockSharp.Logging;
using StockSharp.Configuration;
using StockSharp.Localization;

using StatA.Windows;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Testing;
using StockSharp.Messages;
using StockSharp.Xaml.Charting;
using StockSharp.Xaml;
using Ecng.Collections;
using Ecng.Reflection;
using StockSharp.SmartCom;
using MoreLinq;

namespace StatA
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }
        private const string _settingsFile = "connection.xml";
        private const string _settingsFileEmu = "connectionEmu.xml";
        private bool _isConnected;
        CsvEntityRegistry entityRegistry;
        IStorageRegistry storageRegistry;
        public MainWindow()
        {
            
            InitializeComponent();
            StaticData.Init();
            Instance = this;
            var logManager = new LogManager();
            logManager.Listeners.Add(new FileLogListener("ConnectionLog.log"));

            entityRegistry = new CsvEntityRegistry("Data");

            ConfigManager.RegisterService<IEntityRegistry>(entityRegistry);
            // ecng.serialization invoke in several places IStorage obj
            ConfigManager.RegisterService(entityRegistry.Storage);

            storageRegistry = ConfigManager.GetService<IStorageRegistry>();

            //SerializationContext.DelayAction = entityRegistry.DelayAction = new DelayAction(entityRegistry.Storage, ex => ex.LogError());

            StaticData.Connector = new Connector(entityRegistry, storageRegistry);
            logManager.Sources.Add(StaticData.Connector);

            InitConnector(entityRegistry, _settingsFile);
        }
        private void InitConnector(CsvEntityRegistry entityRegistry, string settingsFile)
        {
            // subscribe on connection successfully event
            StaticData.Connector.Connected += () =>
            {
                this.GuiAsync(() => ChangeConnectStatus(true));
            };

            // subscribe on connection error event
            StaticData.Connector.ConnectionError += error => this.GuiAsync(() =>
            {
                ChangeConnectStatus(false);
                MessageBox.Show(this, error.ToString(), LocalizedStrings.Str2959);
            });

            StaticData.Connector.Disconnected += () => this.GuiAsync(() => ChangeConnectStatus(false));

            // subscribe on error event
            StaticData.Connector.Error += error =>
                this.GuiAsync(() => MessageBox.Show(this, error.ToString(), LocalizedStrings.Str2955));

            // subscribe on error of market data subscription event
            StaticData.Connector.MarketDataSubscriptionFailed += (security, msg, error) =>
                this.GuiAsync(() => MessageBox.Show(this, error.ToString(), LocalizedStrings.Str2956Params.Put(msg.DataType, security)));

            StaticData.Connector.NewSecurity += security => StaticData.securitiesWindow.SecurityPicker.Securities.Add(security);
            StaticData.Connector.NewTrade += trade => StaticData.tradesWindow.TradeGrid.Trades.Add(trade);

            StaticData.Connector.NewOrder += order => StaticData.ordersWindow.OrderGrid.Orders.Add(order);
            StaticData.Connector.NewStopOrder += order => StaticData.stopOrdersWindow.OrderGrid.Orders.Add(order);
            StaticData.Connector.NewMyTrade += trade => StaticData.myTradesWindow.TradeGrid.Trades.Add(trade);

            StaticData.Connector.NewPortfolio += portfolio => StaticData.portfoliosWindow.PortfolioGrid.Portfolios.Add(portfolio);
            StaticData.Connector.NewPosition += position => StaticData.portfoliosWindow.PortfolioGrid.Positions.Add(position);

            // subscribe on error of order registration event
            StaticData.Connector.OrderRegisterFailed += StaticData.ordersWindow.OrderGrid.AddRegistrationFail;
            // subscribe on error of order cancelling event
            StaticData.Connector.OrderCancelFailed += OrderFailed;

            // subscribe on error of stop-order registration event
            StaticData.Connector.OrderRegisterFailed += StaticData.stopOrdersWindow.OrderGrid.AddRegistrationFail;
            // subscribe on error of stop-order cancelling event
            StaticData.Connector.StopOrderCancelFailed += OrderFailed;

            // set market data provider
            StaticData.securitiesWindow.SecurityPicker.MarketDataProvider = StaticData.Connector;

            try
            {
                if (File.Exists(settingsFile))
                    StaticData.Connector.Load(new XmlSerializer<SettingsStorage>().Deserialize(settingsFile));
            }
            catch
            {
            }

            if (StaticData.Connector.StorageAdapter == null)
                return;

            try
            {
                entityRegistry.Init();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString());
            }

            StaticData.Connector.StorageAdapter.DaysLoad = TimeSpan.FromDays(3);
            StaticData.Connector.StorageAdapter.Load();

            ConfigManager.RegisterService<IExchangeInfoProvider>(new StorageExchangeInfoProvider(entityRegistry));
        }
        private void ChangeConnectStatus(bool isConnected)
        {
            _isConnected = isConnected;
            ConnectionButton.Content = isConnected ? "Отключиться" : "Подключиться";
            ConnectionButton.Background = isConnected ? new SolidColorBrush(Color.FromRgb(100, 255, 100)) : new SolidColorBrush(Color.FromRgb(255, 100, 100));
        }
        private void ConnectionSettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (StaticData.Connector.GetType() != typeof (RealTimeEmulationTrader<BasketMessageAdapter>))
                //Если не эмулятор
            {
                if (StaticData.Connector.Configure(this))
                    new XmlSerializer<SettingsStorage>().Serialize(StaticData.Connector.Save(), _settingsFile);
            }
            else
            {
                if (StaticData.Connector.Configure(this))
                    new XmlSerializer<SettingsStorage>().Serialize(StaticData.Connector.Save(), _settingsFileEmu);
                /* ArgumentException, невозможно использовать конфигуратор для обертки эмуляции, компоненты StockSharp.XAML закрыты в данном релизе.*/
                MessageBox.Show("Для настройки подключения необходимо отключить режим эмуляции");
            }

        }

        private void ConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            ConnectionButton.Background = new SolidColorBrush(Color.FromRgb(100, 200, 200));
            ConnectionButton.Content = "Подключение...";
            if (!_isConnected)
            {
                StaticData.Connector.Connect();
            }
            else
            {
                StaticData.Connector.Disconnect();
            }
        }
        private void OrderFailed(OrderFail fail)
        {
            this.GuiAsync(() =>
            {
                MessageBox.Show(this, fail.Error.ToString(), "Ошибка регистрации или отмены ордера");
            });
        }
        private static void ShowOrHide(Window window)
        {
            if (window == null)
                throw new ArgumentNullException(nameof(window));

            if (window.Visibility == Visibility.Visible)
                window.Hide();
            else
                window.Show();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StaticData.DeInit();
            //base.OnClosing(e);
        }

        private void InstrumentsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHide(StaticData.securitiesWindow);
        }

        private void PortfoliosButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHide(StaticData.portfoliosWindow);
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHide(StaticData.ordersWindow);
        }

        private void StopOrdersButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHide(StaticData.stopOrdersWindow);
        }

        private void TicksButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHide(StaticData.tradesWindow);
        }

        private void DealsButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHide(StaticData.myTradesWindow);
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            var candleManager = new CandleManager(StaticData.Connector);
            candleManager.Processing += DrawCandle;
            var sec = StaticData.securitiesWindow.SecurityPicker.SelectedSecurity;
            var series = new CandleSeries(typeof (TimeFrameCandle), sec, new TimeSpan(0,0,5,0));
            candleManager.Start(series);

        }

        private void DrawCandle(CandleSeries series, Candle candle)
        {
            //chartWindow.Chart.Draw((ChartCandleElement) chartWindow.Chart.Areas[0].Elements[0], candle);
            StaticData.chartWindow.GuiAsync(() => StaticData.chartWindow.Chart1.Draw(StaticData.chartWindow.chartCandleElement, candle));
        }

        private void ChartButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHide(StaticData.chartWindow);
        }

        private void ScoringButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHide(StaticData.scoringWindow);
        }

        private void StorageButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHide(StaticData.storageWindow);
        }

        private void SetEmulation()
        {
            if (StaticData.Connector.GetType() != typeof (RealTimeEmulationTrader<BasketMessageAdapter>))
            {
                StaticData.Connector?.Disconnect();
                StaticData.Connector?.Dispose();
                BasketMessageAdapter _realAdapter = new BasketMessageAdapter(new MillisecondIncrementalIdGenerator());
                //try
                //{
                    if (File.Exists(_settingsFileEmu))
                        _realAdapter.Load(new XmlSerializer<SettingsStorage>().Deserialize(_settingsFileEmu));
                    else if (_realAdapter.Configure(this))
                        new XmlSerializer<SettingsStorage>().Serialize(_realAdapter.Save(), _settingsFileEmu);

                _realAdapter.InnerAdapters.ForEach(a => a.RemoveTransactionalSupport());
                //}
                //catch(Exception e)
                //{
                //    MessageBox.Show(e.Message);
                //}
                StaticData.Connector = new RealTimeEmulationTrader<IMessageAdapter>(_realAdapter);
                ((RealTimeEmulationTrader<IMessageAdapter>)StaticData.Connector).EmulationAdapter.Emulator.Settings.TimeZone = TimeHelper.Est;
                ((RealTimeEmulationTrader<IMessageAdapter>)StaticData.Connector).EmulationAdapter.Emulator.Settings.ConvertTime = true;
                //
                StaticData.Connector.Connected += () =>
                {
                    this.GuiAsync(() => ChangeConnectStatus(true));
                };
                StaticData.Connector.ConnectionError += error => this.GuiAsync(() =>
                {
                    ChangeConnectStatus(false);
                    MessageBox.Show(this, error.ToString(), LocalizedStrings.Str2959);
                }); 
                StaticData.Connector.Disconnected += () => this.GuiAsync(() => ChangeConnectStatus(false));
                StaticData.Connector.Error += error =>
                    this.GuiAsync(() => MessageBox.Show(this, error.ToString(), LocalizedStrings.Str2955));
                StaticData.Connector.MarketDataSubscriptionFailed += (security, msg, error) =>
                    this.GuiAsync(() => MessageBox.Show(this, error.ToString(), LocalizedStrings.Str2956Params.Put(msg.DataType, security)));
                StaticData.Connector.NewSecurity += security => StaticData.securitiesWindow.SecurityPicker.Securities.Add(security);
                StaticData.Connector.NewTrade += trade => StaticData.tradesWindow.TradeGrid.Trades.Add(trade);
                StaticData.Connector.NewOrder += order => StaticData.ordersWindow.OrderGrid.Orders.Add(order);
                StaticData.Connector.NewStopOrder += order => StaticData.stopOrdersWindow.OrderGrid.Orders.Add(order);
                StaticData.Connector.NewMyTrade += trade => StaticData.myTradesWindow.TradeGrid.Trades.Add(trade);
                StaticData.Connector.NewPortfolio += portfolio => StaticData.portfoliosWindow.PortfolioGrid.Portfolios.Add(portfolio);
                StaticData.Connector.NewPosition += position => StaticData.portfoliosWindow.PortfolioGrid.Positions.Add(position);
                StaticData.Connector.OrderRegisterFailed += StaticData.ordersWindow.OrderGrid.AddRegistrationFail;
                StaticData.Connector.OrderCancelFailed += OrderFailed;
                StaticData.Connector.OrderRegisterFailed += StaticData.stopOrdersWindow.OrderGrid.AddRegistrationFail;
                StaticData.Connector.StopOrderCancelFailed += OrderFailed;
                StaticData.securitiesWindow.SecurityPicker.MarketDataProvider = StaticData.Connector;

                if (StaticData.Connector.StorageAdapter != null)
                {
                    try
                    {
                        entityRegistry.Init();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(this, ex.ToString());
                    }
                    StaticData.Connector.StorageAdapter.DaysLoad = TimeSpan.FromDays(3);
                    StaticData.Connector.StorageAdapter.Load();
                    ConfigManager.RegisterService<IExchangeInfoProvider>(new StorageExchangeInfoProvider(entityRegistry));
                }

                StaticData.Connector.Connect();
                //
            }
            else
            {
                StaticData.Connector.Disconnect();
                EmulationButton.Background = new SolidColorBrush(Color.FromRgb(221, 221, 221));
            }
        }

        private void EmulationButton_Click(object sender, RoutedEventArgs e)
        {
            SetEmulation();
        }

        private void StrategiesButton_Click(object sender, RoutedEventArgs e)
        {
            ShowOrHide(StaticData.strategiesWindow);
        }

        public void ChangePassword()
        {
            var cpw = new ChangePasswordWindow();
            cpw.Process = () =>
            {
                var cpm = new ChangePasswordMessage();
                cpm.NewPassword = cpw.NewPassword;
                StaticData.Connector.TransactionAdapter.SendInMessage(cpm);
            };
            cpw.ShowModal();
            //TODO: сообщение о смене пароля
        }

        private void ChangePasswordButton_Click(object sender, RoutedEventArgs e)
        {
            ChangePassword();
        }

        private void NewOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var ow = new OrderWindow();
            ow.MarketDataProvider = StaticData.Connector;
            ow.SecurityProvider = StaticData.Connector;
            var portfolios = new ThreadSafeObservableCollection<Portfolio>(new ObservableCollectionEx<Portfolio>());
            portfolios.AddRange(StaticData.Connector.Portfolios);
         foreach (Portfolio item in portfolios.Items)
             ow.Portfolios.Items.Add(item);
            ow.Show();

        }

        private void NewConditionalOrderButton_Click(object sender, RoutedEventArgs e)
        {
            var ocw = new OrderConditionalWindow();
            ocw.MarketDataProvider = StaticData.Connector;
            ocw.SecurityProvider = StaticData.Connector;
            ocw.Adapter = StaticData.Connector.Adapter;
            ocw.Adapters = StaticData.Connector.Adapter.InnerAdapters;
            //ocw.MessageAdapterProvider
            var portfolios = new ThreadSafeObservableCollection<Portfolio>(new ObservableCollectionEx<Portfolio>());
            portfolios.AddRange(StaticData.Connector.Portfolios);
            foreach (var item in portfolios.Items)
            {
                ocw.Portfolios.Add(item);
            }
            ocw.Show();
        }

        private void MonitoringButton_Click(object sender, RoutedEventArgs e)
        {
            var mw = new MonitorWindow();
            mw.BringToFrontOnError = true;
            mw.Show();
            //TODO: мониторинг, GuiLogListener
        }
    }
}
