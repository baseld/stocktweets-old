using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;

namespace StockTweets
{
    public partial class UserPanel : PhoneApplicationPage
    {
        public UserPanel()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.StClient.User == null)
            {
                if (RootPivot.Items.Contains(MePivot))
                    RootPivot.Items.Remove(MePivot);
                if (!RootPivot.Items.Contains(WelcomePivot))
                    RootPivot.Items.Add(WelcomePivot);
                RootPivot.SelectedItem = WelcomePivot;
            } 
            else
            {
               if (!RootPivot.Items.Contains(MePivot))
                    RootPivot.Items.Add(MePivot);
                if (RootPivot.Items.Contains(WelcomePivot))
                    RootPivot.Items.Remove(WelcomePivot);
                RootPivot.SelectedItem = MePivot;
            }
        }

        private void Navigate_BugReportUri(object sender, RoutedEventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("https://github.com/xybu92/stockscouter/issues", UriKind.Absolute);
            webBrowserTask.Show();
        }

        private void Navigate_StockTwitsWebsite(object sender, RoutedEventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("http://stocktwits.com/", UriKind.Absolute);
            webBrowserTask.Show();
        }

        private void Navigate_YahooFinanceWebsite(object sender, RoutedEventArgs e)
        {
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri("http://finance.yahoo.com/", UriKind.Absolute);
            webBrowserTask.Show();
        }

        private void Navigate_Auth(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/StockTwits_Auth.xaml", UriKind.Relative));
        }

        private void Navigate_Watchlist(object sender, System.Windows.Input.GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/WatchlistPage.xaml", UriKind.Relative));
        }

        private void Navigate_Settings(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }
    }
}