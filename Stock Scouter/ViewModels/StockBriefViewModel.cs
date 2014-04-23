using System;
using System.ComponentModel;
using System.Windows.Media;

namespace Stock_Scouter
{
    public class StockBriefViewModel : INotifyPropertyChanged
    {
        public StockBriefViewModel()
        {
            
        }

        private SolidColorBrush _color;
        public SolidColorBrush Color
        {
            get
            {
                if (Convert.ToDouble(this.Change) < 0) return new SolidColorBrush(Colors.Red);
                return new SolidColorBrush(Colors.Green); 
            }
            set
            {
                if (this._color != value)
                {
                    this._color = value;
                    NotifyPropertyChanged("Color");
                }
            }
        }

        private string _change;
        public string Change
        {
            get
            {
                return this._change;
            }
            set
            {
                if (this._change != value)
                {
                    this._change = value;
                    NotifyPropertyChanged("Change");
                }
            }
        }

        private string _lastTradePrice;
        public string LastTradePrice
        {
            get
            {
                return this._lastTradePrice;
            }
            set
            {
                if (this._lastTradePrice != value)
                {
                    this._lastTradePrice = value;
                    NotifyPropertyChanged("LastTradePrice");
                }
            }
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

        private string _askPrice;
        public string AskPrice
        {
            get
            {
                return this._askPrice;
            }
            set
            {
                if (this._askPrice != value)
                {
                    this._askPrice = value;
                    NotifyPropertyChanged("AskPrice");
                }
            }
        }

        private string _bidPrice;
        public string BidPrice
        {
            get
            {
                return this._bidPrice;
            }
            set
            {
                if (this._bidPrice != value)
                {
                    this._bidPrice = value;
                    NotifyPropertyChanged("BidPrice");
                }
            }
        }

        private string _dayRange;
        public string DayRange
        {
            get
            {
                return this._dayRange;
            }
            set
            {
                if (this._dayRange != value)
                {
                    this._dayRange = value;
                    NotifyPropertyChanged("DayRange");
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