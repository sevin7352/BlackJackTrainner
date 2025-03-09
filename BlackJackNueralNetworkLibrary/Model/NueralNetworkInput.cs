using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackNueralNetworkLibrary.Model
{
    public class NueralNetworkInput
    {
        public int PlayerHandSum { get; set; }
        public int DealerUpCard { get; set; }
        public int NumberOfAces { get; set; }
        public int CanSplit { get; set; } // Binary flag: 0 = cannot split, 1 = can split
        public int CanDouble { get; set; } // Binary flag: 0 = cannot split, 1 = can double
        //public float BetAmount { get; set; } // Optional feature
        public int DeckCount { get; set; }
    }
}
