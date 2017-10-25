using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.IO;
using System.Net;
using System.Text;

namespace YahooFinanceImport
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                GetBovespaPrices();
                //GetIndicators();
            }
            catch { }
        }

        static private void GetBovespaPrices()
        {
            try
            {
                /////////////////////////
                //Get bovespa stock codes
                using (var context = new StockExchangeEntities())
                {
                    var stocks = context.Stocks;

                    foreach (var stock in stocks)
                    {
                        try
                        {
                            ////////////////////////////
                            //Log each action and return

                            //Query last update for the stock
                            LogConsole("Querying stock " + stock.ISINCode.Trim() + "...");
                            DateTime lastUpdate = GetLastPriceDateByStock(stock.StockId);
                            LogConsole("Last update found is " + lastUpdate.ToString("dd'/'MM'/'yyyy") + ".");

                            //Query prices from web
                            LogConsole("Getting prices of " + stock.ISINCode.Trim() + " from Yahoo...");
                            List<HistoryPrice> prices = GetPricesFromYahoo(stock.YahooCode, lastUpdate, DateTime.Now.AddDays(1));

                            if (prices != null)
                            {
                                LogConsole("Sending prices updates to database...");
                                UpdateDatabase(stock, prices);
                                LogConsole(stock.ISINCode.Trim() + " updated successfully!!!");
                            }
                            else
                            {
                                LogConsole("ERROR: Prices for the stock " + stock.ISINCode.Trim() + " could not be found.");
                            }
                        }
                        catch(Exception ex)
                        {
                            LogConsole("ERROR: " + ex.Message);
                        }
                        finally
                        {
                            LogConsole("======================================================");
                        }
                    }
                }

            }
            catch { }
        }

        static private string UpdateDatabase(Stock stock, List<HistoryPrice> prices)
        {
            string returnError = string.Empty;
            string line = string.Empty;

            try
            {
                foreach (HistoryPrice hp in prices)
                {
                    using (var context = new StockExchangeEntities())
                    {
                        context.CreateStockPrice(stock.ISINCode,
                                                 hp.Date,
                                                 (decimal)hp.Open,
                                                 (decimal)hp.High,
                                                 (decimal)hp.Low,
                                                 (decimal)hp.Close,
                                                 (decimal)hp.Volume,
                                                (decimal)hp.AdjClose);
                    }
                }

            }
            catch(Exception ex)
            {
                returnError = ex.Message;
            }

            return returnError;
        }

        static private List<HistoryPrice> GetPricesFromYahoo(string stock, DateTime startDate, DateTime endDate)
        {
            List<HistoryPrice> prices = null;

            try
            {
                prices = Historical.Get(stock, startDate, endDate);
            }
            catch (Exception ex)
            {
                Console.Write(ex.ToString());
                    
            }

            return prices;
        } 

        static private DateTime GetLastPriceDateByStock(short stock)
        {
            DateTime returnDateTime = new DateTime(1999, 12, 31);

            try
            {
                //////////////////////
                //Get last update date
                using (var context = new StockExchangeEntities())
                {
                    var lastDateTime = context.StockPrices.Where(p => p.StockId == stock).Max(p => p.PriceDate);

                    if (lastDateTime != null)
                        returnDateTime = (DateTime)lastDateTime;
                }
            }
            catch { }

            return returnDateTime.AddDays(1);
        }

        static private string GetYahooAddress(string stock, DateTime startDate, DateTime endDate)
        {
            StringBuilder yahooAddress = new StringBuilder();

            try
            {
                /////////
                //Header
                yahooAddress.Append("http://real-chart.finance.yahoo.com/table.csv?");
                int monthCode;

                ////////////
                //Stock code
                yahooAddress.Append("s=");
                yahooAddress.Append(stock);
                yahooAddress.Append("&");

                ////////////
                //Start month
                yahooAddress.Append("a=");
                monthCode = startDate.Month;
                monthCode--;
                yahooAddress.Append(monthCode.ToString().PadLeft(2, '0'));
                yahooAddress.Append("&");

                ////////////
                //Start day
                yahooAddress.Append("b=");
                yahooAddress.Append(startDate.Day.ToString().PadLeft(2, '0'));
                yahooAddress.Append("&");

                ////////////
                //Start year
                yahooAddress.Append("c=");
                yahooAddress.Append(startDate.Year);
                yahooAddress.Append("&");

                ////////////
                //End month
                yahooAddress.Append("d=");
                yahooAddress.Append(endDate.Month.ToString().PadLeft(2, '0'));
                monthCode = startDate.Month;
                monthCode--;
                yahooAddress.Append(monthCode.ToString().PadLeft(2, '0'));
                yahooAddress.Append("&");

                ////////////
                //Start day
                yahooAddress.Append("e=");
                yahooAddress.Append(endDate.Day.ToString().PadLeft(2, '0'));
                yahooAddress.Append("&");

                ////////////
                //End year
                yahooAddress.Append("f=");
                yahooAddress.Append(endDate.Year);
                yahooAddress.Append("&");

                /////////
                //Footer
                yahooAddress.Append("g=d&ignore=.csv");
            }
            catch { }

            return yahooAddress.ToString();
        }

        static private DateTime ParseDateTime(string yahooDate)
        {
            DateTime returnDate = DateTime.MinValue;

            try
            {
                string[] date = yahooDate.Split('-');
                returnDate = new DateTime(Int32.Parse(date[0]), Int32.Parse(date[1]), Int32.Parse(date[2]));

            }
            catch { }

            return returnDate;
        }

        static private void LogConsole(string message)
        {
            Console.WriteLine("[" + DateTime.Now.ToString("dd'/'MM'/'yyyy HH:mm:ss") + "]: " + message);
        }
    }
}
