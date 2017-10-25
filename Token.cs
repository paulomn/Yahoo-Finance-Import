using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YahooFinanceImport
{
    public class Token
    {
        public static string Cookie { get; set; }
        public static string Crumb { get; set; }

        private static Regex regex_crumb;
        /// <summary>
        /// Refresh cookie and crumb value Yahoo Fianance
        /// </summary>
        /// <param name="symbol">Stock ticker symbol</param>
        /// <returns></returns>
        public static bool Refresh(string symbol = "SPY")
        {

            try
            {
                Token.Cookie = "";
                Token.Crumb = "";

                string url_scrape = "https://finance.yahoo.com/quote/{0}?p={0}";
                //url_scrape = "https://finance.yahoo.com/quote/{0}/history"

                string url = string.Format(url_scrape, symbol);

                HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);

                request.CookieContainer = new CookieContainer();
                request.Method = "GET";

                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {

                    string cookie = response.GetResponseHeader("Set-Cookie").Split(';')[0];

                    string html = "";

                    using (Stream stream = response.GetResponseStream())
                    {
                        html = new StreamReader(stream).ReadToEnd();
                    }

                    if (html.Length < 5000)
                        return false;
                    string crumb = getCrumb(html);
                    html = "";

                    if (crumb != null)
                    {
                        Token.Cookie = cookie;
                        Token.Crumb = crumb;
                        return true;
                    }

                }

            }
            catch { }

            return false;

        }

        /// <summary>
        /// Get crumb value from HTML
        /// </summary>
        /// <param name="html">HTML code</param>
        /// <returns></returns>
        private static string getCrumb(string html)
        {

            string crumb = null;

            try
            {
                //initialize on first time use
                if (regex_crumb == null)
                    regex_crumb = new Regex("CrumbStore\":{\"crumb\":\"(?<crumb>.+?)\"}",
                RegexOptions.CultureInvariant | RegexOptions.Compiled, TimeSpan.FromSeconds(5));

                MatchCollection matches = regex_crumb.Matches(html);

                if (matches.Count > 0)
                {
                    crumb = matches[0].Groups["crumb"].Value;
                }

                //prevent regex memory leak
                matches = null;

            }
            catch { }

            GC.Collect();
            return crumb;

        }

    }
}
