using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;

namespace Stock_Scouter
{
    /**
     * StockTwits client-side API Implementation in C#
     * Special thanks to the server-side PHP implementation https://github.com/jayzalowitz/StocktwitsAPI/
     * 
     * @author  Xiangyu Bu <xybu92@live.com>
     * 
     */

    class StockTwitsClient
    {
        public static readonly string CLIENT_CONSUMER_KEY = "f3a84eda935dbf94";
        public static readonly string CLIENT_CONSUMER_SECRET = "9f92b9f770fa285abefe4109c82ee514bb362d88";
        private static StockTwitsAPI _client = null;

        public static StockTwitsAPI Instance
        {
            get
            {
                if (_client == null)
                {
                    _client = new StockTwitsAPI(CLIENT_CONSUMER_KEY, CLIENT_CONSUMER_SECRET, "http://stockapp.sige.us/api/callback.php");
                    _client.ResponseType = "code";
                }
                return _client;
            }
        }

        public static string UserId
        {
            get
            {
                if (!App.storageSpace.Contains("StockTwits_UserId"))
                    App.storageSpace.Add("StockTwits_UserId", "");
                return (string)App.storageSpace["StockTwits_UserId"];
            }
            set
            {
                App.storageSpace["StockTwits_UserId"] = value;
            }
        }
    }

    class StockTwitsAPI
    {
        protected static readonly bool API_DEBUG_MODE = false;
        protected static readonly string API_BASE_URL = "https://api.stocktwits.com/api/2/";
        protected static readonly string API_BASE_URL_SECURE = "https://api.stocktwits.com/api/2/";
        protected static readonly string API_SCOPE = "read,watch_lists,publish_messages,publish_watch_lists,direct_messages,follow_users,follow_stocks";
        protected static readonly string API_USER_AGENT = "StockTwits API CSharp v0.1";

        protected string ClientID
        {
            get;
            set;
        }

        protected string ClientSecret
        {
            get;
            set;
        }

        public string AuthorizePath
        {
            get;
            set;
        }

        public string AccessToken
        {
            get;
            set;
        }

        public string AccessTokenPath
        {
            get;
            set;
        }

        public string RequestToken
        {
            get;
            set;
        }

        public string RequestTokenPath
        {
            get;
            set;
        }

        public string RedirectUri
        {
            get;
            set;
        }

        public string UserID
        {
            get;
            set;
        }

        public string UserName
        {
            get;
            set;
        }

        // should be either "code" or "token"
        // does not handle "token" mode well yet
        public string ResponseType
        {
            get;
            set;
        }

        public string AuthorizeUri
        {
            get
            {
                return AuthorizePath + "?client_id=" + ClientID + "&response_type=" + ResponseType + "&redirect_uri=" + RedirectUri + "&scope=" + API_SCOPE;
            }
        }

        public StockTwitsAPI(string clientId, string clientSecret, string redirectUri = "")
        {
            if (redirectUri != "") RedirectUri = redirectUri;
            ClientID = clientId;
            ClientSecret = clientSecret;
            RequestTokenPath = API_BASE_URL_SECURE + "oauth/token";
            AccessTokenPath = API_BASE_URL_SECURE + "oauth/authorize";
            AuthorizePath = API_BASE_URL_SECURE + "oauth/signin";
        }

        public void GetAccessToken(string code, UploadStringCompletedEventHandler handler)
        {
            string POSTData = "client_id=" + ClientID + "&client_secret=" + ClientSecret + "&code=" + code + "&grant_type=authorization_code&redirect_uri=" + RedirectUri;
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            client.Headers[HttpRequestHeader.UserAgent] = API_USER_AGENT;
            client.UploadStringCompleted += handler;
            client.UploadStringAsync(new Uri(RequestTokenPath), "POST", Uri.EscapeDataString(POSTData));
        }

        public void GetProfile(string id, DownloadStringCompletedEventHandler handler)
        {
            string profileUri = API_BASE_URL_SECURE + "streams/user/" + id + ".json";
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            client.Headers[HttpRequestHeader.UserAgent] = API_USER_AGENT;
            client.DownloadStringCompleted += handler;
            client.DownloadStringAsync(new Uri(profileUri));
        }

        public void GetStream(string id, DownloadStringCompletedEventHandler handler)
        {
            if (id == "") id = "home";
            string streamUri = API_BASE_URL_SECURE + "streams/" + id + ".json?access_token=" + AccessToken + "";
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            client.Headers[HttpRequestHeader.UserAgent] = API_USER_AGENT;
            client.DownloadStringCompleted += handler;
            client.DownloadStringAsync(new Uri(streamUri));
        }

        public void GetStreamOfSymbols(string[] symbols, UploadStringCompletedEventHandler handler)
        {
            string queryUri = "https://api.stocktwits.com/api/2/streams/symbols.json?access_token=" + AccessToken;
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            client.Headers[HttpRequestHeader.UserAgent] = API_USER_AGENT;
            client.UploadStringCompleted += handler;
            client.UploadStringAsync(new Uri(RequestTokenPath), "GET", "symbols=" + String.Join(",", symbols));
        }

        public void SearchSymbols(string[] symbols, DownloadStringCompletedEventHandler handler)
        {
            string queryUri = API_BASE_URL_SECURE + "search/symbols.json?access_token=" + AccessToken + "&q=" + String.Join(",", symbols);
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            client.Headers[HttpRequestHeader.UserAgent] = API_USER_AGENT;
            client.DownloadStringCompleted += handler;
            client.DownloadStringAsync(new Uri(queryUri));
        }

        public void SearchUser(string query, DownloadStringCompletedEventHandler handler)
        {
            string queryUri = API_BASE_URL_SECURE + "search/users.json?access_token=" + AccessToken + "&q=" + query;
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            client.Headers[HttpRequestHeader.UserAgent] = API_USER_AGENT;
            client.DownloadStringCompleted += handler;
            client.DownloadStringAsync(new Uri(queryUri));
        }

        public void SearchGeneral(string query, DownloadStringCompletedEventHandler handler)
        {
            string queryUri = API_BASE_URL_SECURE + "search.json?access_token=" + AccessToken + "&q=" + query;
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            client.Headers[HttpRequestHeader.UserAgent] = API_USER_AGENT;
            client.DownloadStringCompleted += handler;
            client.DownloadStringAsync(new Uri(queryUri));
        }

        public void CreateMessage(string body, UploadStringCompletedEventHandler handler, string sentiment = null, string in_reply_to_msg_id = null, string chartUri = null)
        {
            StringBuilder postData = new StringBuilder("body=");
            postData.Append(Uri.EscapeDataString(body));

            if (in_reply_to_msg_id != null)
            {
                postData.Append("&in_reply_to_message_id=");
                postData.Append(in_reply_to_msg_id);
            }

            if (chartUri != null)
            {
                postData.Append("&chart=");
                postData.Append(chartUri);
            }

            if (sentiment != null)
            {
                postData.Append("&sentiment=");
                postData.Append(sentiment);
            }

            string queryUri = "https://api.stocktwits.com/api/2/messages/create.json?access_token=" + AccessToken;
            WebClient client = new WebClient();
            client.Encoding = System.Text.Encoding.UTF8;
            client.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
            client.Headers[HttpRequestHeader.UserAgent] = API_USER_AGENT;
            client.UploadStringCompleted += handler;
            client.UploadStringAsync(new Uri(RequestTokenPath), "POST", postData.ToString());
        }

    }
}
