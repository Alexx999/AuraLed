using System;
using System.Collections.Concurrent;
using System.IO.Pipes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AuraLedHelper.Core
{
    public class JsonPipeClient : IDisposable
    {
        private readonly PipeMessenger _messenger;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private readonly ConcurrentDictionary<int, TaskCompletionSource<ServiceMessage>> _pendingRequests = new ConcurrentDictionary<int, TaskCompletionSource<ServiceMessage>>();
        private int _counter;

        public JsonPipeClient(string name, Func<ServiceCommand, Type> typeResolver)
        {
            var pipe = new NamedPipeClientStream(".", name, PipeDirection.InOut, PipeOptions.Asynchronous);
            pipe.Connect(1);
            _messenger = new PipeMessenger(pipe, new JsonMessageProcessor(typeResolver, Callback));
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
