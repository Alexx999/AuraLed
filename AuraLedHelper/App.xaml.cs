using System;
using System.ComponentModel;
using System.IO.Pipes;
using System.Reflection;
using System.Security.Principal;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using AuraLedHelper.Core.Extensions;

namespace AuraLedHelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private FieldInfo _fieldInfo;
        private readonly Guid _singleInstanceId = new Guid("19aaf8c3-6479-421d-91cc-02ed805cde5e");
        private Mutex _mutex;
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public App()
        {
            var actualValue = DesignerProperties.IsInDesignModeProperty.DefaultMetadata.DefaultValue;
            UpdateDesignerMode(true);
            CheckLicense(Xceed.Wpf.Toolkit.Licenser.LicenseKey);
            UpdateDesignerMode((bool)actualValue);

            TextOptions.TextFormattingModeProperty.OverrideMetadata(typeof(DependencyObject),
                new FrameworkPropertyMetadata(TextFormattingMode.Display, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.Inherits));
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            CheckSingleInstance();
            var service = FirstFloor.XamlSpy.Services.XamlSpyService.Current;
            service.Connect("127.0.0.1", 4530, "01311");
        }

        private void CheckSingleInstance()
        {
            var mutexId = _singleInstanceId.ToString();
            var userId = WindowsIdentity.GetCurrent().User?.Value;
            if (userId != null)
            {
                mutexId += userId;
            }

            bool created;
            var mutex = new Mutex(true, mutexId, out created);
            if (created)
            {
                _mutex = mutex;
                RunMessagePumpAsync();
            }
            else
            {
                SendSignal();
                Shutdown();
            }
        }

        private async void RunMessagePumpAsync()
        {
            var buffer = new byte[1];
            while (!_cts.IsCancellationRequested)
            {
                var s = new NamedPipeServerStream(_singleInstanceId.ToString(), PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                await s.WaitForConnectionAsync();
                await s.ReadAsync(buffer, 0, 1, _cts.Token);
                s.Close();
                Activate();
            }
        }

        private void Activate()
        {
            if (MainWindow == null)
            {
                Shutdown();
                return;
            }
            MainWindow.Activate();
            MainWindow.Focus();
            MainWindow.WindowState = WindowState.Normal;
        }

        private void SendSignal()
        {
            var s = new NamedPipeClientStream(".", _singleInstanceId.ToString(), PipeDirection.Out);
            s.Connect();
            s.WriteByte(1);
            s.Close();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _mutex?.ReleaseMutex();
            _cts.Cancel();
            base.OnExit(e);
        }

        private void CheckLicense(string licenseKey)
        {
            //Sorry - I don't think that using XAML inspectors requires "Plus" but due to bug in licensing it won't work without this hack
        }

        private void UpdateDesignerMode(bool value)
        {
            var metadata = DesignerProperties.IsInDesignModeProperty.DefaultMetadata;
            var field = _fieldInfo ?? (_fieldInfo = typeof(PropertyMetadata).GetField("_defaultValue", BindingFlags.Instance | BindingFlags.NonPublic));
            field?.SetValue(metadata, value);
        }
    }
}
