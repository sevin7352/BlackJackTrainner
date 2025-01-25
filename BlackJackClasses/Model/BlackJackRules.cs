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
        public bool DealerPushOn22 { get; set; }
        public bool FreeDouble { get; set; }
        public bool FreeSplit { get; set; }
        public bool HitOnSplitAces { get; set; }
        public int MaxNumberOfSplits { get; set; }
        public int numberOfDecks { get; set; }
        public bool AutomaticShuffler { get; set; }
    }
}
