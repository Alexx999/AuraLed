using System;
using System.Globalization;
using System.Windows.Media;
using Microsoft.Win32;

namespace AuraLedHelper.Core
{
    public static class SettingsProvider
    {
        private const string RegistryKey = "Software\\AsusLedHelper\\Settings";

        private static Settings GetDefaults()
        {
            return new Settings {Enabled = false, Mode = AuraMode.Breathing, Color = Colors.Red};
        }

        public static Settings LoadDefaultSettings()
        {
            try
            {
                return LoadSettingsInternal();
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex);
                return GetDefaults();
            }
        }

        public static Settings LoadSettings(SettingsLocation location)
        {
            var key = GetKey(location);
            if (key != null)
            {
                return LoadFromRegistry(key);
            }
            return null;
        }
        
        private static RegistryKey GetKey(SettingsLocation location)
        {
            switch (location)
            {
                case SettingsLocation.User:
                {
                    return Registry.CurrentUser;
                }
                case SettingsLocation.System:
                {
                    return Registry.LocalMachine;
                }
                default:
                    return null;
            }
        }

        private static Settings LoadSettingsInternal()
        {
            var settings = LoadSettings(SettingsLocation.User);
            if (settings != null)
            {
                return settings;
            }
            settings = LoadSettings(SettingsLocation.System);
            if (settings != null)
            {
                return settings;
            }

            return GetDefaults();
        }


        private static RegistryKey OpenSettingsKey(RegistryKey registryKey)
        {
            return registryKey.OpenSubKey(RegistryKey);
        }

        public static void SaveSettings(Settings settings, SettingsLocation location)
        {
            
        }

        public static void RemoveSettings(SettingsLocation location)
        {
            
        }

        #region Reading

        private static Settings LoadFromRegistry(RegistryKey registryKey)
        {
            var key = OpenSettingsKey(registryKey);
            if (key == null) return null;

            var enabled = GetEnabled(key);
            if (!enabled.HasValue) return null;

            var mode = GetMode(key);
            if (!mode.HasValue) return null;

            var color = GetColor(key);
            if (!color.HasValue) return null;

            var settings = new Settings
            {
                Enabled = enabled.Value,
                Mode = mode.Value,
                Color = color.Value
            };

            return settings;
        }

        private static Color? GetColor(RegistryKey key)
        {
            var value = key.GetValue("Color") as string;
            int intValue;

            if (!int.TryParse(value, NumberStyles.HexNumber | NumberStyles.AllowHexSpecifier | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out intValue))
            {
                return null;
            }

            return Color.FromArgb((byte)(intValue >> 24),
                             (byte)(intValue >> 16),
                             (byte)(intValue >> 8),
                             (byte)(intValue));
        }

        private static AuraMode? GetMode(RegistryKey key)
        {
            var value = key.GetValue("Mode") as string;
            AuraMode result;

            if (!Enum.TryParse(value, true, out result))
            {
                return null;
            }
            return result;
        }

        private static bool? GetEnabled(RegistryKey key)
        {
            var value = key.GetValue("Enabled") as string;
            bool result;

            if (!bool.TryParse(value, out result))
            {
                return null;
            }
            return result;
        }

        #endregion
    }
}
