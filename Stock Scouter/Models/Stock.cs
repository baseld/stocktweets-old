using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Stock_Scouter.Models
{
    class Stock
    {
        private string _symbol;
        public string Symbol {
            get { return this._symbol; }
            set { this._symbol = value; }
        }
        
        private string _name;
        public string Name
        {
            get { return this._name; }
            set { this._name = value; }
        }
        
        private double _askPrice = 0.0;
        public double AskPrice
        {
            get { return this._askPrice; }
            set { this._askPrice = value; }
        }

        private double _bidPrice = 0.0;

        private double _lastTradePrice = 0.0;

        private string _lastTradeDate;

        private double _openPrice = 0.0;

        private double _dayHighPrice = 0.0;

        private double _dayLowPrice = 0.0;

        private int _volume = 0;

        private string _weekRange;

        private string _dayRange;

        private string _dividend;

        private string _dividendYield;

        private string _peRatio;


        public Stock() {
        }



    }
}
