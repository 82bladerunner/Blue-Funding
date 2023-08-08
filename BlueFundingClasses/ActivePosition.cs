using Binance.Net.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueFunding.BlueFundingClasses
{
    public class ActivePosition
    {

        public UniqueId uniqueId { get; set; }
        public Pair pair { get; set; }
        public double entryPrice { get; set; }
        public double markPrice { get; set; }
        public double amount { get; set; }
        public FuturesOrderDirection futuresOrderDirection { get; set; }
        public double takeProfit { get; set; }
        public double stopLoss { get; set; }
        public double pnl { get; set; }
        public double roe { get; set; }
        public DateTime updateTimeStamp { get; set; }
        public DateTime dateOpened { get; set; }
        public int leverage { get; set; }
        public bool isClosed { get; set; }
    }

    public class UniqueId
    {
        public string Id { get; set; }
        public UniqueId(string symbol, double entryPrice, double amount)
        {
            this.Id = symbol + "/" + entryPrice + "/" + amount;
        }
    }
    public class Pair
    {
        public string symbol { get; set; }
        public string BaseAsset { get; set; }
        public string QuoteAsset { get; set; }

        public Pair(string symbol)
        {
            if(symbol.Contains("USDT"))
            {

                this.symbol = symbol;
                this.BaseAsset = symbol.Split("USDT")[0];
                this.QuoteAsset = symbol.Split(BaseAsset)[1];
            }
            else if(symbol.Contains("BUSD"))
            {

                this.symbol = symbol;
                this.BaseAsset = symbol.Split("BUSD")[0];
                this.QuoteAsset = symbol.Split(BaseAsset)[1];
            }
        }

        public Pair(string baseAsset, string quoteAsset)
        {
            this.BaseAsset = baseAsset;
            this.QuoteAsset = quoteAsset;
        }
    }

    


}
