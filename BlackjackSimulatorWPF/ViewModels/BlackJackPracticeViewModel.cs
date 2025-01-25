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

        public ICommand DealCommand { get; }
        public ICommand StayCommand { get; }
        public ICommand HitCommand { get; }
        public ICommand SplitCommand { get; }
        public ICommand DoubleCommand { get; }



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
        public void UpdateDisplay(int seconds = 4)
        {
            if (GameState.PlayersTurnDone)
            {
                GameState.FinishDealersHand();
            }
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000 * seconds);

                OnPropertyChanged();
                OnPropertyChanged(nameof(GameState));
                
            });
        }
    }
}
