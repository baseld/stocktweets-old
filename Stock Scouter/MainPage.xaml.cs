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
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.IO;
using System.Collections.ObjectModel;
using Stock_Scouter.Models;

namespace Stock_Scouter
{
    public partial class MainPage : PhoneApplicationPage
    {
        private string _currentPivotTitle;

        public string CurrentPivotTitle
        {
            get
            {
                return this._currentPivotTitle;
            }
            set
            {
                this._currentPivotTitle = value;
            }
        }

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
        }

        // may be pretty slow
        public FrameworkElement GetDescendantByName(FrameworkElement element, string name)
        {
            if (element == null || string.IsNullOrWhiteSpace(name)) { return null; }

            if (name.Equals(element.Name, StringComparison.OrdinalIgnoreCase))
            {
                return element;
            }
            var childCount = VisualTreeHelper.GetChildrenCount(element);
            for (int i = 0; i < childCount; i++)
            {
                var result = GetDescendantByName(VisualTreeHelper.GetChild(element, i) as FrameworkElement, name);
                if (result != null) { return result; }
            }
            return null;
        }

        // Load data for the ViewModel Items
        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (!App.ViewModel.IsDataLoaded)
            {
                App.ViewModel.LoadData();
            }
        }

        //app bar add to watchlist
        private void addList_Click(object sender, EventArgs e)
        {
            //NavigationService.Navigate(new Uri("/AddList.xaml", UriKind.Relative));
            var tb = new TextBox();
            var box = new CustomMessageBox()
            {
                Caption = "Add a new list",
                Message = "Please enter the a list name",
                LeftButtonContent = "Add",
                RightButtonContent = "Cancel",
                Content = tb,
                IsFullScreen = false
            };
            box.Dismissed += (s, ev) =>
            {
                if (ev.Result == CustomMessageBoxResult.LeftButton)
                {
                    if (!AppSettings.isPortfolioExist(tb.Text))
                    {
                        // add new portfolio
                        Portfolio p = AppSettings.GetPortfolio(tb.Text);
                        AppSettings.addPortfolio(p);
                        // reload portfolio list
                        App.ViewModel.LoadData();
                        // switch to the newly created portfolio
                        PortfolioPivot.SelectedIndex = App.ViewModel.PageCollection.Count - 1;
                    }
                    else
                    {
                        // prompt for redundant list name
                        string message = "Oops, a list named " + tb.Text + " already exists.\nPlease try another one.";
                        string caption = "List name used";
                        MessageBoxButton buttons = MessageBoxButton.OK;
                        MessageBoxResult result = MessageBox.Show(message, caption, buttons);
                    }
                }
            };

            // focus on the text box by default
            box.Loaded += (s, ev) =>
            {
                tb.Focus();
            };

            box.Show();
        }

        private void CurrentList_Refresh(object sender, EventArgs e)
        {
            App.ViewModel.LoadData();
        }

        private void CurrentList_Delete(object sender, EventArgs e)
        {
            PortfolioViewModel currentView = (PortfolioViewModel)PortfolioPivot.SelectedItem;
            Portfolio currentPortfolio = AppSettings.GetPortfolio(currentView.Title);
            currentPortfolio.Remove();
            App.ViewModel.LoadData();
        }

        private void CurrentList_Edit(object sender, EventArgs e)
        {

        }

        private void ContextMenu_removeItem(object sender, RoutedEventArgs e)
        {
            // sender is a MenuItem.
            MenuItem obj = sender as MenuItem;
            StockBriefViewModel item = obj.DataContext as StockBriefViewModel;
            PortfolioViewModel currentView = (PortfolioViewModel)PortfolioPivot.SelectedItem;
            Portfolio currentPortfolio = AppSettings.GetPortfolio(currentView.Title);
            currentPortfolio.RemoveQuote(AppSettings.GetStock(item.Symbol));
            currentView.LoadData();
        }

        private void SearchButton_onClick(object sender, RoutedEventArgs e)
        {
            string symbol = KeywordStr.Text;
            System.Diagnostics.Debug.WriteLine("Symbol to search is " + symbol);

            List<string> syms = new List<string>();
            string[] symbols = symbol.Split(',');

            foreach (string s in symbols)
            {
                if (s.Length == 0) continue;
                if (!syms.Contains(s)) syms.Add(s);
            }

            if (syms.Count == 0)
            {
                string message = "Please provide more than one symbol to search, separated by a comma.";
                string caption = "Empty symbol to search";
                MessageBoxButton buttons = MessageBoxButton.OK;
                MessageBoxResult result = MessageBox.Show(message, caption, buttons);
                return;
            }

            YahooFinance.get(YahooFinance.GetQuotesXmlUrl(syms), null,
                delegate(Stream str)
                {
                    System.Diagnostics.Debug.WriteLine("Success");
                    // convert stream to string
                    try
                    {
                        StreamReader reader = new StreamReader(str);
                        string result = reader.ReadToEnd();
                        //System.Diagnostics.Debug.WriteLine("stream2str: " + result);
                        ObservableCollection<Quote> quotes = YahooFinance.GetQuotes(result);

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            PortfolioViewModel currentView = (PortfolioViewModel)PortfolioPivot.SelectedItem;
                            Portfolio currentPortfolio = AppSettings.GetPortfolio(currentView.Title);
                            foreach (Quote quote in quotes)
                            {
                                bool ret = currentPortfolio.addQuote(quote);
                                if (ret)
                                {
                                    System.Diagnostics.Debug.WriteLine(quote.Symbol);
                                    currentView.StockViews.Add(new StockBriefViewModel() { Symbol = quote.Symbol, Name = quote.Name, AskPrice = quote.Ask.ToString(), BidPrice = quote.Bid.ToString(), Change = quote.Change.ToString() });
                                }
                            }
                            currentView.LoadData();
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        System.Diagnostics.Debug.WriteLine(ex.Source);
                        //throw;
                    }
                },
                delegate(String reason)
                {
                    System.Diagnostics.Debug.WriteLine("Error: " + reason);
                }
            );
        }

        private void ListBox_onSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("selection is changed! sender type is " + sender.GetType());
            ListBox lb = sender as ListBox;
            if (lb.SelectedIndex == -1) return;
            System.Diagnostics.Debug.WriteLine("selected index is " + lb.SelectedIndex.ToString());

            // what a stupid way!
            string symbol = AppSettings.GetPortfolio(((PortfolioViewModel)PortfolioPivot.SelectedItem).Title).GetStockList().ElementAt(lb.SelectedIndex);

            System.Diagnostics.Debug.WriteLine("symbol is " + symbol);
            NavigationService.Navigate(new Uri("/DetailPage.xaml?symbol=" + symbol, UriKind.Relative));
            lb.SelectedIndex = -1; // reset index
        }

        // this function is used to trigger new features
        private void Test_Trigger(object sender, EventArgs e)
        {
            List<string> syms = new List<string>();
            syms.Add("QQQ");
            syms.Add("SPY");
            syms.Add("MSFT");
            syms.Add("");
            syms.Add("DOESNOTEXIST");

            YahooFinance.get(YahooFinance.GetQuotesXmlUrl(syms), null,
                delegate(Stream str)
                {
                    System.Diagnostics.Debug.WriteLine("Success");
                    // convert stream to string
                    try
                    {
                        StreamReader reader = new StreamReader(str);
                        string result = reader.ReadToEnd();
                        //System.Diagnostics.Debug.WriteLine("stream2str: " + result);
                        ObservableCollection<Quote> quotes = YahooFinance.GetQuotes(result);

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            PortfolioViewModel currentView = (PortfolioViewModel)PortfolioPivot.SelectedItem;
                            Portfolio currentPortfolio = AppSettings.GetPortfolio(currentView.Title);
                            foreach (Quote quote in quotes)
                            {
                                bool ret = currentPortfolio.addQuote(quote);
                                if (ret)
                                {
                                    System.Diagnostics.Debug.WriteLine(quote.Symbol);
                                    currentView.StockViews.Add(new StockBriefViewModel() { Symbol = quote.Symbol, Name = quote.Name, AskPrice = quote.Ask.ToString(), BidPrice = quote.Bid.ToString(), Change = quote.Change.ToString() });
                                }
                            }
                            currentView.LoadData();
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.Message);
                        System.Diagnostics.Debug.WriteLine(ex.Source);
                        //throw;
                    }
                },
                delegate(String reason)
                {
                    System.Diagnostics.Debug.WriteLine("Error: " + reason);
                }
            );
        }

    }
}