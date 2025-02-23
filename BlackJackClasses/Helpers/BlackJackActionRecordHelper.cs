using BlackJackClasses.Enums;
using BlackJackClasses.Model;
using BlackJackClasses.Enums;
using DnsClient.Protocol;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Configuration;
using PlayingCards;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BlackJackClasses.Helpers
{
    public static class BlackJackActionRecordHelper
    {
        private const string ConnectionString = "mongodb://localhost:27017/";
        private const string mongoBatabaseName = "BlackJackDataBase";
        private const string actionResultSummaryCollectionName = "_ActionResultSummary_Deep";
        

        static BlackJackActionRecordHelper()
        {
            
        }

        public static void SaveGameRecords(List<BlackJackActionRecord> records, string RulesName)
        {
            if(records == null || records.Count == 0) return;

            var mongoClient = new MongoClient(ConnectionString);
            var collection = mongoClient.GetDatabase(mongoBatabaseName).GetCollection<BlackJackActionRecord>(RulesName);

            foreach (var record in records) {
                
                //Console.WriteLine($"Before Insert - Id: {record.Id}");
                if(record.Id != null)
                {
                    record.Id = null;
                    //Console.WriteLine($"Before After Reset - Id: {record.Id}");
                }
                //Have to insert one a time.
                collection.InsertOne(record);
                //Thread.Sleep(1);
            }
        }

        public static List<BlackJackActionRecord> LoadAllRecords(string RulesName)
        {
            var mongoClient = new MongoClient(ConnectionString);
            var collection = mongoClient.GetDatabase(mongoBatabaseName).GetCollection<BlackJackActionRecord>(RulesName);




            return collection.AsQueryable().ToList();
        }

        public static List<BlackJackActionRecord> GetRelevantRecords(string RulesName,PlayersHand playerHand,int deckCount) {

            
            var mongoClient = new MongoClient(ConnectionString);
            var ruleDatacollection = mongoClient.GetDatabase(mongoBatabaseName).GetCollection<BlackJackActionRecord>(RulesName);
            //int[] playerHandCardValues = playerHand.hand.OrderByDescending(p => p.Value).Select(x => x.Value).ToArray();
            var results = ruleDatacollection.AsQueryable().Where(p=> p.DeckCount == deckCount &&  p.PlayerTotal == playerHand.CurrentValue && p.DealerUpCard == playerHand.DealersUpCardValue 
                        && p.CanDouble == playerHand.canDouble && p.CanSplit == playerHand.canSplit 
                        && p.ContainsAceValuedAs11 == playerHand.ContainsAceValuedAt11).ToList();

            Console.WriteLine("Relevant Records:" + results.Count + " Dealer Card:" + playerHand.DealersUpCardValue + " Players Hand:" + playerHand.CurrentValue  + " Deck Count:" + deckCount);


            return results;
        }

        public static HandSuggestions GetHandSuggestions(string RulesName, PlayersHand playersHand,int deckCount,int depth = 0)
        {
            //Rework To pull from Summary

            //Rework to Look in summary collection first.
            var mongoClient = new MongoClient(ConnectionString);
            var ruleDatacollection = mongoClient.GetDatabase(mongoBatabaseName).GetCollection<BlackJackActionRecord>(RulesName);
            var ruleSummarycollection = mongoClient.GetDatabase(mongoBatabaseName).GetCollection<SingleHandResult>(RulesName + actionResultSummaryCollectionName);
            //int[] playerHandCardValues = playersHand.hand.OrderByDescending(p => p.Value).Select(x => x.Value).ToArray();
            var summariesForHand = ruleSummarycollection.AsQueryable().Where(p => p.DeckCount == deckCount && p.DealerUpCard == playersHand.DealersUpCardValue && p.CanDouble == playersHand.canDouble && p.CanSplit == playersHand.canSplit && p.CanHit == playersHand.canHit  && p.ContainsAceAs11 == playersHand.ContainsAceValuedAt11 && p.CurrentHandValue == playersHand.CurrentValue).ToList();
            if (summariesForHand.Count() > 0) {
            
                if(summariesForHand.Count()>1)
                {
                    Console.WriteLine("This Should not Happen");
                    var idsToDelete = summariesForHand.OrderBy(p => p.NumberOfHands).ToList();
                    ruleSummarycollection.DeleteOne(p=> p.Id == idsToDelete[0].Id);
                }
                
                    Console.WriteLine("Summary found:" + summariesForHand.Count + " Dealer Card:" + playersHand.DealersUpCardValue + " Players Hand:" + playersHand.CurrentValue.ToString() + " Deck Count:" + deckCount);
                    //Convert To hand Suggestion and Return
                    return HandResultExtensions.GetFromSingleHandResult(summariesForHand.FirstOrDefault());
                
            
            }
            if (depth > 5)  // Prevent infinite recursion
            {
                Console.WriteLine("Max recursion depth reached in GetHandSuggestions.");
                return null;
            }

            //No Summary found.  Create Summary and return it.
            Console.WriteLine("No Summary found, Create One:" + summariesForHand.Count + " Dealer Card:" + playersHand.DealersUpCardValue + " Players Hand:" + playersHand.CurrentValue.ToString() + " Deck Count:" + deckCount);
            var createdSummary = CreateSingleHandResultFromRecords(RulesName,playersHand, deckCount, depth + 1);

            return HandResultExtensions.GetFromSingleHandResult(createdSummary);
        }

        //TODO look at how to look multiple levels deep.  Basically when looking at an action that can be followed by another action (hit and split)
        //only count the hand where the best next action was used. -> recursivly. <- will be slow but worth it.
        private static SingleHandResult CreateSingleHandResultFromRecords(string RulesName, PlayersHand playersHand, int deckCount, int depth = 0)
        {

            var mongoClient = new MongoClient(ConnectionString);
            var ruleSummarycollection = mongoClient.GetDatabase(mongoBatabaseName).GetCollection<SingleHandResult>(RulesName + actionResultSummaryCollectionName);
            var records = GetRelevantRecords(RulesName, playersHand, deckCount);

            //Now Process records into a singleHandResult
            
            if (!records.Any())
            {
                Console.WriteLine("No Records to make Summary from");
                //No Records in 7 million hands?
                return null;
            }

            // Group by action and calculate win probability
            
            var summary = new SingleHandResult(playersHand.CurrentValue,playersHand.DealersUpCardValue, deckCount,playersHand.ContainsAceValuedAt11, playersHand.canSplit, playersHand.canDouble) {CanHit = playersHand.canHit };
            var groupedActions = records.Where(r => IsOptimalFollowOn(r, RulesName, depth)).GroupBy(r => r.Action) //.Where(r => IsOptimalFollowOn(r, RulesName))
                .ToDictionary(g => g.Key, g => new
                {
                    Total = g.Count(),
                    Push = g.Count(x => x.GameOutcome == HandResultTypes.Push),
                    Lost = g.Count(x => x.GameOutcome == HandResultTypes.Loose),
                    Wins = g.Count(r => r.GameOutcome == HandResultTypes.Win || r.GameOutcome == HandResultTypes.BlackJack)
                });
            

            int totalActions = records.Count;

            if (groupedActions.TryGetValue(ActionTypes.Stay, out var stayStats))
            {
                var set = summary.ActionResults.FirstOrDefault(p => p.Type == ActionTypes.Stay);
                if (set != null)
                {
                    summary.ActionResults.Remove(set);
                }

                var toAdd = new ActionResult(ActionTypes.Stay)
                {
                    NumberOfHands = stayStats.Total,
                    NumberOfHandsLost = stayStats.Lost,
                    NumberOfHandspushed = stayStats.Push,
                    NumberOfHandsWon = stayStats.Wins,
                };

                
                    summary.ActionResults.Add(toAdd);
                

            }
            

            if (groupedActions.TryGetValue(ActionTypes.Hit, out var hitStats))
            {
                var set = summary.ActionResults.FirstOrDefault(p => p.Type == ActionTypes.Hit);
                if (set != null)
                {
                    summary.ActionResults.Remove(set);
                }
                var toAdd = new ActionResult(ActionTypes.Hit)
                {
                    NumberOfHands = hitStats.Total,
                    NumberOfHandsLost = hitStats.Lost,
                    NumberOfHandspushed = hitStats.Push,
                    NumberOfHandsWon = hitStats.Wins,
                };

                
                    summary.ActionResults.Add(toAdd);
                
            }

            if (groupedActions.TryGetValue(ActionTypes.Split, out var splitStats))
            {
                var set = summary.ActionResults.FirstOrDefault(p => p.Type == ActionTypes.Split);
                if (set != null)
                {
                    summary.ActionResults.Remove(set);
                }
                var toAdd = new ActionResult(ActionTypes.Split)
                {
                    NumberOfHands = splitStats.Total,
                    NumberOfHandsLost = splitStats.Lost,
                    NumberOfHandspushed = splitStats.Push,
                    NumberOfHandsWon = splitStats.Wins,
                };

                    summary.ActionResults.Add(toAdd);
                
            }

            if (groupedActions.TryGetValue(ActionTypes.Double, out var doubleStats))
            {
                var set = summary.ActionResults.FirstOrDefault(p => p.Type == ActionTypes.Double);
                if (set != null)
                {
                    summary.ActionResults.Remove(set);
                }

                var toAdd = new ActionResult(ActionTypes.Double)
                {
                    NumberOfHands = doubleStats.Total,
                    NumberOfHandsLost = doubleStats.Lost,
                    NumberOfHandspushed = doubleStats.Push,
                    NumberOfHandsWon = doubleStats.Wins,
                };

               
                summary.ActionResults.Add(toAdd);
                
            }


            //Save To DB before Saving
            ruleSummarycollection.InsertOne(summary);

            return summary;
        }
        private static bool IsOptimalFollowOn(BlackJackActionRecord action,string RulesName,int depth = 0)
        {
            var mongoClient = new MongoClient(ConnectionString);
            var ruleDatacollection = mongoClient.GetDatabase(mongoBatabaseName).GetCollection<BlackJackActionRecord>(RulesName);

            // Find all subsequent actions for the same hand
            var followOnActions = ruleDatacollection.AsQueryable().Where(a =>a.GameId == action.GameId && a.HandId == action.HandId && a.ActionIndex > action.ActionIndex) // Find ALL future actions
        .OrderBy(a => a.ActionIndex) // Ensure we process them in order
        .ToList();

            if (followOnActions.Count == 0)
            {
                // If no follow-on actions exist, evaluate this action alone
                return true;
            }

            foreach (var nextAction in followOnActions)
            {
                if (nextAction.Action == ActionTypes.Stay || nextAction.ResultedInBust)
                {
                    // Hand is finished (either the player stood or busted)
                    return true;
                }

                // Get the best recommended action for this situation
                var playerHand = PlayersHandHelper.CreatePlayersHand(
                    nextAction.PlayerTotal, nextAction.DealerUpCard,
                    nextAction.CanSplit, nextAction.CanDouble,
                    nextAction.ContainsAceValuedAs11);

                var suggestion = GetHandSuggestions(RulesName, playerHand, nextAction.DeckCount, depth);

                if (suggestion == null)
                {
                    return false; // No suggestion? Assume not optimal.
                }

                ActionTypes bestAction = suggestion.SuggestedAction(
                    nextAction.CanSplit, nextAction.CanDouble, true);

                // If any action taken is NOT optimal, return false
                if (bestAction != nextAction.Action)
                {
                    return false;
                }
            }

            // If all follow-on actions are optimal, return true
            return true;
        }


    }
}
