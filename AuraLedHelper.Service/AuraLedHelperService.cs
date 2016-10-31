using System;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace AuraLedHelper.Service
{
    public partial class AuraLedHelperService : ServiceBase
    {
        private ServiceCore _core;

        public AuraLedHelperService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            _core = new ServiceCore();
            _core.StartServiceCore();
            _core.LoadSettings();
        }

        protected override void OnStop()
        {
            _core.Dispose();
        }

        protected override void OnSessionChange(SessionChangeDescription cd)
        {
            base.OnSessionChange(cd);

            _core.UserChange();
        }
    }
}
