using BlackJackSimulatorWPF;
using BlackJackSimulatorWPF.ViewModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Windows;

namespace BlackjackSimulatorWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            //DispatcherHelper.Initialize();
            ViewModel = new MainWindowViewModel();
            Mainview = new MainWindow();
            Mainview.DataContext = ViewModel;
            Mainview.Show();
            Current.MainWindow.Closing += new CancelEventHandler(ViewModel.MainWindow_Closing);
        }
        private MainWindow Mainview { get; set; }
        private MainWindowViewModel ViewModel { get; set; }
    }

}
