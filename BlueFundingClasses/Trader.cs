using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueFunding.BlueFundingClasses
{
    
    public class Trader
    {
        public string name { get; set; }
        public string encryptedUid { get; set; }
        public decimal coefficient { get; set; }

        public Trader(string name, string encrytpedUid, decimal coefficient)
        {
            this.name = name;
            this.encryptedUid = encrytpedUid;
            this.coefficient = coefficient;
        }
    }

    
}

