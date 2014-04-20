using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Stock_Scouter;
using Stock_Scouter.Models;

namespace Stock_Scouter
{
    public partial class Addlist : PhoneApplicationPage
    {
        public Addlist()
        {
            InitializeComponent();
        }

        private void searchQuote(object sender, RoutedEventArgs e)
        {
            Stock[] s = new Stock[1];
            s[0] = new Stock("\"XOM\",\"Exxon Mobil Corpo\",\"4/17/2014\",100.42,99.69,100.97,99.69,15439810,\"84.79 - 101.74\",2.52,2.52,13.56");
            EventHandler.savePortfolio(s);
        }
    }
}