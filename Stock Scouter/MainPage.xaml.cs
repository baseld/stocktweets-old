using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml.Linq;

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
                if (App.IsAutoRefreshEnabled)
                {
                    dispatcherTimer = new DispatcherTimer();
                    dispatcherTimer.Tick += new EventHandler(dispatcherTimer_Tick);
                    dispatcherTimer.Interval = new TimeSpan(0, 0, App.AutoRefreshInterval);
                }
            }
            if (!App.IsAutoRefreshEnabled) dispatcherTimer = null;
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
                    if (!App.PortfolioExists(tb.Text))
                    {
                        // add new portfolio
                        Portfolio p = App.GetPortfolio(tb.Text);
                        App.AddPortfolio(p);
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
            Portfolio currentPortfolio = App.GetPortfolio(currentView.Title);
            List<string> currentStockList = currentPortfolio.StockList;

            // do not refresh if the portfolio has nothing
            if (currentStockList.Count == 0)
            {
                if (dispatcherTimer != null) dispatcherTimer.Stop();
                return;
            }

            YahooAPI.get(YahooAPI.GetQuotesXmlUrl(currentStockList), null,
                delegate(Stream str)
                {
                    System.Diagnostics.Debug.WriteLine("Successfully get new data for stocks");
                    // convert stream to string
                    try
                    {
                        StreamReader reader = new StreamReader(str);
                        string result = reader.ReadToEnd();
                        IEnumerable<XElement> xmlObjs = YahooAPI.ParseQuotesXml(result);
                        //System.Diagnostics.Debug.WriteLine("stream2str: " + result);
                        
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            YahooAPI.UpdateQuotes(xmlObjs);
                            
                            //currentView.LoadData();
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
            if (dispatcherTimer != null) dispatcherTimer.Stop();
            PortfolioViewModel currentView = (PortfolioViewModel)PortfolioPivot.SelectedItem;
            App.DeletePortfolio(App.GetPortfolio(currentView.Title));
            App.ViewModel.LoadData();
            if (dispatcherTimer != null) dispatcherTimer.Start();
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
                    if (!App.PortfolioExists(tb.Text))
                    {
                        PortfolioViewModel currentView = (PortfolioViewModel)PortfolioPivot.SelectedItem;
                        int currentViewIndex = PortfolioPivot.SelectedIndex;
                        Portfolio currentPortfolio = App.GetPortfolio(currentView.Title);
                        currentPortfolio.Name = tb.Text;
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
            Quote item = obj.DataContext as Quote;
            PortfolioViewModel currentView = (PortfolioViewModel)PortfolioPivot.SelectedItem;
            Portfolio currentPortfolio = App.GetPortfolio(currentView.Title);
            currentView.RemoveStockFromView(item);
            currentPortfolio.RemoveQuote(item);
            //currentView.LoadData();
        }

        private void SearchButton_onClick(object sender, RoutedEventArgs e)
        {
            if (dispatcherTimer != null) dispatcherTimer.Stop();

            List<string> syms = new List<string>();
            string[] symbols = KeywordStr.Text.Split(',');

            foreach (string s in symbols)
            {
                if (s.Length == 0) continue;
                if (!syms.Contains(s)) syms.Add(s);
            }

            System.Diagnostics.Debug.WriteLine(String.Join(",", symbols));

            if (syms.Count == 0)
            {
                string message = "Please provide symbol(s) to search, separated by a comma.\nE.g., QQQ,SPY";
                string caption = "Empty symbol to search";
                MessageBoxButton buttons = MessageBoxButton.OK;
                MessageBoxResult result = MessageBox.Show(message, caption, buttons);
                return;
            }

            YahooAPI.get(YahooAPI.GetQuotesXmlUrl(syms), null,
                delegate(Stream str)
                {
                    System.Diagnostics.Debug.WriteLine("Success");
                    // convert stream to string
                    try
                    {
                        StreamReader reader = new StreamReader(str);
                        string result = reader.ReadToEnd();
                        IEnumerable<XElement> xmlObjs = YahooAPI.ParseQuotesXml(result);
                        //System.Diagnostics.Debug.WriteLine("stream2str: " + result);
                        
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            YahooAPI.UpdateQuotes(xmlObjs);
                            //this is not good though
                            
                            PortfolioViewModel currentView = (PortfolioViewModel)PortfolioPivot.SelectedItem;
                            Portfolio currentPortfolio = App.GetPortfolio(currentView.Title);
                            foreach (string s in syms)
                            {
                                Quote quote = App.GetQuote(s);
                                if (quote == null)
                                {
                                    System.Diagnostics.Debug.WriteLine("Symbol " + s + " was not found in db.");
                                    continue;
                                }
                                if (currentPortfolio.AddQuote(quote))
                                {
                                    currentView.AddStockToView(quote);
                                }
                            }
                            //App.SavePortfolio(currentPortfolio);
                            //currentView.LoadData();
                            if (dispatcherTimer != null) dispatcherTimer.Start();
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

        private void ListBox_onSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("selection is changed! sender type is " + sender.GetType());
            ListBox lb = sender as ListBox;
            if (lb.SelectedIndex == -1) return;
            System.Diagnostics.Debug.WriteLine("selected index is " + lb.SelectedIndex.ToString());

            // what a stupid way!
            string symbol = App.GetPortfolio(((PortfolioViewModel)PortfolioPivot.SelectedItem).Title).StockList.ElementAt(lb.SelectedIndex);

            System.Diagnostics.Debug.WriteLine("symbol is " + symbol);
            if (dispatcherTimer != null) dispatcherTimer.Stop();
            NavigationService.Navigate(new Uri("/DetailedQuotePage.xaml?symbol=" + symbol, UriKind.Relative));
            lb.SelectedIndex = -1; // reset index
        }

        // this function is used to trigger new features
        private void Test_Trigger(object sender, EventArgs e)
        {
            KeywordStr.Text = "QQQ,SPY,MSFT,,DOESNOTEXIST";
            SearchButton_onClick(SearchButton, null);
            KeywordStr.Text = "";
        }

        private void NavigateTo_Settings(object sender, EventArgs e)
        {
            if (dispatcherTimer != null) dispatcherTimer.Stop();
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

    }

}