using System;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AuraLedHelper.Core
{
    public class JsonPipeClient : IDisposable
    {
        private readonly string _name;
        private readonly Func<ServiceCommand, Type> _typeResolver;
        private const int ConnectTimeout = 50;

        private PipeMessenger _messenger;
        private CancellationTokenSource _cts;

        private readonly ConcurrentDictionary<int, TaskCompletionSource<ServiceMessage>> _pendingRequests =
            new ConcurrentDictionary<int, TaskCompletionSource<ServiceMessage>>();

        private int _counter;

        public JsonPipeClient(string name, Func<ServiceCommand, Type> typeResolver)
        {
            _name = name;
            _typeResolver = typeResolver;
            Initialize();
        }

        private void Initialize()
        {
            var pipe = new NamedPipeClientStream(".", _name, PipeDirection.InOut, PipeOptions.Asynchronous);
            pipe.Connect(ConnectTimeout);
            _cts = new CancellationTokenSource();
            _messenger = new PipeMessenger(pipe, new JsonMessageProcessor(_typeResolver, Callback));
            _messenger.StartReading(_cts.Token);
        }

        private void Callback(ServiceMessage serviceMessage)
        {
            TaskCompletionSource<ServiceMessage> tcs;
            if (_pendingRequests.TryGetValue(serviceMessage.MessageId, out tcs))
            {
                tcs.SetResult(serviceMessage);
            }
        }

        public async Task<ServiceMessage> SendRequestAsync(ServiceMessage message)
        {
            try
            {
                return await SendRequestInternalAsync(message);
            }
            catch (IOException ex)
            {
                LogHelper.LogError(ex);
                SetRequestsFailed(ex);
                _cts.Cancel();
                _messenger.Dispose();
                Initialize();
                return await SendRequestInternalAsync(message);
            }
        }

        private void SetRequestsFailed(Exception ex)
        {
            foreach (var request in _pendingRequests)
            {
                request.Value.SetException(ex);
            }
            _pendingRequests.Clear();
        }

        public async Task<ServiceMessage> SendRequestInternalAsync(ServiceMessage message)
        {
            message.MessageId = Interlocked.Increment(ref _counter);
            var tcs = new TaskCompletionSource<ServiceMessage>();
            _pendingRequests.TryAdd(message.MessageId, tcs);

            var jsonStr = JsonConvert.SerializeObject(message);
            var bytes = Encoding.UTF8.GetBytes(jsonStr);
            await _messenger.SendMessageAsync(bytes, _cts.Token);

            return await tcs.Task;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _cts.Cancel();
            _messenger.Dispose();
        }
    }
}
