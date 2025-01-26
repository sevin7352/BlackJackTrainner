using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BlackJackTrainner.Enums;
using BlackJackTrainner.Model.HandSuggestionGeneration;
using Newtonsoft.Json;
using PlayingCards;

namespace BlackJackTrainner.Model
{
    public class GameState
    {

        public GameState(PlayStrategiesTypes playStrategiesType = PlayStrategiesTypes.SingleHandBook)
        {
            Random = new Random();
            Shute = new List<PlayingCard>();
            PlayersHand = new ObservableCollection<PlayersHand>();
            
            PlayedCards = new List<PlayingCard>();
            DealersHand = new List<PlayingCard>();
            TotalMoney = 1000;
            Bet = 10;
            PlayStrategiesType = playStrategiesType;
        }

        public Random Random { get; set; }

        public bool InGame { get; set; }

        public int NumberOfHandsToPlay { get; set; }

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
            if (TotalMoney == 0 || TotalMoney < Bet * NumberOfHandsToPlay)
            {
                InGame = false;
                return;
            }

            CurrentPlayerIndex = 0;
            
            PlayersHand = new ObservableCollection<PlayersHand>();
            DealersHand = new List<PlayingCard>();
            
            TotalMoney = TotalMoney - (Bet * NumberOfHandsToPlay);


            for (int i = 0; i < NumberOfHandsToPlay; i++)
            {
                PlayersHand.Add(new PlayersHand(Bet));
                DealACard(false,i);
            }
            DealACard(true);
            for (int i = 0; i < NumberOfHandsToPlay; i++)
            {
                DealACard(false, i);
            }

            DealACard(true);
            //DealersHand[1] = new PlayingCard(7,Suits.Clubs);
            for (int i = 0; i < PlayersHand.Count; i++)
            {

                //PlayersHand[i].hand = new List<PlayingCard>() { new PlayingCard(2, Suits.Clubs), new PlayingCard(2, Suits.Diamonds) };
                PlayersHand[i].DealersUpCardValue = DealersValue;
                calculateHandSuggestions(i);
            }
            
            
            if (GameStateExtensions.calculateValue(DealersHand) == 21)
            {

                for (int i = 0; i < PlayersHand.Count; i++)
                {
                    CurrentPlayerIndex = i;
                    stay();
                }

                FinishDealersHand();
            }
        }

        private void DealACard(bool Dealer = true, int PlayersHandIndex=-1)
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
                    if (PlayersHandIndex == -1)
                    {
                        MessageBox.Show("Card delt to No one");
                        return;
                    }
                    else
                    {
                        PlayersHand[PlayersHandIndex].hand.Add(Shute[0]);
                        //PlayersHand[PlayersHandIndex].hand.Add(new PlayingCard(10,Suits.Clubs));
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
            HandsPlayed += PlayersHand.Count;
            
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

            foreach (var playersHand in PlayersHand)
            {
                playersHand.HandResult = HandResultTypes.unkown;
                if (playersHand.CurrentValue > 21)
                {
                    playersHand.HandResult = HandResultTypes.Loose;
                    playersHand.MoneyWon = 0;
                }
                else
                {
                    if (DealersValue == 21 && DealersHand.Count == 2)
                    {
                        playersHand.HandResult = HandResultTypes.Loose;
                        playersHand.MoneyWon = 0;
                    }
                    else if (DealersValue == playersHand.CurrentValue)
                    {
                        playersHand.HandResult = HandResultTypes.Push;
                        playersHand.MoneyWon = playersHand.EndingBet;
                        TotalMoney = TotalMoney + playersHand.EndingBet;
                    }
                    else if (playersHand.CurrentValue > DealersValue || DealersValue > 21)
                    {
                        if (playersHand.hand.Count == 2 && playersHand.CurrentValue == 21 && !playersHand.OverrideCanSplit)
                        {
                            playersHand.HandResult = HandResultTypes.Win;
                            playersHand.MoneyWon = (int)Math.Floor((playersHand.EndingBet * 11) / (decimal)5);
                            TotalMoney = TotalMoney + (int)Math.Floor((playersHand.EndingBet * 11) / (decimal)5);
                        }
                        else
                        {
                            playersHand.HandResult = HandResultTypes.Win;
                            playersHand.MoneyWon = playersHand.EndingBet*2;
                            TotalMoney = TotalMoney + playersHand.EndingBet * 2;
                        }

                    }
                    else if(DealersValue > playersHand.CurrentValue)
                    {
                        playersHand.HandResult = HandResultTypes.Loose;
                        playersHand.MoneyWon = 0;
                    }
                }

                if (playersHand.HandResult == HandResultTypes.unkown)
                {
                    MessageBox.Show("No Result For Hand");
                }
            }
        }

        public bool CanStay
        {
            get { return !PlayersTurnDone; }
        }

        public void stay()
        {
            PlayersHand[CurrentPlayerIndex].HandResults.Add(new SingleHandResult(DeckHelper.DeckCardNumber(CurrentPlayer.hand.ToList()), CurrentPlayer.canSplit(), CurrentPlayer.canDouble()));
            PlayersHand[CurrentPlayerIndex].handOver = true;
            PlayersHand[CurrentPlayerIndex].ActionsTaken.Add(ActionTypes.Stay);
        }

        public bool CanHit
        {
            get { return PlayersHand[CurrentPlayerIndex].canHit(); }
        }

        public void Hit()
        {
            PlayersHand[CurrentPlayerIndex].HandResults.Add(new SingleHandResult(DeckHelper.DeckCardNumber(CurrentPlayer.hand.ToList()), CurrentPlayer.canSplit(), CurrentPlayer.canDouble()));
            PlayersHand[CurrentPlayerIndex].ActionsTaken.Add(ActionTypes.Hit);
            DealACard(false,CurrentPlayerIndex);
            if (PlayersHand[CurrentPlayerIndex].CurrentValue > 21)
            {
                PlayersHand[CurrentPlayerIndex].handOver = true;
            }
            calculateHandSuggestions(CurrentPlayerIndex);
        }

        public bool canDoubleDown
        {
            get
            {
                if (!PlayersTurnDone && PlayersHand[CurrentPlayerIndex].canDouble() && TotalMoney >= Bet)
                {
                    return true;
                }
                return false;
            }
        }

        public void DoubleDown()
        {
            PlayersHand[CurrentPlayerIndex].HandResults.Add(new SingleHandResult(DeckHelper.DeckCardNumber(CurrentPlayer.hand.ToList()), CurrentPlayer.canSplit(), CurrentPlayer.canDouble()));
            PlayersHand[CurrentPlayerIndex].ActionsTaken.Add(ActionTypes.Double);
            PlayersHand[CurrentPlayerIndex].handOver = true;
            TotalMoney = TotalMoney - PlayersHand[CurrentPlayerIndex].StartingBet;
            PlayersHand[CurrentPlayerIndex].EndingBet += PlayersHand[CurrentPlayerIndex].EndingBet;
            DealACard(false,CurrentPlayerIndex);
        }

        public bool CanSplit
        {
            get { return PlayersHand[CurrentPlayerIndex].canSplit(); }
        }

        public void Split()
        {
            var serializedobject = JsonConvert.SerializeObject(PlayersHand[CurrentPlayerIndex]);
            PlayersHand handToAdd = JsonConvert.DeserializeObject<PlayersHand>(serializedobject);
            handToAdd.hand.RemoveAt(1);
            handToAdd.HandResults.Add(new SingleHandResult(DeckHelper.DeckCardNumber(CurrentPlayer.hand.ToList()), true, CurrentPlayer.canDouble()));
            handToAdd.OverrideCanSplit = true;
            handToAdd.ActionsTaken.Add(ActionTypes.Split);
            PlayersHand[CurrentPlayerIndex].ActionsTaken.Add(ActionTypes.Split);
            PlayersHand[CurrentPlayerIndex].HandResults.Add(new SingleHandResult(DeckHelper.DeckCardNumber(CurrentPlayer.hand.ToList()), true, CurrentPlayer.canDouble()));
            PlayersHand[CurrentPlayerIndex].hand.RemoveAt(1);
            PlayersHand[CurrentPlayerIndex].OverrideCanSplit = true;
            PlayersHand.Insert(CurrentPlayerIndex+1,handToAdd);
            




            TotalMoney = TotalMoney - PlayersHand[CurrentPlayerIndex].EndingBet;
            DealACard(false,CurrentPlayerIndex);
            DealACard(false, CurrentPlayerIndex+1);
            

            calculateHandSuggestions(CurrentPlayerIndex);

        }

        public int DealersValue
        {
            get
            {
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


        


        public int CurrentPlayerIndex { get; set; }

        public PlayersHand CurrentPlayer
        {
            get
            {
                if (PlayersHand.Count > CurrentPlayerIndex)
                {
                    return PlayersHand[CurrentPlayerIndex];
                }

                return null;
            }
        }

        public List<SingleHandResult> HandResults { get; set; }
        public PlayStrategiesTypes PlayStrategiesType { get; set; }
        public int HandsPlayed { get; set; }
        public int ShutesPlayed { get; set; }
        public int TotalMoney { get; set; }
        public int Bet { get; set; }
        public int SecondBet { get; set; }

        public List<PlayingCard> PlayedCards { get; set; }

        public List<PlayingCard> Shute { get; set; }

        public ObservableCollection<PlayersHand> PlayersHand { get; set; }
        
        public List<PlayingCard> DealersHand { get; set; }

        public bool PlayersTurnDone
        {
            get { return PlayersHand.All(p => p.handOver); }
        }

        

        public void calculateHandSuggestions(int playerIndexToCalulateFor)
        {
            
            switch (PlayStrategiesType)
            {
                case PlayStrategiesTypes.SingleHandBook:
                    PlayersHand[playerIndexToCalulateFor].HandSuggesstion = SingleHandBook.LookUpSuggestions(PlayersHand[playerIndexToCalulateFor].hand.ToList(), PlayersHand[playerIndexToCalulateFor].DealersUpCardValue, PlayersHand[playerIndexToCalulateFor].canSplit());
                    break;
                case PlayStrategiesTypes.Random:
                    PlayersHand[playerIndexToCalulateFor].HandSuggesstion = HandResultExtensions.GetRandomSuggestions(PlayersHand[playerIndexToCalulateFor],Random);
                    break;
                case PlayStrategiesTypes.SingleHandAdaptive:
                    HandResults.GetSingleHandSuggestions(PlayersHand[playerIndexToCalulateFor], Random);

                break;
                    
            }

            return;
        }

    }
}
