using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using System;
using BlackJackTrainner.ViewModel;

namespace BlackJackTrainner.ViewModel
{
    public static class Locator
    {

        private static readonly Lazy<MainWindowViewModel> __mainViewModel = new Lazy<MainWindowViewModel>(CreateMainViewModel);

        private static MainWindowViewModel CreateMainViewModel()
        {
            return new MainWindowViewModel();
        }

        public static MainWindowViewModel Main
        {
            get
            {
                return __mainViewModel.Value;
            }
        }
        
        public static void Cleanup()
        {
            // TODO Clear the ViewModels
        }
    }
}