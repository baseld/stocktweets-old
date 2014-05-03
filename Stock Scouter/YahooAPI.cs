using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Xml.Linq;

namespace Stock_Scouter
{
    /**
     * A Yahoo Finance API data model
     * 
     * Refer to http://www.gummy-stuff.org/Yahoo-data.htm
     * 
     * @author  Xiangyu Bu
     */

    class YahooAPI
    {

        private const string BASE_URL = "http://query.yahooapis.com/v1/public/yql?q={0}&env=store%3A%2F%2Fdatatables.org%2Falltableswithkeys";
        private const string BASE_QUERY = "select * from yahoo.finance.quotes where symbol in ({0})";

        // timeSpan = {1d, 5d, 1m, 3m, 6m, 1y, 2y, 5y, max}
        public static Uri GetQuoteGraphUrl(string symbol, string timeSpan, List<string> vs = null)
        {
            string vsStr;
            if (vs != null && vs.Count > 0)
            {
                if (vs.Contains("S&P500"))
                {
                    vs.Remove("S&P500");
                    vs.Add("%5EGSPC");
                }
                vsStr = "&c=" + String.Join(",", vs);
            }
            else
            {
                vsStr = "&c=";
            }
            return new Uri("http://chart.finance.yahoo.com/z?s=" + symbol + "&t=" + timeSpan + "&q=l&l=on&z=l" + vsStr + "&a=v&p=s&lang=en-US&region=US");
        }

        public static Uri GetQuotesXmlUrl(List<string> symbols)
        {
            symbols.Remove("");
            string symbolList = String.Join("%2C", symbols.Select(w => "%22" + w + "%22").ToArray());
            string sql = string.Format(BASE_QUERY, symbolList);
            string url = string.Format(BASE_URL, sql);

            System.Diagnostics.Debug.WriteLine((new Uri(url)).ToString());
            return new Uri(url);
        }

        public static void UpdateQuotes(string xmlData)
        {
            XDocument doc = XDocument.Parse(xmlData);
            XElement results = doc.Root.Element("results");
            IEnumerable<XElement> quotes = results.Elements("quote").Where(x => x.Element("ErrorIndicationreturnedforsymbolchangedinvalid").Value == "");

            foreach (XElement q in quotes)
            {
                System.Diagnostics.Debug.WriteLine("Processing " + q.Element("Symbol").Value);

                bool needSave = false;
                Quote quote = App.GetQuote(q.Element("Symbol").Value);
                if (quote == null)
                {
                    needSave = true;
                    quote = new Quote();
                }

                quote.Symbol = q.Element("Symbol").Value;
                quote.Ask = GetDecimal(q.Element("Ask").Value);
                if (quote.Ask == null) quote.Ask = GetDecimal(q.Element("AskRealtime").Value);
                quote.Bid = GetDecimal(q.Element("Bid").Value);
                if (quote.Bid == null) quote.Bid = GetDecimal(q.Element("BidRealtime").Value);
                quote.AverageDailyVolume = GetDecimal(q.Element("AverageDailyVolume").Value);
                quote.BookValue = GetDecimal(q.Element("BookValue").Value);
                quote.Change = GetDecimal(q.Element("Change").Value);
                quote.DividendShare = GetDecimal(q.Element("DividendShare").Value);
                quote.LastTradeDate = GetDateTime(q.Element("LastTradeDate").Value + " " + q.Element("LastTradeTime").Value);
                quote.LastTradeTime = q.Element("LastTradeTime").Value;
                quote.EarningsShare = GetDecimal(q.Element("EarningsShare").Value);
                quote.EpsEstimateCurrentYear = GetDecimal(q.Element("EPSEstimateCurrentYear").Value);
                quote.EpsEstimateNextYear = GetDecimal(q.Element("EPSEstimateNextYear").Value);
                quote.EpsEstimateNextQuarter = GetDecimal(q.Element("EPSEstimateNextQuarter").Value);
                quote.DailyLow = GetDecimal(q.Element("DaysLow").Value);
                quote.DailyHigh = GetDecimal(q.Element("DaysHigh").Value);
                quote.YearlyLow = GetDecimal(q.Element("YearLow").Value);
                quote.YearlyHigh = GetDecimal(q.Element("YearHigh").Value);
                quote.MarketCapitalization = q.Element("MarketCapitalization").Value;
                quote.Ebitda = q.Element("EBITDA").Value;
                quote.ChangeFromYearLow = GetDecimal(q.Element("ChangeFromYearLow").Value);
                quote.PercentChangeFromYearLow = GetDecimal(q.Element("PercentChangeFromYearLow").Value);
                quote.ChangeFromYearHigh = GetDecimal(q.Element("ChangeFromYearHigh").Value);
                quote.LastTradePrice = GetDecimal(q.Element("LastTradePriceOnly").Value);
                quote.PercentChangeFromYearHigh = GetDecimal(q.Element("PercebtChangeFromYearHigh").Value); //missspelling in yahoo for field name
                quote.FiftyDayMovingAverage = GetDecimal(q.Element("FiftydayMovingAverage").Value);
                quote.TwoHunderedDayMovingAverage = GetDecimal(q.Element("TwoHundreddayMovingAverage").Value);
                quote.ChangeFromTwoHundredDayMovingAverage = GetDecimal(q.Element("ChangeFromTwoHundreddayMovingAverage").Value);
                quote.PercentChangeFromTwoHundredDayMovingAverage = GetDecimal(q.Element("PercentChangeFromTwoHundreddayMovingAverage").Value);
                quote.PercentChangeFromFiftyDayMovingAverage = GetDecimal(q.Element("PercentChangeFromFiftydayMovingAverage").Value);
                quote.Name = q.Element("Name").Value;
                quote.Open = GetDecimal(q.Element("Open").Value);
                quote.PreviousClose = GetDecimal(q.Element("PreviousClose").Value);
                quote.ChangeInPercent = q.Element("ChangeinPercent").Value;
                quote.PriceSales = GetDecimal(q.Element("PriceSales").Value);
                quote.PriceBook = GetDecimal(q.Element("PriceBook").Value);
                quote.ExDividendDate = GetDateTime(q.Element("ExDividendDate").Value);
                quote.PeRatio = GetDecimal(q.Element("PERatio").Value);
                quote.DividendPayDate = GetDateTime(q.Element("DividendPayDate").Value);
                quote.DividendYield = GetDecimal(q.Element("DividendYield").Value);
                quote.PegRatio = GetDecimal(q.Element("PEGRatio").Value);
                quote.PriceEpsEstimateCurrentYear = GetDecimal(q.Element("PriceEPSEstimateCurrentYear").Value);
                quote.PriceEpsEstimateNextYear = GetDecimal(q.Element("PriceEPSEstimateNextYear").Value);
                quote.ShortRatio = GetDecimal(q.Element("ShortRatio").Value);
                quote.OneYearPriceTarget = GetDecimal(q.Element("OneyrTargetPrice").Value);
                quote.Volume = GetDecimal(q.Element("Volume").Value);
                quote.StockExchange = q.Element("StockExchange").Value;
                quote.LastUpdate = DateTime.Now;

                if (needSave) App.AddQuote(quote);
            }
        }

        private static decimal? GetDecimal(string input)
        {
            if (input == null) return null;

            input = input.Replace("%", "");

            decimal value;

            if (Decimal.TryParse(input, out value)) return value;
            return null;
        }

        private static DateTime? GetDateTime(string input)
        {
            if (input == null) return null;

            DateTime value;

            if (DateTime.TryParse(input, out value)) return value;
            return null;
        }

        public static Uri GetRssXmlUrl(List<string> symbols)
        {
            symbols.Remove("");
            return new Uri("http://finance.yahoo.com/rss/headline?s=" + String.Join(",", symbols.ToArray()));
        }

        public static ObservableCollection<RssItemViewModel> ParseRssXml(string xmlData)
        {
            ObservableCollection<RssItemViewModel> collection = new ObservableCollection<RssItemViewModel>();

            XDocument doc = XDocument.Parse(xmlData);
            XElement channel = doc.Root.Element("channel");
            IEnumerable<XElement> items = channel.Elements("item");
            System.Diagnostics.Debug.WriteLine("There are " + items.Count() + " news items.");
            foreach (XElement item in items)
            {
                collection.Add(new RssItemViewModel() { Title = item.Element("title").Value, Description = item.Element("description").Value, Link = item.Element("link").Value, PubDate = GetDateTime(item.Element("pubDate").Value) });
            }
            return collection;
        }
    }
}
