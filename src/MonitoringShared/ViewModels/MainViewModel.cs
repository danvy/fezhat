using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Danvy.Tools;
using Danvy.ViewModels;
using IoTSuiteLib;
using Microsoft.Azure.Devices.Client;
using MonitoringShared.Services;
using MonitoringShared.Tools;
using Newtonsoft.Json;

namespace MonitoringShared.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private DeviceClient _client = null;
        private Timer _timer = null;
        private bool _telemetry = true;
        private bool _started;
        private RelayCommand _switchLightCommand;
        private RelayCommand _startStopCommand;
        private IBoardService _board;
        private bool _lightOn;
        private double _temperature;
        private LEDColor _lightColor = LEDColor.Blue;
        private double _humidity;
        private double? _externalTemperature;
        private RelayCommand _settingsCommand;
        public double? ExternalTemperature
        {
            get
            {
                return _externalTemperature;
            }
            set
            {
                if (value == _externalTemperature)
                    return;
                _externalTemperature = value;
                RaisePropertyChanged();
            }
        }
        public double Humidity
        {
            get
            {
                return _humidity;
            }
            set
            {
                if (value == _humidity)
                    return;
                _humidity = value;
                RaisePropertyChanged();
            }
        }
        public LEDColor LightColor
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
                RaisePropertyChanged();
            }
        }
        public bool LightOn
        {
            get
            {
                return _lightOn;
            }
            set
            {
                if (value == _lightOn)
                    return;
                _lightOn = value;
                RaisePropertyChanged();
            }
        }
        public bool Started
        {
            get
            {
                return _started;
            }
            private set
            {
                if (value == _started)
                    return;
                _started = value;
                RaisePropertyChanged();
            }
        }
        public string StartStopCaption { get; private set; }
        public double Temperature
        {
            get
            {
                return _temperature;
            }
            set
            {
                if (value == _temperature)
                    return;
                _temperature = value;
                RaisePropertyChanged();
            }
        }
        public ICommand SwitchLightCommand
        {
            get
            {
                return _switchLightCommand ?? (_switchLightCommand = new RelayCommand(() =>
                {
                    SwitchLight();
                }));
            }
        }
        public ICommand SettingsCommand
        {
            get
            {
                return _settingsCommand ?? (_settingsCommand = new RelayCommand(() =>
                {
                    //
                }));
            }
        }
        public ICommand StartStopCommand
        {
            get
            {
                return _startStopCommand ?? (_startStopCommand = new RelayCommand(() =>
                {
                    if (Started)
                    {
                        Stop();
                    }
                    else
                    {
                        Start();
                    }
                }));
            }
        }
        public async Task InitAsync()
        {
            _board = IoC.Instance.Resolve<IBoardService>();
            //if (_board == null)
            //    throw new Exception("A board must be registered");
            var connectionString = string.Format("HostName={0}.azure-devices.net;DeviceId={1};SharedAccessKey={2}",
                SettingsViewModel.Default.Host, SettingsViewModel.Default.DeviceId, SettingsViewModel.Default.Key);
            _client = DeviceClient.CreateFromConnectionString(connectionString, TransportType.Http1);
            var data = new DeviceMetaData();
            data.Version = "1.0";
            data.IsSimulatedDevice = false;
            data.Properties.DeviceID = SettingsViewModel.Default.DeviceId;
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
        }
        public void SwitchLight()
        {
            LightOn = !LightOn;
        }
        public void Start()
        {
            if (_timer == null)
                _timer = new Timer(timerCallback, null, TimeSpan.FromTicks(0), TimeSpan.FromMilliseconds(200));
            Started = true;
            StartStopCaption = "Stop";
        }
        private void Stop()
        {
            if (_timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
            StartStopCaption = "Start";
            Started = false;
        }
        private async void timerCallback(object state)
        {
            if (_board != null)
            {
                if (_board.ButtonPressed)
                    SwitchLight();
                Humidity = _board.Humidity;
                Temperature = _board.Temperature;
                ExternalTemperature = _board.ExternalTemperature;
            }
            //Every 5 seconds
            if (DateTime.Now.Second % 5 != 0)
                return;
            if (_client == null)
                return;
            //Send message
            if (_telemetry)
            {
                var data = new DeviceMonitoringData();
                data.DeviceId = SettingsViewModel.Default.DeviceId;
                data.Humidity = Humidity;
                data.Temperature = Temperature;
                data.ExternalTemperature = ExternalTemperature;
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
                        SwitchLight();
                        await _client.CompleteAsync(message);
                    }
                    else if (command.Name == "LightColor")
                    {
                        if (command.Parameters.Count == 1)
                        {
                            var color = LEDColor.Black;
                            if (Enum.TryParse<LEDColor>(command.Parameters[0].Value, out color))
                            {
                                LightColor = color;
                                await _client.CompleteAsync(message);
                            }
                            else
                            {
                                await _client.RejectAsync(message);
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
    }
}
