using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackJackClasses.Enums;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using PlayingCards;

namespace BlackJackClasses.Model
{
    public class SingleHandResult
    {

        public SingleHandResult(int handValue, int dealerUpCard , int deckCount, bool aceAs11, bool canSplit = false, bool canDouble = false)
        {
            CurrentHandValue = handValue;
            DeckCount = deckCount;
            CanSplit = canSplit;
            CanDouble = canDouble;
            DealerUpCard = dealerUpCard;
            ContainsAceAs11 = aceAs11;
            ActionResults = new List<ActionResult>()
            {

            };
            
        }

        [BsonId]  // MongoDB will map this to the document's `_id`
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        public int DealerUpCard { get; set; }
        public int DeckCount { get; set; } 
        public int CurrentHandValue
        {
            get; set;
        }

        public int NumberOfHands
        {
            get { return ActionResults.Sum(p => p.NumberOfHands); }
        }

        public List<ActionResult> ActionResults { get; set; }

        public bool CanSplit { get; set; }
        public bool CanDouble { get; set; }

        public bool ContainsAceAs11 { get; set; }

        public bool CanHit { get; set; }

    }

    public class ActionResult
    {
        public ActionResult(ActionTypes type)
        {
            Type = type;
        }

        public ActionTypes Type { get; set; }
        public int NumberOfHands { get; set; }
        public int NumberOfHandsWon { get; set; }
        public int NumberOfHandsLost { get; set; }
        public int NumberOfHandspushed { get; set; }
        public double Return
        {
            get
            {
                if (NumberOfHands == 0)
                {
                    return 0; // Avoid division by zero
                }

                return (double)(NumberOfHandsWon - NumberOfHandsLost) / NumberOfHands;
            }
        }
    }

    public static class ActionResultHelper
    {
        public static ActionResult Combine(ActionResult one, ActionResult two)
        {


            int TotalHands = one.NumberOfHands + two.NumberOfHands;
            
            return new ActionResult(one.Type)
            {
                
                NumberOfHands = TotalHands,
                NumberOfHandsWon = one.NumberOfHandsWon + two.NumberOfHandsWon,
                NumberOfHandsLost = one.NumberOfHandsLost + two.NumberOfHandsLost,
                NumberOfHandspushed = one.NumberOfHandspushed + two.NumberOfHandspushed,
            };
        }
    }











}


