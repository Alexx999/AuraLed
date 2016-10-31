namespace AuraLedHelper.Core
{
    public class SettingsAndLocation
    {
        public SettingsAndLocation()
        {
        }

        public SettingsAndLocation(Settings settings, SettingsLocation location)
        {
            Settings = settings;
            Location = location;
        }

        public SettingsLocation Location { get; set; }
        public Settings Settings { get; set; }
    }
}