using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using Stock_Scouter.Models;


namespace Stock_Scouter
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            this.Stocks = new ObservableCollection<StockBriefViewModel>();
        }
        
        public ObservableCollection<StockBriefViewModel> Stocks { get; private set; }

        private int _numOfStocks = 0;
        /// <summary>
        /// Sample ViewModel property; this property is used in the view to display its value using a Binding
        /// </summary>
        /// <returns></returns>
        public int NumOfStocks
        {
            get
            {
                return _numOfStocks;
            }
            set
            {
                if (value != _numOfStocks)
                {
                    _numOfStocks = value;
                    NotifyPropertyChanged("NumOfStocks");
                }
            }
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Creates and adds a few ItemViewModel objects into the Items collection.
        /// </summary>
        public void LoadData()
        {
            // clear previously rendered list
            // but this is not ideal
            this.Stocks.Clear();

            Portfolio p = new Portfolio();
            foreach (KeyValuePair<string, Stock> entry in p.getStockList())
            {
                this.Stocks.Add(new StockBriefViewModel() { Symbol = entry.Value.Symbol, Name = entry.Value.Name, PriceDescription = "Ask: " + entry.Value.AskPrice.ToString() + " | Bid: " + entry.Value.BidPrice.ToString() + " | Day Range: " + entry.Value.DayRange + "" });
                System.Diagnostics.Debug.WriteLine("Added stock " + entry.Key + " to list.");
            }
            // disable this so far
            // this.IsDataLoaded = true;
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