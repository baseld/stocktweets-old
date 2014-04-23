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

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string symbol = "";

            if (NavigationContext.QueryString.TryGetValue("symbol", out symbol))
                PagePanorama.Title = symbol; // an example of passing args


        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("This is an onload event!");
            //System.Diagnostics.Debug.WriteLine("WebClient returns " + YahooFinance.GetQuotes(new string[]{"XOM"}));
        }
    }
}