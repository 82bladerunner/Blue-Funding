using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueFunding.BlueFundingClasses
{
    public class TraderList
    {
        public static List<Trader> Traders = new List<Trader>()
        {
            new Trader("trader1", "D****1", 0.0001m),
            new Trader("trader2", "FB***3", 0.0001m),
            new Trader("trader3", "D6***62", 0.0001m),
            new Trader("trader4", "8***77", 0.00001m),
            new Trader("trader5", "C***2", 0.0001m),
            new Trader("trader6", "0****9", 0.01m),
            new Trader("trader7", "0***6", 0.001m),
            new Trader("TEST", "...", 1m)

        };
    }
}
