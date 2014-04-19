using System;
using System.Net;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Stock_Scouter.Models;

namespace Stock_Scouter
{   
    /**
     * APIHandler processes HTTP requests
     * 
     * Yahoo stock API is at http://www.gummy-stuff.org/Yahoo-data.htm
     */
    class APIHandler
    {

        /**
         * parameter: symbols - array of strings to fetch stock info
         * return: array of Stock objects
         */
        void getQuote(string[] symbols)
        {
            string symbolStr = String.Join("+", symbols);
            string sUrl = "http://download.finance.yahoo.com/d/quotes.csv?s=" + symbolStr + "&f=snd1l1ohgvwdyr";
            


        }
    }
}
