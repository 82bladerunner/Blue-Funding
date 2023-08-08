using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using BlueFunding.Binance;
using BlueFunding.Binance.clients;
using BlueFunding.Binance.models;
using BlueFunding.BlueFundingClasses;
using Serilog;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace BlueFunding
{
    class Program
    {

        public static DateTime appRunDate = DateTime.UtcNow;

        static async Task Main()
        {

            #region SSL Handler
            ServicePointManager.ServerCertificateValidationCallback = MyCertHandler;

            static bool MyCertHandler(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors error)
            {
                // Ignore errors
                return true;
            }

            ServicePointManager.ServerCertificateValidationCallback +=
            (mender, certificate, chain, sslPolicyErrors) => true;

            ServicePointManager.ServerCertificateValidationCallback +=
            (sender, certificate, chain, errors) =>
            {
                return errors == SslPolicyErrors.None;
            };

            #endregion

            #region Logger
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File(@"C:\Users\Administrator\Desktop\log.txt", rollingInterval: RollingInterval.Minute)
                .CreateLogger();

            Log.Information("starting");
            #endregion

            var trader = getTrader(); 
            List<ActivePosition> myPositions = new List<ActivePosition>();
            List<ActivePosition> oldlist = new List<ActivePosition>();
            List<ActivePosition> dontOpenPool = new List<ActivePosition>();
            List<ActivePosition> closedByProgram = new List<ActivePosition>();
            bool sendRequests = true;
            bool testing = true;

            BinanceIntegrator binanceIntegrator = new BinanceIntegrator();
            BinanceNetClient binanceNetClient = new BinanceNetClient();

            var balance = binanceNetClient.GetUsdtBalanceAsync().Result;
            var exchangeInfo = GetFuturesExchangeInfoAsync().Result;
            var getCopyPositions = new List<ActivePosition>();

            Dictionary<string, int> pairPricePrecision = new Dictionary<string, int>();
            foreach (var item in exchangeInfo.Symbols)
            {
                pairPricePrecision.TryAdd(item.BaseAsset, item.PricePrecision);
            }

            while (sendRequests)
            {
                var toRemove = new List<ActivePosition>();

                if(testing)
                {
                    getCopyPositions = await binanceIntegrator.GetAllUserPositionsAsync(trader);
                }
                else
                {
                    getCopyPositions = await binanceIntegrator.StartWatchingUserPositionsAsync(trader, appRunDate);
                }

                oldlist = myPositions; //eğer elle kapatilan varsa diye

                myPositions = await binanceNetClient.GetOpenPositionsAsync();
                if (oldlist.Count > myPositions.Count)
                {
                    foreach (var item in oldlist.Except(myPositions))
                    {
                        if(!closedByProgram.Contains(item))
                        dontOpenPool.Add(item);
                    }
                }

                //adding positions to the pool
                foreach (var item in getCopyPositions)
                {
                    if (myPositions.Exists(q => q.pair.symbol == item.pair.symbol) && myPositions.Exists(q => q.futuresOrderDirection == item.futuresOrderDirection)
                        || dontOpenPool.Exists(q => q.pair.symbol == item.pair.symbol) && dontOpenPool.Exists(q => q.futuresOrderDirection == item.futuresOrderDirection)) { continue; }

                    else
                    {

                        //price precision
                        int precision = pairPricePrecision[item.pair.BaseAsset];

                        var preciseAmount = Math.Abs(decimal.Round((decimal)item.amount * trader.coefficient, precision));
                        if ((double)preciseAmount * item.markPrice < 20) 
                        {
                            preciseAmount = (decimal.Round(20 / (decimal)item.markPrice, 1));
                        }
                       
                        if(balance.Available >= 20)
                        {
                            try
                            {
                                //pool.Add(item);
                                Log.Information("Adding: " + item.pair + "position to the pool.");
                                var openedPos = binanceNetClient.PostMarketOrderAsync(item.pair.symbol, OrderSideConverter(item), (decimal)preciseAmount);

                                Log.Information("Opening position: " + item.pair.symbol);
                            }
                            catch (Exception e)
                            {
                                Log.Error(e.Message);
                            }
                        }
                    }
                }

                for (int i = 0; i < myPositions.Count; i++)
                {
                    bool toAdd = false;
                    for (int y = 0; y < getCopyPositions.Count; y++)
                    {
                        if (myPositions[i].pair.symbol == getCopyPositions[y].pair.symbol)
                        {
                            toAdd = true;
                        }
                    }
                    if (!toAdd)
                    {
                        toRemove.Add(myPositions[i]);
                    }
                }

                foreach (var item in toRemove)
                {
                    
                    try
                    {
                        //pool.Remove(item);
                        Log.Information("Removing " + item.pair.symbol + " from the pool");
                        var closedPos = binanceNetClient.CloseFuturesPositionAsync(item).Result;
                        closedByProgram.Add(item);
                        Log.Information("Closing " + closedPos.Symbol + " position.");

                    }
                    catch (Exception e)
                    {
                        Log.Error(e.Message);
                    }
                }
            }
        }

        private static Trader getTrader()
        {
            Console.WriteLine("Select trader: ");

            for (int i = 0; i < TraderList.Traders.Count; i++)
            {
                Console.WriteLine(i + " - " + TraderList.Traders[i].name);
            }

            Trader trader = TraderList.Traders[Convert.ToInt32(Console.ReadLine())];
            Console.WriteLine("Selected: " + trader.name + ", coefficient:" + trader.coefficient);

            return trader;
        }

        private static async Task<BinanceFuturesUsdtExchangeInfo> GetFuturesExchangeInfoAsync()
        {
            BinanceNetClient client = new BinanceNetClient();

            return await client.GetExchangeInfoAsync();
        }

        private static void PrintTraderPositions(int index, ActivePosition value)
        {
            //var lastPrice = (double)GetLastPriceAsync(value.symbol).Result;

            Console.WriteLine("[" + index + "]");
            Console.WriteLine("Pair: " + value.pair);
            //Console.WriteLine("Amount: " + value.amount + "( ~" + HumanizeNumber(value.amount / lastPrice) + "$ )");
            Console.WriteLine("Entry price: " + value.entryPrice);
            Console.WriteLine("Mark price: " + value.markPrice);
            Console.WriteLine("Leverage: " + CalculateLeverage(value.entryPrice, value.markPrice, value.roe));

            if (value.pnl > 0) { Console.ForegroundColor = ConsoleColor.Green; }
            else { Console.ForegroundColor = ConsoleColor.Red; }
            Console.WriteLine("PNL: " + HumanizeNumber(value.pnl));
            Console.WriteLine("ROE: " + "%" + Math.Round(value.roe * 100, 2));
            Console.ResetColor();

            Console.WriteLine("---");

        }

        private static OrderSide OrderSideConverter(double amount)
        {
            if (amount < 0)
            {
                return OrderSide.Sell;
            }
            else return OrderSide.Buy;
        }

        private static OrderSide OrderSideConverter(ActivePosition pos)
        {
            if (pos.futuresOrderDirection == FuturesOrderDirection.SHORT)
            {
                return OrderSide.Sell;
            }
            else return OrderSide.Buy;
        }


        private static string CalculateLeverage(double entryPrice, double markPrice, double posRoe)
        {
            var pricePnl = (Math.Abs((markPrice - entryPrice) / entryPrice));
            return (Math.Round((Math.Abs(posRoe) / pricePnl), MidpointRounding.ToEven)).ToString();
        }

        public static string HumanizeNumber(double number)
        {
            string[] suffix = { "f", "a", "p", "n", "μ", "m", string.Empty, "k", "M", "G", "T", "P", "E" };

            var absnum = Math.Abs(number);

            int mag;
            if (absnum < 1)
            {
                mag = (int)Math.Floor(Math.Floor(Math.Log10(absnum)) / 3);
            }
            else
            {
                mag = (int)(Math.Floor(Math.Log10(absnum)) / 3);
            }

            var shortNumber = number / Math.Pow(10, mag * 3);

            return $"{shortNumber:0.###}{suffix[mag + 6]}";
        }

        public static async Task<decimal> GetLastPriceAsync(string symbol)
        {
            BinanceNetClient client = new BinanceNetClient();
            var getObj = client.GetOrderBookAsync(symbol).Result;

            return getObj.Bids.FirstOrDefault().Price;
        }

        #region Test

        private static void TestBinanceNetClient()
        {
            BinanceNetClient client = new BinanceNetClient();
            client.GetFuturesBalanceAsync().Wait();
        }

        public static async Task<OtherPosition> TestBinanceHttp()
        {
            BinanceHttpClient client = new BinanceHttpClient();
            return await client.GetPositionsAsync("FB***43");
        }

        #endregion
    }
}