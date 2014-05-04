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
            
            WebPage.LoadCompleted += WebPage_LoadCompleted;
            WebPage.Navigate(new Uri(StockTwitsClient.Instance.AuthorizeUri));
        }

        void WebPage_LoadCompleted(object sender, NavigationEventArgs e)
        {
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
                StockTwitsClient.Instance.GetAccessToken(code, delegate(object obj, UploadStringCompletedEventArgs args)
                {
                    System.Diagnostics.Debug.WriteLine(args.Result);
                    StockTwits_OAuth_Token r = JsonConvert.DeserializeObject<StockTwits_OAuth_Token>(args.Result);
                    StockTwitsClient.Code = code;
                    StockTwitsClient.User = r;
                    this.NavigationService.GoBack();
                });
            }
        }
    }
}