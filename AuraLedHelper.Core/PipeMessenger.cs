using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace AuraLedHelper.Core
{
    public class PipeMessenger : IDisposable
    {
        private const int BufferSize = 64*1024;
        private readonly PipeStream _pipe;
        private readonly IMessageProcessor _processor;
        private readonly byte[] _buffer = new byte[BufferSize];
        private int _pos;

        public PipeMessenger(PipeStream pipe, IMessageProcessor processor)
        {
            if (!pipe.IsAsync)
            {
                throw new NotSupportedException("Pipe is not async");
            }

            if (!pipe.IsConnected)
            {
                throw new NotSupportedException("Pipe is not connected");
            }

            _pipe = pipe;
            _processor = processor;
        }

        public async void StartReading(CancellationToken ct)
        {
            try
            {
                await ReadLoopAsync(ct).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex);
            }
        }

        private async Task ReceiveAndProcessMessageAsync(CancellationToken ct)
        {
            _pos += await _pipe.ReadAsync(_buffer, _pos, _buffer.Length - _pos, ct);

            do
            {
                ct.ThrowIfCancellationRequested();
            } while (ReadNextPacket());
        }

        private bool ReadNextPacket()
        {
            if (_pos < 2)
            {
                return false;
            }
            using (var ms = new MemoryStream(_buffer, 0, _pos, false))
            {
                using (var reader = new BinaryReader(ms))
                {
                    var len = reader.ReadUInt16();

                    var totalLength = len + ms.Position;
                    if (totalLength > _pos)
                    {
                        return false;
                    }

                    var msg = reader.ReadBytes(len);

                    Buffer.BlockCopy(_buffer, (int) totalLength, _buffer, 0, (int) totalLength);
                    _pos -= (int) totalLength;

                    ProcessMessage(msg);

                    return true;
                }
            }
        }

        private void ProcessMessage(byte[] msg)
        {
            try
            {
                _processor.ProcessMessage(msg);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex);
            }
        }

        private async Task ReadLoopAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                await ReceiveAndProcessMessageAsync(ct);
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _pipe.Close();
        }

        public async Task SendMessageAsync(byte[] message, CancellationToken ct)
        {
            if (message.Length > BufferSize)
            {
                throw new NotSupportedException("Message too big");
            }

            var lenBytes = BitConverter.GetBytes((short) message.Length);

            Debug.Assert(lenBytes.Length == 2);

            await _pipe.WriteAsync(lenBytes, 0, lenBytes.Length, ct);
            await _pipe.WriteAsync(message, 0, message.Length, ct);
        }
    }
}
