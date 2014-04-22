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

        public void addStock(Stock s)
        {
            if (this.stockList.Contains(s.Symbol)) return;
            this.stockList.Add(s.Symbol);
            AppSettings.SetStock(s.Symbol, s);
            AppSettings.SetPortfolio(this.Name, this);
        }

        public void removeStock(Stock s)
        {
            if (this.stockList.Contains(s.Symbol))
            {
                this.stockList.Remove(s.Symbol);
                AppSettings.SetPortfolio(this.Name, this);
                //TODO: clean internal storage
            }
        }

        public void updateStock(Stock s)
        {
            if (this.stockList.Contains(s.Symbol))
            {
                IsolatedStorageSettings.ApplicationSettings["stocks"] = this.stockList;
            }
        }
    }
}
