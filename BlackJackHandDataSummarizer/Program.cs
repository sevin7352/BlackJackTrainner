// See https://aka.ms/new-console-template for more information
using BlackJackClasses.Model;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using PlayingCards;
using BlackJackClasses.Helpers;

Console.WriteLine("Black Jack Data Summarizer");
var rules = BlackJackRuleSetHelper.AllowUserToSelectRuleSet();


// Deck count range (-40 to 40)
var deckCounts = Enumerable.Range(-40, 81);
deckCounts = deckCounts.OrderBy(d => Math.Abs(d)).ToList();
DateTime start = DateTime.Now;

// Parallel processing
foreach(var deckCount in deckCounts)
{
    Console.WriteLine($"Processing deck count: {deckCount}");

    // Loop through all possible player hand values (4 to 21 in Blackjack)
    for (int playerTotal = 4; playerTotal <= 21; playerTotal++)
    {
        //loop through all possible dealer cards
        var dealerUpCards = Enumerable.Range(2, 10);
        Parallel.ForEach(dealerUpCards, dealerUpCard => // Dealer cards (2 - Ace)
        {
            //need to look at all combinations of can split can double, aceValued as 11.
            int firstCard = playerTotal - 2;
            int secondCard = 2;
            var playersHand = new PlayersHand(10) { hand = new ObservableCollection<PlayingCard> { new PlayingCard(firstCard, Suits.Hearts), new PlayingCard(secondCard, Suits.Hearts) }, DealersUpCardValue = dealerUpCard };
            HandSuggestions summary;

            if (Math.IEEERemainder(playerTotal, 2) == 0)
            {
                int cardNumber = playerTotal / 2;
                //CanSplit
                playersHand = PlayersHandHelper.CreatePlayersHand(playerTotal, dealerUpCard, true, true, false);
                if (playersHand != null)
                {
                    summary = BlackJackActionRecordHelper.GetHandSuggestions(rules.name, playersHand, deckCount);
                }
                if (playerTotal == 12)
                {
                    //12 could be aces or 6s
                    playersHand = PlayersHandHelper.CreatePlayersHand(playerTotal, dealerUpCard, true, true, true);
                    if (playersHand != null)
                    {
                        summary = BlackJackActionRecordHelper.GetHandSuggestions(rules.name, playersHand, deckCount);
                    }
                }
            }
            
            //Cannot Split need to look at being about to double and not double. with and without ace as 11

            //Can Double
            playersHand = playersHand = PlayersHandHelper.CreatePlayersHand(playerTotal, dealerUpCard, false, true, false);
            summary = BlackJackActionRecordHelper.GetHandSuggestions(rules.name, playersHand, deckCount);

        //CanDouble and Ace
        if (playerTotal>11)
            {
                playersHand = playersHand = PlayersHandHelper.CreatePlayersHand(playerTotal, dealerUpCard, false, true, true);
                if (playersHand != null)
                {
                    summary = BlackJackActionRecordHelper.GetHandSuggestions(rules.name, playersHand, deckCount);
                }
            }
            

            //CannotDouble
            playersHand = playersHand = playersHand = PlayersHandHelper.CreatePlayersHand(playerTotal, dealerUpCard, false, false, false);
            if (playersHand != null)
            {
                summary = BlackJackActionRecordHelper.GetHandSuggestions(rules.name, playersHand, deckCount);
            }
            //Cannot Double and Ace
            if (playerTotal > 11)
            {
                playersHand = playersHand = playersHand = PlayersHandHelper.CreatePlayersHand(playerTotal, dealerUpCard, false, false, true);
                if (playersHand != null)
                {
                    summary = BlackJackActionRecordHelper.GetHandSuggestions(rules.name, playersHand, deckCount);
                }
            }
        });
    }
}

Console.WriteLine("Start: " + start.ToString("G"));
Console.WriteLine("End: " + DateTime.Now.ToString("G"));
Console.WriteLine("elapsed Time: " + (DateTime.Now - start).ToString("hh:mm:ss"));



