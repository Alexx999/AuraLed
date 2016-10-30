using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace AsusLedHelperService
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
