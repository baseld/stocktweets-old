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
    public partial class EachstockPages : PhoneApplicationPage
    {
        private static string currentSymbol;
        Quote q;
        public EachstockPages()
        {
            InitializeComponent();
        }


        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.TryGetValue("symbol", out currentSymbol))
            {
                q = AppSettings.GetStock(currentSymbol);
                PagePanorama.Title = q.Name; // an example of passing args
                Price.Text = q.LastTradePrice.ToString();
                Change.Text=q.Change.ToString();
                ChangeInPercent.Text=q.ChangeInPercent.ToString();
                Dailyhigh.Text = q.DailyHigh.ToString();
                Dailylow.Text = q.DailyLow.ToString();

            }

            System.Diagnostics.Debug.WriteLine(q.Name);
            System.Diagnostics.Debug.WriteLine(q.Change);
            System.Diagnostics.Debug.WriteLine(q.ChangeInPercent);
        }

        private void graphtime_click(object sender, RoutedEventArgs e)
        {

        }

    }
}