using BlackJackClasses.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackClasses.Model.HandSuggestionGeneration
{
    public class SingleHandBookEntry
    {
        public SingleHandBookEntry(int dealersUpCard, int playersHandvalue, HandSuggestions handSuggestion, int[] playersHand = null)
        {
            DealersUpCard = dealersUpCard;
            PlayersHandvalue = playersHandvalue;
            PlayersHand = playersHand;
            HandSuggestion = handSuggestion;
        }


        public int DealersUpCard { get; set; }
        public int[] PlayersHand { get; set; }
        public int PlayersHandvalue { get; set; }
        public HandSuggestions HandSuggestion { get; set; }

    }
}
