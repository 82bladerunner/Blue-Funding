using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlueFunding.BlueFundingClasses
{
    public class ActivePositionsPool
    {
        public HashSet<ActivePosition> positions { get; set; }

        public HashSet<ActivePosition> activePosHash = new HashSet<ActivePosition>();
    }
}
