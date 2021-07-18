using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace BlackJackTrainner.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {

        public MainWindowViewModel()
        {
            PracticeViewModel = new BlackJackPracticeViewModel();
            SimulateGamesViewModel = new SimulateGamesViewModel();
        }

        public BlackJackPracticeViewModel PracticeViewModel { get; set; }

        public SimulateGamesViewModel SimulateGamesViewModel { get; set; }

    public void MainWindow_Closing(object sender, CancelEventArgs e)
         {
            
         }

    }
}
