using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Stock_Scouter.Models;

namespace Stock_Scouter
{
    public partial class AddList : PhoneApplicationPage
    {
        public AddList()
        {
            InitializeComponent();
        }

        private void CreateNewList(object sender, RoutedEventArgs e)
        {
            //YahooFinance.GetQuotes(new string[] {});
            //System.Diagnostics.Debug.WriteLine(Symbol.Text);
            Portfolio p = AppSettings.GetPortfolio(Name.Text);
            AppSettings.addPortfolio(p);
        }

        private void AddToPortfolio(object sender, System.Windows.Input.GestureEventArgs e)
        {
            Stock[] s = YahooFinance.CsvToStock("\"XOM\",\"Exxon Mobil Corpo\",\"4/17/2014\",100.42,99.69,100.97,99.69,15439810,\"84.79 - 101.74\",2.52,2.52,13.56");
            Portfolio p = new Portfolio();
            foreach (Stock item in s)
            {
                p.addStock(item);
            }
            NavigationService.Navigate(new Uri("/MainPage.xaml", UriKind.Relative));
        }
    }
}