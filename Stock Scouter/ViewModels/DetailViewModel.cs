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
    public class DetailViewModel : INotifyPropertyChanged
    {

        private string _symbol;
        public string Symbol
        {
            get
            {
                return _symbol;
            }
            set
            {
                if (_symbol != value)
                {
                    _symbol = value;
                    NotifyPropertyChanged("Symbol");
                }
            }
        }

        private Quote _quote;
        public Quote Quote
        {
            get
            {
                return _quote;
            }
            set
            {
                if (_quote != value)
                {
                    _quote = value;
                    NotifyPropertyChanged("Quote");
                }
            }
        }

        public DetailViewModel()
        {
        }

        public bool IsDataLoaded
        {
            get;
            private set;
        }
        
        public void LoadData()
        {
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
