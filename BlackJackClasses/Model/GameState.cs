using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BlackJackClasses.Helpers;
using BlackJackClasses.Enums;
using Newtonsoft.Json;
using PlayingCards;
using BlackJackClasses.Model.HandSuggestionGeneration;

namespace BlackJackClasses.Model
{
    public class GameState
    {

        public GameState(BlackJackRules rules, PlayStrategiesTypes playStrategiesType = PlayStrategiesTypes.SingleHandBook,int numberOfHandsToPlay = 1)
        {
            NumberOfTimesNoSummaryFound = 0;
            Random = new Random();
            GameId = DateTimeOffset.Now.UtcTicks;
            CardCount = 0;
            Rules = rules;
            Shute = new List<PlayingCard>();
            PlayersHand = new ObservableCollection<PlayersHand>();
            PlayedCards = new List<PlayingCard>();
            DealersHand = new List<PlayingCard>();
            ActionRecords = new List<BlackJackActionRecord> { };
            TotalMoney = 100000;
            Bet = 10;
            PlayStrategiesType = playStrategiesType;
            NumberOfHandsToPlay = numberOfHandsToPlay;
        }

        public long GameId;
        public int CardCount { get; set; }
        public int NumberOfTimesNoSummaryFound { get; set; }
        public int TrueCardCount { get
            {
                int numberOfCards = (Rules.numberOfDecks * 52);
                int numberOfCardsLeft = numberOfCards - PlayedCards.Count;
                double numberofDecksLeft = (double)numberOfCardsLeft / 52;  
                double temp = ((double)CardCount / numberofDecksLeft);
                if(temp < -40 || temp > 40)
                {
                    int i = 0;
                }
                return (int)Math.Round(temp,0);
            } }
        public Random Random { get; set; }
        public BlackJackRules Rules { get; set; }
        public List<BlackJackActionRecord> ActionRecords { get; set; }
        public bool InGame { get; set; }

        public int NumberOfHandsToPlay { get; set; }

        public bool ShuffleShuteAfterHand { get; set; }
        public int ShuffleShuteNumberOfCardsLeft { get; set; }
        public void ShuffleShute()
        {
            PlayedCards = new List<PlayingCard>();
            Shute = new List<PlayingCard>();
            for (int i = 0; i < Rules.numberOfDecks; i++)
            {
                Shute.AddRange(DeckHelper.FreshDeck);
            }
            int NumberOfCards = Rules.numberOfDecks * 52;

            double numberOfCardsLeftpercentage = Random.Next(70, 80);
            int NumberOfCardsLeft = (int)(NumberOfCards * ((100 - numberOfCardsLeftpercentage) / 100));
            ShuffleShuteNumberOfCardsLeft = NumberOfCardsLeft;
            Shute = DeckHelper.ShuffleDeck(Shute, 25, Random);
            ShutesPlayed++;
            CardCount = 0;

            //Wait for Actions To save
            BlackJackActionRecordHelper.SaveGameRecords(ActionRecords, Rules.name);
            //reset Actions for next game.
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
            if(ShuffleShuteAfterHand)
            {
                ShuffleShuteAfterHand = false;
                ShuffleShute();
            }

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
                


                if (Shute.Count < ShuffleShuteNumberOfCardsLeft)
                {
                    ShuffleShuteAfterHand = true;
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
                if (cardPlayed.Value < 6 && !cardPlayed.isAce)
                {
                    CardCount++;
                }
                else if (cardPlayed.Value >= 7 && cardPlayed.Value <= 9)
                {

                }
                else if(cardPlayed.Value >= 10 || cardPlayed.isAce)
                {
                    CardCount--;
                }
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
                    foreach(var actionRecord in playersHand.ActionRecords.Where(p=>p.GameOutcome == HandResultTypes.unkown))
                    {
                        actionRecord.FinalHandValue = playersHand.CurrentValue;
                        actionRecord.GameOutcome = HandResultTypes.Loose;
                    }
                }
                else //Player did not bust
                {
                    
                    if (DealersValue == 21 && DealersHand.Count == 2)// Dealer BlackJack
                    {
                        if(playersHand.CurrentValue == 21 && playersHand.hand.Count() ==2)
                        {
                            foreach (var actionRecord in playersHand.ActionRecords.Where(p => p.GameOutcome == HandResultTypes.unkown))
                            {
                                actionRecord.FinalHandValue = playersHand.CurrentValue;
                                actionRecord.GameOutcome = HandResultTypes.Push;
                            }
                            playersHand.HandResult = HandResultTypes.Push;
                            playersHand.MoneyWon = playersHand.EndingBet;

                        }
                        else // Dealer BlackJack Player does not have Black Jack.
                        {
                            foreach (var actionRecord in playersHand.ActionRecords.Where(p => p.GameOutcome == HandResultTypes.unkown))
                            {
                                actionRecord.FinalHandValue = playersHand.CurrentValue;
                                actionRecord.GameOutcome = HandResultTypes.Loose;
                            }
                            playersHand.HandResult = HandResultTypes.Loose;
                            playersHand.MoneyWon = 0;
                        }
                        
                    }
                    else if (DealersValue == playersHand.CurrentValue)
                    {
                        foreach (var actionRecord in playersHand.ActionRecords.Where(p => p.GameOutcome == HandResultTypes.unkown))
                        {
                            actionRecord.FinalHandValue = playersHand.CurrentValue;
                            actionRecord.GameOutcome = HandResultTypes.Push;
                        }
                        playersHand.HandResult = HandResultTypes.Push;
                        playersHand.MoneyWon = playersHand.EndingBet;
                        TotalMoney = TotalMoney + playersHand.EndingBet;
                    }
                    else if (playersHand.CurrentValue > DealersValue || DealersValue > 21) //player beat dealer or dealer bust
                    {
                        //BlackJack Handled Already

                        //Pay out if Hand Wins
                        foreach (var actionRecord in playersHand.ActionRecords.Where(p => p.GameOutcome == HandResultTypes.unkown))
                        {
                            actionRecord.FinalHandValue = playersHand.CurrentValue;
                            actionRecord.GameOutcome = HandResultTypes.Win;
                        }
                        playersHand.HandResult = HandResultTypes.Win;
                        playersHand.MoneyWon = playersHand.EndingBet * 2;
                        TotalMoney = TotalMoney + playersHand.EndingBet * 2;
                    }
                    else if (DealersValue > playersHand.CurrentValue)
                    {
                        foreach (var actionRecord in playersHand.ActionRecords.Where(p => p.GameOutcome == HandResultTypes.unkown))
                        {
                            actionRecord.FinalHandValue = playersHand.CurrentValue;
                            actionRecord.GameOutcome = HandResultTypes.Loose;
                        }
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
                    PlayersHand[i].ActionRecords.Add(new BlackJackActionRecord(GameId, HandsPlayed+i, 0, TrueCardCount, false, false, PlayersHand[i].CurrentValue, DealersValue, PlayersHand[i].ContainsAceValuedAt11) { GameOutcome = HandResultTypes.BlackJack,Action = ActionTypes.None,FinalHandValue = 21, });
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
            PlayersHand[CurrentPlayerIndex].ActionRecords.Add(new BlackJackActionRecord(GameId, HandsPlayed+CurrentPlayerIndex, PlayersHand[CurrentPlayerIndex].ActionRecords.Count, TrueCardCount, PlayersHand[CurrentPlayerIndex].canSplit, PlayersHand[CurrentPlayerIndex].canDouble,
                PlayersHand[CurrentPlayerIndex].CurrentValue, DealersValue, PlayersHand[CurrentPlayerIndex].ContainsAceValuedAt11) { Action = ActionTypes.Stay });

            PlayersHand[CurrentPlayerIndex].handOver = true;
            CheckPlayersTurnIsOverOrAdvanceToNextHand();
        }

        public bool CanHit
        {
            get { return PlayersHand[CurrentPlayerIndex].canHit; }
        }

        public void Hit()
        {
            var ActionToAdd = new BlackJackActionRecord(GameId, HandsPlayed + CurrentPlayerIndex, PlayersHand[CurrentPlayerIndex].ActionRecords.Count, TrueCardCount, PlayersHand[CurrentPlayerIndex].canSplit, PlayersHand[CurrentPlayerIndex].canDouble,
                 PlayersHand[CurrentPlayerIndex].CurrentValue, DealersValue, PlayersHand[CurrentPlayerIndex].ContainsAceValuedAt11)
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
                if (!PlayersTurnDone && PlayersHand[CurrentPlayerIndex].canDouble && TotalMoney >= Bet)
                {
                    return true;
                }
                return false;
            }
        }

        public void DoubleDown()
        {
            var ActionToAdd = new BlackJackActionRecord(GameId, HandsPlayed + CurrentPlayerIndex, PlayersHand[CurrentPlayerIndex].ActionRecords.Count, TrueCardCount, PlayersHand[CurrentPlayerIndex].canSplit, PlayersHand[CurrentPlayerIndex].canDouble,
                 PlayersHand[CurrentPlayerIndex].CurrentValue, DealersValue, PlayersHand[CurrentPlayerIndex].ContainsAceValuedAt11)
            { Action = ActionTypes.Double };


            PlayersHand[CurrentPlayerIndex].handOver = true;
            TotalMoney = TotalMoney - PlayersHand[CurrentPlayerIndex].StartingBet;
            PlayersHand[CurrentPlayerIndex].EndingBet += PlayersHand[CurrentPlayerIndex].EndingBet;
            
            var cardDelt = DealACard(false, CurrentPlayerIndex);
            //ToDo Compare cardDelt with card added to players hand.
            ActionToAdd.CardDrawn = cardDelt.Value;
            if (PlayersHand[CurrentPlayerIndex].CurrentValue > 21)
            {               
                ActionToAdd.GameOutcome = HandResultTypes.Loose;
                ActionToAdd.ResultedInBust = true;
                ActionToAdd.FinalHandValue = PlayersHand[CurrentPlayerIndex].CurrentValue;
            }
            PlayersHand[CurrentPlayerIndex].ActionRecords.Add(ActionToAdd);
            CheckPlayersTurnIsOverOrAdvanceToNextHand();
        }

        public bool CanSplit
        {
            get { return PlayersHand[CurrentPlayerIndex].canSplit; }
        }

        public void Split()
        {

            var serializedobject = JsonConvert.SerializeObject(PlayersHand[CurrentPlayerIndex]);
            PlayersHand handToAdd = JsonConvert.DeserializeObject<PlayersHand>(serializedobject);

            var ActionToAdd1 = new BlackJackActionRecord(GameId, HandsPlayed + CurrentPlayerIndex, PlayersHand[CurrentPlayerIndex].ActionRecords.Count, TrueCardCount, PlayersHand[CurrentPlayerIndex].canSplit, PlayersHand[CurrentPlayerIndex].canDouble,
                 PlayersHand[CurrentPlayerIndex].CurrentValue, DealersValue, PlayersHand[CurrentPlayerIndex].ContainsAceValuedAt11)
            { Action = ActionTypes.Split };

            //TODO, Will this Break when inserting a new Hand? Custom Insert To Update all other actions?
            var ActionToAdd2 = new BlackJackActionRecord(GameId, HandsPlayed + CurrentPlayerIndex + 1, PlayersHand[CurrentPlayerIndex].ActionRecords.Count, TrueCardCount, PlayersHand[CurrentPlayerIndex].canSplit, PlayersHand[CurrentPlayerIndex].canDouble,
                 PlayersHand[CurrentPlayerIndex].CurrentValue, DealersValue, PlayersHand[CurrentPlayerIndex].ContainsAceValuedAt11)
            { Action = ActionTypes.Split };


            handToAdd.hand.RemoveAt(1);

            //Not Sure Why this was overridden
            //handToAdd.OverrideCanSplit = true;
            //PlayersHand[CurrentPlayerIndex].OverrideCanSplit = true;

            PlayersHand[CurrentPlayerIndex].hand.RemoveAt(0);
            
            //Update Action Hand Ids for other hands if there are any.
            for (int i = CurrentPlayerIndex + 1; i < PlayersHand.Count; i++) {
                foreach (var action in PlayersHand[i].ActionRecords) {
                    action.HandId++;
                }
            }
            PlayersHand.Insert(CurrentPlayerIndex + 1, handToAdd);

            TotalMoney = TotalMoney - PlayersHand[CurrentPlayerIndex].EndingBet;

            

            var card1 = DealACard(false, CurrentPlayerIndex);
            ActionToAdd1.CardDrawn = card1.Value;
            var card2 = DealACard(false, CurrentPlayerIndex + 1);
            ActionToAdd2.CardDrawn = card2.Value;

            PlayersHand[CurrentPlayerIndex].ActionRecords.Add(ActionToAdd1);
            PlayersHand[CurrentPlayerIndex+1].ActionRecords.Add(ActionToAdd2);

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
                    PlayersHand[playerIndexToCalulateFor].HandSuggesstion = SingleHandBook.LookUpSuggestions(PlayersHand[playerIndexToCalulateFor].hand.ToList(), PlayersHand[playerIndexToCalulateFor].DealersUpCardValue, PlayersHand[playerIndexToCalulateFor].canSplit);
                    break;
                case PlayStrategiesTypes.Random:
                    PlayersHand[playerIndexToCalulateFor].HandSuggesstion = HandResultExtensions.GetRandomSuggestions(PlayersHand[playerIndexToCalulateFor], Random);
                    break;
                case PlayStrategiesTypes.SingleHandAdaptive:
                    var playersHand = PlayersHand[playerIndexToCalulateFor];
                    var suggestion = BlackJackActionRecordHelper.GetHandSuggestions(Rules.name, playersHand, TrueCardCount);
                    if (suggestion != null) {
                        playersHand.HandSuggesstion = suggestion;                    
                    }
                    else
                    {
                        NumberOfTimesNoSummaryFound++;
                        Console.WriteLine("No Relevant Records-Use Random");
                        playersHand.HandSuggesstion = HandResultExtensions.GetRandomSuggestions(PlayersHand[playerIndexToCalulateFor], Random);

                    }

                    break;

            }

            return;
        }

    }
}
