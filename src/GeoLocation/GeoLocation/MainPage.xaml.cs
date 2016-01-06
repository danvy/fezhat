using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GeoLocation
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            var accessStatus = await Geolocator.RequestAccessAsync();
            if (accessStatus == GeolocationAccessStatus.Allowed)
            {
                var geolocator = new Geolocator { DesiredAccuracyInMeters = 10 };
                var pos = await geolocator.GetGeopositionAsync();
                var myLocation = pos.Coordinate.Point;
                //MyMap.MapServiceToken = "H6z8ftBVcuZ4yzVr7F1R~75EEFgMxnnWTCB6siPVpCw~AvB6UYdWbDQvUp52uRUsHPg7bbFuiEFEKplS_iGxUPrI3Kx40dgxtk7LhfJr82DR";
                //MyMap.Center = myLocation;
                //MyMap.ZoomLevel = 12;
            };
        }
    }
}
