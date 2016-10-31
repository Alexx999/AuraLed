using System;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace AuraLedHelper.Service
{
    public partial class AuraLedHelperService : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);
        
        private ServiceCore _core;

        public AuraLedHelperService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 50;
            SetServiceStatus(ServiceHandle, ref serviceStatus);

            _core = new ServiceCore();
            _core.StartServiceCore();

            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(ServiceHandle, ref serviceStatus);
        }

        protected override void OnStop()
        {
        }
    }
}
