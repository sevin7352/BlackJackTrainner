using BlackJackClasses.Enums;
using BlackJackClasses.Model;
using BlackJackTrainner.Enums;
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
        private const string actionResultSummaryCollectionName = "_ActionResultSummary";
        

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

            //Rework to Look in summary collection first.
            var mongoClient = new MongoClient(ConnectionString);
            var ruleDatacollection = mongoClient.GetDatabase(mongoBatabaseName).GetCollection<BlackJackActionRecord>(RulesName);
            int[] playerHandCardValues = playerHand.hand.OrderByDescending(p => p.Value).Select(x => x.Value).ToArray();
            var results = ruleDatacollection.AsQueryable().Where(p=> p.DeckCount == deckCount &&  p.PlayerTotal == playerHand.CurrentValue && p.DealerUpCard == playerHand.DealersUpCardValue 
                        && p.CanDouble == playerHand.canDouble && p.CanSplit == playerHand.canSplit 
                        && p.PlayersHand.OrderByDescending(p=>p).ToArray() == playerHandCardValues).ToList();

            Console.WriteLine("Relevant Records:" + results.Count + " Dealer Card:" + playerHand.DealersUpCardValue + " Players Hand:" + GameStateExtensions.calculateValue(playerHand.hand.ToList()).ToString());


            return results;
        }

        public static HandSuggestions GetHandSuggestions(string RulesName, PlayersHand playersHand,int deckCount)
        {
            //Rework To pull from Summary

            //Rework to Look in summary collection first.
            var mongoClient = new MongoClient(ConnectionString);
            var ruleDatacollection = mongoClient.GetDatabase(mongoBatabaseName).GetCollection<BlackJackActionRecord>(RulesName);
            var ruleSummarycollection = mongoClient.GetDatabase(mongoBatabaseName).GetCollection<SingleHandResult>(RulesName + actionResultSummaryCollectionName);
            int[] playerHandCardValues = playersHand.hand.OrderByDescending(p => p.Value).Select(x => x.Value).ToArray();
            var summariesForHand = ruleSummarycollection.AsQueryable().Where(p => p.DeckCount == deckCount && p.CurrentHandValue == playersHand.CurrentValue && p.DealerUpCard == playersHand.DealersUpCardValue && p.CanDouble == playersHand.canDouble && p.CanSplit == playersHand.canSplit && p.CurrentHand == playerHandCardValues).ToList();
            if (summariesForHand.Count() > 0) {
            
                if(summariesForHand.Count()>1)
                {
                    Console.WriteLine("This Should not Happen");
                }
                else
                {
                    Console.WriteLine("Summary found:" + summariesForHand.Count + " Dealer Card:" + playersHand.DealersUpCardValue + " Players Hand:" + GameStateExtensions.calculateValue(playersHand.hand.ToList()).ToString());
                    //Convert To hand Suggestion and Return
                    return HandResultExtensions.GetFromSingleHandResult(summariesForHand.FirstOrDefault());


                }
            
            }
            //No Summary found.  Create Summary and return it.
            Console.WriteLine("No Summary found, Create One:" + summariesForHand.Count + " Dealer Card:" + playersHand.DealersUpCardValue + " Players Hand:" + GameStateExtensions.calculateValue(playersHand.hand.ToList()).ToString());
            var createdSummary = CreateSingleHandResultFromRecords(RulesName,playersHand, deckCount);

            return HandResultExtensions.GetFromSingleHandResult(createdSummary);
        }

        //TODO look at how to look multiple levels deep.  Basically when looking at an action that can be followed by another action (hit and split)
        //only count the hand where the best next action was used. -> recursivly. <- will be slow but worth it.
        private static SingleHandResult CreateSingleHandResultFromRecords(string RulesName, PlayersHand playersHand, int deckCount)
        {

            var mongoClient = new MongoClient(ConnectionString);
            var ruleSummarycollection = mongoClient.GetDatabase(mongoBatabaseName).GetCollection<SingleHandResult>(RulesName + actionResultSummaryCollectionName);
            var records = GetRelevantRecords(RulesName, playersHand, deckCount);

            //Now Process records into a singleHandResult
            
            if (!records.Any())
            {
                Console.WriteLine("No Records to make Summary from");
                return null;
            }

            // Group by action and calculate win probability
            var summary = new SingleHandResult(playersHand.hand.Select(p=>p.Value).OrderDescending().ToArray(), deckCount, playersHand.canSplit, playersHand.canDouble);
            var groupedActions = records.GroupBy(r => r.Action)
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
                summary.ActionResults.Add(new ActionResult(ActionTypes.Stay)
                {
                    NumberOfHands = stayStats.Total,
                    NumberOfHandsLost = stayStats.Lost,
                    NumberOfHandspushed = stayStats.Push,
                    NumberOfHandsWon = stayStats.Wins,
                });
            }
            

            if (groupedActions.TryGetValue(ActionTypes.Hit, out var hitStats))
            {
                summary.ActionResults.Add(new ActionResult(ActionTypes.Hit)
                {
                    NumberOfHands = hitStats.Total,
                    NumberOfHandsLost = hitStats.Lost,
                    NumberOfHandspushed = hitStats.Push,
                    NumberOfHandsWon = hitStats.Wins,
                });
            }

            if (groupedActions.TryGetValue(ActionTypes.Split, out var splitStats))
            {
                summary.ActionResults.Add(new ActionResult(ActionTypes.Split)
                {
                    NumberOfHands = splitStats.Total,
                    NumberOfHandsLost = splitStats.Lost,
                    NumberOfHandspushed = splitStats.Push,
                    NumberOfHandsWon = splitStats.Wins,
                });
            }

            if (groupedActions.TryGetValue(ActionTypes.Double, out var doubleStats))
            {
                summary.ActionResults.Add(new ActionResult(ActionTypes.Double)
                {
                    NumberOfHands = doubleStats.Total,
                    NumberOfHandsLost = doubleStats.Lost,
                    NumberOfHandspushed = doubleStats.Push,
                    NumberOfHandsWon = doubleStats.Wins,
                });
            }


            //Save To DB before Saving
            ruleSummarycollection.InsertOne(summary);

            return summary;
        }
        
    }
}
