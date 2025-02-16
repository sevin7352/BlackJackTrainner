using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackClasses.Model
{
    public class BlackJackRules
    {
        public string name { get; set; }
        public double payoutForBlackJack { get; set; }
        public int numberOfDecks { get; set; }
        //not Implemented
        public bool DealerPushOn22 { get; set; }
        //not Implemented
        public bool FreeDouble { get; set; }
        //not Implemented
        public bool FreeSplit { get; set; }
        //not Implemented
        public bool HitOnSplitAces { get; set; }
        //not Implemented
        public int MaxNumberOfSplits { get; set; }
        
        //not Implemented
        //public bool AutomaticShuffler { get; set; }
    }
}
