using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using BlackJackTrainner.Enums;
using BlackJackTrainner.Model;
using BlackJackTrainner.Model.HandSuggestionGeneration;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm;
using Newtonsoft.Json;
using PlayingCards;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading;

namespace BlackJackSimulatorWPF.ViewModel
{
   public class SimulateGamesViewModel :ObservableObject
    {
        public SimulateGamesViewModel()
        {
            
            StartCommand = new RelayCommand(Start, CanStart);
           SaveResultsCommand = new RelayCommand(SaveResults);
           LoadResultsCommand = new RelayCommand(LoadResults);
           SaveAsCsvCommand = new RelayCommand(SaveResultsAsCsv);
           ClearHandResultsCommand = new RelayCommand(ClearHandResults);
            Random = new Random();
           StartingBankroll = 100000000;
           BetBase = 10;
           TotalHandsPlayed = 0;
           MaxShutesToPlay = 1000;
           CurrentBankRoll = StartingBankroll;
           HandResults = new List<SingleHandResult>();
           PlayStrategy = PlayStrategiesTypes.SingleHandAdaptive;
           NumberOfSeatsToPlay = 5;

           
           playersCardNumbers = new int[4]{10,3,0,0};
           DealerValue = 7;
        }

        
        public Random Random { get; set; }
        public ICommand StartCommand { get; }
        public ICommand SaveResultsCommand { get; }
        public ICommand LoadResultsCommand { get; }
        public ICommand SaveAsCsvCommand { get; }
        public ICommand ClearHandResultsCommand { get; }


        

        public bool IsLearningMode { get; set; }

        public int BetBase { get; set; }
        public int MaxShutesToPlay { get; set; }
        public int ShutesPlayed { get; set; }
        public int StartingBankroll { get; set; }
        public int CurrentBankRoll { get; set; }

        public int NumberOfSeatsToPlay { get; set; }

        public List<SingleHandResult> HandResults { get; set; }

        public double ReturnPerHand
        {
            get
            {
                var difference = CurrentBankRoll - StartingBankroll;
                
                return Math.Round((double)difference / (TotalHandsPlayed), 6);
            }
        }

        public double PerformanceVsHouse {
            get { return 
                (((ReturnPerHand) / BetBase)+1)/2; }

        }

        private int _totalHandsPlayed { get; set; }
        public int TotalHandsPlayed
        {
            get { return _totalHandsPlayed; }
            set
            {
                _totalHandsPlayed = value;
                OnPropertyChanged();
            }
        }
        private PlayStrategiesTypes _playStrategy { get; set; }
        public PlayStrategiesTypes PlayStrategy
        {
            get { return _playStrategy; }
            set
            {
                _playStrategy = value;
                OnPropertyChanged();
            }
        }


        public Dictionary<PlayStrategiesTypes, string> PlayStrategiesToShow
        {
            get { return PlayStrategies.AvailablePlayStrategies; }
        }
        

        public bool CanStart()
        {
            return true;
        }
        public void Start()
        {

            Task.Factory.StartNew(() =>
            {
                ShutesPlayed = 0;
                CurrentBankRoll = StartingBankroll;
                TotalHandsPlayed = 0;
                GameState game = new GameState()
                {
                    TotalMoney = CurrentBankRoll,
                    Bet = BetBase,
                    Random = Random,
                    PlayStrategiesType = PlayStrategy,
                    NumberOfHandsToPlay =  NumberOfSeatsToPlay,
                };
                if (IsLearningMode)
                {
                    game.PlayStrategiesType = PlayStrategiesTypes.Random;
                }
                else
                {
                    game.HandResults = HandResults;
                }
                game.Start();
                int updatecount = 0;
                List<SingleHandResult> ResultsToAdd = new List<SingleHandResult>();
                while (game.InGame && game.ShutesPlayed <= MaxShutesToPlay)
                {

                    while (!game.PlayersTurnDone)
                    {
                        var playerHand = game.CurrentPlayer;
                        while (!playerHand.handOver)
                        {
                            switch (playerHand.HandSuggesstion.SuggestedAction(playerHand.canSplit(), playerHand.canDouble(), playerHand.canHit()))
                            {

                                case ActionTypes.Stay:
                                    game.stay();
                                    break;
                                case ActionTypes.Double:
                                    game.DoubleDown();
                                    break;
                                case ActionTypes.Hit:
                                    game.Hit();
                                    break;
                                case ActionTypes.Split:
                                    game.Split();
                                    break;
                            }
                        }

                        game.CurrentPlayerIndex++;
                    }

                    game.FinishDealersHand();

                    if (PlayStrategy == PlayStrategiesTypes.SingleHandAdaptive)
                    {
                        foreach (var player in game.PlayersHand)
                        {
                            ResultsToAdd.AddRange(HandResultExtensions.GetSingleHandResults(player));
                        }

                        if (game.PlayersTurnDone)
                        {
                            HandResults.AddSingleHandResults(ResultsToAdd);
                            ResultsToAdd = new List<SingleHandResult>();
                        }
                    }
                    

                    game.Deal();
                    

                    
                    if (game.ShutesPlayed > ShutesPlayed)
                    {
                        ShutesPlayed = game.ShutesPlayed;
                        updatecount++;
                    }

                    if (!game.InGame && IsLearningMode)
                    {
                        if (game.TotalMoney == 0)
                        {
                            game.TotalMoney = StartingBankroll;
                            StartingBankroll += StartingBankroll;
                            game.InGame = true;
                        }
                    }
                    
                    if (updatecount == 10)
                    {
                        updatecount = 0;
                        
                            CurrentBankRoll = game.TotalMoney;
                            TotalHandsPlayed = game.HandsPlayed;
                        OnPropertyChanged(nameof(CurrentBankRoll));
                        OnPropertyChanged(nameof(ReturnPerHand));
                        OnPropertyChanged(nameof(PerformanceVsHouse));
                        OnPropertyChanged();
                        
                    }
                }

                CurrentBankRoll = game.TotalMoney;
                TotalHandsPlayed = game.HandsPlayed;
                HandResults = HandResults.OrderByDescending(p => p.NumberOfHands).ToList();
                    OnPropertyChanged();
            });
            
                
        }


        #region Controls

        string subFolder
        {
            get
            {
                return PlayStrategiesToShow?.FirstOrDefault(p => p.Key == PlayStrategy).Value;

                return "Unkown";
            }
        }
        string rootFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), @"BlackJackTrainer\Results");

        public void SaveResults()
        {
            
            FilesHelper.EnsureFolderExists(rootFolder);
            string FullPath = Path.Combine(rootFolder, subFolder+ ".SingleHandResult");
            string dataToSave = String.Empty;
            switch (PlayStrategy)
            {
                case PlayStrategiesTypes.Random:
                    break;
                case PlayStrategiesTypes.SingleHandBook:
                    dataToSave = JsonConvert.SerializeObject(HandResults);
                    File.WriteAllText(FullPath, dataToSave);
                    
                    break;
                case PlayStrategiesTypes.SingleHandAdaptive:
                    dataToSave = JsonConvert.SerializeObject(HandResults);
                    File.WriteAllText(FullPath,dataToSave);
                    break;
            }


        }

        public void LoadResults()
        {
            string FullPath = Path.Combine(rootFolder, subFolder + ".SingleHandResult");
            string dataToSave = String.Empty;
            switch (PlayStrategy)
            {
                case PlayStrategiesTypes.Random:
                    break;
                case PlayStrategiesTypes.SingleHandBook:
                    if (File.Exists(FullPath))
                    {
                        dataToSave = File.ReadAllText(FullPath);
                        HandResults = JsonConvert.DeserializeObject<List<SingleHandResult>>(dataToSave);
                    }
                    break;
                    
                case PlayStrategiesTypes.SingleHandAdaptive:
                    if (File.Exists(FullPath))
                    {
                        dataToSave = File.ReadAllText(FullPath);
                        HandResults = JsonConvert.DeserializeObject<List<SingleHandResult>>(dataToSave);
                    }
                    break;
            }
            OnPropertyChanged();
        }

        public void SaveResultsAsCsv()
        {
            string FullPath = Path.Combine(rootFolder, subFolder + ".csv");
            string dataToSave =
                HandResultExtensions.getCsvHeaders();
            foreach (var handResult in HandResults)
            {
                dataToSave += handResult.WriteToCsv();
                
            }
            File.WriteAllText(FullPath,dataToSave);
        }


        public void ClearHandResults()
        {
            HandResults = new List<SingleHandResult>();
            OnPropertyChanged();
        }
        #endregion

        #region lookupResults
        public HandSuggestions HandSuggestion { get; set; }

        public void LookupHandsuggestion()
        {
            PlayersHand playersHand = new PlayersHand(BetBase);
            playersHand.DealersUpCardValue = DealerValue;
            foreach (var cardnumber in playersCardNumbers.Where(p => p > 0))
            {
                playersHand.hand.Add(new PlayingCard(cardnumber, Suits.Clubs));
            }
            switch (PlayStrategy)
            {
                case PlayStrategiesTypes.Random:
                    HandSuggestion = HandResultExtensions.GetRandomSuggestions(playersHand, Random);
                    break;
                case PlayStrategiesTypes.SingleHandBook:
                    HandSuggestion = SingleHandBook.LookUpSuggestions(playersHand.hand.ToList(), playersHand.DealersUpCardValue, playersHand.canSplit());
                    break;

                case PlayStrategiesTypes.SingleHandAdaptive:
                    HandSuggestion = HandResults.GetSingleHandSuggestions(playersHand, Random);
                    break;

            }





            OnPropertyChanged();
        }

        private int _dealervalue { get; set; }

        public int DealerValue
        {
            get { return _dealervalue; }
            set
            {
                _dealervalue = value;
                LookupHandsuggestion();
                OnPropertyChanged();
            }
        }

        public int[] playersCardNumbers { get; set; } 
        public int CardNumber1
        {
            get { return playersCardNumbers[0]; }
            set
            {
                playersCardNumbers[0] = value;
                LookupHandsuggestion();
                OnPropertyChanged();
            }
        }

        public int CardNumber2
        {
            get { return playersCardNumbers[1]; }
            set
            {
                playersCardNumbers[1] = value;
                LookupHandsuggestion();
                OnPropertyChanged();
            }
        }

        public int CardNumber3
        {
            get { return playersCardNumbers[2]; }
            set
            {
                playersCardNumbers[2] = value;
                LookupHandsuggestion();
                OnPropertyChanged();
            }
        }
        
        public int CardNumber4
        {
            get { return playersCardNumbers[3]; }
            set
            {
                playersCardNumbers[3] = value;
                LookupHandsuggestion();
                OnPropertyChanged();
            }
        }


        #endregion

    }
}
