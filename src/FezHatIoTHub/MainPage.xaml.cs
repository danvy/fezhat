using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using GHIElectronics.UWP.Shields;
using Microsoft.Azure.Devices.Client;
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

namespace FezHatIoTHub
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private DeviceClient _client;
        private FEZHAT _hat;
        private string _deviceId = "2bdf835c-f2f6-42a1-8c3d-f279b322d91e";
        private string _key = "Q2U8h4C27UiBPRthw0ETfw==";
        private string _host = "DaIoTMonitor";
        private DispatcherTimer _timer;
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _hat = await FEZHAT.CreateAsync();
            _timer = new DispatcherTimer();
            var c = new HttpClient();
            var s = await c.GetAsync("http://danvy.tv");
            Debug.WriteLine(s);
            var connectionString = string.Format("HostName={0}.azure-devices.net;DeviceId={1};SharedAccessKey={2}", _host, _deviceId, _key);
            //var authentication = AuthenticationMethodFactory.CreateAuthenticationWithSharedAccessPolicyKey(_deviceId, "", _key);
            //var connection = IotHubConnectionStringBuilder.Create();
            _client = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Http1);
            _timer.Interval = TimeSpan.FromMilliseconds(1000);
            _timer.Tick += _timer_Tick;
            _timer.Start();
        }

        private async void _timer_Tick(object sender, object e)
        {
            //Send message
            var data = new RemoteMonitorTelemetryData();
            data.DeviceId = _deviceId;
            data.Humidity = 0;
            data.Temperature = _hat.GetTemperature();
            var content = Danvy.Tools.Json.Serialize(data);
            await _client.SendEventAsync(new Message(Encoding.UTF8.GetBytes(content)));
            //receive messages
            Message message;
            while ((message = await _client.ReceiveAsync()) != null)
            { 
                content = Encoding.ASCII.GetString(message.GetBytes());
                Debug.WriteLine("{0}> Received message: {1}", DateTime.Now.ToLocalTime(), content);
                await _client.CompleteAsync(message);
            }
        }
    }
}
