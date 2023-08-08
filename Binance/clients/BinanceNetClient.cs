using Binance.Net.Clients;
using Binance.Net.Enums;
using Binance.Net.Objects;
using Binance.Net.Objects.Models.Futures;
using BlueFunding.BlueFundingClasses;
using CryptoExchange.Net.Authentication;
using CryptoExchange.Net.CommonObjects;

namespace BlueFunding.Binance.clients
{
    public class BinanceNetClient
    {
        //user
        public string APIKEY = "***";
        public string APISECRET = "***";

        public static BinanceClient client = new BinanceClient();

        public BinanceNetClient()
        {
            BinanceClient.SetDefaultOptions(new BinanceClientOptions()
            {
                ApiCredentials = new ApiCredentials(APIKEY, APISECRET),
            });
        }

        public async Task<List<ActivePosition>> GetOpenOrdersAsync()
        {
            var getObj = await client.UsdFuturesApi.Trading.GetOpenOrdersAsync();

            var returnObj = new List<ActivePosition>();

            if(getObj != null)
            {
                foreach (var item in getObj.Data)
                {
                    var tempObj = new ActivePosition();

                    //item.Id;
                    tempObj.pair = new Pair(item.Symbol);
                    tempObj.entryPrice = (double)item.Price;
                    tempObj.amount = (double)item.Quantity;
                    //item.QuantityFilled;
                    //item.Status;
                    //item.Side;
                    //tempObj.futuresOrderType = item.Type;
                    tempObj.dateOpened = item.CreateTime;

                    returnObj.Add(tempObj);
                }
            }

            return returnObj;
        }

        public async Task<List<ActivePosition>> GetOpenPositionsAsync()
        {
            var getObj = client.UsdFuturesApi.CommonFuturesClient.GetPositionsAsync().Result.Data;

            List<ActivePosition> returnPositions = new List<ActivePosition>();
            ActivePosition tempObj = new ActivePosition();

            if(getObj != null) {
                foreach (var item in getObj)
                {
                    if (item.EntryPrice > 0)
                    {
                        tempObj.amount = (double)item.Quantity;
                        tempObj.entryPrice = (double)item.EntryPrice;
                        tempObj.leverage = (int)item.Leverage;
                        tempObj.markPrice = (double)item.MarkPrice;
                        tempObj.pair = new Pair(item.Symbol);
                        tempObj.pnl = (double)item.UnrealizedPnl;
                        tempObj.uniqueId = new UniqueId(item.Symbol, (double)item.EntryPrice, (double)item.Quantity);

                        if(item.Quantity > 0)
                        {
                            tempObj.futuresOrderDirection = FuturesOrderDirection.LONG;
                        }
                        else tempObj.futuresOrderDirection= FuturesOrderDirection.SHORT;

                        returnPositions.Add(tempObj);
                    }
                }
            }


            return returnPositions;

        }

        public async Task<Balance> GetUsdtBalanceAsync()
        {
            var getObj = await client.UsdFuturesApi.Account.GetBalancesAsync();
            Balance balance = new Balance();

            foreach (var item in getObj.Data)
            {
                if(item.Asset == "USDT")
                {
                    balance.Asset = "USDT";
                    balance.Available = item.AvailableBalance;
                    balance.Total = item.WalletBalance;
                }
            }

            return balance;
        }

        public async Task<BinanceFuturesUsdtExchangeInfo> GetExchangeInfoAsync()
        {
            var getObj = await client.UsdFuturesApi.ExchangeData.GetExchangeInfoAsync();

            return getObj.Data;
        }
        public async Task<OrderBook> GetOrderBookAsync(string symbol)
        {
            var getObj = await client.SpotApi.CommonSpotClient.GetOrderBookAsync(symbol);

            return getObj.Data;
        }

        public async void GetFuturesFundingRateAsync(string symbol)
        {
            var getObj = await client.CoinFuturesApi.ExchangeData.GetFundingRatesAsync(symbol);
        }

        public async Task<IEnumerable<BinanceFuturesAccountBalance>> GetFuturesBalanceAsync()
        {
            var result = await client.CoinFuturesApi.Account.GetBalancesAsync();

            return result.Data;

        }
        public async Task<BinanceFuturesPlacedOrder> PostMarketOrderAsync(string symbol, OrderSide
            side, decimal quantity)
        {
            var result = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol, side, FuturesOrderType.Market, quantity);
            return result.Data;
        }

        public async Task<BinanceFuturesPlacedOrder> PostLimitOrderAsync(string symbol, OrderSide side, decimal quantity, decimal price)
        {
            var result = await client.UsdFuturesApi.Trading.PlaceOrderAsync(symbol,
                side,
                FuturesOrderType.Limit,
                quantity,
                price,
                PositionSideConverter(side),
                TimeInForce.GoodTillCanceled);
            return result.Data;
        }

        public async Task<BinanceFuturesPlacedOrder> CloseFuturesPositionAsync(ActivePosition pos)
        {


            OrderSide closingSide = default;

            if(pos.futuresOrderDirection == FuturesOrderDirection.LONG)
            {
                closingSide = OrderSide.Sell;
            }
            else closingSide = OrderSide.Buy;

            var result = await client.UsdFuturesApi.Trading.PlaceOrderAsync(
                reduceOnly: true,
                symbol: pos.pair.symbol,
                side: closingSide,
                type: FuturesOrderType.Market,
                quantity: Math.Abs((decimal)pos.amount * 10)
                );

            return result.Data;
        }

        public async Task<List<BinanceFuturesUsdtTrade>> GetLastUserTradesAsync(Pair pair)
        {
            var getObj = client.UsdFuturesApi.Trading.GetUserTradesAsync(pair.symbol).Result.Data;

            return getObj.ToList();
        }

        private PositionSide PositionSideConverter(OrderSide side)
        {
            if (side == OrderSide.Buy)
            {
                return PositionSide.Long;
            }
            else return PositionSide.Short;
        }

    }
}
