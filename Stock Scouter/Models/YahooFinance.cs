using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace Stock_Scouter.Models
{
    /**
     * A Yahoo Finance API data model
     * 
     * Refer to http://www.gummy-stuff.org/Yahoo-data.htm
     * 
     * @author  Xiangyu Bu
     */

    class YahooFinance
    {

        public static string GetQuotes(string[] symbols)
        {
            string symbolStr = String.Join("+", symbols);
            string result = "";
            Uri u = new Uri("http://download.finance.yahoo.com/d/quotes.csv?s=" + symbolStr + "&f=snd1l1ohgvwdyr");

            // not compatible with WinRT
            var client = new WebClient();
            client.DownloadStringCompleted += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine("YahooFinance: I got " + e.Result);
                result = e.Result;
            };

            client.DownloadStringAsync(u);
            return result;
        }

        public static Stock[] CsvToStock(string csv)
        {
            string[] stocks = csv.Split('\n');
            Stock[] array = new Stock[stocks.Length];
            int i = 0;

            foreach (string line in stocks)
            {
                //"XOM","Exxon Mobil Corpo","4/17/2014",100.42,99.69,100.97,99.69,15439810,"84.79 - 101.74",2.52,2.52,13.56
                string[] items = line.Split(',');
                Stock s = new Stock();
                s.Symbol = items[0].Replace("\"", "");
                s.Name = items[1].Replace("\"", "");
                s.LastTradeDate = items[2].Replace("\"", "");
                s.LastTradePrice = Convert.ToDouble(items[3]);
                s.OpenPrice = Convert.ToDouble(items[4]);
                s.DayHighPrice = Convert.ToDouble(items[5]);
                s.DayLowPrice = Convert.ToDouble(items[6]);
                s.Volume = Convert.ToInt32(items[7]);
                s.DayRange = items[8].Replace("\"", "");
                s.Dividend = items[9];
                s.DividendYield = items[10];
                s.PERatio = items[11];
                array[i++] = s;
            }

            return array;
        }
    }
}
