using System.IO.Pipes;
using System.Threading.Tasks;

namespace AuraLedHelper.Core.Extensions
{
    public static class NamedPipeEx
    {
        public static Task WaitForConnectionAsync(this NamedPipeServerStream tgt)
        {
            return Task.Factory.FromAsync(tgt.BeginWaitForConnection(null, null), tgt.EndWaitForConnection);
        }
    }
}
