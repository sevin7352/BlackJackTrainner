using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackJackClasses.Enums;
using BlackJackClasses.Model.HandSuggestionGeneration;
using PlayingCards;

namespace BlackJackClasses.Model
{
    public class PlayersHand
    {


        public PlayersHand(int startingBet)
        {
            hand = new ObservableCollection<PlayingCard>();
            ActionRecords = new List<BlackJackActionRecord>();
            HandResult = HandResultTypes.unkown;
            StartingBet = startingBet;
            EndingBet = startingBet;
        }

        public ObservableCollection<PlayingCard> hand { get; set; }
        public List<BlackJackActionRecord> ActionRecords { get; set; }
        public List<int> CardCountAtAction { get; set; }
        public int StartingBet { get; set; }
        public int EndingBet { get; set; }
        public double MoneyWon { get; set; }
        public int DealersUpCardValue { get; set; }
        public int NumberOfAces { get
            {
                return hand.Count(p=>p.CardNumber == 1);
            } }

        public bool ContainsAceValuedAt11
        {
            get
            {
                return GameStateExtensions.IsAceCountedAs11(hand.ToList());
            }
        }
        public bool handOver { get; set; }

        public int CurrentValue
        {
            get { return GameStateExtensions.calculateValue(hand.ToList()); }
        }

        public HandResultTypes HandResult { get; set; }

        public bool canDouble
        {
            get
            {
                return hand.Count == 2 && CurrentValue < 21 && !handOver;
            }
        }

        public bool OverrideCanSplit { get; set; }
        public bool canSplit { get
            {
                //ToDo Allow Mulitple Splits.
                if (OverrideCanSplit)
                {
                    return false;
                }
                else
                {
                    return hand.Count == 2 && hand[0].CardNumber == hand[1].CardNumber && !handOver;
                }
            }
        }
        

        public bool canHit
        {
            get
            {
                return CurrentValue < 21 && !handOver;
            }
        }

        public HandSuggestions HandSuggesstion { get; set; }

        public double MoneyReturn
        {
            get { return (MoneyWon - EndingBet) / StartingBet; }
        }
    }
}
