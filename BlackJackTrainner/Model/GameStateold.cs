using System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using BlackJackTrainner.Enums;
using BlackJackTrainner.Model.HandSuggestionGeneration;
using PlayingCards;

namespace BlackJackTrainner.Model
{
    public class GameStateOld
    {

        public GameStateOld(PlayStrategiesTypes playStrategiesType = PlayStrategiesTypes.SingleHandBook)
        {
            Random = new Random();
            Shute = new List<PlayingCard>();
            PlayersHand = new List<PlayingCard>();
            PlayersSecondHand = new List<PlayingCard>();
            PlayedCards = new List<PlayingCard>();
            DealersHand = new List<PlayingCard>();
            TotalMoney = 1000;
            Bet = 10;
            PlayStrategiesType = playStrategiesType;
        }

        public Random Random { get; set; }

        public bool InGame { get; set; }

        public void ShuffleShute()
        {
            PlayedCards = new List<PlayingCard>();
            for (int i = 0; i < 6; i++)
            {
                Shute.AddRange(DeckHelper.FreshDeck);
            }

            Shute = DeckHelper.ShuffleDeck(Shute, 25, Random);
            ShutesPlayed++;
        }
        public void Start()
        {
            InGame = true;
            HandsPlayed = 0;
            Shute = new List<PlayingCard>();
            ShuffleShute();
            Deal();
        }

        public void Deal()
        {
            if (TotalMoney == 0)
            {
                InGame = false;
                return;
            }
            
            IntialBet = Bet;
            PlayersHand.RemoveAll(p => p.CardNumber > 0);
            DealersHand.RemoveAll(p => p.CardNumber > 0);
            PlayersSecondHand.RemoveAll(p => p.CardNumber > 0);

            //need to deal with cards from last hand.
            PlayersTurnDone = false;
            if (TotalMoney < Bet)
            {
                if (TotalMoney > 0)
                {
                    Bet = TotalMoney;
                }
            }
            TotalMoney = TotalMoney - Bet;
            
                
                DealACard();
                DealACard(true);
                DealACard();
                DealACard(true);

                CalculateHandSuggesstions();
                if (PlayersValue == 21)
                {
                    //stay();
                }

                if (GameStateExtensions.calculateValue(DealersHand) == 21 && DealersHand[1].isAce)
                {
                    stay();
                }
        }

        private void DealACard(bool Dealer = false,bool secondHand = false)
        {
            try
            {
                if (Shute.Count < 52)
                {
                    ShuffleShute();
                }

                if (Dealer)
                    {
                        DealersHand.Add(Shute[0]);
                    }
                    else
                    {
                        if (secondHand)
                        {
                            PlayersSecondHand.Add(Shute[0]);
                        }
                        else
                        {
                            PlayersHand.Add(Shute[0]);
                        }

                    }

                    PlayedCards.Add(Shute[0]);
                    Shute.RemoveAt(0);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        }
        
        public void FinishDealersHand()
        {
            HandsPlayed++;
            FirstHandResultTypes = HandResultTypes.Loose;
            SecondHandResultTypes = HandResultTypes.Loose;
            if (PlayersSecondHand.Count > 0 && NeedToPlaySecondHand)
            {
                var temp = PlayersHand;
                PlayersHand = PlayersSecondHand;
                PlayersSecondHand = temp;
                PlayersTurnDone = false;
                NeedToPlaySecondHand = false;
                CalculateHandSuggesstions();
                SecondBet = Bet;
                Bet = IntialBet;
                if (PlayersValue == 21)
                {
                    stay();
                }
                return;
            }
            bool HitAgain = true;
            while (HitAgain)
            {
                if (DealersValue >= 17)
                {
                    HitAgain = false;
                }
                else
                {
                    DealACard(true);
                }
            }

            bool PlayerWins = false;
            if (PlayersValue <= 21)
            {

                if (DealersValue == 21 && DealersHand.Count == 2)
                {
                    FirstHandResultTypes = HandResultTypes.Loose;
                    FirstHandWinAmount = -Bet;
                }
                else if (DealersValue == PlayersValue)
                {
                    FirstHandResultTypes = HandResultTypes.Push;
                    FirstHandWinAmount = 0;
                    TotalMoney = TotalMoney + Bet;
                }
                else if (PlayersValue> DealersValue || DealersValue > 21)
                {
                    if (PlayersHand.Count == 2 && PlayersValue == 21 && !HasSecondHand)
                    {
                        FirstHandResultTypes = HandResultTypes.Win;
                        FirstHandWinAmount = (int) Math.Floor((Bet * 11) / (decimal) 5)-Bet;
                        TotalMoney = TotalMoney + (int)Math.Floor((Bet * 11)/(decimal)5);
                    }
                    else
                    {
                        FirstHandResultTypes = HandResultTypes.Win;
                        FirstHandWinAmount = Bet;
                        TotalMoney = TotalMoney + Bet * 2;
                    }
                    
                }

            }
            else
            {
                FirstHandResultTypes = HandResultTypes.Loose;
                FirstHandWinAmount = -Bet;
            }
            if (PlayersSecondHand.Count > 0)
            {
                if (PlayersSecondValue <= 21)
                {
                    if (DealersValue == PlayersSecondValue)
                    {
                        TotalMoney = TotalMoney + SecondBet;
                        SecondHandResultTypes = HandResultTypes.Push;
                        SecondHandWinAmount = 0;
                    }
                    else if (PlayersSecondValue > DealersValue || DealersValue > 21)
                    {
                        SecondHandResultTypes = HandResultTypes.Win;
                        SecondHandWinAmount = SecondBet;
                        TotalMoney = TotalMoney + SecondBet * 2;
                    }
                    

                }
                else
                {
                    SecondHandResultTypes = HandResultTypes.Loose;
                    SecondHandWinAmount = -SecondBet;
                }
            }
            
            Bet = IntialBet;
        }

        public bool CanStay
        {
            get { return !PlayersTurnDone; }
        }

        public void stay()
        {
            PlayersTurnDone = true;
            FinishDealersHand();
        }

        public bool CanHit
        {
            get { return !PlayersTurnDone; }
        }

        public void Hit()
        {
            DealACard();
            if (PlayersValue > 21)
            {
                PlayersTurnDone = true;
                FinishDealersHand();
            }
            CalculateHandSuggesstions();
        }

        public bool canDoubleDown
        {
            get
            {
                if (!PlayersTurnDone && PlayersHand.Count == 2 && TotalMoney >= Bet)
                {
                    return true;
                }
                return false;
            }
        }

        public void DoubleDown()
        {
            PlayersTurnDone = true;
            TotalMoney = TotalMoney - Bet;
            Bet = Bet * 2;
            DealACard();
            FinishDealersHand();
        }

        public bool CanSplit
        {
            get
            {
                if (HasSecondHand)
                {
                    return false;
                }


                if (  !PlayersTurnDone && PlayersHand.Count == 2 &&
                    (PlayersHand[0].CardNumber == PlayersHand[1].CardNumber) && TotalMoney >= Bet)
                {
                    return true;
                }
                return false;
            }
        }

        public void Split()
        {
            NeedToPlaySecondHand = true;
            PlayersSecondHand.Add(PlayersHand[1]);
            PlayersHand.RemoveAt(1);
            TotalMoney = TotalMoney - Bet;
            DealACard();
            DealACard(false,true);
            if (PlayersValue == 21)
            {
                //stay();
            }

            CalculateHandSuggesstions();
            
        }

        public int DealersValue
        {
            get {
                if (PlayersTurnDone)
                {
                    return GameStateExtensions.calculateValue(DealersHand);
                }

                if (DealersHand[1].isAce)
                {
                    return 11;
                }
                return DealersHand[1].Value;
            }
        }


        public int PlayersValue
        {
            get { return GameStateExtensions.calculateValue(PlayersHand); }
        }
        public int PlayersSecondValue
        {
            get { return GameStateExtensions.calculateValue(PlayersSecondHand); }
        }

        

        public void CalculateHandSuggesstions()
        {
            switch (PlayStrategiesType)
            {
                case PlayStrategiesTypes.SingleHandBook:
                    HandSuggestions = SingleHandBook.LookUpSuggestions(PlayersHand, DealersValue, CanSplit);
                    break;
                case PlayStrategiesTypes.Random:
                    HandSuggestions = new HandSuggestions()
                    {
                        DoubleDown = Random.Next(0, 100),
                        Stay = Random.Next(0, 100),
                        Split = Random.Next(0, 100),
                        Hit = Random.Next(0, 100),
                    };
                    break;
                case PlayStrategiesTypes.SingleHandAdaptive:
                    var results = HandResults.Where(p => DeckHelper.ContainsCardsValues(PlayersHand, p.CurrentHand) && p.DealersUpCardValue == DealersValue).ToList();
                    if (!results.Any())
                    {
                        HandSuggestions = new HandSuggestions()
                        {
                            DoubleDown = Random.Next(0, 100),
                            Stay = Random.Next(0, 100),
                            Split = Random.Next(0, 100),
                            Hit = Random.Next(0, 100),
                        };
                        return;
                    }
                    else
                    {
                        
                        

                    }


                    break;
            }
            
        }

        public List<SingleHandResult> HandResults { get; set; }
        public PlayStrategiesTypes PlayStrategiesType { get; set; }
        public int HandsPlayed { get; set; }
        public int ShutesPlayed { get; set; }
        public int TotalMoney { get; set; }
        public int Bet { get; set; }
        public int SecondBet { get; set; }
        public int IntialBet { get; set; }

        public List<PlayingCard> PlayedCards { get; set; }

        public List<PlayingCard> Shute { get; set; }

        public List<PlayingCard> PlayersHand { get; set; }
        public List<PlayingCard> PlayersSecondHand { get; set; }
        public bool HasSecondHand { get { return PlayersSecondHand.Count > 0; } }
        public bool NeedToPlaySecondHand { get; set; }

        public List<PlayingCard> DealersHand { get; set; }

        public bool PlayersTurnDone { get; set; }
        public HandSuggestions HandSuggestions { get; set; }

        #region Hand Results
        public HandResultTypes FirstHandResultTypes { get; set; }
        public int FirstHandWinAmount { get; set; }

        public HandResultTypes SecondHandResultTypes { get; set; }
        public int SecondHandWinAmount { get; set; }

        #endregion
    }
}
