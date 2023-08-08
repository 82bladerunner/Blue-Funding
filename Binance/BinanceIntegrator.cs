using Binance.Net.Enums;
using Binance.Net.Objects.Models.Futures;
using BlueFunding.Binance.clients;
using BlueFunding.BlueFundingClasses;

namespace BlueFunding.Binance
{
    public class BinanceIntegrator
    {
        public async Task<List<ActivePosition>> GetAllUserPositionsAsync(Trader userId)
        {
            BinanceHttpClient client = new BinanceHttpClient();
            var getObj = client.GetPositionsAsync(userId.encryptedUid).Result;

            var returnObj = new List<ActivePosition>();


            foreach (var position in getObj.data.otherPositionRetList)
            {
                returnObj.Add(ParseActivePosition(position));
            }

            return returnObj;
        }

        public async Task<List<ActivePosition>> StartWatchingUserPositionsAsync(Trader trader, DateTime dateInitialized)
        {
            //returns positions only if they are opened after initializing this client

            BinanceHttpClient client = new BinanceHttpClient();
            var getObj = await client.GetPositionsAsync(trader.encryptedUid);

            var returnObj = new List<ActivePosition>();

            if(getObj.data != null)
            {
                foreach (var position in getObj.data.otherPositionRetList)
                {
                    var tempPos = ParseActivePosition(position);

                    if (dateInitialized.CompareTo(tempPos.dateOpened) <= 0)
                    {
                        returnObj.Add(tempPos);
                    }
                }
            }
            return returnObj;

        }

        public async Task<ActivePosition> GetLastUserTradeAsync(Pair pair)
        {
            BinanceNetClient client = new BinanceNetClient();
            var getObj = await client.GetLastUserTradesAsync(pair);
            var returnObj = new ActivePosition();
            var buyerList = new List<BinanceFuturesUsdtTrade>();

            var buyers = getObj.FindAll(q => !q.Buyer);
            
            foreach (var buyer in buyers)
            {
                returnObj.amount += (double)buyer.Quantity;
                returnObj.pnl += (double)buyer.RealizedPnl;
                returnObj.isClosed = true;
                returnObj.updateTimeStamp = buyer.Timestamp;
            }
         
            return returnObj;
        }

        private OrderSide OrderTypeConvert(FuturesOrderDirection futuresOrderType)
        {
            if (futuresOrderType == FuturesOrderDirection.LONG)
            {
                return OrderSide.Buy;
            }
            else return OrderSide.Sell;
        }

        private ActivePosition ParseActivePosition(models.OtherPositionRetList position)
        {
            var tempPos = new ActivePosition();

            tempPos.pair = new Pair(position.symbol);
            tempPos.entryPrice = position.entryPrice;
            tempPos.markPrice = position.markPrice;
            tempPos.amount = position.amount;
            tempPos.pnl = position.pnl;
            tempPos.roe = position.roe;
            tempPos.uniqueId = new UniqueId(position.symbol, position.entryPrice, position.amount);
            tempPos.updateTimeStamp = DateTime.UtcNow;
            tempPos.leverage = CalculateLeverage(position.entryPrice, position.markPrice, position.roe);
            tempPos.futuresOrderDirection = DirectionFinder(position.amount);
            tempPos.dateOpened = new DateTime(
                position.updateTime[0], //Year
                position.updateTime[1], //Month
                position.updateTime[2], //Day
                position.updateTime[3], //Hour
                position.updateTime[4], //Minute
                position.updateTime[5]); //Second

            return tempPos;
        }

        private FuturesOrderDirection DirectionFinder(double amount)
        {
            if (amount < 0)
            {
                return FuturesOrderDirection.SHORT;
            }
            else return FuturesOrderDirection.LONG;
        }

        private static int CalculateLeverage(double entryPrice, double markPrice, double posRoe)
        {
            var pricePnl = (Math.Abs((markPrice - entryPrice) / entryPrice));
            return (int)(Math.Round((Math.Abs(posRoe) / pricePnl), MidpointRounding.ToEven));
        }

    }
}
