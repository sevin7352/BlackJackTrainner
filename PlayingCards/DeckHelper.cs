using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace PlayingCards
{
    public static class DeckHelper
    {

        public const string  BackOfCard =  "\\CardGraphics\\blue_back.png";

        public static List<PlayingCard> FreshDeck
        {
            get
            {
                List<PlayingCard> Toreturn = new List<PlayingCard>();
                for (int count = 1; count <= 13; count++)
                {
                    Toreturn.Add(new PlayingCard(count, Suits.Clubs));
                }
                for (int count = 1; count <= 13; count++)
                {
                    Toreturn.Add(new PlayingCard(count, Suits.Diamonds));
                }
                for (int count = 1; count <= 13; count++)
                {
                    Toreturn.Add(new PlayingCard(count, Suits.Spades));
                }
                for (int count = 1; count <= 13; count++)
                {
                    Toreturn.Add(new PlayingCard(count, Suits.Hearts));
                }

                return Toreturn;
            }
        }

        public static void DisplayDeck(List<PlayingCard> deck)
        {
            int count = 0;
            foreach (var playingCard in deck)
            {
                if (count == 13)
                {
                    count = 0;
                    Console.WriteLine();
                }
                Console.Write(playingCard.DisplayCharacter + ", ");
                count++;
            }
            Console.WriteLine();
        }

        public static string DisplayDeckString(List<PlayingCard> deck)
        {
            string Toreturn = String.Empty;

            foreach (var playingCard in deck)
            {
                Toreturn = Toreturn + playingCard.DisplayCharacter + "-";
            }

            return Toreturn;
        }

        public static int[] DeckCardNumber(List<PlayingCard> deck)
        {
            List<int> toReturn = new List<int>();
            foreach (var card in deck.OrderBy(p=>p.CardNumber))
            {
                toReturn.Add(card.CardNumber);
            }

            return toReturn.ToArray();
        }

        public static bool ContainsCardsValues(List<PlayingCard> deck, int[] cardNumbers)
        {
            bool toReturn = true;
            var CardNumberlist = cardNumbers.ToList();
            foreach (var cardNumber in cardNumbers)
            {
                if (cardNumber > 10)
                {
                    string flag = String.Empty;
                }
            
                if (deck.Count(p => p.Value == cardNumber) == CardNumberlist.Count(p=>p == cardNumber))
                {

                }
                else
                {
                    toReturn = false;
                }
            }

            return toReturn;
        }

        public static List<PlayingCard> ShuffleDeck (List<PlayingCard> deck, int NumberOfShuffles = 1,Random rand = null)
        {
            Random Rand = new Random();
            if (rand != null)
            {
                Rand = rand;
            }

            
            for (int count = 0; count < NumberOfShuffles; count++)
            {
                for (int i = deck.Count - 1; i > 0; i--)
                {
                    int n = Rand.Next(i + 1);
                    PlayingCard A = deck[i];
                    PlayingCard B = deck[n];
                    deck[i] = B;
                    deck[n] = A;
                }
            }

            return deck;


        }



    }
}
