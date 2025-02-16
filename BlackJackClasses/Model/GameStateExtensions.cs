using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlackJackClasses.Enums;
using PlayingCards;

namespace BlackJackClasses.Model
{
    public static class GameStateExtensions
    {


        public static int calculateValue(List<PlayingCard> cards)
        {
            int sum = 0;
            foreach (var card in cards)
            {
                sum += card.Value;
            }

            if (cards.Count(p => p.isAce) > 0)
            {
                if(cards.Count(p => p.isAce) > 1)
                {
                    int i = 0;
                }

                if (sum + 10 <= 21)
                {
                    return sum + 10;
                }
            }

            return sum;
        }

        public static int calculateValue(int[] cards, bool countAceAs11 = false)
        {
            int sum = 0;
            foreach (var card in cards)
            {
                if (card >= 10)
                {
                    sum += 10;
                }
                else
                {
                    sum += card;
                }

            }

            if (cards.Count(p => p == 1) > 0)
            {
                if (sum + 10 <= 21 || countAceAs11)
                {
                    return sum + 10;
                }
            }

            return sum;
        }
    }
}
