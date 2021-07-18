using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayingCards
{
    public enum Suits {
        Hearts =0,
        Diamonds = 1,
        Spades =2,
        Clubs =3,
    }

    public class PlayingCard
    {
        public PlayingCard(int cardNumber, Suits suit)
        {
            CardNumber = cardNumber;
            Suit = suit;
        }

        public int Value
        {
            get
            {
                if (CardNumber >= 2 && CardNumber <= 10)
                {
                    return CardNumber;
                }

                if (CardNumber == 1)
                {
                    return 1;
                }

                if (CardNumber == 11)
                {
                    return 10;
                }

                if (CardNumber == 12)
                {
                    return 10;
                }

                if (CardNumber == 13)
                {
                    return 10;
                }

                return -1;
            }
        }

        public Suits Suit { get; set; }

        public int CardNumber { get; set; } // Ace = 1 king = 13

        public bool isAce
        {
            get { return CardNumber == 1; }
        }

        public string DisplayCharacter
        {
            get
            {
                string suit = "";
                switch (Suit)
                {
                    case Suits.Clubs:
                        suit = "C";
                        break;
                    case Suits.Spades:
                        suit = "S";
                        break;
                    case Suits.Hearts:
                        suit = "H";
                        break;
                    case Suits.Diamonds:
                        suit = "D";
                        break;
                }


                    if (CardNumber >= 2 && CardNumber <= 10)
                {
                    return CardNumber.ToString() + suit;
                }

                if (CardNumber == 1)
                {
                    return "A" + suit;
                }

                if (CardNumber == 11)
                {
                    return "J"+suit;
                }

                if (CardNumber == 12)
                {
                    return "Q" + suit;
                }

                if (CardNumber == 13)
                {
                    return "K"+suit;
                }

                return "unkown";
            }
        }

        public string GraphicPath
        {
            get { return Environment.CurrentDirectory +"\\CardGraphics\\" + DisplayCharacter + ".png"; }
        }

    
    }
}
