using System;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AuraLedHelper.Core.Extensions;
using Newtonsoft.Json;

namespace AuraLedHelper.Core
{
    public class JsonPipeServer : IDisposable
    {
        private readonly string _name;
        private readonly Func<ServiceCommand, Type> _typeResolver;
        private readonly Func<ServiceMessage, Task<ServiceCommand>> _processor;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private int _counter;
        private readonly ConcurrentBag<PipeMessenger> _messengers = new ConcurrentBag<PipeMessenger>();

        public JsonPipeServer(string name, Func<ServiceCommand, Type> typeResolver, Func<ServiceMessage, Task<ServiceCommand>> processor)
        {
            _name = name;
            _typeResolver = typeResolver;
            _processor = processor;
        }

        public async Task StartServerAsync()
        {
            var ps = new PipeSecurity();
            var sid = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
            var par = new PipeAccessRule(sid, PipeAccessRights.ReadWrite, AccessControlType.Allow);
            ps.AddAccessRule(par);
            sid = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            par = new PipeAccessRule(sid, PipeAccessRights.FullControl, AccessControlType.Allow);
            ps.AddAccessRule(par);

            while (!_cts.IsCancellationRequested)
            {
                var pipe = new NamedPipeServerStream(_name, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, 0, 0, ps);
                await pipe.WaitForConnectionAsync();
                PipeMessenger messenger = null;
                // ReSharper disable once AccessToModifiedClosure
                messenger = new PipeMessenger(pipe, new JsonMessageProcessor(_typeResolver, msg => Callback(messenger, msg)));
                messenger.StartReading(_cts.Token);
                _messengers.Add(messenger);
            }
        }

        private async void Callback(PipeMessenger messenger, ServiceMessage serviceMessage)
        {
            var response = _processor(serviceMessage);

            await SendResponseAsync(messenger, new ServiceMessage(await response));
        }

        public Task SendResponseAsync(PipeMessenger messenger, ServiceMessage message)
        {
            message.MessageId = Interlocked.Increment(ref _counter);

            var jsonStr = JsonConvert.SerializeObject(message);
            var bytes = Encoding.UTF8.GetBytes(jsonStr);
            return messenger.SendMessageAsync(bytes, _cts.Token);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _cts.Cancel();
            foreach (var messenger in _messengers)
            {
                messenger.Dispose();
            }
        }
    }
}
