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
        private const string actionResultSummaryCollectionName = "_ActionResultSummary_Deep2";
        

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
            
            var mongoClient = new MongoClient(ConnectionString);
            var ruleDatacollection = mongoClient.GetDatabase(mongoBatabaseName).GetCollection<BlackJackActionRecord>(RulesName);
            var ruleSummarycollection = mongoClient.GetDatabase(mongoBatabaseName).GetCollection<SingleHandResult>(RulesName + actionResultSummaryCollectionName);
            //int[] playerHandCardValues = playersHand.hand.OrderByDescending(p => p.Value).Select(x => x.Value).ToArray();
            var summariesForHand = ruleSummarycollection.AsQueryable().Where(p => p.DeckCount == deckCount && p.DealerUpCard == playersHand.DealersUpCardValue && p.CanDouble == playersHand.canDouble && p.CanSplit == playersHand.canSplit && p.CanHit == playersHand.canHit  && p.ContainsAceAs11 == playersHand.ContainsAceValuedAt11 && p.CurrentHandValue == playersHand.CurrentValue).ToList();
            if (summariesForHand.Count() > 0) {
            
                if(summariesForHand.Count()>1)
                {
                    Console.WriteLine("This Should not Happen");
                    var idToKeep = summariesForHand.OrderByDescending(p => p.NumberOfHands).ToList().FirstOrDefault().Id;
                    var idsToDelete = summariesForHand.Where(p=>p.Id != idToKeep).Select(x=>x.Id).ToList();
                    ruleSummarycollection.DeleteMany(p=> idsToDelete.Contains(p.Id));
                    return GetHandSuggestions(RulesName, playersHand, deckCount,depth);
                }
                
                    Console.WriteLine("Summary found:" + summariesForHand.Count + " Dealer Card:" + playersHand.DealersUpCardValue + " Players Hand:" + playersHand.CurrentValue.ToString() + " Deck Count:" + deckCount);
                    //Convert To hand Suggestion and Return
                    return HandResultExtensions.GetFromSingleHandResult(summariesForHand.FirstOrDefault());
                
            
            }
            if (depth > 15)  // Prevent infinite recursion
            {
                Console.WriteLine("Max recursion depth reached in GetHandSuggestions.");
                return null;
            }

            //No Summary found.  Create Summary and return it.
            Console.WriteLine("No Summary found, Create One:" + summariesForHand.Count + " Dealer Card:" + playersHand.DealersUpCardValue + " Players Hand:" + playersHand.CurrentValue.ToString() + " Deck Count:" + deckCount);
            var createdSummary = CreateSingleHandResultFromRecords(RulesName,playersHand, deckCount, depth + 1);

            return HandResultExtensions.GetFromSingleHandResult(createdSummary);
        }

        
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

            
            //TODO need to get all data from these hands.
            // Sort actions in descending order (work backwards)
            var handsByGame = records.GroupBy(r => new { r.GameId, r.HandId })
                                     .Select(g => g.OrderByDescending(a => a.ActionIndex).ToList())
                                     .ToList();


            var summary = new SingleHandResult(playersHand.CurrentValue,playersHand.DealersUpCardValue, deckCount,playersHand.ContainsAceValuedAt11, playersHand.canSplit, playersHand.canDouble) {CanHit = playersHand.canHit };


            foreach (var handActions in handsByGame)
            {
                bool isHandOptimal = true;  // Assume the hand is optimal at first

                for (int i = 0; i < handActions.Count; i++)
                {
                    var action = handActions[i];

                    if (!IsOptimalFollowOn(action, RulesName,depth))
                    {
                        isHandOptimal = false;
                    }

                    if (isHandOptimal)
                    {
                        AddToSummary(summary, action);
                    }
                }
            }

            ruleSummarycollection.InsertOne(summary);
            return summary;
        }

        private static bool IsOptimalFollowOn(BlackJackActionRecord action,string RulesName,int depth = 0)
        {
            var mongoClient = new MongoClient(ConnectionString);
            var ruleDatacollection = mongoClient.GetDatabase(mongoBatabaseName).GetCollection<BlackJackActionRecord>(RulesName);

            // Find the next action in this hand (moving backward)
            var nextAction = ruleDatacollection.AsQueryable()
                .Where(a => a.GameId == action.GameId && a.HandId == action.HandId && a.ActionIndex > action.ActionIndex)
                .OrderBy(a => a.ActionIndex)  // Get the next action
                .FirstOrDefault();

            if (nextAction == null)
            {
                return true;  // If this is the last action, assume it's optimal
            }

            var playerHand = PlayersHandHelper.CreatePlayersHand(nextAction.PlayerTotal, nextAction.DealerUpCard, nextAction.CanSplit, nextAction.CanDouble, nextAction.ContainsAceValuedAs11);
            var suggestion = GetHandSuggestions(RulesName, playerHand, nextAction.DeckCount, depth);

            if (suggestion == null)
            {
                //No Suggestion Found we don't know if this is optimal or not.
                return false;
            }

            ActionTypes bestAction = suggestion.SuggestedAction(nextAction.CanSplit, nextAction.CanDouble, true);

            return bestAction == nextAction.Action;
        }

        private static void AddToSummary(SingleHandResult summary, BlackJackActionRecord action)
        {
            var existing = summary.ActionResults.FirstOrDefault(p => p.Type == action.Action);
            if (existing != null)
            {
                summary.ActionResults.Remove(existing);
            }

            summary.ActionResults.Add(new ActionResult(action.Action)
            {
                NumberOfHands = existing?.NumberOfHands + 1 ?? 1,
                NumberOfHandsLost = existing?.NumberOfHandsLost + (action.GameOutcome == HandResultTypes.Loose ? 1 : 0) ?? (action.GameOutcome == HandResultTypes.Loose ? 1 : 0),
                NumberOfHandspushed = existing?.NumberOfHandspushed + (action.GameOutcome == HandResultTypes.Push ? 1 : 0) ?? (action.GameOutcome == HandResultTypes.Push ? 1 : 0),
                NumberOfHandsWon = existing?.NumberOfHandsWon + (action.GameOutcome == HandResultTypes.Win || action.GameOutcome == HandResultTypes.BlackJack ? 1 : 0) ?? (action.GameOutcome == HandResultTypes.Win || action.GameOutcome == HandResultTypes.BlackJack ? 1 : 0),
            });
        }

    }
}
