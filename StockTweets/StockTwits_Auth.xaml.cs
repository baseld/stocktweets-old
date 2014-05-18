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
using Newtonsoft.Json;

namespace StockTweets
{
    public partial class StockTwits_Auth : PhoneApplicationPage
    {

        private static string returnPage = null;
        private ProgressIndicator indicator;

        public StockTwits_Auth()
        {
            InitializeComponent();

            SystemTray.SetOpacity(this, 0);

            indicator = new ProgressIndicator();
            indicator.IsVisible = false;
            indicator.IsIndeterminate = true;

            SystemTray.SetProgressIndicator(this, indicator);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            NavigationContext.QueryString.TryGetValue("returnPage", out returnPage);

            WebPage.Navigating += WebPage_Navigating;
            
            WebPage.LoadCompleted += WebPage_LoadCompleted;
            WebPage.Navigate(new Uri(App.StClient.AuthorizeUri));
        }

        void WebPage_LoadCompleted(object sender, NavigationEventArgs e)
        {
            indicator.IsVisible = false;
            if (WebPage.Source == null) return;

            if (WebPage.Source.ToString().Contains("/oauth/authorize?"))
            {
                // because the page isn't compatible with WebBrowser control, this trick is needed
                string[] args = { "$(\"#allow\").attr('value', 1);$(\"#authorize_form\").submit();" };
                WebPage.InvokeScript("eval", args);
            }
        }

        void WebPage_Navigating(object sender, NavigatingEventArgs e)
        {
            indicator.IsVisible = true;
            if (e.Uri == null) return;

            System.Diagnostics.Debug.WriteLine("Navigating to " + e.Uri);
            if (e.Uri.ToString().Contains("?code="))
            {
                e.Cancel = true;
                string url = e.Uri.ToString();
                int index = url.IndexOf("?code=");
                string code = url.Substring(index + 6);
                System.Diagnostics.Debug.WriteLine(code);
                WebPage.NavigateToString("Authorization success. Now fetching access token...");
                App.StClient.GetAccessToken(code, delegate(object obj, UploadStringCompletedEventArgs args)
                {
                    System.Diagnostics.Debug.WriteLine(args.Result);
                    StockTwits_OAuth_Token r = JsonConvert.DeserializeObject<StockTwits_OAuth_Token>(args.Result);
                    App.StClient.Code = code;
                    App.StClient.User = r;
                    if (returnPage != null)
                    {
                        Uri u = new Uri(returnPage, UriKind.Relative);
                        returnPage = "";
                        NavigationService.Navigate(u);
                    } else NavigationService.GoBack();
                });
            }
        }
    }
}