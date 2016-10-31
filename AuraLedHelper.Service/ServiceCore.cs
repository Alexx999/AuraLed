using System;
using System.Collections.Generic;
using AuraLedHelper.Core;

namespace AuraLedHelper.Service
{
    class ServiceCore
    {
        private readonly JsonPipeServer _server;
        private static readonly Dictionary<ServiceCommand, Type> TypeMap = new Dictionary<ServiceCommand, Type>
        {
            [ServiceCommand.ApplySettings] = typeof(ServiceMessage<SettingsAndLocation>),
            [ServiceCommand.ClearSettings] = typeof(ServiceMessage<SettingsLocation>)
        };

        private static readonly Dictionary<ServiceCommand, Func<ServiceMessage, ServiceCommand>> MethodMap = new Dictionary<ServiceCommand, Func<ServiceMessage, ServiceCommand>>
        {
            [ServiceCommand.ApplySettings] = ApplySettings,
            [ServiceCommand.ClearSettings] = ClearSettings,
            [ServiceCommand.ReloadSettings] = ReloadSettings
        };

        private static ServiceCommand ReloadSettings(ServiceMessage serviceMessage)
        {
            return ServiceCommand.ResponseOk;
        }

        private static ServiceCommand ClearSettings(ServiceMessage serviceMessage)
        {
            var param = (ServiceMessage<SettingsLocation>)serviceMessage;
            return ServiceCommand.ResponseOk;
        }

        private static ServiceCommand ApplySettings(ServiceMessage serviceMessage)
        {
            var param = (ServiceMessage<SettingsAndLocation>) serviceMessage;
            return ServiceCommand.ResponseOk;
        }

        public ServiceCore()
        {
            _server = new JsonPipeServer(ServiceConfig.PipeName, TypeResolver, Processor);
        }

        public async void StartServiceCore()
        {
            await _server.StartServerAsync();
        }

        private ServiceCommand Processor(ServiceMessage serviceMessage)
        {
            Func<ServiceMessage, ServiceCommand> method;
            if (MethodMap.TryGetValue(serviceMessage.Command, out method))
            {
                return method(serviceMessage);
            }
            return ServiceCommand.ResponseFail;
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
    }
}
