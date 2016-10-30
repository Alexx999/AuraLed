using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using AuraLedHelper.Core;
using Prism.Commands;

namespace AuraLedHelper
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private AuraMode _operationMode;
        private bool _enabled;
        private Color _color;
        private bool _hasError;
        private string _errorMessage;

        public MainViewModel()
        {
            ApplyCommand = new DelegateCommand<SettingsLocation?>(Apply);
            LoadCommand = new DelegateCommand<SettingsLocation?>(Load);
            ClearCommand = new DelegateCommand<SettingsLocation?>(Clear);
            LoadData();
        }

        #region Properties

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                _enabled = value;
                OnPropertyChanged();
            }
        }

        public AuraMode OperationMode
        {
            get { return _operationMode; }
            set
            {
                _operationMode = value;
                OnPropertyChanged();
            }
        }

        public Color Color
        {
            get { return _color; }
            set
            {
                _color = value;
                OnPropertyChanged();
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        public bool HasError
        {
            get { return _hasError; }
            set
            {
                _hasError = value;
                OnPropertyChanged();
            }
        }

        public ICommand ApplyCommand { get; }
        public ICommand LoadCommand { get; }
        public ICommand ClearCommand { get; }

        #endregion

        private void Reset()
        {
            var settings = SettingsProvider.LoadDefaultSettings();
            ApplySettings(settings);
        }

        private void ApplySettings(Settings settings)
        {
            Enabled = settings.Enabled;
            OperationMode = settings.Mode;
            Color = settings.Color;
        }

        private void Apply(SettingsLocation? location)
        {
            ClearError();
            if (!location.HasValue) return;
            var settings = new Settings
            {
                Enabled = Enabled,
                Mode = OperationMode,
                Color = Color
            };

            bool success;
            try
            {
                success = SettingsProvider.SaveSettings(settings, location.Value);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex);
                ShowError($"Error saving {location.Value} settings");
                return;
            }

            if (!success)
            {
                ShowError($"Removing {location.Value} failed");
            }
        }

        private void ClearError()
        {
            HasError = false;
            ErrorMessage = string.Empty;
        }

        private void Load(SettingsLocation? location)
        {
            ClearError();
            if (!location.HasValue) return;

            Settings settings;
            try
            {
                settings = SettingsProvider.LoadSettings(location.Value);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex);
                ShowError($"Error loading {location.Value} settings");
                return;
            }
            if (settings == null)
            {
                ShowError($"No {location.Value} settings found");
                return;
            }
            ApplySettings(settings);
        }

        private void Clear(SettingsLocation? location)
        {
            ClearError();
            if (!location.HasValue) return;

            bool success;
            try
            {
                success = SettingsProvider.RemoveSettings(location.Value);
            }
            catch (Exception ex)
            {
                LogHelper.LogError(ex);
                ShowError($"Error removing {location.Value} settings");
                return;
            }

            if (!success)
            {
                ShowError($"Removing {location.Value} failed");
            }
        }

        private void LoadData()
        {
            Reset();
        }

        private void ShowError(string text)
        {
            ErrorMessage = text;
            HasError = true;
        }


        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
