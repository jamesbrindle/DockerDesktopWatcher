using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace DockerDesktopWatcher
{
    internal class ProcessHelper
    {
        /// <summary>
        /// Executes a process and reads the output (std out and error out) - Useful for working with command line utilties in managed code
        /// </summary>
        /// <param name="path">Path of program to execute</param>
        /// <param name="arguments">Optionally include and command line arguments</param>
        /// <param name="workingDirectory">Working folder path</param>
        /// <param name="timeoutSeconds">End process timeout. Defaults to 30 seconds. Set to -1 for no timeout (infinite)</param>
        /// <param name="throwOnError">Should the method throw an error when exception caught or just ignore</param>
        /// <returns>Return STD + Err output</returns>
        internal static string ExecuteProcessAndReadStdOut(
            string path,
            out string errorOutput,
            string arguments = "",
            string workingDirectory = "",
            int timeoutSeconds = 60,
            bool throwOnError = true)
        {
            var _outputStringBuilder = new StringBuilder();
            var _errorStringBuilder = new StringBuilder();
            int timeoutInMs = timeoutSeconds * 1000;

            var process = new Process();

            try
            {
                process.StartInfo.FileName = path;
                process.StartInfo.Arguments = arguments;
                process.StartInfo.WorkingDirectory = string.IsNullOrEmpty(workingDirectory) ? Path.GetDirectoryName(path) : workingDirectory;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.UseShellExecute = false;

                using (var outputWaitHandle = new AutoResetEvent(false))
                using (var errorWaitHandle = new AutoResetEvent(false))
                {
                    void outputHandler(object sender, DataReceivedEventArgs e)
                    {
                        if (e.Data == null)
                            outputWaitHandle.Set();
                        else
                            _outputStringBuilder.Append(e.Data);
                    }

                    void errorHandler(object sender, DataReceivedEventArgs e)
                    {
                        if (e.Data == null)
                            errorWaitHandle.Set();
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(e.Data) && !e.Data.Contains("diacritics") &&
                                !string.IsNullOrWhiteSpace(e.Data) && !e.Data.Contains("symbol") &&
                                !string.IsNullOrWhiteSpace(e.Data) && !e.Data.Contains("ZapfDingbats"))
                                _errorStringBuilder.Append(e.Data);
                        }
                    }

                    process.OutputDataReceived += outputHandler;
                    process.ErrorDataReceived += errorHandler;

                    try
                    {
                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();

                        if (timeoutSeconds == -1)
                        {
                            process.WaitForExit();
                            _errorStringBuilder.Append("Exit: " + process.ExitCode);
                        }
                        else
                        {
                            if (process.WaitForExit(timeoutInMs) &&
                                outputWaitHandle.WaitOne(timeoutInMs) &&
                                errorWaitHandle.WaitOne(timeoutInMs))
                            {
                                if (process.ExitCode != 0)
                                    _errorStringBuilder.Append("Exit: " + process.ExitCode);
                            }
                            else
                            {
                                if (!process.HasExited)
                                {
                                    process.Kill();
                                    throw new Exception("ERROR: Process took too long to finish");
                                }
                            }
                        }
                    }
                    finally
                    {
                        process.OutputDataReceived -= outputHandler;
                        process.ErrorDataReceived -= errorHandler;
                    }
                }
            }
            catch (Exception e)
            {
                if (throwOnError)
                    throw new Exception("Execution error: " + e.Message);
            }
            finally
            {
                process.Close();
                try
                {
                    process.Kill();
                }
                catch { }
            }

            errorOutput = _errorStringBuilder.ToString();
            return _outputStringBuilder.ToString();
        }

        internal static void KillProcess(string processName)
        {
            var processes = Process.GetProcessesByName(processName);
            if (processes != null && processes.Length > 0)
            {
                foreach (var process in processes)
                {
                    try
                    {
                        process.Kill();
                    }
                    catch { }
                }

                Thread.Sleep(2000);
            }
        }
    }
}
