using Danvy.ViewModels;

namespace MonitoringShared.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private string _host = "DaIoTMonitoring";
        private string _deviceId = "8ca300ca-2405-4f5f-8a75-2f8ed4803a6a";
        private string _key = "fy4hWfdRitdk2vNMZFRlQw==";
        public static readonly SettingsViewModel Default = new SettingsViewModel();
        public string Host
        {
            get
            {
                return _host;
            }

            set
            {
                if (value == _host)
                    return;
                _host = value;
                RaisePropertyChanged();
            }
        }

        public string DeviceId
        {
            get
            {
                return _deviceId;
            }
            set
            {
                if (value == _deviceId)
                    return;
                _deviceId = value;
                RaisePropertyChanged();
            }
        }
        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                if (value == _key)
                    return;
                _key = value;
                RaisePropertyChanged();
            }
        }
    }
}
