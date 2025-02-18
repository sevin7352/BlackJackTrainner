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

        public SingleHandResult(int[] hand, int deckCount, bool canSplit = false, bool canDouble = false)
        {
            CurrentHand = hand;
            DeckCount = deckCount;
            CanSplit = canSplit;
            CanDouble = canDouble;
            ActionResults = new List<ActionResult>()
            {

            };
            if (hand != null)
            {
                if (hand.Length >= 2)
                {
                    ActionResults.Add(new ActionResult(ActionTypes.Stay));
                }

                if (CanHit)
                {
                    ActionResults.Add(new ActionResult(ActionTypes.Hit));
                }
                if (canDouble)
                {
                    ActionResults.Add(new ActionResult(ActionTypes.Double));
                }
                if (canSplit)
                {
                    ActionResults.Add(new ActionResult(ActionTypes.Split));
                    if (hand[0] > 10)
                    {
                        hand[0] = 10;
                    }
                    CurrentHand = new int[] { hand[0], hand[0] };
                }

                if (canSplit && !canDouble)
                {
                    int i = 0;
                }
            }
        }

        [BsonId]  // MongoDB will map this to the document's `_id`
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public int[] CurrentHand { get; set; }
        public int DealerUpCard { get; set; }
        public int DeckCount { get; set; }
        private int _currentValue { get; set; }

        public int CurrentHandValue
        {
            get
            {
                if (_currentValue == 0 && CurrentHand.Length > 0)
                {
                    if (CurrentHand.Length == 1)
                    {
                        _currentValue = GameStateExtensions.calculateValue(new int[] { CurrentHand[0], CurrentHand[0] });
                    }
                    else
                    {
                        _currentValue = GameStateExtensions.calculateValue(CurrentHand);
                    }

                }

                return _currentValue;


            }
        }

        public int NumberOfHands
        {
            get { return ActionResults.Sum(p => p.NumberOfHands); }
        }

        public List<ActionResult> ActionResults { get; set; }

        public bool CanSplit { get; set; }
        public bool CanDouble { get; set; }

        public bool ContainsAceValuedAt11
        {
            get
            {
                if (CurrentHand.Contains(1) && GameStateExtensions.calculateValue(CurrentHand, true) <= 21)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool CanHit
        {
            get { return CurrentHand.Length >= 2 && CurrentHandValue < 21; }
        }

        public bool isSpecial
        {
            get { return CurrentHand.IsHandSpecial(); }
        }

        public string WriteToCsv()
        {
            string message = string.Empty;
            foreach (var actionResult in ActionResults)
            {
                message += Math.Round(actionResult.Return, 6) + "," + DealerUpCard + "," + actionResult.Type + "," + actionResult.NumberOfHands + "," + actionResult.NumberOfHandsWon + "," + actionResult.NumberOfHandsLost + "," + actionResult.NumberOfHandspushed + "," +
                                 CurrentHandValue + "," + isSpecial + "," + CanSplit + "," + CanDouble + "," + ContainsAceValuedAt11 + ",";
                foreach (var hand in CurrentHand)
                {
                    message += hand + ",";
                }

                message += " \n";
            }

            return message;
        }
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
        //ToDo Define How Return should be determined.  using expected value.
        public double Return
        {
            get
            {
                if (NumberOfHands == 0) return 0; // Avoid division by zero

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


