using BlackJackNueralNetworkLibrary.Model;
using BlackJackClasses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using BlackJackClasses.Model;
using BlackJackClasses.Model.HandSuggestionGeneration;

namespace BlackJackClasses.Helpers
{
    public static class NerualNetworkHelper
    {

        public static int GetActionFromSuggestion(HandSuggestions suggestions)
        {
            if ((suggestions.DoubleDown >= suggestions.Stay && suggestions.DoubleDown >= suggestions.Hit && suggestions.DoubleDown >= suggestions.Split))
            {
                return (int)ActionTypes.Double;
            }
            if (suggestions.Split >= suggestions.DoubleDown && suggestions.Split >= suggestions.Hit && suggestions.Split >= suggestions.Stay)
            {
                return (int)ActionTypes.Split;
            }
            if (((suggestions.Hit >= suggestions.DoubleDown) && (suggestions.Hit >= suggestions.Stay) && suggestions.Hit >= suggestions.Stay))
            {
                return (int)ActionTypes.Hit;
            }
            if ((suggestions.Stay >= suggestions.DoubleDown) && (suggestions.Stay >= suggestions.Split) && suggestions.Stay >= suggestions.Hit)
            {
                return (int)ActionTypes.Stay;
            }

            return (int)ActionTypes.Stay;
        }

        public static List<NueralNetTrainingDataEntry> GetTrainingDataFromBook()
        {
            List<NueralNetTrainingDataEntry> trainingData = new List<NueralNetTrainingDataEntry>();
            foreach(var singleHandBookEntry in SingleHandBook.SingleHandBookEntries)
            {
                trainingData.Add(new NueralNetTrainingDataEntry()
                {
                    PlayerHandSum = singleHandBookEntry.PlayersHandvalue,
                    DealerUpCard = singleHandBookEntry.DealersUpCard,
                    NumberOfAces = 0,
                    CanSplit = 0,
                    CanDouble = 1,
                    //BetAmount = 10, //Hardcoded for now will use in adaptive learning
                    Action = GetActionFromSuggestion(singleHandBookEntry.HandSuggestion),

                });

            }

            foreach(var singleHandBookEntrySpecial in SingleHandBook.SingleHandBookSpecialEntries)
            {
                trainingData.Add(new NueralNetTrainingDataEntry()
                {
                    PlayerHandSum = singleHandBookEntrySpecial.PlayersHandvalue,
                    DealerUpCard = singleHandBookEntrySpecial.DealersUpCard,
                    NumberOfAces = singleHandBookEntrySpecial.PlayersHand.Count(p=>p == 1),
                    CanSplit = singleHandBookEntrySpecial.PlayersHand.Distinct().Count() == 1 ? 1 : 0, //only ever 2 cards in the bookrules.
                    CanDouble = 1,
                    //BetAmount = 10, //Hardcoded for now will use in adaptive learning
                    Action = GetActionFromSuggestion(singleHandBookEntrySpecial.HandSuggestion),

                });
            }

            var DataWhereYouCanSplit = trainingData.Where(p => p.CanSplit >0);
            var DataWhereYouCanDouble = trainingData.Where((p)=>p.CanDouble >0);
            return trainingData;

        }

        public static void CreateTrainingDataFile()
        {
            string TrainningDateLocation = "C:\\Users\\Grant.Sevin\\Source\\Repos\\BlackJackTrainner\\BlackJackNueralNetworkLibrary\\TrainingData.csv";
            var DataToTrainOn = NerualNetworkHelper.GetTrainingDataFromBook();

            string ToWrite = "Action,PlayerHandSum,DealerUpCard,NumberOfAces,CanSplit,CanDouble\n";
            foreach (var model in DataToTrainOn)
            {
                ToWrite += model.Action + "," + model.PlayerHandSum + "," + model.DealerUpCard + "," + model.NumberOfAces + "," +model.CanSplit+","+model.CanDouble+"\n";
            }

            File.WriteAllText(TrainningDateLocation, ToWrite);


        }


    }
}
