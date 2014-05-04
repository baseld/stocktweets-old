using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Xml.Linq;
using System.Net;
using Newtonsoft.Json;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace Stock_Scouter
{
    public partial class DetailedQuotePage : PhoneApplicationPage
    {
        private static string _currentSymbol;
        private static Quote _currentQuote;
        private static RssViewModel rssViewModel = null;
        private static TweetViewModel tweetViewModel = null;
        private static ProgressIndicator progressBar = null;

        public static string CurrentSymbol
        {
            get
            {
                return _currentSymbol;
            }
            set
            {
                _currentSymbol = value;
            }
        }

        public static RssViewModel RssView
        {
            get
            {
                if (rssViewModel == null) rssViewModel = new RssViewModel();
                return rssViewModel;
            }
            set
            {
                rssViewModel = value;
            }
        }

        public static TweetViewModel TweetView
        {
            get
            {
                if (tweetViewModel == null) tweetViewModel = new TweetViewModel();
                return tweetViewModel;
            }
            set
            {
                tweetViewModel = value;
            }
        }

        public static Quote CurrentQuote
        {
            get
            {
                return _currentQuote;
            }
            set
            {
                _currentQuote = value;
            }
        }

        public static ProgressIndicator ProgressBar
        {
            get
            {
                if (progressBar == null)
                {
                    progressBar = new ProgressIndicator();
                    progressBar.IsVisible = true;
                    progressBar.IsIndeterminate = true;
                }
                return progressBar;
            }
        }

        public DetailedQuotePage()
        {
            InitializeComponent();
            this.DataContext = this;

            SystemTray.SetProgressIndicator(this, ProgressBar);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.TryGetValue("symbol", out _currentSymbol))
            {
                System.Diagnostics.Debug.WriteLine("DetailedQuotedPage: symbol is " + CurrentSymbol);
                CurrentQuote = App.GetQuote(CurrentSymbol);

                TweetView.EmptyData();

                if (CurrentQuote.Change > 0)
                {
                    Change.Foreground = new SolidColorBrush(Colors.Green);
                }
                else if (CurrentQuote.Change < 0)
                {
                    Change.Foreground = new SolidColorBrush(Colors.Red);
                }
                
                if (StockTwitsClient.User == null)
                {
                }
                else
                {
                    // signed in
                    StockTwitsClient.Instance.AccessToken = StockTwitsClient.User.access_token;
                    StockTwitsClient.Instance.UserID = StockTwitsClient.User.user_id;
                    StockTwitsClient.Instance.UserName = StockTwitsClient.User.username;
                }

                RssView.Symbol = CurrentSymbol;
                ProgressBar.IsVisible = false;

                RefreshView(RootPanorama.SelectedIndex);
            }
        }

        private void GraphIndicator_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // round step to 1
            GraphIndicator.Value = Math.Round(e.NewValue);
            System.Diagnostics.Debug.WriteLine("Slider got new value: " + GraphIndicator.Value);
            string[] prompts = {"1d", "5d", "1m", "3m", "6m", "1y", "5y", "max"};
            GraphIndicator_Prompt.Text = prompts[(int)GraphIndicator.Value];
        }

        private void UpdateGraph()
        {
            List<string> syms = App.StrToSymbols(Graph_CompareWith.Text);
            Uri graphUri = YahooAPI.GetQuoteGraphUrl(CurrentSymbol, GraphIndicator_Prompt.Text, syms);
            WebClient client = new WebClient();
            client.OpenReadCompleted += async (obj, args) =>
            {
                Stream stream = new MemoryStream();
                await args.Result.CopyToAsync(stream);
                BitmapImage b = new BitmapImage();
                b.SetSource(stream);
                this.GraphHolder.Source = b;
                ProgressBar.IsVisible = false;
                GraphHolder_Prompt.Visibility = Visibility.Collapsed;
            };
            client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(GraphDownloadProgressChanged);
            client.OpenReadAsync(graphUri);
            ProgressBar.IsVisible = true;
            progressBar.IsIndeterminate = false;
            progressBar.Value = 0;
        }

       private void GraphDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
       {
           ProgressBar.Value = e.ProgressPercentage;
       }

        private void GraphIndicator_LoadImage(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Slider: should load image now");
            UpdateGraph();
        }

        // open the link in the browser
        private void NewsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.SelectedIndex < 0) return;

            RssItemViewModel rvm = lb.SelectedItem as RssItemViewModel;
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri(rvm.Link, UriKind.Absolute);
            lb.SelectedIndex = -1;
            webBrowserTask.Show();
        }

        // when panorama item changes
        private void Panorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int currentIndex = RootPanorama.SelectedIndex;
            RefreshView(currentIndex);
        }

        private void RefreshView(int panoramaIndex)
        {
            if (panoramaIndex < 2)
            {
                WebClient client = new WebClient();
                client.DownloadStringCompleted += (obj, args) =>
                {
                    YahooAPI.UpdateQuotes(args.Result.ToString());
                    
                    if (CurrentQuote.Change > 0)
                    {
                        Change.Foreground = new SolidColorBrush(Colors.Green);
                    }
                    else if (CurrentQuote.Change < 0)
                    {
                        Change.Foreground = new SolidColorBrush(Colors.Red);
                    }
                };
                client.DownloadStringAsync(YahooAPI.GetQuotesXmlUrl(new List<string>() { CurrentSymbol }));
            }
            if (panoramaIndex == 2)
            {
                // graph page
                UpdateGraph();
            }
            else if (panoramaIndex == 3)
            {
                // rss news page
                ProgressBar.IsIndeterminate = true;
                ProgressBar.IsVisible = true;
                RssView.LoadData(ProgressBar);
            }
            else if (panoramaIndex == 4)
            {
                // tweets page
                ProgressBar.IsIndeterminate = true;
                ProgressBar.IsVisible = true;
                TweetView.Symbol = CurrentSymbol;
                TweetView.LoadData(ProgressBar);
            }
        }

        private void CurrentView_Refresh(object sender, EventArgs e)
        {
            RefreshView(RootPanorama.SelectedIndex);
        }

        private void NavigateTo_Settings(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/SettingsPage.xaml" + CurrentSymbol, UriKind.Relative));
        }

        private void GraphHolder_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {

        }

        private void NavigateTo_SignInWithStockTwits(object sender, EventArgs e)
        {
            if (StockTwitsClient.User == null)
                NavigationService.Navigate(new Uri("/StockTwits_Auth.xaml?lastSymbol=" + CurrentSymbol, UriKind.Relative));
            else
            {
                string message = "Viewing profile function will be added in a later version.";
                string caption = "TODO";
                MessageBoxButton buttons = MessageBoxButton.OK;
                MessageBoxResult result = MessageBox.Show(message, caption, buttons);
            }
        }

        private void StockTwits_NewTweet(object sender, EventArgs e)
        {
            if (StockTwitsClient.User != null)
            {
                var tb = new TextBox();
                tb.MinHeight = 120;
                tb.Text = "$" + CurrentSymbol + " ";
                var box = new CustomMessageBox()
                {
                    Caption = "New Tweet",
                    LeftButtonContent = "Post",
                    RightButtonContent = "Cancel",
                    Content = tb,
                    IsFullScreen = false
                };
                box.Dismissed += (s, ev) =>
                {
                    if (ev.Result == CustomMessageBoxResult.LeftButton)
                    {
                        // a very basic new tweet form
                        StockTwitsClient.Instance.CreateMessage(tb.Text, delegate(object obj, UploadStringCompletedEventArgs args)
                        {
                            System.Diagnostics.Debug.WriteLine(args.Result);
                            TweetView.LoadData(ProgressBar);
                        });
                    }
                };

                // focus on the text box by default
                box.Loaded += (s, ev) =>
                {
                    tb.Focus();
                };

                box.Show();
            }
            else
            {
                string message = "You need to sign in with StockTwits to post tweets.";
                string caption = "Oops";
                MessageBoxButton buttons = MessageBoxButton.OK;
                MessageBoxResult result = MessageBox.Show(message, caption, buttons);
            }
        }

    }

    public class RssViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<RssItemViewModel> rssCollection = null;

        public RssViewModel()
        {
            // rssCollection = new ObservableCollection<RssItemViewModel>();
            IsDataLoaded = false;
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        public string Symbol
        {
            get;
            set;
        }

        public ObservableCollection<RssItemViewModel> RssCollection
        {
            get { return rssCollection; }
            set
            {
                if (rssCollection != value)
                {
                    rssCollection = value;
                    NotifyPropertyChanged("RssCollection");
                }
            }
        }

        public void LoadData(ProgressIndicator p = null)
        {
            System.Diagnostics.Debug.WriteLine("Now start to load rss data for symbol " + Symbol);
            List<string> sym = new List<string>() { Symbol };
            
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (obj, e) =>
            {
                RssCollection = YahooAPI.ParseRssXml(e.Result.ToString());
                if (p != null) p.IsVisible = false;
            };
            client.DownloadStringAsync(YahooAPI.GetRssXmlUrl(sym));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class RssItemViewModel : INotifyPropertyChanged
    {
        private string title;
        private string link;
        private string description;
        private DateTime? pubDate;

        public RssItemViewModel()
        {
        }

        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                this.title = value;
                NotifyPropertyChanged("Title");
            }
        }

        public string Link
        {
            get
            {
                return link;
            }
            set
            {
                this.link = value;
                NotifyPropertyChanged("Link");
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
            set
            {
                this.description = value;
                NotifyPropertyChanged("Description");
            }
        }

        public DateTime? PubDate
        {
            get
            {
                return pubDate;
            }
            set
            {
                this.pubDate = value;
                NotifyPropertyChanged("PubDate");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class TweetViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<TweetItemViewModel> tweetCollection = null;

        public TweetViewModel()
        {
            tweetCollection = new ObservableCollection<TweetItemViewModel>();
            IsDataLoaded = false;
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        public string Symbol
        {
            get;
            set;
        }

        public static ProgressIndicator ProgressBar
        {
            get;
            set;
        }

        public ObservableCollection<TweetItemViewModel> TweetCollection
        {
            get { return tweetCollection; }
            set
            {
                if (tweetCollection != value)
                {
                    tweetCollection = value;
                    NotifyPropertyChanged("TweetCollection");
                }
            }
        }

        private StockTwits_Stream_Cursor _cursor = null;
        public StockTwits_Stream_Cursor Cursor
        {
            get
            {
                return this._cursor;
            }
            set
            {
                _cursor = value;
                NotifyPropertyChanged("Cursor");
            }
        }

        public void EmptyData()
        {
            Cursor = null;
            TweetCollection.Clear();
        }

        public void LoadData(ProgressIndicator p = null)
        {
            ProgressBar = p;
            if (Cursor == null)
            {
                StockTwitsClient.Instance.GetStreamOfSymbol(Symbol, ProcessTweetMessages);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine(Cursor.since);
                StockTwitsClient.Instance.GetStreamOfSymbol(Symbol, ProcessTweetMessages, Cursor.since);
            }
        }

        private void ProcessTweetMessages(object obj, DownloadStringCompletedEventArgs args)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(args.Result);
                StockTwits_Stream_Symbol s = JsonConvert.DeserializeObject<StockTwits_Stream_Symbol>(args.Result);
                Cursor = s.cursor;
                foreach (StockTwits_Message m in s.messages)
                {
                    TweetCollection.Add(new TweetItemViewModel() { Author = m.user.name + " (" + m.user.username + ")", Content = m.body, PubDate = m.created_at });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.Source);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
            }
            if (ProgressBar != null) ProgressBar.IsVisible = false;
            this.IsDataLoaded = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }

    public class TweetItemViewModel : INotifyPropertyChanged
    {
        private string author;
        private string content;
        private DateTime? pubDate;

        public TweetItemViewModel()
        {
        }

        public string Author
        {
            get
            {
                return author;
            }
            set
            {
                this.author = value;
                NotifyPropertyChanged("Author");
            }
        }

        public string Content
        {
            get
            {
                return content;
            }
            set
            {
                this.content = value;
                NotifyPropertyChanged("Content");
            }
        }

        public DateTime? PubDate
        {
            get
            {
                return pubDate;
            }
            set
            {
                this.pubDate = value;
                NotifyPropertyChanged("PubDate");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}