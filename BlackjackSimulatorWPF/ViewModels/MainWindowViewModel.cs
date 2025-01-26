using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.ComponentModel;


namespace BlackJackSimulatorWPF.ViewModel
{
    public class MainWindowViewModel: ObservableObject
    {

        public MainWindowViewModel()
        {
            Thread UIthread = Thread.CurrentThread;
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
