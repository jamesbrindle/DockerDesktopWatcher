using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Timers;

namespace DockerDesktopWatcher
{
    public partial class Service : ServiceBase
    {
        private System.Timers.Timer timer = new System.Timers.Timer();

        public Service()
        {
            InitializeComponent();
        }

        public void Test()
        {
            CheckAndRestartDocker();

            OnStart(new string[] { "test" });
            while (true)
                Thread.Sleep(100);
        }

        protected override void OnStart(string[] args)
        {
            timer.Elapsed += new ElapsedEventHandler(OnTimedEvent);

            if (args != null && args.Length > 0 && !string.IsNullOrWhiteSpace(args[0]) && args[0] == "test")
                timer.Interval = 60000; // 60 seconds
            else
                timer.Interval = Program.IntervalMinutes * 60 * 1000;

            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Start();
        }

        private static void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            CheckAndRestartDocker();
        }

        private static void CheckAndRestartDocker()
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

        protected override void OnStop()
        {
            try
            {
                timer.Stop();
            }
            catch { }

            try
            {
                timer.Enabled = false;
            }
            catch { }
        }
    }
}
