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

        private string _changeInPercent;
        public string ChangeInPercent
        {
            get
            {
                return this._changeInPercent;
            }
            set
            {
                if (this._changeInPercent != value)
                {
                    this._changeInPercent = value;
                    NotifyPropertyChanged("ChangeInPercent");
                }
            }
        }

        private string _lastTradeDate;
        public string LastTradeDate
        {
            get
            {
                return this._lastTradeDate;
            }
            set
            {
                if (this._lastTradeDate != value)
                {
                    this._lastTradeDate = value;
                    NotifyPropertyChanged("LastTradeDate");
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