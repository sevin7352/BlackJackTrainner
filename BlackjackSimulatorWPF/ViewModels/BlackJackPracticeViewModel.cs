using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using BlackJackTrainner.Model;
using CommunityToolkit.Mvvm.Input;
using PlayingCards;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using CommunityToolkit.Mvvm.Messaging;

namespace BlackJackSimulatorWPF.ViewModel
{
    public class BlackJackPracticeViewModel : ObservableObject
    {
        public BlackJackPracticeViewModel()
        {
            
            GameState = new GameState()
            {
                NumberOfHandsToPlay = 1,
            };
            GameState.Start();
            DealCommand = new RelayCommand(Deal, CanDeal);
            StayCommand = new RelayCommand(Stay, CanStay);
            HitCommand = new RelayCommand(Hit, CanHit);
            SplitCommand = new RelayCommand(Split, CanSplit);
            DoubleCommand = new RelayCommand(DoubleDown, CanDoubleDown);
            
        }

     
        public bool HasSecondHand { get { return GameState.PlayersHand.Count == 2; } }

        public GameState GameState { get; set; }

        public RelayCommand DealCommand { get; }
        public RelayCommand StayCommand { get; }
        public RelayCommand HitCommand { get; }
        public RelayCommand SplitCommand { get; }
        public RelayCommand DoubleCommand { get; }



        public string TotalMoneyString
        {
            get { return  GameState.TotalMoney.ToString("C0"); }
        }

        public string GetDealerDownCard
        {
            get {
                if (GameState.PlayersTurnDone)
                {
                    
                    return GameState.DealersHand[0].GraphicPath;
                }
                return Environment.CurrentDirectory + DeckHelper.BackOfCard;
            }
        }
        public string GetDealerUpCard
        {
            get { return GameState.DealersHand[1].GraphicPath; }
        }

        public bool CanDeal()
        {
            return GameState.PlayersTurnDone;
        }


        public int NumberOfHands
        {
            get { return GameState.HandsPlayed; }
        }

        public void Deal()
        {
            GameState.Deal();
            UpdateDisplay();
            OnPropertyChanged();
        }


        public bool CanStay()
        {
            return GameState.CanStay;
        }
        public void Stay()
        {
            GameState.stay();
            UpdateDisplay();
            OnPropertyChanged(nameof(GetDealerDownCard));
            OnPropertyChanged();
        }
        public bool CanHit()
        {
            return GameState.CanHit;
        }
        public void Hit()
        {
            GameState.Hit();
            UpdateDisplay();
            OnPropertyChanged();
        }
        public bool CanSplit()
        {
            return GameState.CanSplit;
        }
        public void Split()
        {
            GameState.Split();
            UpdateDisplay();
            OnPropertyChanged(); ;
        }
        
        public bool CanDoubleDown()
        {
            return GameState.canDoubleDown;
        }
        public void DoubleDown()
        {
            GameState.DoubleDown();
            UpdateDisplay();
            OnPropertyChanged();
        }

        //Need to Update to force Display To Update?
        public void UpdateDisplay(int seconds = 0)
        {
            if (GameState.PlayersTurnDone)
            {
                GameState.FinishDealersHand();
            }
            
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000 * seconds);
                Application.Current.Dispatcher.BeginInvoke(() => {
                   
                    OnPropertyChanged(nameof(GameState));
                    DealCommand.NotifyCanExecuteChanged();
                    SplitCommand.NotifyCanExecuteChanged();
                    StayCommand.NotifyCanExecuteChanged();
                    DoubleCommand.NotifyCanExecuteChanged();
                    HitCommand.NotifyCanExecuteChanged();
                    OnPropertyChanged(nameof(GameState.PlayersHand));
                    OnPropertyChanged(nameof(GameState.CurrentPlayerIndex));
                });
                
                
            });
        }
    }
}
