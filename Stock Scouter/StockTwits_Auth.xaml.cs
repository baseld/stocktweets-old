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

namespace Stock_Scouter
{
    public partial class StockTwits_Auth : PhoneApplicationPage
    {
        public StockTwits_Auth()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            WebPage.Navigating += WebPage_Navigating;
            WebPage.Navigated += WebPage_Navigated;
            WebPage.Navigate(new Uri(StockTwitsClient.Instance.AuthorizeUri));
        }

        void WebPage_Navigating(object sender, NavigatingEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Navigating to " + e.Uri);
        }

        void WebPage_Navigated(object sender, NavigationEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Navigated to " + e.Uri);
            System.Diagnostics.Debug.WriteLine(WebPage.SaveToString());
        }

    }
}