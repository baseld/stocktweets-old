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
    public class PortfolioViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<StockBriefViewModel> _stockViews;

        public PortfolioViewModel()
        {
            this._stockViews = new ObservableCollection<StockBriefViewModel>();
        }

        private string _title;
        public string Title
        {
            get
            {
                return this._title;
            }
            set
            {
                if (this._title != value)
                {
                    this._title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        public ObservableCollection<StockBriefViewModel> StockViews
        {
            get { return _stockViews; }
            set
            {
                if (_stockViews != value)
                {
                    _stockViews = value;
                    NotifyPropertyChanged("StockViews");
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
            this.StockViews.Clear();

            Portfolio p = AppSettings.GetPortfolio(this.Title);
            
            foreach (string entry in p.GetStockList())
            {
                Stock s = AppSettings.GetStock(entry);
                this.StockViews.Add(new StockBriefViewModel() { Symbol = s.Symbol, Name = s.Name, PriceDescription = "Ask: " + s.AskPrice.ToString() + " | Bid: " + s.BidPrice.ToString() + " | Day Range: " + s.DayRange + "" });
                System.Diagnostics.Debug.WriteLine("Added stock " + s.Symbol + " to list.");
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