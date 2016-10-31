namespace AuraLedHelper.Core
{
    public interface IService
    {
        void ReloadSettings();
        void ApplySettings(Settings settings, SettingsLocation location);
        void ClearSettings(SettingsLocation location);
    }
}