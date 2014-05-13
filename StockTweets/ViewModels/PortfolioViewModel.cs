using System;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace StockTweets
{
    public class PortfolioViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<Quote> _stockViews;

        public PortfolioViewModel()
        {
            this._stockViews = new ObservableCollection<Quote>();
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

        public ObservableCollection<Quote> StockViews
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

            Portfolio p = App.GetPortfolio(this.Title);
            
            foreach (string entry in p.StockList)
            {
                AddStockToView(App.GetQuote(entry));
            }
            // disable this so far
            this.IsDataLoaded = true;
        }

        public void RemoveStockFromView(Quote s)
        {
            this.StockViews.Remove(s);
        }

        public void AddStockToView(Quote s)
        {
            StockViews.Add(s);
            System.Diagnostics.Debug.WriteLine("Added stock " + s.Symbol + " to list " + Title + ".");
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