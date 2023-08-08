using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueFunding.BlueFundingClasses
{
    public class UserList
    {
        public static List<User> Users = new List<User>()
        {
            new User("user1", "-", "...", "..."),
            new User("user2", "-", "...", "... ")
        };
    }
}
