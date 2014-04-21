using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Stock_Scouter.Models;

namespace Stock_Scouter
{
    public partial class DetailPage : PhoneApplicationPage
    {
        public DetailPage()
        {
            InitializeComponent();
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("This is an onload event!");
            System.Diagnostics.Debug.WriteLine("WebClient returns " + YahooFinance.GetQuotes(new string[]{"XOM"}));
        }
    }
}