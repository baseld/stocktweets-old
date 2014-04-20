using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Stock_Scouter
{
    public class StockBriefViewModel : INotifyPropertyChanged
    {
        public StockBriefViewModel()
        {
        }

        private string _symbol;
        public string Symbol
        {
            get
            {
                return this._symbol;
            }
            set
            {
                if (this._symbol != value)
                {
                    this._symbol = value;
                    NotifyPropertyChanged("Symbol");
                }
            }
        }

        private string _name;
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                if (this._name != value)
                {
                    this._name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string _priceDescription;
        public string PriceDescription
        {
            get
            {
                return this._priceDescription;
            }
            set
            {
                if (this._priceDescription != value)
                {
                    this._priceDescription = value;
                    NotifyPropertyChanged("PriceDescription");
                }
            }
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