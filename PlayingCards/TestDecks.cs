using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayingCards;


namespace GenerateTestDecks
{
    public class TestDecks
    {

        public TestDecks()
        {
            Decks = new List<Deck>();
        }
        public List<Deck> Decks;
    }

    public class Deck
    {
        public Deck(List<PlayingCard> newDeck)
        {
            deck = newDeck;
        }

        public List<PlayingCard> deck;
    }
}
