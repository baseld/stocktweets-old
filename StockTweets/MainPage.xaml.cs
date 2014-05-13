using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace StockTweets
{
    public partial class MainPage : PhoneApplicationPage
    {
        public static EventHandler pageTimerHandler = null;
        private static ProgressIndicator progressBar = null;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Set the data context of the listbox control to the sample data
            DataContext = App.ViewModel;
            this.Loaded += new RoutedEventHandler(MainPage_Loaded);
            progressBar = new ProgressIndicator();
            progressBar.IsIndeterminate = true;
            SystemTray.SetProgressIndicator(this, progressBar);
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
                if (App.Timer != null)
                {
                    pageTimerHandler = new EventHandler(dispatcherTimer_Tick);
                    App.Timer.Tick += pageTimerHandler;
                    if (!App.Timer.IsEnabled) App.Timer.Start();
                }
            }
        }

        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("A tick event is triggered.");
            CurrentList_RefreshView();
        }

        //app bar add to watchlist
        private void addList_Click(object sender, EventArgs e)
        {
            if (App.Timer != null) App.Timer.Stop();
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
                if (App.Timer != null) App.Timer.Start();
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
            if (App.Timer != null) App.Timer.Stop();

            PortfolioViewModel currentView = (PortfolioViewModel)PortfolioPivot.SelectedItem;
            Portfolio currentPortfolio = App.GetPortfolio(currentView.Title);
            List<string> currentStockList = currentPortfolio.StockList;

            // do not refresh if the portfolio has nothing
            if (currentStockList.Count == 0) return;
            
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (obj, args) =>
            {
                try
                {
                    YahooAPI.UpdateQuotes(args.Result.ToString());
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    System.Diagnostics.Debug.WriteLine(ex.Source);
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                }
                if (App.Timer != null) App.Timer.Start();
            };
            client.DownloadStringAsync(YahooAPI.GetQuotesXmlUrl(currentStockList));
        }

        private void CurrentList_Refresh(object sender, EventArgs e)
        {
            CurrentList_RefreshView();
        }

        private void CurrentList_Delete(object sender, EventArgs e)
        {
            if (App.Timer != null) App.Timer.Stop();
            PortfolioViewModel currentView = (PortfolioViewModel)PortfolioPivot.SelectedItem;
            int currentIndex = PortfolioPivot.SelectedIndex - 1;
            if (currentIndex < 0) currentIndex = 0;

            App.DeletePortfolio(App.GetPortfolio(currentView.Title));
            App.ViewModel.PageCollection.Remove(currentView);
            PortfolioPivot.SelectedIndex = currentIndex;
            
            if (App.Timer != null) App.Timer.Start();
        }

        private void CurrentList_Edit(object sender, EventArgs e)
        {
            if (App.Timer != null) App.Timer.Stop();
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
                if (App.Timer != null) App.Timer.Start();
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
            if (App.Timer != null) App.Timer.Stop();

            List<string> syms = App.StrToSymbols(KeywordStr.Text);

            if (syms.Count == 0)
            {
                string message = "Please provide symbol(s) to search, separated by a comma.\nE.g., QQQ,SPY";
                string caption = "Empty symbol to search";
                MessageBoxButton buttons = MessageBoxButton.OK;
                MessageBoxResult result = MessageBox.Show(message, caption, buttons);
                return;
            }

            progressBar.IsVisible = true;
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (obj, args) =>
            {
                try
                {
                    YahooAPI.UpdateQuotes(args.Result);
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
                }
                catch (Exception ex)
                {
                    // can be caused by network issue
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                    System.Diagnostics.Debug.WriteLine(ex.Source);
                    System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                }
                if (App.Timer != null) App.Timer.Start();
                progressBar.IsVisible = false;
            };
            client.DownloadStringAsync(YahooAPI.GetQuotesXmlUrl(syms));
        }

        private void ListBox_onSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ListBox lb = sender as ListBox;
            if (lb.SelectedIndex == -1) return;

            if (App.Timer != null)
            {
                App.Timer.Stop();
                App.Timer.Tick -= pageTimerHandler;
            }
            System.Diagnostics.Debug.WriteLine("selection is changed!\n - dataContext type is " + lb.DataContext.GetType());
            System.Diagnostics.Debug.WriteLine(" - selected index is " + lb.SelectedIndex.ToString());

            string symbol = ((PortfolioViewModel)lb.DataContext).StockViews.ElementAt(lb.SelectedIndex).Symbol;
            System.Diagnostics.Debug.WriteLine(" - symbol to navigate is " + symbol);
            
            NavigationService.Navigate(new Uri("/DetailedQuotePage.xaml?symbol=" + symbol, UriKind.Relative));
        }

        // this function is used to trigger new features
        private void PickTopSymbolsFromServer(object sender, EventArgs e)
        {
            if (App.Timer != null) App.Timer.Stop();
            progressBar.IsVisible = true;
            WebClient client = new WebClient();
            client.DownloadStringCompleted += (obj, args) =>
            {
                progressBar.IsVisible = false;
                KeywordStr.Text = args.Result;
                SearchButton_onClick(obj, null);
                KeywordStr.Text = "";
            };
            client.DownloadStringAsync(YahooAPI.GetStockAppTopPickUri());
        }

        private void NavigateTo_Settings(object sender, EventArgs e)
        {
            if (App.Timer != null)
            {
                App.Timer.Stop();
                App.Timer.Tick -= pageTimerHandler;
            }
            NavigationService.Navigate(new Uri("/SettingsPage.xaml", UriKind.Relative));
        }

        private void PortfolioPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

    }

}