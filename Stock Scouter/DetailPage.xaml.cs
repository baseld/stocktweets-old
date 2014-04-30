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
        private static string currentSymbol;
        private static DetailViewModel ViewModel = null;

        public DetailPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.TryGetValue("symbol", out currentSymbol))
            {
                PagePanorama.Title = currentSymbol; // an example of passing args
            }

            Quote q = AppSettings.GetStock(currentSymbol);
            System.Diagnostics.Debug.WriteLine(q.Name);
            ViewModel = new DetailViewModel() {Symbol = currentSymbol, Quote = AppSettings.GetStock(currentSymbol)};
        }

        void OnLoad(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("This is an onload event!");
            //this.currentQuote = AppSettings.GetStock(currentSymbol);
            //System.Diagnostics.Debug.WriteLine("WebClient returns " + YahooFinance.GetQuotes(new string[]{"XOM"}));
        }
    }
}