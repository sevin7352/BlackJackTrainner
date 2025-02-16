using BlackJackTrainner.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackClasses.Model
{
    public class BlackJackActionRecord
    {
        public BlackJackActionRecord(long gameId,int handId,int actionIndex,int deckCount,bool canSplit,bool canDouble, int[] playersHand,int dealerUpCard)
        {
            GameId = gameId;
            HandId = HandId;
            ActionIndex = actionIndex;
            DeckCount = deckCount;
            CanSplit = canSplit;
            CanDouble = canDouble;
            PlayersHand = playersHand;
            DealerUpCard = dealerUpCard;
        }

        public long GameId { get; set; } // Unique identifier for the game
        public int HandId { get; set; } // If splitting is allowed, track hands separately
        public int ActionIndex { get; set; }// Order of action in a hand
        public int DeckCount { get; set; } //only used when counting cards is enabled
        public bool CanSplit { get; set; }
        public bool CanDouble {get;set;}
        public int[] PlayersHand { get; set; }
        public int PlayerTotal { get { return GameStateExtensions.calculateValue(PlayersHand);} } // Total hand value before the action
        public int DealerUpCard { get; set; } // Dealer's face-up card

        public ActionTypes Action { get; set; } // "Hit", "Stand", "Double", etc.
        public int CardDrawn { get; set; } // If the action was a hit, what card was drawn
        public bool ResultedInBust { get; set; } // Did the hand bust after this action?
        public int FinalHandValue { get; set; } // Final hand value at the end of the game
        public HandResultTypes GameOutcome { get; set; } // Win, Lose, Push, BlackJack
    }
}
