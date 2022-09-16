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
using DevExpress.Mvvm.Native;

namespace StatA.Windows
{
    /// <summary>
    /// Логика взаимодействия для CustomTFWindow.xaml
    /// </summary>
    public partial class CustomTFWindow : Window
    {
        public CustomTFWindow()
        {
            InitializeComponent();
            this.DataContext = StaticData.chartWindow.settings;
        }

        private void timeFrameTB_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        //TODO: Возможность отмены (не через байндинг, назначение ТФ по закрытии)
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
