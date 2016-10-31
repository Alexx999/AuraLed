using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuraLedHelper.Core;

namespace AuraLedHelper.Service
{
    class ServiceCore : IDisposable
    {
        private readonly JsonPipeServer _server;
        private readonly SettingsProvider _settingsProvider = new SettingsProvider();
        private static readonly Dictionary<ServiceCommand, Type> TypeMap = new Dictionary<ServiceCommand, Type>
        {
            [ServiceCommand.ApplySettings] = typeof(ServiceMessage<SettingsAndLocation>),
            [ServiceCommand.ClearSettings] = typeof(ServiceMessage<SettingsLocation>)
        };

        private readonly Dictionary<ServiceCommand, Func<ServiceMessage, Task<ServiceCommand>>> _methodMap;
        private string _activeUser;

        private Task<ServiceCommand> ReloadSettings(ServiceMessage serviceMessage)
        {
            LoadSettings();
            return Task.FromResult(ServiceCommand.ResponseOk);
        }

        private async Task<ServiceCommand> ClearSettings(ServiceMessage serviceMessage)
        {
            var param = (ServiceMessage<SettingsLocation>)serviceMessage;
            var result = await _settingsProvider.RemoveSettingsAsync(param.Payload);
            return result ? ServiceCommand.ResponseOk : ServiceCommand.ResponseFail;
        }

        private async Task<ServiceCommand> ApplySettings(ServiceMessage serviceMessage)
        {
            var param = (ServiceMessage<SettingsAndLocation>) serviceMessage;
            var result = await _settingsProvider.SaveSettingsAsync(param.Payload.Settings, param.Payload.Location);
            return result ? ServiceCommand.ResponseOk : ServiceCommand.ResponseFail;
        }

        public ServiceCore()
        {
            _server = new JsonPipeServer(ServiceConfig.PipeName, TypeResolver, Processor);
            _methodMap = new Dictionary<ServiceCommand, Func<ServiceMessage, Task<ServiceCommand>>>
            {
                [ServiceCommand.ApplySettings] = ApplySettings,
                [ServiceCommand.ClearSettings] = ClearSettings,
                [ServiceCommand.ReloadSettings] = ReloadSettings
            };
        }

        public async void StartServiceCore()
        {
            await _server.StartServerAsync();
        }

        private Task<ServiceCommand> Processor(ServiceMessage serviceMessage)
        {
            Func<ServiceMessage, Task<ServiceCommand>> method;
            if (_methodMap.TryGetValue(serviceMessage.Command, out method))
            {
                return method(serviceMessage);
            }
            return Task.FromResult(ServiceCommand.ResponseFail);
        }

        private Type TypeResolver(ServiceCommand key)
        {
            Type type;

            if (TypeMap.TryGetValue(key, out type))
            {
                return type;
            }
            return typeof(ServiceMessage);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _server.Dispose();
        }

        public void LoadSettings()
        {
            var sid = WtsHelper.GetCurrentAccountSID();
            _activeUser = sid;
            LoadSettings(sid);
        }

        public void LoadSettings(string sid)
        {
            var settings = LoadSettingsForUser(sid);
            if (settings == null)
            {
                return;
            }

            AuraController.ApplySettings(settings);
        }

        private Settings LoadSettingsForUser(string sid)
        {
            if (sid == null)
            {
                return _settingsProvider.LoadSettings(SettingsLocation.System);
            }
            return _settingsProvider.LoadSettingsForUser(sid);
        }

        public void UserChange()
        {
            var sid = WtsHelper.GetCurrentAccountSID();
            if (Equals(sid, _activeUser))
            {
                return;
            }

            LoadSettings(sid);
        }
    }
}
