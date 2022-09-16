using StockSharp.BusinessEntities;
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

namespace StatA.Windows
{
    /// <summary>
    /// Логика взаимодействия для NewOrderWindow.xaml
    /// </summary>
    public partial class NewOrderWindow : Window
    {
        Security Sec;
        public NewOrderWindow(Security security)
        {
            InitializeComponent();
            securityTextBox.Text = security.Code;
            Sec = security;
        }

        void SendOrder()
        {
            var order = new Order();
            order.Security = Sec;
            //order.Direction
            //order.Comment
        }
    }
}
