// See https://aka.ms/new-console-template for more information
using BlackJackClasses.Enums;
using BlackJackClasses.Helpers;
using BlackJackClasses.Model;

Console.WriteLine("Hello, World!");

//Get Rule Set to simulate
var rules = BlackJackRuleSetHelper.AllowUserToSelectRuleSet();

//var existingRecords = BlackJackActionRecordHelper.LoadAllRecords(rules.name);
int MaxShutesToPlay = 1000;
var game = new GameState(rules, PlayStrategiesTypes.SingleHandAdaptive);
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
        Console.WriteLine("Number Of Shutes Played:"+currentshute);
        Console.WriteLine("TotalMoney:" + game.TotalMoney);
        Console.WriteLine("NumberOfTimesNoSummaryFound:" + game.NumberOfTimesNoSummaryFound);
    }

    while (!game.PlayersTurnDone)
    {
        var playerHand = game.CurrentPlayer;
        while (!playerHand.handOver)
        {
            switch (playerHand.HandSuggesstion.SuggestedAction(playerHand.canSplit, playerHand.canDouble, playerHand.canHit))
            {

                case ActionTypes.Stay:
                    game.stay();
                    break;
                case ActionTypes.Double:
                    game.DoubleDown();
                    break;
                case ActionTypes.Hit:
                    game.Hit();
                    break;
                case ActionTypes.Split:
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
Console.ReadLine();


