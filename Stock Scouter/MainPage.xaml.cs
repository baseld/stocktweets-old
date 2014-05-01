using Microsoft.Phone.Controls;
using Stock_Scouter.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Stock_Scouter
{
    public partial class MainPage : PhoneApplicationPage
    {
        private static DispatcherTimer dispatcherTimer = null;

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
                if (AppSettings.EnableAutoRefresh)
                {
                    dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
                    dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                    dispatcherTimer.Interval = new TimeSpan(0, 0, AppSettings.AutoRefreshInterval);
                }
            }
            if (!AppSettings.EnableAutoRefresh) dispatcherTimer = null;
            if (dispatcherTimer != null) dispatcherTimer.Start();
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("A tick event is triggered.");
            CurrentList_RefreshView();
        }

        //app bar add to watchlist
        private void addList_Click(object sender, EventArgs e)
        {
            //NavigationService.Navigate(new Uri("/AddList.xaml", UriKind.Relative));
            if (dispatcherTimer != null) dispatcherTimer.Stop();
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
                if (dispatcherTimer != null) dispatcherTimer.Start();
            };

            // focus on the text box by default
            box.Loaded += (s, ev) =>
            {
                tb.Focus();
            };

            box.Show();
        }

        private void CurrentList_RefreshView()
        {
            PortfolioViewModel currentView = (PortfolioViewModel)PortfolioPivot.SelectedItem;
            Portfolio currentPortfolio = AppSettings.GetPortfolio(currentView.Title);
            List<string> currentStockList = currentPortfolio.GetStockList();

            // do not refresh if the portfolio has nothing
            if (currentStockList.Count == 0)
            {
                if (dispatcherTimer != null) dispatcherTimer.Stop();
                return;
            }

            YahooFinance.get(YahooFinance.GetQuotesXmlUrl(currentStockList), null,
                delegate(Stream str)
                {
                    System.Diagnostics.Debug.WriteLine("Successfully get new data for stocks");
                    // convert stream to string
                    try
                    {
                        StreamReader reader = new StreamReader(str);
                        string result = reader.ReadToEnd();
                        //System.Diagnostics.Debug.WriteLine("stream2str: " + result);
                        ObservableCollection<Quote> quotes = YahooFinance.GetQuotes(result);

                        foreach (Quote q in quotes)
                        {
                            AppSettings.SetQuote(q.Symbol, q);
                        }

                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            AppSettings.SetPortfolio(currentPortfolio.Name, currentPortfolio);
                            currentView.LoadData();
                            //App.ViewModel.LoadData();
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

        private void CurrentList_Refresh(object sender, EventArgs e)
        {
            CurrentList_RefreshView();
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
            if (dispatcherTimer != null) dispatcherTimer.Stop();
            var tb = new TextBox();
            var box = new CustomMessageBox()
            {
                Caption = "Rename list",
                Message = "Please enter the new name",
                LeftButtonContent = "Rename",
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
                        PortfolioViewModel currentView = (PortfolioViewModel)PortfolioPivot.SelectedItem;
                        int currentViewIndex = PortfolioPivot.SelectedIndex;
                        Portfolio currentPortfolio = AppSettings.GetPortfolio(currentView.Title);
                        AppSettings.RenamePortfolio(tb.Text, currentPortfolio);
                        currentView.Title = tb.Text;
                        //currentView.LoadData();
                        App.ViewModel.LoadData();
                        PortfolioPivot.SelectedIndex = currentViewIndex;
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
                if (dispatcherTimer != null) dispatcherTimer.Start();
            };

            // focus on the text box by default
            box.Loaded += (s, ev) =>
            {
                tb.Focus();
            };

            box.Show();
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
            if (dispatcherTimer != null) dispatcherTimer.Stop();
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
                                    System.Diagnostics.Debug.WriteLine("Added " + quote.Symbol);
                                }
                            }
                            AppSettings.SetPortfolio(currentPortfolio.Name, currentPortfolio);
                            currentView.LoadData();
                            if (dispatcherTimer != null) dispatcherTimer.Start();
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
            if (dispatcherTimer != null) dispatcherTimer.Stop();
            NavigationService.Navigate(new Uri("/EachstockPages.xaml?symbol=" + symbol, UriKind.Relative));
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
                                }
                            }
                            AppSettings.SetPortfolio(currentPortfolio.Name, currentPortfolio);
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

        private void NavigateTo_Settings(object sender, EventArgs e)
        {
            if (dispatcherTimer != null) dispatcherTimer.Stop();
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

    }
}