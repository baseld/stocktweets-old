using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.IO;

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
        private static readonly Dictionary<string, string> Fields = new Dictionary<string, string>()
        {
            {"Ask", "a"},
            {"Average Daily Volume", "a2"},
            {"Ask Size", "a5"},
            {"Bid", "b"},
            {"Ask (Real-time)", "b2"},
            {"Bid (Real-time)", "b3"},
            {"Book Value", "b4"},
            {"Bid Size", "b6"},
            {"Change & Percent Change", "c"},
            {"Change", "c1"},
            {"Commission", "c3"},
            {"Change (Real-time)", "c6"},
            {"After Hours Change (Real-time)", "c8"},
            {"Dividend/Share", "d"},
            {"Last Trade Date", "d1"},
            {"Trade Date", "d2"},
            {"Earnings/Share", "e"},
            {"Error Indication (returned for symbol changed / invalid)", "e1"},
            {"EPS Estimate Current Year", "e7"},
            {"EPS Estimate Next Year", "e8"},
            {"EPS Estimate Next Quarter", "e9"},
            {"Float Shares", "f6"},
            {"Day's Low", "g"},
            {"Day's High", "h"},
            {"52-week Low", "j"},
            {"52-week High", "k"},
            {"Holdings Gain Percent", "g1"},
            {"Annualized Gain", "g3"},
            {"Holdings Gain", "g4"},
            {"Holdings Gain Percent (Real-time)", "g5"},
            {"Holdings Gain (Real-time)", "g6"},
            {"More Info", "i"},
            {"Order Book (Real-time)", "i5"},
            {"Market Capitalization", "j1"},
            {"Market Cap (Real-time)", "j3"},
            {"EBITDA", "j4"},
            {"Change From 52-week Low", "j5"},
            {"Percent Change From 52-week Low", "j6"},
            {"Last Trade (Real-time) With Time", "k1"},
            {"Change Percent (Real-time)", "k2"},
            {"Last Trade Size", "k3"},
            {"Change From 52-week High", "k4"},
            {"Percent Change From 52-week High", "k5"},
            {"Last Trade (With Time)", "l"},
            {"Last Trade (Price Only)", "l1"},
            {"High Limit", "l2"},
            {"Low Limit", "l3"},
            {"Day's Range", "m"},
            {"Day's Range (Real-time)", "m2"},
            {"50-day Moving Average", "m3"},
            {"200-day Moving Average", "m4"},
            {"Change From 200-day Moving Average", "m5"},
            {"Percent Change From 200-day Moving Average", "m6"},
            {"Change From 50-day Moving Average", "m7"},
            {"Percent Change From 50-day Moving Average", "m8"},
            {"Name", "n"},
            {"Notes", "n4"},
            {"Open", "o"},
            {"Previous Close", "p"},
            {"Price Paid", "p1"},
            {"Change in Percent", "p2"},
            {"Price/Sales", "p5"},
            {"Price/Book", "p6"},
            {"Ex-Dividend Date", "q"},
            {"P/E Ratio", "r"},
            {"Dividend Pay Date", "r1"},
            {"P/E Ratio (Real-time)", "r2"},
            {"PEG Ratio", "r5"},
            {"Price/EPS Estimate Current Year", "r6"},
            {"Price/EPS Estimate Next Year", "r7"},
            {"Symbol", "s"},
            {"Shares Owned", "s1"},
            {"Short Ratio", "s7"},
            {"Last Trade Time", "t1"},
            {"Trade Links", "t6"},
            {"Ticker Trend", "t7"},
            {"1 yr Target Price", "t8"},
            {"Volume", "v"},
            {"Holdings Value", "v1"},
            {"Holdings Value (Real-time)", "v7"},
            {"52-week Range", "w"},
            {"Day's Value Change", "w1"},
            {"Day's Value Change (Real-time)", "w4"},
            {"Stock Exchange", "x"},
            {"Dividend Yield", "y"}
        };

        public delegate void RESTSuccessCallback(Stream stream);
        public delegate void RESTErrorCallback(String reason);

        public static void get(Uri uri, Dictionary<String, String> extra_headers, RESTSuccessCallback success_callback, RESTErrorCallback error_callback)
        {
            HttpWebRequest request = WebRequest.CreateHttp(uri);

            if (extra_headers != null)
                foreach (String header in extra_headers.Keys)
                    try
                    {
                        request.Headers[header] = extra_headers[header];
                    }
                    catch (Exception) { }

            request.BeginGetResponse((IAsyncResult result) =>
            {
                HttpWebRequest req = result.AsyncState as HttpWebRequest;
                if (req != null)
                {
                    try
                    {
                        WebResponse response = req.EndGetResponse(result);
                        success_callback(response.GetResponseStream());
                    }
                    catch (WebException e)
                    {
                        error_callback(e.Message);
                        return;
                    }
                }
            }, request);
        }

        public static void post(Uri uri, Dictionary<String, String> post_params, Dictionary<String, String> extra_headers, RESTSuccessCallback success_callback, RESTErrorCallback error_callback)
        {
            HttpWebRequest request = WebRequest.CreateHttp(uri);
            request.ContentType = "application/x-www-form-urlencoded";
            request.Method = "POST";

            if (extra_headers != null)
                foreach (String header in extra_headers.Keys)
                    try
                    {
                        request.Headers[header] = extra_headers[header];
                    }
                    catch (Exception) { }


            request.BeginGetRequestStream((IAsyncResult result) =>
            {
                HttpWebRequest preq = result.AsyncState as HttpWebRequest;
                if (preq != null)
                {
                    Stream postStream = preq.EndGetRequestStream(result);

                    StringBuilder postParamBuilder = new StringBuilder();
                    if (post_params != null)
                        foreach (String key in post_params.Keys)
                            postParamBuilder.Append(String.Format("{0}={1}&", key, post_params[key]));

                    Byte[] byteArray = Encoding.UTF8.GetBytes(postParamBuilder.ToString());

                    postStream.Write(byteArray, 0, byteArray.Length);
                    postStream.Close();


                    preq.BeginGetResponse((IAsyncResult final_result) =>
                    {
                        HttpWebRequest req = final_result.AsyncState as HttpWebRequest;
                        if (req != null)
                        {
                            try
                            {
                                WebResponse response = req.EndGetResponse(final_result);
                                success_callback(response.GetResponseStream());
                            }
                            catch (WebException e)
                            {
                                error_callback(e.Message);
                                return;
                            }
                        }
                    }, preq);
                }
            }, request);
        }


        public static Uri GetQuoteUri(string[] symbols)
        {
            string symbolStr = String.Join("+", symbols).Replace("++", "+");
            return new Uri("http://download.finance.yahoo.com/d/quotes.csv?s=" + symbolStr + "&f=snd1l1ohgvmdyrc1");
        }

        public static Uri GetDetailedQuoteUri(string[] symbols)
        {
            string symbolStr = String.Join("+", symbols).Replace("++", "+");
            return new Uri("http://download.finance.yahoo.com/d/quotes.csv?s=" + symbolStr + "&f=snd1l1ohgvmdyrc1");
        }

        public static List<Stock> CsvToStock(string csv)
        {
            string[] stocks = csv.Split('\n');
            List<Stock> list = new List<Stock>();

            foreach (string line in stocks)
            {
                System.Diagnostics.Debug.WriteLine(line);
                //"XOM","Exxon Mobil Corpo","4/17/2014",100.42,99.69,100.97,99.69,15439810,"84.79 - 101.74",2.52,2.52,13.56,+1
                string[] items = line.Split(',');
                if (items.Length < 13 || items[2] == "\"N/A\"") continue;
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
                s.Change = Convert.ToDouble(items[12]);
                list.Add(s);
            }

            return list;
        }
    }
}
