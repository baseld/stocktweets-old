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
using System.Windows.Media.Imaging;

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

                QuoteName.Text = CurrentQuote.Name;
                DetailedSymbol.Text = CurrentQuote.Symbol + " (" + CurrentQuote.StockExchange + ")";
                LastTradePrice.Text = CurrentQuote.LastTradePrice.ToString();
                ChangeInPercent.Text = CurrentQuote.ChangeInPercent.ToString();
                LastTradeDate.Text = CurrentQuote.LastTradeDate.ToString();
                PreviousClose.Text = CurrentQuote.PreviousClose.ToString();

                Change.Text = CurrentQuote.Change.ToString();
                if (CurrentQuote.Change > 0)
                {
                    Change.Foreground = new SolidColorBrush(Colors.Green);
                }
                else if (CurrentQuote.Change < 0)
                {
                    Change.Foreground = new SolidColorBrush(Colors.Red);
                }
                
                if (CurrentQuote.MarketCapitalization == "")
                {
                    MarketCapitalization.Text = "N/A";
                }
                else
                {
                    MarketCapitalization.Text = CurrentQuote.MarketCapitalization.ToString();
                }

                Open.Text = CurrentQuote.Open.ToString();
                decimal volume = CurrentQuote.Volume.Value;
                volume = volume / 1000000;
                decimal avgVol = CurrentQuote.AverageDailyVolume.Value;
                avgVol = avgVol / 1000000;
                Vol_AvgVol.Text = volume.ToString("#.#") + "M/" + avgVol.ToString("#.#") + "M";
                DaysRange.Text = CurrentQuote.DailyLow.ToString() + " - " + CurrentQuote.DailyHigh.ToString();
                if (CurrentQuote.PeRatio == null)
                {
                    PERatio.Text = "N/A";
                }
                else
                {
                    PERatio.Text = CurrentQuote.PeRatio.ToString();
                }
                YearsRange.Text = CurrentQuote.YearlyLow.ToString() + " - " + CurrentQuote.YearlyHigh.ToString();
                if (CurrentQuote.DividendYield == null)
                {
                    DividendYield.Text = "N/A";
                }
                else
                {
                    DividendYield.Text = CurrentQuote.DividendYield.ToString();
                }

                RssView.Symbol = CurrentSymbol;
                ProgressBar.IsVisible = false;
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
            if (panoramaIndex == 3)
            {
                progressBar.Value = 0;
                ProgressBar.IsVisible = true;
                progressBar.IsIndeterminate = true;
                RssView.LoadData(ProgressBar);
            }
        }

        private void CurrentView_Refresh(object sender, EventArgs e)
        {
            RefreshView(RootPanorama.SelectedIndex);
        }

        private void NavigateTo_Settings(object sender, EventArgs e)
        {

        }

        private void GraphHolder_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
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

        public void LoadData()
        {
            System.Diagnostics.Debug.WriteLine("Now start to load rss data for symbol " + Symbol);
            List<string> sym = new List<string>() { Symbol };
            HttpWrapper.get(YahooAPI.GetRssXmlUrl(sym), null,
                delegate(Stream str)
                {
                    try
                    {
                        StreamReader reader = new StreamReader(str);
                        string response = reader.ReadToEnd();
                        System.Diagnostics.Debug.WriteLine("stream2str: " + response);

                        /*ObservableCollection<RssItemViewModel> tmp = YahooAPI.ParseRssXml(response);

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            foreach (RssItemViewModel r in tmp) TweetCollection.Add(r);
                            this.IsDataLoaded = true;
                        });
                         */
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        System.Diagnostics.Debug.WriteLine(ex.Source);
                    }
                },
                delegate(String reason)
                {
                    System.Diagnostics.Debug.WriteLine("Error: " + reason);
                }
            );
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