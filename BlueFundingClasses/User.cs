using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueFunding.BlueFundingClasses
{
        public class User
        {
            public string name { get; set; }
            public string email { get; set; }
            public string APIKEY { get; set; }
            public string APISECRET { get; set; }

            public User(string name, string email, string APIKEY, string APISECRET)
            {
                this.name = name;
                this.email = email;
                this.APIKEY = APIKEY;
                this.APISECRET = APISECRET; 
            }
        }
}

