// See https://aka.ms/new-console-template for more information
using BlackJackClasses.Enums;
using BlackJackClasses.Helpers;
using BlackJackClasses.Model;
using BlackJackNueralNetworkLibrary.Model;

Console.WriteLine("Hello, World!");

//Get Rule Set to simulate
var rules = BlackJackRuleSetHelper.AllowUserToSelectRuleSet();
var NueralNetModel = new BlackJackNueralNetModel(rules.name);
string loopStatsFile = NueralNetModel.TrainingLoopStatsFile;
File.WriteAllText(loopStatsFile, "Number Of Shutes Played, Number Of Hands Played, Money Remaining,Randomness\n");
int NumberOfLoops = 100;
double RandomnessPercentage = 0.95;
Random random = new Random();

for(int i = 0; i < NumberOfLoops; i++)
{
    RandomnessPercentage *= 0.95;
    //var existingRecords = BlackJackActionRecordHelper.LoadAllRecords(rules.name);
    int MaxShutesToPlay = 1000;
    var game = new GameState(rules, PlayStrategiesTypes.MachineLearning);
    game.Start();
    int updatecount = 0;
    List<SingleHandResult> ResultsToAdd = new List<SingleHandResult>();
    int currentshute = 0;
    int startingMoney = game.TotalMoney;
    while (game.InGame && game.ShutesPlayed <= MaxShutesToPlay)
    {
        if (game.ShutesPlayed > currentshute)
        {
            //game.TotalMoney = startingMoney;
            currentshute = game.ShutesPlayed;
            Console.WriteLine("Number Of Shutes Played:" + currentshute);
            Console.WriteLine("TotalMoney:" + game.TotalMoney);
            Console.WriteLine("NumberOfTimesNoSummaryFound:" + game.NumberOfTimesNoSummaryFound);
        }

        while (!game.PlayersTurnDone)
        {
            var playerHand = game.CurrentPlayer;
            while (!playerHand.handOver)
            {
                //playerHand.HandSuggesstion.SuggestedAction(playerHand.canSplit, playerHand.canDouble, playerHand.canHit)
                var input = new NueralNetTrainingDataEntry()
                {
                    CanDouble = playerHand.canDouble ? 1 : 0,
                    CanSplit = playerHand.canSplit ? 1 : 0,
                    PlayerHandSum = playerHand.CurrentValue,
                    DealerUpCard = playerHand.DealersUpCardValue,
                    DeckCount = game.TrueCardCount,
                    NumberOfAces = playerHand.ContainsAceValuedAt11 ? 1 : 0,

                };
                var predictedAction = (ActionTypes)NueralNetModel.PredictAction(input);
                if (random.NextDouble() < RandomnessPercentage)
                {
                    game.PlayersHand[game.CurrentPlayerIndex].HandSuggesstion = HandResultExtensions.GetRandomSuggestions(game.PlayersHand[game.CurrentPlayerIndex], game.Random);
                    predictedAction = game.PlayersHand[game.CurrentPlayerIndex].SuggestedAction;
                    //return GetRandomValidAction(canSplit, canDouble, canHit);
                }

                switch (predictedAction)
                {

                    case ActionTypes.Stay:
                        game.stay();
                        break;
                    case ActionTypes.Double:
                        if (!playerHand.canDouble)
                        {
                            Console.WriteLine("Cannot Double but was told too");
                        }
                        game.DoubleDown();
                        break;
                    case ActionTypes.Hit:
                        game.Hit();
                        break;
                    case ActionTypes.Split:
                        if (!playerHand.canSplit)
                        {
                            Console.WriteLine("Cannot Split but was told too");
                        }
                        game.Split();
                        break;
                }
            }

            game.CheckPlayersTurnIsOverOrAdvanceToNextHand();
        }

        game.FinishDealersHand();

        game.Deal();
    }

    Console.WriteLine("NumbersOfHandsPlayed:" + game.HandsPlayed + " EndingMoney:" + game.TotalMoney);
    Console.WriteLine("NumberOfTimesNoSummaryFound:" + game.NumberOfTimesNoSummaryFound);
    File.AppendAllText(loopStatsFile, game.ShutesPlayed + "," + game.HandsPlayed + "," + game.TotalMoney +","+RandomnessPercentage +"\n");


    var DataToTrainOn = NerualNetworkHelper.GetTrainingDataFromDatabse(rules.name,game.ActionRecordCollectionNameModifier);
    NueralNetModel.TrainOrRetrainModel(DataToTrainOn);


}

Console.ReadLine();



public class LoopResults 
{
    public LoopResults(int handsPlayed, int moneyRemaining)
    {
        HandsPlayed = handsPlayed;
        MoneyRemaining = moneyRemaining;
    }
    public int HandsPlayed { get; set; }
    public int MoneyRemaining { get; set; }
}
