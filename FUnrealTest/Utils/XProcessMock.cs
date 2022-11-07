using FUnreal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    internal class XProcessMock : IXProcess
    {
        private bool _isSuccess;
        private int _waitMillis;

        public string BinaryPath { get; private set; }
        public string[] Args { get; private set; }

        public XProcessMock(bool isSuccess, int waitMillis = 0) 
        {
            _isSuccess = isSuccess;
            _waitMillis = waitMillis;
        }

        public async Task<XProcessResult> RunAsync(string binaryPath, string[] args)
        {
            await Task.Run(() =>
            {
                Thread.Sleep(_waitMillis);

                BinaryPath = binaryPath;
                Args = args;
            });

            return new XProcessResult(0, "", _isSuccess);
        }
    }
}
