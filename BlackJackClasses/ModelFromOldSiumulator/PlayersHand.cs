using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackJackTrainner.Enums;
using BlackJackTrainner.Model.HandSuggestionGeneration;
using PlayingCards;

namespace BlackJackTrainner.Model
{
    public class PlayersHand
    {


        public PlayersHand(int startingBet)
        {
            hand = new ObservableCollection<PlayingCard>();
            ActionsTaken = new List<ActionTypes>();
            HandResults = new List<SingleHandResult>();
            HandResult = HandResultTypes.unkown;
            StartingBet = startingBet;
            EndingBet = startingBet;
        }

        public ObservableCollection<PlayingCard> hand { get; set; }
        public List<ActionTypes> ActionsTaken { get; set; }

        public List<SingleHandResult> HandResults { get; set; }
        public List<int> CardCountAtAction { get; set; }
        public int StartingBet { get; set; }
        public int EndingBet { get; set; }
        public double MoneyWon { get; set; }
        public int DealersUpCardValue { get; set; }

        public bool handOver { get; set; }

        public int CurrentValue
        {
            get { return GameStateExtensions.calculateValue(hand.ToList()); }
        }

        public HandResultTypes HandResult { get; set; }

        public bool canDouble()
        {
            return hand.Count == 2 && CurrentValue < 21 && !handOver;
        }

        public bool OverrideCanSplit { get; set; }
        public bool canSplit()
        {

            if (OverrideCanSplit)
            {
                return false;
            }
            else
            {
                return hand.Count == 2 && hand[0].CardNumber == hand[1].CardNumber && !handOver;
            }
        }

        public bool canHit()
        {
            return CurrentValue < 21 && !handOver;
        }

        public HandSuggestions HandSuggesstion { get; set; }

        public double MoneyReturn
        {
            get { return (MoneyWon - EndingBet) / StartingBet; }
        }
    }
}
