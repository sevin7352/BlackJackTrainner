using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackJackClasses.Model;
using BlackJackClasses.Enums;
using PlayingCards;

namespace BlackJackTrainner.Model
{
    public static class HandResultExtensions
    {

        public static string getCsvHeaders()
        {
            return
                "Return,DealerUpCard,Action,#OfHands,# Won, # Lost,# Pushed,CurrentValue,IsSpecial,CanSplit,CanDouble,ContainsAceAt11,Card1,Card2 \n";
        }

        public static bool AddSingleHandResults(this List<SingleHandResult> handResults,
            List<SingleHandResult> resultsToAdd)
        {
            foreach (var handResultToAdd in resultsToAdd)
            {
                try
                {
                    List<SingleHandResult> existingResults = null;
                    existingResults = handResults.Where(p =>
                        p.CurrentHandValue == handResultToAdd.CurrentHandValue &&
                        p.DealersUpCardValue == handResultToAdd.DealersUpCardValue && p.CanDouble == handResultToAdd.CanDouble && p.CanSplit == handResultToAdd.CanSplit && p.ContainsAceValuedAt11 == handResultToAdd.ContainsAceValuedAt11 ).ToList();

                    if (existingResults == null || existingResults.Count ==0)
                    {
                        handResults.Add(handResultToAdd);
                    }
                    else if (existingResults.Count > 1)
                    {
                        string tempstring = null;
                    }
                    else
                    {
                        foreach (var actionResult in handResultToAdd.ActionResults.Where(p =>
                            p.NumberOfHands > 0))
                        {
                            foreach (var tempActionResult in existingResults[0].ActionResults.Where(p =>
                                p.Type == actionResult.Type))
                            {
                                var combinedActionResult =
                                    ActionResultHelper.Combine(tempActionResult, actionResult);
                                tempActionResult.NumberOfHands = combinedActionResult.NumberOfHands;
                                tempActionResult.NumberOfHandsLost = combinedActionResult.NumberOfHandsLost;
                                tempActionResult.NumberOfHandsWon = combinedActionResult.NumberOfHandsWon;
                                tempActionResult.NumberOfHandspushed = combinedActionResult.NumberOfHandspushed;
                                tempActionResult.Return = combinedActionResult.Return;
                            }
                        }
                    }

                    

                }
                catch (IndexOutOfRangeException e)
                {
                    string tempe = e.Message;
                }
            }


            return true;
        }


        public static bool IsHandSpecial(this int[] cardNumbers)
        {
            if (cardNumbers.Length ==1 || (cardNumbers.Length == 2 && cardNumbers[0] == cardNumbers[1]))
            {
                return true;
            }

            if (cardNumbers.Length == 2)
            {
                return true;

            }

            if (cardNumbers.Contains(1) && GameStateExtensions.calculateValue(cardNumbers,true) <= 21)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool canDouble(this int[] cardNumbers)
        {
            if (cardNumbers.Length == 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool canSplit(this int[] cardNumbers)
        {
            if (cardNumbers.Length == 2 && cardNumbers[0] == cardNumbers[1])
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static HandSuggestions GetRandomSuggestions(PlayersHand playersHand, Random Random)
        {
            var handSuggesstion = new HandSuggestions()
            {
                Stay = Random.Next(0, 100),
            };

            if (playersHand.canHit())
            {
                handSuggesstion.Hit = Random.Next(0, 100);
            }
            if (playersHand.canDouble)
            {
                handSuggesstion.DoubleDown = Random.Next(0, 100);
            }
            if (playersHand.canSplit)
            {
                handSuggesstion.Split = Random.Next(0, 100);
            }

            return handSuggesstion;
        }

        public static HandSuggestions GetSingleHandSuggestions(this List<SingleHandResult> handResults, PlayersHand playersHand,Random random)
        {
            
             if (DeckHelper.DeckCardNumber(playersHand.hand.ToList()).IsHandSpecial())
             {

                 if (playersHand.canSplit)
                 {
                     var splitresults = handResults.Where(p => playersHand.hand[0].Value == p.CurrentHand[0]&& p.CurrentHand.Length==1 && p.DealersUpCardValue == playersHand.DealersUpCardValue).ToList();
                     var nonSplitresults = handResults.Where(p => DeckHelper.ContainsCardsValues(playersHand.hand.ToList(), p.CurrentHand) && p.DealersUpCardValue == playersHand.DealersUpCardValue).ToList();
                     List<ActionResult> actionResults = new List<ActionResult>();
                     if (splitresults.Count > 0)
                     {
                         if (splitresults.Count > 1)
                         {
                             string Flag= String.Empty;
                         }
                         actionResults.AddRange(splitresults[0].ActionResults);
                     }

                     if (nonSplitresults.Count > 0)
                     {
                         if (nonSplitresults.Count > 1)
                         {
                             string Flag = String.Empty;
                         }
                        actionResults.AddRange(nonSplitresults[0].ActionResults);
                     }

                    if (actionResults.Count == 0)
                     {
                         playersHand.HandSuggesstion = GetRandomSuggestions(playersHand, random);

                     }
                     else
                     {
                         playersHand.HandSuggesstion = new HandSuggestions();
                         if (actionResults.Count > 4)
                         {
                             string flag = String.Empty;
                         }
                         foreach (var result in actionResults)
                         {
                                 switch (result.Type)
                                 {
                                     case ActionTypes.Stay:
                                         playersHand.HandSuggesstion.Stay = result.Return;
                                         break;
                                     case ActionTypes.Hit:
                                         playersHand.HandSuggesstion.Hit = result.Return;
                                         break;
                                     case ActionTypes.Double:
                                         playersHand.HandSuggesstion.DoubleDown = result.Return;
                                         break;

                                     case ActionTypes.Split:
                                         playersHand.HandSuggesstion.Split = result.Return;
                                         break;
                                 }
                         }
                         

                     }

                     return playersHand.HandSuggesstion;
                 }
                 else if (playersHand.hand.Count(p => p.isAce) > 0)
                 {
                     //need to adjust to only worry about the value.
                     var Aceresults = handResults.Where(p => p.isSpecial && p.CurrentHandValue == playersHand.CurrentValue && p.DealersUpCardValue == playersHand.DealersUpCardValue).ToList();
                     if (!Aceresults.Any())
                     {
                         playersHand.HandSuggesstion = GetRandomSuggestions(playersHand, random);

                     }
                     else
                     {
                         playersHand.HandSuggesstion = new HandSuggestions();
                         if (Aceresults.Count == 1)
                         {
                             foreach (var result in Aceresults[0].ActionResults)
                             {
                                 switch (result.Type)
                                 {
                                     case ActionTypes.Stay:
                                         playersHand.HandSuggesstion.Stay = result.Return;
                                         break;
                                     case ActionTypes.Hit:
                                         playersHand.HandSuggesstion.Hit = result.Return;
                                         break;
                                     case ActionTypes.Double:
                                         playersHand.HandSuggesstion.DoubleDown = result.Return;
                                         break;

                                     case ActionTypes.Split:
                                         playersHand.HandSuggesstion.Split = result.Return;
                                         break;
                                 }
                             }
                         }

                     }
                     return playersHand.HandSuggesstion;
                }
             }

             var results = handResults.Where(p => p.CurrentHandValue == playersHand.CurrentValue && p.DealersUpCardValue == playersHand.DealersUpCardValue && !p.isSpecial).ToList();
                if (!results.Any())
                {
                    if (playersHand.CurrentValue <= 21)
                    {
                        string flag = String.Empty;
                    }
                    playersHand.HandSuggesstion = GetRandomSuggestions(playersHand, random);
                }
                else
                {
                    playersHand.HandSuggesstion = new HandSuggestions();
                    if (results.Count == 1)
                    {
                        foreach (var result in results[0].ActionResults)
                        {
                            switch (result.Type)
                            {
                                case ActionTypes.Stay:
                                    playersHand.HandSuggesstion.Stay = result.Return;
                                    break;
                                case ActionTypes.Hit:
                                    playersHand.HandSuggesstion.Hit = result.Return;
                                    break;
                                case ActionTypes.Double:
                                    playersHand.HandSuggesstion.DoubleDown = result.Return;
                                    break;

                                case ActionTypes.Split:
                                    playersHand.HandSuggesstion.Split = result.Return;
                                    break;
                            }
                        }
                    }

                }
                return playersHand.HandSuggesstion;
            

            

            
        }

     
        
    }
}
