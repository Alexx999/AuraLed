﻿using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows.Media;
using AuraLedHelper.Core;
using GalaSoft.MvvmLight.CommandWpf;

namespace AuraLedHelper
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private AuraMode _operationMode;
        private bool _enabled;
        private Color _color;

        public MainViewModel()
        {
            ApplyCommand = new RelayCommand(Apply);
            ResetCommand = new RelayCommand(Reset);
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

        public ICommand ApplyCommand { get; }

        public ICommand ResetCommand { get; }

        #endregion
        
        private void Reset()
        {
            throw new NotImplementedException();
        }

        private void Apply()
        {
            throw new NotImplementedException();
        }

        private void LoadData()
        {
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
