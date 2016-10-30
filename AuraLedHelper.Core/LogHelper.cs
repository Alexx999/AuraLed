using System;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace AuraLedHelper.Core
{
    public static class LogHelper
    {
        public static void LogError(Exception ex)
        {
            try
            {
                LogToFolder(GetLogPath(), ex);
            }
            catch (Exception userLogEx)
            {
                try
                {
                    LogToFolder(Path.GetTempPath(), userLogEx);
                    LogToFolder(Path.GetTempPath(), ex);
                }
                catch (Exception logEx)
                {
                    MessageBox.Show($"Attemts to write log failed. Error that was going to log:\n{ex}\nError that caused log file failure:\n{logEx}", "AuraLedHelper Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

        }

        private static void LogToFolder(string path, Exception ex)
        {
            Directory.CreateDirectory(path);

            var filePath = Path.Combine(path, "AuraLedHelper.log");

            using (var file = File.AppendText(filePath))
            {
                file.WriteLine(DateTime.UtcNow.ToString(CultureInfo.InvariantCulture));
                file.WriteLine(ex.ToString());
            }
        }

        private static string GetLogPath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create), "AuraLedHelper\\");
        }
    }
}
