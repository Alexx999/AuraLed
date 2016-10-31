using System;
using System.Globalization;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Media;
using Microsoft.Win32;

namespace AuraLedHelper.Core
{
    public class SettingsProvider
    {
        private const string ColorValueName = "Color";
        private const string ModeValueName = "Mode";
        private const string EnabledValueName = "Enabled";
        private const string RegistryKey = "SOFTWARE\\AsusLedHelper\\Settings";
        private ServiceProxy _proxy;

        private static Settings GetDefaults()
        {
            return new Settings {Enabled = false, Mode = AuraMode.Breathing, Color = Colors.Red};
        }

        public void ConnectToService()
        {
            _proxy = new ServiceProxy();
        }

        public Settings LoadDefaultSettings()
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

        public Settings LoadSettings(SettingsLocation location)
        {
            var key = GetKey(location);
            if (key != null)
            {
                return LoadFromRegistry(key);
            }
            return null;
        }

        public Settings LoadSettingsForUser(string sid)
        {
            var key = GetKeyForSid(sid);
            if (key != null)
            {
                return LoadFromRegistry(key);
            }
            return null;
        }

        private RegistryKey GetKeyForSid(string sid)
        {
            return Registry.Users.OpenSubKey(sid);
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

        private Settings LoadSettingsInternal()
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

        private static RegistryKey CreateSettingsKey(RegistryKey registryKey)
        {
            return registryKey.CreateSubKey(RegistryKey);
        }

        private static void DeleteSettingsKey(RegistryKey registryKey)
        {
            registryKey.DeleteSubKeyTree(RegistryKey);
        }

        public Task<bool> SaveSettingsAsync(Settings settings, SettingsLocation location)
        {
            var key = GetKey(location);
            RegistryKey settingsKey = null;
            bool hasAccess = true;
            try
            {
                settingsKey = CreateSettingsKey(key);
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
                hasAccess = false;
            }

            if (hasAccess)
            {
                SaveToRegistry(settings, settingsKey);
                return _proxy?.ReloadSettingsAsync() ?? Task.FromResult(false);
            }

            return _proxy?.ApplySettingsAsync(settings, location) ?? Task.FromResult(false);
        }

        public Task<bool> RemoveSettingsAsync(SettingsLocation location)
        {
            var key = GetKey(location);
            try
            {
                DeleteSettingsKey(key);
                return Task.FromResult(true);
            }
            catch (Exception ex) when (ex is SecurityException || ex is UnauthorizedAccessException)
            {
            }
            catch (ArgumentException)
            {
                //Subkey doesn't exist
                return Task.FromResult(true);
            }

            return _proxy?.ClearSettingsAsync(location) ?? Task.FromResult(false);
        }

        #region Writing
        
        private static void SaveToRegistry(Settings settings, RegistryKey settingsKey)
        {
            SaveEnabled(settings.Enabled, settingsKey);
            SaveColor(settings.Color, settingsKey);
            SaveMode(settings.Mode, settingsKey);
        }

        private static void SaveEnabled(bool value, RegistryKey key)
        {
            key.SetValue(EnabledValueName, value.ToString(CultureInfo.InvariantCulture), RegistryValueKind.String);
        }

        private static void SaveMode(AuraMode value, RegistryKey key)
        {
            key.SetValue(ModeValueName, value.ToString(), RegistryValueKind.String);
        }

        private static void SaveColor(Color value, RegistryKey key)
        {
            var intValue = (value.A << 24) | (value.R << 16) | (value.G << 8) | value.B;

            key.SetValue(ColorValueName, intValue.ToString("X", CultureInfo.InvariantCulture));
        }

        #endregion

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
            var value = key.GetValue(ColorValueName) as string;
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
            var value = key.GetValue(ModeValueName) as string;
            AuraMode result;

            if (!Enum.TryParse(value, true, out result))
            {
                return null;
            }
            return result;
        }

        private static bool? GetEnabled(RegistryKey key)
        {
            var value = key.GetValue(EnabledValueName) as string;
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
