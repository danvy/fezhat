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
using Newtonsoft.Json;
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
        private DispatcherTimer _hatTimer;
        private DispatcherTimer _pullTimer;
        private bool _light1Switch;
        private bool _light2Switch;
        private bool _invalidateFezHat = true;

        public bool Light1Switch
        {
            get
            {
                return _light1Switch;
            }
            set
            {
                if (value == _light1Switch)
                    return;
                _light1Switch = value;
                _invalidateFezHat = true;
            }
        }
        public bool Light2Switch
        {
            get
            {
                return _light2Switch;
            }
            set
            {
                if (value == _light2Switch)
                    return;
                _light2Switch = value;
                _invalidateFezHat = true;
            }
        }
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }

        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            _hat = await FEZHAT.CreateAsync();
            _hatTimer = new DispatcherTimer();
            _pullTimer = new DispatcherTimer();
            var connectionString = string.Format("HostName={0}.azure-devices.net;DeviceId={1};SharedAccessKey={2}", _host, _deviceId, _key);
            //var authentication = AuthenticationMethodFactory.CreateAuthenticationWithSharedAccessPolicyKey(_deviceId, "", _key);
            //var connection = IotHubConnectionStringBuilder.Create();
            _client = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Http1);
            var data = new DeviceMetaData();
            data.Version = "1.0";
            data.IsSimulatedDevice = false;
            data.Properties.DeviceID = _deviceId;
            data.Properties.FirmwareVersion = "1.42";
            data.Properties.HubEnabledState = true;
            data.Properties.Processor = "ARM";
            data.Properties.Platform = "UWP";
            data.Properties.SerialNumber = "1234567890";
            data.Properties.InstalledRAM = "1024 MB";
            data.Properties.ModelNumber = "007-BOND";
            data.Properties.Manufacturer = "Raspberry";
            data.Properties.UpdatedTime = DateTime.UtcNow;
            data.Properties.DeviceState = DeviceState.Normal;
            var light1Command = new DeviceCommand();
            light1Command.Name = "SwitchLight1";
            //lightCommand.Parameters.Add(new DeviceCommandParameter() { Name = "On", Type = DeviceCommandParameterType.Double });
            data.Commands.Add(light1Command);
            var light2Command = new DeviceCommand();
            light2Command.Name = "SwitchLight2";
            //lightCommand.Parameters.Add(new DeviceCommandParameter() { Name = "On", Type = DeviceCommandParameterType.Double });
            data.Commands.Add(light2Command);
            var settings = new JsonSerializerSettings();
            //settings.DateFormatString = "";
            settings.Converters.Add(new BoolToIntConverter());
            var content = JsonConvert.SerializeObject(data, settings);
            await _client.SendEventAsync(new Message(Encoding.UTF8.GetBytes(content)));
            _pullTimer.Interval = TimeSpan.FromMilliseconds(5000);
            _pullTimer.Tick += _pullTimer_Tick;
            _pullTimer.Start();
            _hatTimer.Interval = TimeSpan.FromMilliseconds(100);
            _hatTimer.Tick += _hatTimer_Tick;
            _hatTimer.Start();
        }

        private void _hatTimer_Tick(object sender, object e)
        {
            if (_hat.IsDIO18Pressed())
                Light1Switch = !Light1Switch;
            if (_hat.IsDIO22Pressed())
                Light2Switch = !Light2Switch;
            UpdateFezHat();
        }
        private void UpdateFezHat()
        {
            if (_hat == null)
                return;
            if (!_invalidateFezHat)
                return;
            _hat.D2.Color = Light1Switch ? FEZHAT.Color.Green : FEZHAT.Color.Black;
            _hat.D3.Color = Light2Switch ? FEZHAT.Color.Red : FEZHAT.Color.Black;
            _invalidateFezHat = false;
        }
        private async void _pullTimer_Tick(object sender, object e)
        {
            //Send message
            var data = new TelemetryRemoteMonitorData();
            data.DeviceId = _deviceId;
            data.Humidity = 0;
            data.Temperature = _hat == null ? 0 : _hat.GetTemperature();
            var content = JsonConvert.SerializeObject(data);
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
