using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.IsolatedStorage;

namespace Stock_Scouter.Models
{
    class Portfolio
    {
        Dictionary<string, Stock> stockList;

        public Portfolio()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            if (settings.Contains("stocks"))
            {
                this.stockList = (Dictionary<string, Stock>)settings["stocks"];
            }
            else
            {
                this.stockList = new Dictionary<string, Stock>();
                settings.Add("stocks", this.stockList);
            }
        }

        public int getNumOfStocks()
        {
            return this.stockList.Count();
        }

        public Dictionary<string, Stock> getStockList()
        {
            return this.stockList;
        }

        public void addStock(Stock s)
        {
            try
            {
                if (this.stockList.ContainsKey(s.Symbol)) return;
                this.stockList.Add(s.Symbol, s);
                IsolatedStorageSettings.ApplicationSettings["stocks"] = this.stockList;
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("Caught exception " + e.Message);
                System.Diagnostics.Debug.WriteLine(e.Source);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);
            }
        }

        public void removeStock(Stock s)
        {
            if (this.stockList.ContainsKey(s.Symbol))
            {
                this.stockList.Remove(s.Symbol);
                IsolatedStorageSettings.ApplicationSettings["stocks"] = this.stockList;
            }
        }

        public void updateStock(Stock s)
        {
            if (this.stockList.ContainsKey(s.Symbol))
            {
                this.stockList[s.Symbol] = s;
                IsolatedStorageSettings.ApplicationSettings["stocks"] = this.stockList;
            }
        }

    }
}
