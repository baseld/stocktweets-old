using System;
using System.Collections.Generic;
using System.Linq;
using System.IO.IsolatedStorage;

namespace Stock_Scouter.Models
{
    class Portfolio
    {
        private List<string> stockList;
        private string _portfolioName;

        public string Name
        {
            get
            {
                return _portfolioName;
            }
            set
            {
                this._portfolioName = value;
            }
        }

        public Portfolio()
        {
            this.stockList = new List<string>();
        }

        public int getNumOfStocks()
        {
            return this.stockList.Count();
        }

        public List<string> GetStockList()
        {
            return this.stockList;
        }

        public bool addQuote(Quote q)
        {
            if (stockList.Contains(q.Symbol)) return false;
            stockList.Add(q.Symbol);
            AppSettings.SetQuote(q.Symbol, q);
            AppSettings.SetPortfolio(this.Name, this);
            return true;
        }

        public void RemoveQuote(Quote s)
        {
            if (stockList.Contains(s.Symbol))
            {
                stockList.Remove(s.Symbol);
                AppSettings.SetPortfolio(this.Name, this);
                //TODO: clean internal storage
            }
        }
    }
}
