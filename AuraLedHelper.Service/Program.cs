using System.ServiceProcess;

namespace AuraLedHelper.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new AuraLedHelperService()
            };
            ServiceBase.Run(ServicesToRun);

            /*
            axdata axdata = new axdataClass();
            axdata.iAcpiSetItem(0x13060041, 0, 1);
            axdata.iAcpiSetItem(0x13060042, 4, 1);*/
        }
    }
}
