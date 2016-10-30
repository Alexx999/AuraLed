using System;
using System.Globalization;
using System.Linq;
using System.Windows.Media;
using Microsoft.Win32;

namespace AuraLedHelper.Core
{
    public static class SettingsProvider
    {
        private const string RegistryKey = "Software\\AsusLedHelper\\Settings";

        public static Settings GetDefaults()
        {
            return new Settings {Enabled = true, Mode = AuraMode.Static, Color = Colors.Red};
        }

        public static Settings LoadSettings()
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

        private static Settings LoadSettingsInternal()
        {
            if (HasRegistry(Registry.CurrentUser))
            {
                return LoadFromRegistry(Registry.CurrentUser);
            }
            if (HasRegistry(Registry.LocalMachine))
            {
                return LoadFromRegistry(Registry.LocalMachine);
            }

            return GetDefaults();
        }

        private static Settings LoadFromRegistry(RegistryKey registryKey)
        {
            var key = registryKey.OpenSubKey(RegistryKey);
            var settings = new Settings
            {
                Enabled = GetEnabled(key),
                Mode = GetMode(key),
                Color = GetColor(key)
            };
            return settings;
        }

        private static Color GetColor(RegistryKey key)
        {
            var value = key.GetValue("Color") as string;
            int intValue;

            if (!int.TryParse(value, NumberStyles.HexNumber | NumberStyles.AllowHexSpecifier | NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite, CultureInfo.InvariantCulture, out intValue))
            {
                return GetDefaults().Color;
            }

            return Color.FromArgb((byte)(intValue >> 24),
                             (byte)(intValue >> 16),
                             (byte)(intValue >> 8),
                             (byte)(intValue));
        }

        private static AuraMode GetMode(RegistryKey key)
        {
            var value = key.GetValue("Mode") as string;
            AuraMode result;

            if (!Enum.TryParse(value, true, out result))
            {
                return GetDefaults().Mode;
            }
            return result;
        }

        private static bool GetEnabled(RegistryKey key)
        {
            var value = key.GetValue("Enabled") as string;
            bool result;

            if (!bool.TryParse(value, out result))
            {
                return GetDefaults().Enabled;
            }
            return result;
        }

        private static bool HasRegistry(RegistryKey registryKey)
        {
            return GetSettingsKey(registryKey) != null;
        }

        private static RegistryKey GetSettingsKey(RegistryKey registryKey)
        {
            return registryKey.OpenSubKey(RegistryKey);
        }

        public static void SaveSettings(Settings settings)
        {
            
        }
    }
}
