using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Stock_Scouter.Models;
using System.IO;
using System.IO.IsolatedStorage;

namespace Stock_Scouter
{
    class EventHandler
    {
        /**
         * Yahoo stock API is at http://www.gummy-stuff.org/Yahoo-data.htm
         * parameter: symbols - array of strings to fetch stock info
         * return: array of Stock objects
         */
        public static string getQuoteUrl(string[] symbols)
        {
            string symbolStr = String.Join("+", symbols);
            return "http://download.finance.yahoo.com/d/quotes.csv?s=" + symbolStr + "&f=snd1l1ohgvwdyr";
        }

        public static Stock[] getPortfolio()
        {
            var settings = IsolatedStorageSettings.ApplicationSettings;
            if (!settings.Contains("stocks")) return new Stock[0];
            return (Stock[])settings["stocks"];
        }

        public static void savePortfolio(Stock[] stocks)
        {
            Stock[] oldPortfolio = getPortfolio();
            Stock[] newPortfolio = new Stock[oldPortfolio.Length + stocks.Length];
            oldPortfolio.CopyTo(newPortfolio, 0);
            stocks.CopyTo(newPortfolio, oldPortfolio.Length);
            var settings = IsolatedStorageSettings.ApplicationSettings;
            settings.Remove("stocks");
            settings.Add("stocks", newPortfolio);
        }

    }
}
