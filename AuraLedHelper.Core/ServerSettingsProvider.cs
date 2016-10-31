using System.Threading.Tasks;

namespace AuraLedHelper.Core
{
    public class ServerSettingsProvider : SettingsProvider
    {
        protected override Task<bool> NoProxyRealoadResult()
        {
            return Task.FromResult(true);
        }
    }
}
