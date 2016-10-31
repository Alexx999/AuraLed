using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuraLedHelper.Core
{
    public class ServiceProxy : IService
    {
        private JsonPipeClient _client;

        public ServiceProxy()
        {
            _client = new JsonPipeClient(ServiceConfig.PipeName, t => typeof(ServiceMessage));
        }

        public void ReloadSettings()
        {
            
        }

        public void ApplySettings(Settings settings, SettingsLocation location)
        {
            
        }

        public void ClearSettings(SettingsLocation location)
        {
            
        }
    }
}
