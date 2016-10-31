using System;
using System.Windows.Media;
using atkexComSvc;
using AuraLedHelper.Core;

namespace AuraLedHelper.Service
{
    class AuraController
    {
        public static void ApplySettings(Settings settings)
        {
            if(Environment.MachineName == "MAINPC") return;

            if (!settings.Enabled)
            {
                DisableAura();
            }

            switch (settings.Mode)
            {
                case AuraMode.Static:
                {
                    SetStaticColor(settings.Color);
                    break;
                }
                case AuraMode.Breathing:
                {
                    SetBreathing(settings.Color);
                    break;
                }
                case AuraMode.Strobing:
                {
                    SetStrobing(settings.Color);
                    break;
                }
                case AuraMode.Cycle:
                {
                    EnableColorCycle();
                    break;
                }
            }
        }

        private static void SetStrobing(Color color)
        {
        }

        private static void SetBreathing(Color color)
        {
        }

        private static void SetStaticColor(Color color)
        {
            axdata axdata = new axdataClass();
            axdata.iAcpiSetItem(0x13060041, 0, 1);

            var intValue = (color.R << 8) | (color.G << 16) | (color.B << 24) | 1;
            axdata.iAcpiSetItem(0x13060042, (uint) intValue, 1);
        }

        private static void EnableColorCycle()
        {
            axdata axdata = new axdataClass();
            axdata.iAcpiSetItem(0x13060041, 0, 1);
            axdata.iAcpiSetItem(0x13060042, 4, 1);
        }

        private static void DisableAura()
        {
        }
    }
}
