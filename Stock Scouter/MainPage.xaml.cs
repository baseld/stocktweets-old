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
            NavigationService.Navigate(new Uri("/AddList.xaml", UriKind.Relative));
        }

        private void ContextMenu_removeItem(object sender, RoutedEventArgs e)
        {
            if (sender is StockBriefViewModel)
            {
                System.Diagnostics.Debug.WriteLine("At least I guess the type right!");
            }
        }
        
        private void Pivot_Loaded(object sender, RoutedEventArgs e)
        {
        }
        
        private void SearchButton_onClick(object sender, RoutedEventArgs e)
        {
            string sym = ((System.Windows.Controls.TextBox)(GetDescendantByName(PortfolioPivot, "KeywordStr"))).Text;
            System.Diagnostics.Debug.WriteLine(sym);

            if (sym.Length == 0) return;

            string[] symbols = sym.Split(',');
            System.Diagnostics.Debug.WriteLine("There are " + symbols.Length.ToString() + " symbols in the string.");

            YahooFinance.get(YahooFinance.GetQuoteUri(symbols), null, 
                delegate(Stream str)
                {
                    System.Diagnostics.Debug.WriteLine("Success");
                    // convert stream to string
                    try
                    {
                        StreamReader reader = new StreamReader(str);
                        string result = reader.ReadToEnd();
                        System.Diagnostics.Debug.WriteLine("stream2str: " + result);
                        List<Stock> list = YahooFinance.CsvToStock(result);

                        Deployment.Current.Dispatcher.BeginInvoke(() => 
                        {
                            PortfolioViewModel currentView = (PortfolioViewModel)PortfolioPivot.SelectedItem;
                            Portfolio currentPortfolio = AppSettings.GetPortfolio(currentView.Title);
                            if (PortfolioPivot.SelectedItem is PortfolioViewModel)
                            {
                                System.Diagnostics.Debug.WriteLine("I got the portfolio view model: " + currentView.Title + "\n -stocks: " + currentPortfolio.GetStockList().ToString());
                            }
                            foreach (Stock s in list)
                            {
                                bool ret = currentPortfolio.addStock(s);
                                if (ret)
                                {
                                    System.Diagnostics.Debug.WriteLine(s.Symbol);
                                    currentView.StockViews.Add(new StockBriefViewModel() { Symbol = s.Symbol, Name = s.Name, AskPrice = s.AskPrice.ToString(), BidPrice = s.BidPrice.ToString(), DayRange = s.DayRange, Change = s.Change.ToString() });
                                    //PortfolioPivot.Items.Add(new StockBriefViewModel() { Symbol = s.Symbol, Name = s.Name, AskPrice = s.AskPrice.ToString(), BidPrice = s.BidPrice.ToString(), DayRange = s.DayRange, Change = s.Change.ToString() })
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

        private void CurrentList_Refresh(object sender, EventArgs e)
        {

        }

        private void CurrentList_Delete(object sender, EventArgs e)
        {

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

    }
}