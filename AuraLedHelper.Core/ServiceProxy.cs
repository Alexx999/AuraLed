using System.Threading.Tasks;

namespace AuraLedHelper.Core
{
    public class ServiceProxy : IService
    {
        private readonly JsonPipeClient _client;

        public ServiceProxy()
        {
            _client = new JsonPipeClient(ServiceConfig.PipeName, t => typeof(ServiceMessage));
        }

        public async Task<bool> ReloadSettingsAsync()
        {
            var msg = new ServiceMessage(ServiceCommand.ReloadSettings);
            var response = await _client.SendRequestAsync(msg);
            return response.Command == ServiceCommand.ResponseOk;
        }

        public async Task<bool> ApplySettingsAsync(Settings settings, SettingsLocation location)
        {
            var msg = new ServiceMessage<SettingsAndLocation>(ServiceCommand.ApplySettings, new SettingsAndLocation(settings, location));
            var response = await _client.SendRequestAsync(msg);
            return response.Command == ServiceCommand.ResponseOk;
        }

        public async Task<bool> ClearSettingsAsync(SettingsLocation location)
        {
            var msg = new ServiceMessage<SettingsLocation>(ServiceCommand.ClearSettings, location);
            var response = await _client.SendRequestAsync(msg);
            return response.Command == ServiceCommand.ResponseOk;
        }
    }
}
