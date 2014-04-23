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
            Portfolio p = AppSettings.GetPortfolio(ListName.Text);
            AppSettings.addPortfolio(p);
        }

        
    }
}