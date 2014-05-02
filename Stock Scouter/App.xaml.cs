using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.ComponentModel;
using System.Collections.ObjectModel;
using Stock_Scouter.Models;

namespace Stock_Scouter
{
    public partial class App : Application
    {

        private static int DEFAULT_AUTO_REFRESH_INTERVAL = 5; // in seconds
        private static bool DEFAULT_ENABLE_AUTO_REFRESH = true;
        private static string DEFAULT_PORTFOLIO_NAME = "Watchlist";

        private static IsolatedStorageSettings storageSpace = IsolatedStorageSettings.ApplicationSettings;
        private static List<Portfolio> portfolioList = null;
        private static ObservableCollection<Quote> quoteList = null;
        private static MainViewModel viewModel = null;

        /// <summary>
        /// A static ViewModel used by the views to bind against.
        /// </summary>
        /// <returns>The MainViewModel object.</returns>
        public static MainViewModel ViewModel
        {
            get
            {
                // Delay creation of the view model until necessary
                if (viewModel == null)
                    viewModel = new MainViewModel();

                return viewModel;
            }
        }

        public static bool IsAutoRefreshEnabled
        {
            get
            {
                if (!storageSpace.Contains("EnableAutoRefresh"))
                    storageSpace.Add("EnableAutoRefresh", DEFAULT_ENABLE_AUTO_REFRESH);
                return (bool)storageSpace["EnableAutoRefresh"];
            }
            set
            {
                storageSpace["EnableAutoRefresh"] = value;
            }
        }

        public static int AutoRefreshInterval
        {
            get
            {
                if (!storageSpace.Contains("AutoRefreshInterval"))
                    storageSpace.Add("AutoRefreshInterval", DEFAULT_AUTO_REFRESH_INTERVAL);
                return (int)storageSpace["AutoRefreshInterval"];
            }
            set
            {
                storageSpace["AutoRefreshInterval"] = value;
            }
        }

        public static List<Portfolio> PortfolioList
        {
            get
            {
                if (portfolioList == null)
                {
                    if (storageSpace.Contains("PortfolioList"))
                    {
                        portfolioList = (List<Portfolio>)storageSpace["PortfolioList"];
                    }
                    else
                    {
                        portfolioList = new List<Portfolio>();
                        storageSpace.Add("PortfolioList", portfolioList);
                    }
                }

                if (portfolioList.Count == 0)
                {
                    portfolioList.Add(new Portfolio() { Name = DEFAULT_PORTFOLIO_NAME, Order = 0 });
                }

                return portfolioList;
            }
            set
            {
                portfolioList = value;
            }
        }

        public static void AddPortfolio(Portfolio p)
        {
            PortfolioList.Add(p);
            System.Diagnostics.Debug.WriteLine("Added portfolio " + p.Name + " to list.");
        }

        public static bool PortfolioExists(string pkey)
        {
            var query = from p in PortfolioList where p.Name == pkey select p;
            return query.Count() > 0;
        }

        public static Portfolio GetPortfolio(string pkey)
        {
            var query = from p in PortfolioList where p.Name == pkey select p;
            System.Diagnostics.Debug.WriteLine(query.Count() + " result(s) of querying portfolio " + pkey + ".");
            if (query.Count() > 0) return query.First();
            
            Portfolio np = new Portfolio() { Name = pkey, Order = PortfolioList.Count};
            //PortfolioList.Add(np);
            System.Diagnostics.Debug.WriteLine("Portfolio " + pkey + " not found. Created it.");
            return np;
        }

        public static void SavePortfolio(Portfolio sp)
        {
            var query = from p in PortfolioList where p.Name == sp.Name select p;
            if (query.Count() > 0)
            {
                Portfolio p = query.First();
                int index = PortfolioList.IndexOf(query.First());
                PortfolioList.RemoveAt(index);
                PortfolioList.Insert(index, sp);
            }
            else
            {
                AddPortfolio(sp);
                System.Diagnostics.Debug.WriteLine("Save non-existent portfolio " + sp.Name);
            }
        }

        public static void DeletePortfolio(Portfolio p)
        {
            PortfolioList.Remove(p);
        }

        public static ObservableCollection<Quote> QuoteList
        {
            get
            {
                if (quoteList == null)
                {
                    if (storageSpace.Contains("QuoteList"))
                    {
                        quoteList = (ObservableCollection<Quote>)storageSpace["QuoteList"];
                    }
                    else
                    {
                        quoteList = new ObservableCollection<Quote>();
                        storageSpace.Add("QuoteList", quoteList);
                    }
                }

                return quoteList;
            }
            set
            {
                quoteList = value;
            }
        }

        public static Quote GetQuote(string symbol)
        {
            var query = from q in QuoteList where q.Symbol == symbol select q;
            if (query.Count() > 0) return query.First();
            return null;
        }

        public static void AddQuote(Quote quote)
        {
            QuoteList.Add(quote);
        }

        public static void DeleteQuote(string symbol)
        {
            var query = from q in QuoteList where q.Symbol == symbol && q.ReferenceCount > 0 select q;
            foreach (Quote q in query)
            {
                System.Diagnostics.Debug.WriteLine("Quote " + q.Symbol + " gets deleted since its ref count is 0.");
                QuoteList.Remove(q);
            }
        }

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are handed off GPU with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;

                // Disable the application idle detection by setting the UserIdleDetectionMode property of the
                // application's PhoneApplicationService object to Disabled.
                // Caution:- Use this under debug mode only. Application that disables user idle detection will continue to run
                // and consume battery power when the user is not using the phone.
                PhoneApplicationService.Current.UserIdleDetectionMode = IdleDetectionMode.Disabled;
            }

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            // Ensure that application state is restored appropriately
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            // Ensure that required application state is persisted here.
            storageSpace["PortfolioList"] = PortfolioList;
            storageSpace["QuoteList"] = QuoteList;
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            storageSpace["PortfolioList"] = PortfolioList;
            storageSpace["QuoteList"] = QuoteList;
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }

    /**
     * The quote model is adapted from the example given by http://jarloo.com
     * Licensed under CC-BY-SA 3.0 Unported
     */

    public class Quote : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string symbol;
        private decimal? averageDailyVolume;
        private decimal? bid;
        private decimal? ask;
        private decimal? bookValue;
        private decimal? change;
        private decimal? dividendShare;
        private DateTime? lastTradeDate;
        private string lastTradeTime;
        private decimal? earningsShare;
        private decimal? epsEstimateCurrentYear;
        private decimal? epsEstimateNextYear;
        private decimal? epsEstimateNextQuarter;
        private decimal? dailyLow;
        private decimal? dailyHigh;
        private decimal? yearlyLow;
        private decimal? yearlyHigh;
        private decimal? marketCapitalization;
        private decimal? ebitda;
        private decimal? changeFromYearLow;
        private decimal? percentChangeFromYearLow;
        private decimal? changeFromYearHigh;
        private decimal? percentChangeFromYearHigh;
        private decimal? lastTradePrice;
        private decimal? fiftyDayMovingAverage;
        private decimal? twoHunderedDayMovingAverage;
        private decimal? changeFromTwoHundredDayMovingAverage;
        private decimal? percentChangeFromFiftyDayMovingAverage;
        private string name;
        private decimal? open;
        private decimal? previousClose;
        private string changeInPercent;
        private decimal? priceSales;
        private decimal? priceBook;
        private DateTime? exDividendDate;
        private decimal? pegRatio;
        private decimal? priceEpsEstimateCurrentYear;
        private decimal? priceEpsEstimateNextYear;
        private decimal? shortRatio;
        private decimal? oneYearPriceTarget;
        private decimal? dividendYield;
        private DateTime? dividendPayDate;
        private decimal? percentChangeFromTwoHundredDayMovingAverage;
        private decimal? peRatio;
        private decimal? volume;
        private string stockExchange;
        private DateTime lastUpdate;
        private int referenceCount = 0;

        public DateTime LastUpdate
        {
            get { return lastUpdate; }
            set
            {
                lastUpdate = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("LastUpdate"));
            }
        }

        public int ReferenceCount
        {
            get { return referenceCount; }
            set
            {
                referenceCount = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ReferenceCount"));
            }
        }

        public string StockExchange
        {
            get { return stockExchange; }
            set
            {
                stockExchange = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("StockExchange"));
            }
        }


        public decimal? Volume
        {
            get { return volume; }
            set
            {
                volume = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Volume"));
            }
        }

        public decimal? PeRatio
        {
            get { return peRatio; }
            set
            {
                peRatio = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("PeRatio"));
            }
        }

        public decimal? PercentChangeFromTwoHundredDayMovingAverage
        {
            get { return percentChangeFromTwoHundredDayMovingAverage; }
            set
            {
                percentChangeFromTwoHundredDayMovingAverage = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("PercentChangeFromTwoHundredDayMovingAverage"));
            }
        }

        public Quote(string ticker)
        {
            symbol = ticker;
        }

        public DateTime? DividendPayDate
        {
            get { return dividendPayDate; }
            set
            {
                dividendPayDate = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("DividendPayDate"));
            }
        }

        public decimal? DividendYield
        {
            get { return dividendYield; }
            set
            {
                dividendYield = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("DividendYield"));
            }
        }


        public decimal? OneYearPriceTarget
        {
            get { return oneYearPriceTarget; }
            set
            {
                oneYearPriceTarget = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("OneYearPriceTarget"));
            }
        }

        public decimal? ShortRatio
        {
            get { return shortRatio; }
            set
            {
                shortRatio = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ShortRatio"));
            }
        }


        public decimal? PriceEpsEstimateNextYear
        {
            get { return priceEpsEstimateNextYear; }
            set
            {
                priceEpsEstimateNextYear = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("PriceEpsEstimateNextYear"));
            }
        }


        public decimal? PriceEpsEstimateCurrentYear
        {
            get { return priceEpsEstimateCurrentYear; }
            set
            {
                priceEpsEstimateCurrentYear = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("PriceEpsEstimateCurrentYear"));
            }
        }


        public decimal? PegRatio
        {
            get { return pegRatio; }
            set
            {
                pegRatio = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("PegRatio"));
            }
        }


        public DateTime? ExDividendDate
        {
            get { return exDividendDate; }
            set
            {
                exDividendDate = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ExDividendDate"));
            }
        }


        public decimal? PriceBook
        {
            get { return priceBook; }
            set
            {
                priceBook = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("PriceBook"));
            }
        }


        public decimal? PriceSales
        {
            get { return priceSales; }
            set
            {
                priceSales = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("PriceSales"));
            }
        }

        public string ChangeInPercent
        {
            get { return changeInPercent; }
            set
            {
                changeInPercent = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ChangeInPercent"));
            }
        }

        public decimal? PreviousClose
        {
            get { return previousClose; }
            set
            {
                previousClose = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("PreviousClose"));
            }
        }

        public decimal? Open
        {
            get { return open; }
            set
            {
                open = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Open"));
            }
        }


        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }


        public decimal? PercentChangeFromFiftyDayMovingAverage
        {
            get { return percentChangeFromFiftyDayMovingAverage; }
            set
            {
                percentChangeFromFiftyDayMovingAverage = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("PercentChangeFromFiftyDayMovingAverage"));
            }
        }


        public decimal? ChangeFromTwoHundredDayMovingAverage
        {
            get { return changeFromTwoHundredDayMovingAverage; }
            set
            {
                changeFromTwoHundredDayMovingAverage = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ChangeFromTwoHundredDayMovingAverage"));
            }
        }


        public decimal? TwoHunderedDayMovingAverage
        {
            get { return twoHunderedDayMovingAverage; }
            set
            {
                twoHunderedDayMovingAverage = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("TwoHunderedDayMovingAverage"));
            }
        }


        public decimal? FiftyDayMovingAverage
        {
            get { return fiftyDayMovingAverage; }
            set
            {
                fiftyDayMovingAverage = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("FiftyDayMovingAverage"));
            }
        }


        public decimal? LastTradePrice
        {
            get { return lastTradePrice; }
            set
            {
                lastTradePrice = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("LastTradePrice"));
            }
        }


        public decimal? PercentChangeFromYearHigh
        {
            get { return percentChangeFromYearHigh; }
            set
            {
                percentChangeFromYearHigh = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("PercentChangeFromYearHigh"));
            }
        }


        public decimal? ChangeFromYearHigh
        {
            get { return changeFromYearHigh; }
            set
            {
                changeFromYearHigh = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ChangeFromYearHigh"));
            }
        }


        public decimal? PercentChangeFromYearLow
        {
            get { return percentChangeFromYearLow; }
            set
            {
                percentChangeFromYearLow = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("PercentChangeFromYearLow"));
            }
        }


        public decimal? ChangeFromYearLow
        {
            get { return changeFromYearLow; }
            set
            {
                changeFromYearLow = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("ChangeFromYearLow"));
            }
        }


        public decimal? Ebitda
        {
            get { return ebitda; }
            set
            {
                ebitda = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Ebitda"));
            }
        }


        public decimal? MarketCapitalization
        {
            get { return marketCapitalization; }
            set
            {
                marketCapitalization = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("MarketCapitalization"));
            }
        }


        public decimal? YearlyHigh
        {
            get { return yearlyHigh; }
            set
            {
                yearlyHigh = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("YearlyHigh"));
            }
        }


        public decimal? YearlyLow
        {
            get { return yearlyLow; }
            set
            {
                yearlyLow = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("YearlyLow"));
            }
        }


        public decimal? DailyHigh
        {
            get { return dailyHigh; }
            set
            {
                dailyHigh = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("DailyHigh"));
            }
        }


        public decimal? DailyLow
        {
            get { return dailyLow; }
            set
            {
                dailyLow = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("DailyLow"));
            }
        }


        public decimal? EpsEstimateNextQuarter
        {
            get { return epsEstimateNextQuarter; }
            set
            {
                epsEstimateNextQuarter = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("EpsEstimateNextQuarter"));
            }
        }


        public decimal? EpsEstimateNextYear
        {
            get { return epsEstimateNextYear; }
            set
            {
                epsEstimateNextYear = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("EpsEstimateNextYear"));
            }
        }


        public decimal? EpsEstimateCurrentYear
        {
            get { return epsEstimateCurrentYear; }
            set
            {
                epsEstimateCurrentYear = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("EpsEstimateCurrentYear"));
            }
        }


        public decimal? EarningsShare
        {
            get { return earningsShare; }
            set
            {
                earningsShare = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("EarningsShare"));
            }
        }


        public DateTime? LastTradeDate
        {
            get { return lastTradeDate; }
            set
            {
                lastTradeDate = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("LastTradeDate"));
            }
        }

        public string LastTradeTime
        {
            get { return lastTradeTime; }
            set
            {
                lastTradeTime = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("LastTradeTime"));
            }
        }

        public decimal? DividendShare
        {
            get { return dividendShare; }
            set
            {
                dividendShare = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("DividendShare"));
            }
        }


        public decimal? Change
        {
            get { return change; }
            set
            {
                change = value;
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("Change"));
                    PropertyChanged(this, new PropertyChangedEventArgs("Color"));
                }
            }
        }

        public decimal? BookValue
        {
            get { return bookValue; }
            set
            {
                bookValue = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("BookValue"));
            }
        }


        public decimal? Ask
        {
            get { return ask; }
            set
            {
                ask = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Ask"));
            }
        }


        public decimal? Bid
        {
            get { return bid; }
            set
            {
                bid = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Bid"));
            }
        }


        public decimal? AverageDailyVolume
        {
            get { return averageDailyVolume; }
            set
            {
                averageDailyVolume = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("AverageDailyVolume"));
            }
        }


        public string Symbol
        {
            get { return symbol; }
            set
            {
                symbol = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Symbol"));
            }
        }

        public SolidColorBrush Color
        {
            get
            {
                if (Convert.ToDouble(Change) < 0) return new SolidColorBrush(Colors.Red);
                return new SolidColorBrush(Colors.Green);
            }
        }

        public Quote()
        {
        }
    }

    public class Portfolio : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private List<string> _stockList;
        private string _portfolioName;
        private int _order;

        public string Name
        {
            get
            {
                return _portfolioName;
            }
            set
            {
                this._portfolioName = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Name"));
            }
        }

        public int Order
        {
            get
            {
                return this._order;
            }
            set
            {
                _order = value;
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("Order"));
            }
        }

        public List<string> StockList
        {
            get
            {
                return this._stockList;
            }
        }

        public Portfolio()
        {
            _stockList = new List<string>();
        }

        public bool AddQuote(Quote q)
        {
            if (StockList.Contains(q.Symbol))
            {
                q.ReferenceCount++;
                return false;
            }
            StockList.Add(q.Symbol);
            System.Diagnostics.Debug.WriteLine("Added Quote " + q.Symbol + " to Portfolio " + Name);
            if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("StockList"));
            return true;
        }

        public void RemoveQuote(Quote s)
        {
            if (StockList.Contains(s.Symbol))
            {
                StockList.Remove(s.Symbol);
                App.DeleteQuote(s.Symbol);
                if (PropertyChanged != null) PropertyChanged(this, new PropertyChangedEventArgs("StockList"));
            }
        }

    }
}