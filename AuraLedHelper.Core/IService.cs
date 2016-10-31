using System.Threading.Tasks;

namespace AuraLedHelper.Core
{
    public interface IService
    {
        Task<bool> ReloadSettingsAsync();
        Task<bool> ApplySettingsAsync(Settings settings, SettingsLocation location);
        Task<bool> ClearSettingsAsync(SettingsLocation location);
    }
}