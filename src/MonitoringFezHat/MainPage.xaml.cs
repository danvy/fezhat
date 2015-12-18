using System;
using System.Diagnostics;
using System.Text;
using GHIElectronics.UWP.Shields;
using IoTSuiteLib;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
namespace MonitoringFezHat
{
    public sealed partial class MainPage : Page
    {
        private DeviceClient _client = null;
        private FEZHAT _hat = null;
        private string _host = "DaIoTMon";
        private string _deviceId = "79f24c65-15da-42bd-b10a-2565be6f3fd0";
        private string _key = "/1lRd7/WKMnoDcosgAFidw==";
        private DispatcherTimer _hatTimer = null;
        private DispatcherTimer _pullTimer = null;
        private bool _lightSwitch = false;
        private FEZHAT.Color _lightColor = FEZHAT.Color.Blue;
        private bool _invalidateFezHat = true;
        private bool _telemetry = true;
        public MainPage()
        {
            this.InitializeComponent();
            Loaded += MainPage_Loaded;
        }
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily.Contains("IoT"))
            {
                _hat = await FEZHAT.CreateAsync();
            }
            _hatTimer = new DispatcherTimer();
            _pullTimer = new DispatcherTimer();
            var connectionString = string.Format("HostName={0}.azure-devices.net;DeviceId={1};SharedAccessKey={2}", _host, _deviceId, _key);
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
            //data.Properties.UpdatedTime = DateTime.UtcNow;
            data.Properties.DeviceState = DeviceState.Normal;
            data.Commands.Add(new DeviceCommandDefinition("SwitchLight"));
            data.Commands.Add(new DeviceCommandDefinition("LightColor",
                new DeviceCommandParameterDefinition[] {
                    new DeviceCommandParameterDefinition("Color", DeviceCommandParameterType.String),
                    new DeviceCommandParameterDefinition("Color2", DeviceCommandParameterType.String)
                }));
            data.Commands.Add(new DeviceCommandDefinition("PingDevice"));
            data.Commands.Add(new DeviceCommandDefinition("StartTelemetry"));
            data.Commands.Add(new DeviceCommandDefinition("StopTelemetry"));
            var content = JsonConvert.SerializeObject(data);
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
            if (_hat == null)
                return;
            if (_hat.IsDIO18Pressed() || _hat.IsDIO22Pressed())
                LightSwitch = !LightSwitch;
            UpdateFezHat();
        }
        private void UpdateFezHat()
        {
            if (_hat == null)
                return;
            if (!_invalidateFezHat)
                return;
            _hat.D2.Color = LightSwitch ? LightColor : FEZHAT.Color.Black;
            //_hat.D3.Color = Light2Switch ? FEZHAT.Color.Red : FEZHAT.Color.Black;
            _invalidateFezHat = false;
        }
        private async void _pullTimer_Tick(object sender, object e)
        {
            //Send message
            if (_telemetry)
            {
                var data = new DeviceMonitoringData();
                data.DeviceId = _deviceId;
                data.Humidity = 0;
                data.Temperature = _hat == null ? 0 : _hat.GetTemperature();
                var content = JsonConvert.SerializeObject(data);
                await _client.SendEventAsync(new Message(Encoding.UTF8.GetBytes(content)));
            }
            //receive messages
            Message message;
            while ((message = await _client.ReceiveAsync()) != null)
            {
                var content = Encoding.ASCII.GetString(message.GetBytes());
                Debug.WriteLine("{0}> Received message: {1}", DateTime.Now.ToLocalTime(), content);
                var command = JsonConvert.DeserializeObject<DeviceCommand>(content);
                if (command != null)
                {
                    if (command.Name == "SwitchLight")
                    {
                        LightSwitch = !LightSwitch;
                        await _client.CompleteAsync(message);
                    }
                    else if (command.Name == "LightColor")
                    {
                        if (command.Parameters.Count == 1)
                        {
                            var color = FEZHAT.Color.Black;
                            if (command.Parameters[0].Value == "Red")
                            {
                                LightColor = FEZHAT.Color.Red;
                            }
                            else if (command.Parameters[0].Value == "Blue")
                            {
                                LightColor = FEZHAT.Color.Blue;
                            }
                            if (command.Parameters[0].Value == "Green")
                            {
                                LightColor = FEZHAT.Color.Green;
                            }
                            if (color == FEZHAT.Color.Black)
                            {
                                await _client.RejectAsync(message);
                            }
                            else
                            {
                                await _client.CompleteAsync(message);
                            }
                        }
                    }
                    else if (command.Name == "PingDevice")
                    {
                        await _client.CompleteAsync(message);
                    }
                    else if (command.Name == "StartTelemetry")
                    {
                        _telemetry = true;
                        await _client.CompleteAsync(message);
                    }
                    else if (command.Name == "StopTelemetry")
                    {
                        _telemetry = false;
                        await _client.CompleteAsync(message);
                    }
                    else
                    {
                        await _client.RejectAsync(message);
                    }
                }
            }
        }
        public bool LightSwitch
        {
            get
            {
                return _lightSwitch;
            }
            set
            {
                if (value == _lightSwitch)
                    return;
                _lightSwitch = value;
                _invalidateFezHat = true;
            }
        }
        public FEZHAT.Color LightColor
        {
            get
            {
                return _lightColor;
            }
            set
            {
                if (value == _lightColor)
                    return;
                _lightColor = value;
                _invalidateFezHat = true;
            }
        }
    }
}
