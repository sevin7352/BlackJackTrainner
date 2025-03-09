using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using BlackJackClasses.Enums;
using PlayingCards;

namespace BlackJackClasses.Model
{
    public static class HandResultExtensions
    {

        public static string getCsvHeaders()
        {
            return
                "Return,DealerUpCard,Action,#OfHands,# Won, # Lost,# Pushed,CurrentValue,IsSpecial,CanSplit,CanDouble,ContainsAceAt11,Card1,Card2 \n";
        }

        //this needs to be reworked.

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
                        p.DealerUpCard == handResultToAdd.DealerUpCard && p.CanDouble == handResultToAdd.CanDouble && p.CanSplit == handResultToAdd.CanSplit && p.ContainsAceAs11 == handResultToAdd.ContainsAceAs11).ToList();

                    if (existingResults == null || existingResults.Count == 0)
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
            if (cardNumbers.Length == 1 || cardNumbers.Length == 2 && cardNumbers[0] == cardNumbers[1])
            {
                return true;
            }

            if (cardNumbers.Length == 2)
            {
                return true;

            }

            if (cardNumbers.Contains(1) && GameStateExtensions.calculateValue(cardNumbers, true) <= 21)
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

            if (playersHand.canHit)
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

        public static HandSuggestions GetFromSingleHandResult(SingleHandResult result)
        {
            if(result == null)
            {
                Console.WriteLine("Result passed into Get Single hand is null");
                return null;
            }

            HandSuggestions handSuggesstion = new HandSuggestions();
            var actionResults = result.ActionResults;
            if (actionResults.Count > 4)
            {
                string flag = string.Empty;
            }
            if(result.NumberOfHands <10)
            {
                Console.WriteLine("Less than 10 samples");
            }

            foreach (var actionresult in actionResults)
            {
                switch (actionresult.Type)
                {
                    case ActionTypes.Stay:
                        handSuggesstion.Stay = actionresult.Return;
                        break;
                    case ActionTypes.Hit:
                        handSuggesstion.Hit = actionresult.Return;
                        break;
                    case ActionTypes.Double:
                        handSuggesstion.DoubleDown = actionresult.Return*2;
                        break;

                    case ActionTypes.Split:
                        handSuggesstion.Split = actionresult.Return;
                        break;
                }
            }


            return handSuggesstion;
        }

        public static HandSuggestions GetSingleHandSuggestions(this List<SingleHandResult> handResults, PlayersHand playersHand, Random random)
        {

            if (DeckHelper.DeckCardNumber(playersHand.hand.ToList()).IsHandSpecial())
            {

                if (playersHand.canSplit)
                {
                    var splitresults = handResults.Where(p => playersHand.CurrentValue == p.CurrentHandValue && p.ContainsAceAs11 == playersHand.ContainsAceValuedAt11 && p.DealerUpCard == playersHand.DealersUpCardValue).ToList();
                    var nonSplitresults = handResults.Where(p => p.CanSplit).ToList();
                    List<ActionResult> actionResults = new List<ActionResult>();
                    if (splitresults.Count > 0)
                    {
                        if (splitresults.Count > 1)
                        {
                            string Flag = string.Empty;
                        }
                        actionResults.AddRange(splitresults[0].ActionResults);
                    }

                    if (nonSplitresults.Count > 0)
                    {
                        if (nonSplitresults.Count > 1)
                        {
                            string Flag = string.Empty;
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
                            string flag = string.Empty;
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
                    var Aceresults = handResults.Where(p => p.ContainsAceAs11 == playersHand.ContainsAceValuedAt11 && p.CurrentHandValue == playersHand.CurrentValue && p.DealerUpCard == playersHand.DealersUpCardValue).ToList();
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

            var results = handResults.Where(p => p.CurrentHandValue == playersHand.CurrentValue && p.DealerUpCard == playersHand.DealersUpCardValue && p.ContainsAceAs11 == playersHand.ContainsAceValuedAt11 ).ToList();
            if (!results.Any())
            {
                if (playersHand.CurrentValue <= 21)
                {
                    string flag = string.Empty;
                }
                playersHand.HandSuggesstion = GetRandomSuggestions(playersHand, random);
            }
            else
            {
                playersHand.HandSuggesstion = new HandSuggestions();
                if (results.Count() == 1)
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
