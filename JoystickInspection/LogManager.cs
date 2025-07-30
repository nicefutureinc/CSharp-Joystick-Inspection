using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace JoystickInspection
{
    public class LogManager
    {
        private string logDirectory;
        private string filePath;

        // Set the default log directory in the constructor.
        public LogManager()
        {
            // Getting the path of the executable file
            string exePath = Application.StartupPath;

            logDirectory = Path.Combine(exePath, "logs");

            string directoryPath = GetLogDirectoryPath();
            filePath = GetLogFilePath();

            // Create directory if it does not exist
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
        }

        // Method for recording logs
        public void Log(string message)
        {
            // Log messages to a log file
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {message}");
            }
        }

        public void Log(string kind, byte[] nData, int len)
        {
            // Log messages to a log file
            using (StreamWriter writer = new StreamWriter(filePath, true))
            {
                string str2 = "";

                if (len > 0)
                {
                    str2 += (nData[0].ToString("X2") + " ");
                    for (int j = 1; j < len; j++)
                    {
                        str2 += (nData[j].ToString("X2") + " ");
                    }
                }

                writer.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {kind} {str2}");
            }
        }


        // Create directory paths by year and month
        private string GetLogDirectoryPath()
        {
            string year = DateTime.Now.Year.ToString();
            string month = DateTime.Now.Month.ToString("D2"); // Two-digit format (01, 02, ..., 12)

            // Set the log directory path to year/month
            return Path.Combine(logDirectory, year, month);
        }

        // Create log file paths based on execution time
        private string GetLogFilePath()
        {
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string directoryPath = GetLogDirectoryPath();
            return Path.Combine(directoryPath, $"log_{timestamp}.txt");
        }
    }
}
    