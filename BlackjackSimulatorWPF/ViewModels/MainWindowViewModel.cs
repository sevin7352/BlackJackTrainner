using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm;
using CommunityToolkit.Mvvm.ComponentModel;
using BlackJackSimulatorWPF.ViewModel;

namespace BlackJackSimulatorWPF.ViewModel
{
    public class MainWindowViewModel: ObservableObject
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
