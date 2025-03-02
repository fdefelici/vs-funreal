using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Microsoft.VisualStudio.Threading;
using System.Security.Cryptography.X509Certificates;
using stdole;

namespace FUnreal
{
    public class XProcessResult
    {
        public XProcessResult() { }
        public XProcessResult(int exitCode, string stdOut, bool isSuccess)
        {
            ExitCode = exitCode;
            StdOut = stdOut;
            IsSuccess = isSuccess;
        }

        public int ExitCode { get; internal set; }
        public string StdOut { get; internal set; }
        public bool IsSuccess { get; internal set; }
        public bool IsError { get { return !IsSuccess; } }
    }

    public interface IXProcess
    {
        Task<XProcessResult> RunAsync(string binaryPath, string[] args);
    }

    public class XProcess : IXProcess
    {
        public static async Task<XProcessResult> ExecAsync(string binaryPath, string[] args)
        {
            StringBuilder argsBuilder = new StringBuilder();
            foreach (string arg in args) argsBuilder.Append(arg).Append(" ");
            string argsString = argsBuilder.ToString();

            Process process = new Process();
            process.StartInfo.FileName = binaryPath;
            process.StartInfo.Arguments = argsString;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            XDebug.Info("Executing: " + binaryPath + " " + argsString);

            int code;
            string stdout;
            try { 
                process.Start();
            
                Task<string> outputTask = process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                code = process.ExitCode;
                #pragma warning disable VSTHRD103
                stdout = outputTask.Result; //skip wait warning because of process.WaitForExit() called before
                #pragma warning restore VSTHRD103
            } catch (Exception e)
            {
                return new XProcessResult()
                {
                    StdOut = e.ToString(),
                    ExitCode = -1,
                    IsSuccess = false
                };
            }

            XProcessResult result = new XProcessResult();
            result.StdOut = stdout;
            result.ExitCode = process.ExitCode;
            result.IsSuccess = code == 0;

            XDebug.Info($"Result: {binaryPath} [Success: {result.IsSuccess}][Code: {result.ExitCode}]");

            return result;
        }

        public async Task<XProcessResult> RunAsync(string binaryPath, string[] args)
        {
            return await XProcess.ExecAsync(binaryPath, args);
        }
    }
}
