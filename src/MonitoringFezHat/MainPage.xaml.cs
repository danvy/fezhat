using System;
using System.Diagnostics;
using System.Text;
using Danvy.Tools;
using GHIElectronics.UWP.Shields;
using IoTSuiteLib;
using Microsoft.Azure.Devices.Client;
using MonitoringFezHatShared.Services;
using MonitoringShared.Services;
using MonitoringShared.ViewModels;
using Newtonsoft.Json;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace MonitoringFezHat
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel _viewModel;
        public MainViewModel ViewModel { get { return _viewModel; } }
        public MainPage()
        {
            IoC.Instance.Register<ILogService>(() => { return new DebugLogService(); });
            IoC.Instance.Register<IBoardService>(() => { return new FezHatBoardService(); });
            _viewModel = new MainViewModel();
            this.InitializeComponent();
            //DataContext = ViewModel;
            Loaded += MainPage_Loaded;
        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitAsync();
        }
    }
}
