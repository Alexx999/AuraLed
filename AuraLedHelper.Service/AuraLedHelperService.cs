using System.ServiceProcess;

namespace AuraLedHelper.Service
{
    public partial class AuraLedHelperService : ServiceBase
    {
        public AuraLedHelperService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
        }

        protected override void OnStop()
        {
        }
    }
}
