using System.Configuration;
using System.Diagnostics;
using System.Threading;

namespace DockerDesktopWatcher.App
{
    internal class Program
    {
        static void Main()
        {
            try
            {
                var result = ProcessHelper.ExecuteProcessAndReadStdOut("docker", out string errrorOutput, "info", timeoutSeconds: 30);

                if (errrorOutput.Contains("ERROR: error during connect:"))
                {
                    RestartDockerActual();
                }
                else
                {
                    var dockerProcesses = Process.GetProcessesByName("Docker Desktop");
                    if (dockerProcesses == null || dockerProcesses.Length == 0)
                    {
                        RestartDockerActual();
                    }
                }
            }
            catch
            {
                RestartDockerActual();
            }
        }

        private static void RestartDockerActual()
        {
            ProcessHelper.KillProcess("Docker Desktop Backend");
            ProcessHelper.KillProcess("com.docker.backend");
            ProcessHelper.KillProcess("com.docker.diagnose");
            ProcessHelper.KillProcess("com.docker.build");
            ProcessHelper.KillProcess("com.docker.dev-envs");
            ProcessHelper.KillProcess("Docker Desktop");

            Thread.Sleep(30000); // 30 seconds

            try
            {
                Process.Start(Program.DockerDesktopPath);
            }
            catch { }
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
