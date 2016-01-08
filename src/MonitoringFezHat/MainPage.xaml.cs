using System;
using System.Diagnostics;
using System.Text;
using Danvy.Services;
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
using Windows.UI.Xaml.Navigation;

namespace MonitoringFezHat
{
    public sealed partial class MainPage : Page
    {
        private MainViewModel _viewModel;
        public MainViewModel ViewModel { get { return _viewModel; } }
        public MainPage()
        {
            IoC.Instance.Register<ILogService>(() => { return new DebugLogService(); });
            IoC.Instance.Register<IDispatcherService>(() => { return new CoreDispatcherService(); });
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily.Contains("IoT"))
            {
                IoC.Instance.Register<IBoardService>(() => { return new FezHatBoardService(); });
            }
            _viewModel = new MainViewModel();
            this.InitializeComponent();
            //DataContext = ViewModel;
            Loaded += MainPage_Loaded;
        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.InitAsync();
        }
        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (_viewModel != null)
                _viewModel.Stop();
            base.OnNavigatingFrom(e);
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (_viewModel != null)
                _viewModel.Start();
        }
    }
}
