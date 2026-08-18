using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chris_602473_Prg281_Proj
{
    public static class SystemLogger                 // Records important station operations, warnings and errors in a
    {                                                // text file. This also provides the project's logging bonus feature.
        private static readonly string LogFile = "station_log.txt";

        public static void Log(string message)
        {
            try
            {
                string entry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";

                File.AppendAllText(LogFile, entry + Environment.NewLine);
            }
            catch (IOException)
            {
                // Logging should never be allowed to crash the station.
            }
        }

        public static void ShowLogs()
        {
            Console.WriteLine();
            Console.WriteLine(
                "============================================================"
            );
            Console.WriteLine(
                "                       SYSTEM LOGS"
            );
            Console.WriteLine(
                "============================================================"
            );

            try
            {
                if (!File.Exists(LogFile))
                {
                    Console.WriteLine("No system logs have been created yet.");
                    return;
                }

                string[] logs = File.ReadAllLines(LogFile);

                if (logs.Length == 0)
                {
                    Console.WriteLine("The system log is currently empty.");
                    return;
                }

                foreach (string log in logs)
                {
                    Console.WriteLine(log);
                }
            }
            catch (IOException ex)
            {
                Console.WriteLine("Unable to read system logs: " + ex.Message);
            }
        }
    }
}
