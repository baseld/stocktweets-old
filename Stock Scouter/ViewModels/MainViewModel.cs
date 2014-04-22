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
            // clear previously rendered list
            // but this is not ideal

            this.PageCollection.Clear();

            List<string> portfolios = AppSettings.GetPortfolioList();
            foreach (string entry in portfolios)
            {
                this.PageCollection.Add(new PortfolioViewModel() {Title = entry});
                System.Diagnostics.Debug.WriteLine("Added portfolio view " + entry + " to main page.");
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