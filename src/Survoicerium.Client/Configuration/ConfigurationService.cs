using System.Configuration;
using System.Runtime.CompilerServices;

namespace Survoicerium.Client.Configuration
{
    public class ConfigurationService : IConfigurationService
    {
        private readonly System.Configuration.Configuration _configuration;

        public ConfigurationService()
        {
            _configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        }

        public string ApiKey
        {
            get
            {
                return GetValue();
            }
            set
            {
                UpdateValue(value);
            }
        }

        public string GetBackendExternalAddress()
        {
#if DEBUG
            return "http://localhost:5000";
#else
            return "http://api.survarium-voice.party";
#endif
        }

        public string GetFrontendExternalAddress()
        {
#if DEBUG
            return "http://localhost:5001";
#else
            return "http://survarium-voice.party";
#endif
        }

        private void UpdateValue(string newValue, [CallerMemberName]string setting = null)
        {
            // probably it is better use something like Set or check if setting exists, but who cares
            _configuration.AppSettings.Settings.Remove(setting);
            _configuration.AppSettings.Settings.Add(setting, newValue);
            _configuration.Save();
        }

        private string GetValue([CallerMemberName]string setting = null)
        {
            return _configuration.AppSettings.Settings[setting]?.Value;
        }
    }
}
