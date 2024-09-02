using System;
using System.Configuration;
using System.ServiceProcess;

namespace DockerDesktopWatcher
{
    static class Program
    {
        public static void Main()
        {
            // DEBUG
            //new Service().Test();

            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new Service()
            };
            ServiceBase.Run(ServicesToRun);
        }

        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }

        public static int IntervalMinutes
        {
            get
            {
                try
                {
                    return Convert.ToInt32(ConfigurationManager.AppSettings["IntervalMinutes"]);
                }
                catch
                {
                    return 5;
                }
            }
        }

        public static string DockerDesktopPath
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["DockerDesktopPath"];
                }
                catch
                {
                    return @"C:\Program Files\Docker\Docker\Docker Desktop.exe";
                }
            }
        }
    }
}
