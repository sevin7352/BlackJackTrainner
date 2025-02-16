using BlackJackClasses.Enums;
using BlackJackClasses.Model;
using BlackJackTrainner.Model;
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
            var collection = mongoClient.GetDatabase(mongoBatabaseName).GetCollection<BlackJackActionRecord>(RulesName);

            var results =  collection.AsQueryable().Where(p=> p.DeckCount == deckCount &&  p.PlayerTotal == playerHand.CurrentValue && p.DealerUpCard == playerHand.DealersUpCardValue && p.CanDouble == playerHand.canDouble && p.CanSplit == playerHand.canSplit).ToList();

            return results;
        }

        public static HandSuggestions GetHandSuggestions(string RulesName, PlayersHand playersHand,int deckCount)
        {
            var records = GetRelevantRecords(RulesName, playersHand, deckCount);
            Console.WriteLine("Relevant Records:" + records.Count +" Dealer Card:" + playersHand.DealersUpCardValue +" Players Hand:" + GameStateExtensions.calculateValue(playersHand.hand.ToList()).ToString());
            
            return ProcessRecordsToSuggestions(records);
        }

        private static HandSuggestions ProcessRecordsToSuggestions(List<BlackJackActionRecord> records)
        {
            if(records.Count == 0)
            {
                return null;
            }

            var handSuggestions = new HandSuggestions();
            if (!records.Any()) return handSuggestions;

            // Group by action and calculate win probability
            var groupedActions = records.GroupBy(r => r.Action)
                .ToDictionary(g => g.Key, g => new
                {
                    Total = g.Count(),
                    Wins = g.Count(r => r.GameOutcome == HandResultTypes.Win || r.GameOutcome == HandResultTypes.BlackJack)
                });

            int totalActions = records.Count;

            // Assign probabilities
            if (groupedActions.TryGetValue(ActionTypes.Stay, out var stayStats))
                handSuggestions.Stay = (double)stayStats.Wins / stayStats.Total;

            if (groupedActions.TryGetValue(ActionTypes.Hit, out var hitStats))
                handSuggestions.Hit = (double)hitStats.Wins / hitStats.Total;

            if (groupedActions.TryGetValue(ActionTypes.Split, out var splitStats))
                handSuggestions.Split = (double)splitStats.Wins / splitStats.Total;

            if (groupedActions.TryGetValue(ActionTypes.Double, out var doubleStats))
                handSuggestions.DoubleDown = (double)doubleStats.Wins / doubleStats.Total;

            return handSuggestions;
        }



    }
}
