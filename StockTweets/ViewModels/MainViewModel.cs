using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace StockTweets
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<PortfolioViewModel> _pageCollection;
        
        public MainViewModel()
        {
            this._pageCollection = new ObservableCollection<PortfolioViewModel>();
        }

        public ObservableCollection<PortfolioViewModel> PageCollection
        {
            get { return _pageCollection; }
            set
            {
                if (_pageCollection != value)
                {
                    _pageCollection = value;
                    NotifyPropertyChanged("PageCollection");
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
            this.PageCollection.Clear();

            List<Portfolio> portfolios = App.PortfolioList;
            foreach (Portfolio entry in portfolios)
            {
                PortfolioViewModel pvm = new PortfolioViewModel() { Title = entry.Name };
                pvm.LoadData();
                this.PageCollection.Add(pvm);
                System.Diagnostics.Debug.WriteLine("Added portfolio view " + entry.Name + " to main page.");
            }
            
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