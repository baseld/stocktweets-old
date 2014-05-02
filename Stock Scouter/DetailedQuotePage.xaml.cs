using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Controls;
using System.Collections.ObjectModel;
using Microsoft.Phone.Shell;
using System.ComponentModel;
using System.IO;

namespace Stock_Scouter
{
    public partial class DetailedQuotePage : PhoneApplicationPage
    {
        private static string _currentSymbol;
        private static Quote _currentQuote;
        private static RssViewModel rssViewModel = null;
        private static TweetViewModel tweetViewModel = null;

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

        public DetailedQuotePage()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (NavigationContext.QueryString.TryGetValue("symbol", out _currentSymbol))
            {
                System.Diagnostics.Debug.WriteLine("DetailedQuotedPage: symbol is " + CurrentSymbol);
                CurrentQuote = App.GetQuote(CurrentSymbol);
                RssView.Symbol = CurrentSymbol;
            }
        }

        private void GraphIndicator_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // round step to 1
            GraphIndicator.Value = Math.Round(e.NewValue);
            System.Diagnostics.Debug.WriteLine("Slider got new value: " + GraphIndicator.Value);
            if (GraphIndicator.Value == 0)
            {
                GraphIndicator_Prompt.Text = "1d";
            }
            else if (GraphIndicator.Value == 1)
            {
                GraphIndicator_Prompt.Text = "1w";
            }
            else if (GraphIndicator.Value == 2)
            {
                GraphIndicator_Prompt.Text = "1m";
            }
            else if (GraphIndicator.Value == 3)
            {
                GraphIndicator_Prompt.Text = "3m";
            }
            else if (GraphIndicator.Value == 4)
            {
                GraphIndicator_Prompt.Text = "6m";
            }
            else if (GraphIndicator.Value == 5)
            {
                GraphIndicator_Prompt.Text = "1y";
            }
            else if (GraphIndicator.Value == 6)
            {
                GraphIndicator_Prompt.Text = "5y";
            }
            else
            {
                GraphIndicator_Prompt.Text = "??";
            }
        }

        private void GraphIndicator_LoadImage(object sender, System.Windows.Input.MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Slider: should load image now");
        }

        // open the link in the browser
        private void NewsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("The type of sender is " + sender.GetType());
            // sender is a ListBox
            RssItemViewModel rvm = ((ListBox)sender).SelectedItem as RssItemViewModel;
            
            WebBrowserTask webBrowserTask = new WebBrowserTask();
            webBrowserTask.Uri = new Uri(rvm.Link, UriKind.Absolute);
            webBrowserTask.Show();
        }

        // when panorama item changes
        private void Panorama_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Panorama p = sender as Panorama;
            int currentIndex = p.SelectedIndex;
            System.Diagnostics.Debug.WriteLine("Panorama selection is changed.\nselectedIndex is " + currentIndex);
            if (currentIndex == 2)
            {
                // news page
                RssView.LoadData();
            }
        }

        private void CurrentView_Refresh(object sender, EventArgs e)
        {

        }

        private void NavigateTo_Settings(object sender, EventArgs e)
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

        public void LoadData()
        {
            System.Diagnostics.Debug.WriteLine("Now start to load rss data for symbol " + Symbol);
            List<string> sym = new List<string>() { Symbol };
            YahooAPI.get(YahooAPI.GetRssXmlUrl(sym), null,
                delegate(Stream str)
                {
                    try
                    {
                        StreamReader reader = new StreamReader(str);
                        string response = reader.ReadToEnd();
                        System.Diagnostics.Debug.WriteLine("stream2str: " + response);

                        ObservableCollection<RssItemViewModel> tmp = YahooAPI.ParseRssXml(response);

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            RssCollection = tmp;
                            // foreach (RssItemViewModel r in tmp) RssCollection.Add(r);
                            // this.IsDataLoaded = true;
                        });
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
            YahooAPI.get(YahooAPI.GetRssXmlUrl(sym), null,
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