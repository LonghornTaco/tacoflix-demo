using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TacoFlix.Model.Configuration;

namespace TacoFlix.Client.Configuration
{
    public class AppConfiguration : IXconnectConfiguration, IMovieInfoProviderConfiguration
    {
        public string XconnectUrl => GetValue<string>("XconnectUrl");
        public string ContactSource => GetValue<string>("ContactSource");
        public string InteractionUserAgent => GetValue<string>("InteractionUserAgent");
        public string MovieRenterEraOutcomeId => GetValue<string>("MovieRenterEraOutcomeId");
        public string MovieRentedOutcomeId => GetValue<string>("MovieRentedOutcomeId");

        public string MovieDbApiKey => GetValue<string>("MovieDbApiKey");

        private static T GetValue<T>(string key)
        {
            var returnValue = default(T);
            var converter = TypeDescriptor.GetConverter(typeof(T));
            object value = ConfigurationManager.AppSettings[key];
            if (value != null)
            {
                try
                {
                    returnValue = (T)converter.ConvertFrom(value);
                }
                catch (Exception)
                {
                    Trace.TraceError($"Failed trying to convert '{value}' to type '{key}'");
                }
            }
            else
            {
                Trace.TraceError($"Could not find the config value '{key}'");
            }

            return returnValue;
        }
    }
}
