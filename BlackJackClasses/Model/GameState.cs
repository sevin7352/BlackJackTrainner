using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BlackJackClasses.Helpers;
using BlackJackTrainner.Enums;
using BlackJackTrainner.Model;
using BlackJackTrainner.Model.HandSuggestionGeneration;
using Newtonsoft.Json;
using PlayingCards;

namespace BlackJackClasses.Model
{
    public class GameState
    {

        public GameState(BlackJackRules rules, PlayStrategiesTypes playStrategiesType = PlayStrategiesTypes.SingleHandBook)
        {
            Random = new Random();
            GameId = DateTimeOffset.Now.UtcTicks;
            CardCount = 0;
            Rules = rules;
            Shute = new List<PlayingCard>();
            PlayersHand = new ObservableCollection<PlayersHand>();
            PlayedCards = new List<PlayingCard>();
            DealersHand = new List<PlayingCard>();
            TotalMoney = 1000;
            Bet = 10;
            PlayStrategiesType = playStrategiesType;
        }

        public long GameId;
        public int CardCount { get; set; }
        public Random Random { get; set; }
        public BlackJackRules Rules { get; set; }
        public List<BlackJackActionRecord> ActionRecords { get; set; }
        public bool InGame { get; set; }

        public int NumberOfHandsToPlay { get; set; }

        public void ShuffleShute()
        {
            PlayedCards = new List<PlayingCard>();
            for (int i = 0; i < Rules.numberOfDecks; i++)
            {
                Shute.AddRange(DeckHelper.FreshDeck);
            }

            Shute = DeckHelper.ShuffleDeck(Shute, 25, Random);
            ShutesPlayed++;
            CardCount = 0;
            //Wait for Actions To save
            BlackJackActionRecordHelper.SaveGameRecordsAsync(ActionRecords, Rules.name).GetAwaiter().GetResult();
            GameId = DateTimeOffset.Now.UtcTicks;
            ActionRecords.Clear();
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
            //TODO add in logic for automatic Shuffler
            if (TotalMoney == 0 || TotalMoney < Bet * NumberOfHandsToPlay)
            {
                InGame = false;
                return;
            }

            CurrentPlayerIndex = 0;

            PlayersHand = new ObservableCollection<PlayersHand>();
            DealersHand = new List<PlayingCard>();

            TotalMoney = TotalMoney - Bet * NumberOfHandsToPlay;


            for (int i = 0; i < NumberOfHandsToPlay; i++)
            {
                PlayersHand.Add(new PlayersHand(Bet));
                DealACard(false, i);
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

                //PlayersHand[i].hand = new ObservableCollection<PlayingCard>() { new PlayingCard(2, Suits.Clubs), new PlayingCard(2, Suits.Diamonds) };
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

            //If Dealer Doesn't have blackjack Payout BlackJack.
            CheckAndPayBlackJack();
        }

        private PlayingCard DealACard(bool Dealer = true, int PlayersHandIndex = -1)
        {
            try
            {
                //Todo, Do NOT SHuffle in middle of Hand.
                //TODO, Implement random number of cards left in shute. <-look up estimates.
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
                        return null;
                    }
                    else
                    {
                        PlayersHand[PlayersHandIndex].hand.Add(Shute[0]);
                        //PlayersHand[PlayersHandIndex].hand.Add(new PlayingCard(10,Suits.Clubs));
                    }

                }
                var cardPlayed = Shute[0];
                PlayedCards.Add(Shute[0]);
                Shute.RemoveAt(0);
                return cardPlayed;
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

            //ToDo Implement Push On 22
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

            //TODO add Hand results to all actions in Players Hands
            foreach (var playersHand in PlayersHand.Where(p=>p.HandResult == HandResultTypes.unkown))
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
                        //BlackJack Handled Already
                        
                        //Pay out if Hand Wins
                        playersHand.HandResult = HandResultTypes.Win;
                        playersHand.MoneyWon = playersHand.EndingBet * 2;
                        TotalMoney = TotalMoney + playersHand.EndingBet * 2;
                    }
                    else if (DealersValue > playersHand.CurrentValue)
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
            
            if(PlayersHand.Count(p=>p.HandResult == HandResultTypes.unkown) >0 || PlayersHand.Any(p=>p.ActionRecords.Any(x=>x.GameOutcome == HandResultTypes.unkown)))
            {
                MessageBox.Show("Something Bad Happened \n Players result Unkown, or ActionRecords missing Game Outcome");
                Console.WriteLine("Something Bad Happened \n Players  result Unkown, or ActionRecords missing Game Outcome");
            }


            foreach(var player in PlayersHand)
            {
                ActionRecords.AddRange(player.ActionRecords);
            }

        }

        public void CheckAndPayBlackJack()
        {
            for (int i = 0; i < PlayersHand.Count; i++)
            {

                if (PlayersHand[i].CurrentValue == 21)
                {
                    
                    PlayersHand[i].HandResult = HandResultTypes.BlackJack;
                    PlayersHand[i].MoneyWon = PlayersHand[i].EndingBet * Rules.payoutForBlackJack;
                    PlayersHand[i].ActionRecords.Add(new BlackJackActionRecord(GameId, HandsPlayed+i, 0, CardCount, false, false, DeckHelper.DeckCardNumber(CurrentPlayer.hand.ToList()), DealersValue) { GameOutcome = HandResultTypes.BlackJack,Action = ActionTypes.None,FinalHandValue = 21, });
                    PlayersHand[i].handOver = true;
                }

            }
        }

        public bool CanStay
        {
            get { return !PlayersTurnDone; }
        }

        public void stay()
        {
            PlayersHand[CurrentPlayerIndex].ActionRecords.Add(new BlackJackActionRecord(GameId, HandsPlayed+CurrentPlayerIndex, PlayersHand[CurrentPlayerIndex].ActionRecords.Count, CardCount, PlayersHand[CurrentPlayerIndex].canSplit, PlayersHand[CurrentPlayerIndex].canDouble, 
                DeckHelper.DeckCardNumber(CurrentPlayer.hand.ToList()), DealersValue) { Action = ActionTypes.Stay });

            PlayersHand[CurrentPlayerIndex].handOver = true;
            CheckPlayersTurnIsOverOrAdvanceToNextHand();
        }

        public bool CanHit
        {
            get { return PlayersHand[CurrentPlayerIndex].canHit(); }
        }

        public void Hit()
        {
            var ActionToAdd = new BlackJackActionRecord(GameId, HandsPlayed + CurrentPlayerIndex, PlayersHand[CurrentPlayerIndex].ActionRecords.Count, CardCount, PlayersHand[CurrentPlayerIndex].canSplit, PlayersHand[CurrentPlayerIndex].canDouble,
                DeckHelper.DeckCardNumber(CurrentPlayer.hand.ToList()), DealersValue)
            { Action = ActionTypes.Hit };
            

            var cardDelt = DealACard(false, CurrentPlayerIndex);
            //ToDo Compare cardDelt with card added to players hand.
            ActionToAdd.CardDrawn = cardDelt.Value;

            
            if (PlayersHand[CurrentPlayerIndex].CurrentValue > 21)
            {
                PlayersHand[CurrentPlayerIndex].handOver = true;
                ActionToAdd.GameOutcome = HandResultTypes.Loose;
                ActionToAdd.ResultedInBust = true;
                ActionToAdd.FinalHandValue = PlayersHand[CurrentPlayerIndex].CurrentValue;
                PlayersHand[CurrentPlayerIndex].ActionRecords.Add(ActionToAdd);
                CheckPlayersTurnIsOverOrAdvanceToNextHand();
            }
            else
            {
                PlayersHand[CurrentPlayerIndex].ActionRecords.Add(ActionToAdd);
                calculateHandSuggestions(CurrentPlayerIndex);
            }

            
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
            DealACard(false, CurrentPlayerIndex);
            CheckPlayersTurnIsOverOrAdvanceToNextHand();
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
            PlayersHand[CurrentPlayerIndex].hand.RemoveAt(0);
            PlayersHand[CurrentPlayerIndex].OverrideCanSplit = true;
            PlayersHand.Insert(CurrentPlayerIndex + 1, handToAdd);

            TotalMoney = TotalMoney - PlayersHand[CurrentPlayerIndex].EndingBet;
            DealACard(false, CurrentPlayerIndex);
            DealACard(false, CurrentPlayerIndex + 1);


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

        public void CheckPlayersTurnIsOverOrAdvanceToNextHand()
        {
            while (!PlayersTurnDone && CurrentPlayer.handOver && CurrentPlayerIndex + 1 < PlayersHand.Count)
            {
                CurrentPlayerIndex++;
            }
        }



        public void calculateHandSuggestions(int playerIndexToCalulateFor)
        {

            switch (PlayStrategiesType)
            {
                case PlayStrategiesTypes.SingleHandBook:
                    PlayersHand[playerIndexToCalulateFor].HandSuggesstion = SingleHandBook.LookUpSuggestions(PlayersHand[playerIndexToCalulateFor].hand.ToList(), PlayersHand[playerIndexToCalulateFor].DealersUpCardValue, PlayersHand[playerIndexToCalulateFor].canSplit());
                    break;
                case PlayStrategiesTypes.Random:
                    PlayersHand[playerIndexToCalulateFor].HandSuggesstion = HandResultExtensions.GetRandomSuggestions(PlayersHand[playerIndexToCalulateFor], Random);
                    break;
                case PlayStrategiesTypes.SingleHandAdaptive:
                    HandResults.GetSingleHandSuggestions(PlayersHand[playerIndexToCalulateFor], Random);

                    break;

            }

            return;
        }

    }
}
