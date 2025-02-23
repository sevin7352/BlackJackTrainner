using BlackJackClasses.Model;
using PlayingCards;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlackJackClasses.Helpers
{
    public static class PlayersHandHelper
    {

        public static PlayersHand CreatePlayersHand(int playerTotal, int dealerUpCard,bool canSplit,bool canDouble,bool containsAceas11)
        {

            if (canSplit) //That means we can double
            {
                if (containsAceas11 && playerTotal == 12)
                {
                    //12 could be aces or 6s
                    var playersHand2 = new PlayersHand(10) { hand = new ObservableCollection<PlayingCard> { new PlayingCard(1, Suits.Hearts), new PlayingCard(1, Suits.Hearts) }, DealersUpCardValue = dealerUpCard };
                    return playersHand2;
                }
                int cardNumber = playerTotal / 2;
                //CanSplit
                var playersHand = new PlayersHand(10) { hand = new ObservableCollection<PlayingCard> { new PlayingCard(cardNumber, Suits.Hearts), new PlayingCard(cardNumber, Suits.Hearts) }, DealersUpCardValue = dealerUpCard };
                return playersHand;
                
            }
            else
            {
                //Cannot Split need to look at being about to double and not double. with and without ace as 11 when hand is over 12
                
                //intialize variables
                int firstCard = (int)Math.Round((double)playerTotal / 2, 0);
                int secondCard = playerTotal - firstCard;
                int thirdCard;
                PlayersHand playersHand;

                //CanDouble and Ace
                if (canDouble)
                {

                    if(containsAceas11 && playerTotal>12)
                    {
                        firstCard = playerTotal - 11;
                        secondCard = 1;
                        playersHand = new PlayersHand(10) { hand = new ObservableCollection<PlayingCard> { new PlayingCard(firstCard, Suits.Hearts), new PlayingCard(secondCard, Suits.Hearts) }, DealersUpCardValue = dealerUpCard };
                        return playersHand;
                    }

                    //Can Double
                    firstCard = (int)Math.Round((double)playerTotal / 2, 0);
                    secondCard = playerTotal - firstCard;
                    if(firstCard == secondCard )
                    {
                        if (firstCard == 10) {
                            firstCard++; //make it a jack.
                        }
                        else
                        {
                            firstCard++;
                            secondCard--;
                        }
                        
                    }
                    playersHand = new PlayersHand(10) { hand = new ObservableCollection<PlayingCard> { new PlayingCard(firstCard, Suits.Clubs), new PlayingCard(secondCard, Suits.Hearts) }, DealersUpCardValue = dealerUpCard };
                    return playersHand;

                }

                //Cannot Double and Ace
                if (!canDouble && containsAceas11 && playerTotal > 11)
                {
                    firstCard = playerTotal - 4;
                    secondCard = 3;
                    if (firstCard > 10)
                    {

                        secondCard = secondCard + (firstCard - 10);
                        firstCard = 10;
                    }
                    thirdCard = 1;
                    
                        playersHand = new PlayersHand(10) { hand = new ObservableCollection<PlayingCard> { new PlayingCard(firstCard, Suits.Hearts), new PlayingCard(secondCard, Suits.Hearts), new PlayingCard(thirdCard, Suits.Hearts) }, DealersUpCardValue = dealerUpCard };
                        return playersHand;
                }

                if(playerTotal > 5)
                {
                    //cannot double and no ace
                    firstCard = playerTotal - 4;
                    secondCard = 2;
                    if (firstCard>10)
                    {
                        
                        secondCard = secondCard + (firstCard - 10);
                        firstCard = 10;
                    }
                    thirdCard = 2;
                    playersHand = new PlayersHand(10) { hand = new ObservableCollection<PlayingCard> { new PlayingCard(firstCard, Suits.Hearts), new PlayingCard(secondCard, Suits.Hearts), new PlayingCard(thirdCard, Suits.Hearts) }, DealersUpCardValue = dealerUpCard };
                    return playersHand;
                }
                
            }
            return null;

        }



    }
}
