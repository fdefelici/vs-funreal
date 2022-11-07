using FUnreal;
using Microsoft.VisualStudio.Threading;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace FUnrealTest
{
    [TestClass]
    public class FUnrealBuildToolTest
    {
        [TestMethod]
        public void SuccessfulExecution()
        {
            XProcessMock mock = new XProcessMock(true, 20);
            
            FUnrealBuildTool ubt = new FUnrealBuildTool("engine/bin/ubt/ubt.exe", mock);

            XProcessResult result = ubt.GenerateVSProjectFilesAsync("file.uproject").GetAwaiter().GetResult();

            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual("engine/bin/ubt/ubt.exe", mock.BinaryPath);
            Assert.AreEqual(4,  mock.Args.Count());
            Assert.AreEqual("-projectFiles", mock.Args[0]);
            Assert.AreEqual("-project=\"file.uproject\"", mock.Args[1]);
            Assert.AreEqual("-game", mock.Args[2]);
            Assert.AreEqual("-rocket", mock.Args[3]);
        }

        [TestMethod]
        public void FailedExecution()
        {
            XProcessMock mock = new XProcessMock(false, 20);

            FUnrealBuildTool ubt = new FUnrealBuildTool("engine/bin/ubt/ubt.exe", mock);

            XProcessResult result = ubt.GenerateVSProjectFilesAsync("file.uproject").GetAwaiter().GetResult();

            Assert.IsFalse(result.IsSuccess);
            Assert.IsTrue(result.IsError);
            Assert.AreEqual("engine/bin/ubt/ubt.exe", mock.BinaryPath);
            Assert.AreEqual(4, mock.Args.Count());
            Assert.AreEqual("-projectFiles", mock.Args[0]);
            Assert.AreEqual("-project=\"file.uproject\"", mock.Args[1]);
            Assert.AreEqual("-game", mock.Args[2]);
            Assert.AreEqual("-rocket", mock.Args[3]);
        }

        [TestMethod]
        public void Integration_WrongCommand()
        {
            FUnrealBuildTool ubt = new FUnrealBuildTool("UNEXISTENT.exe");

            XProcessResult result = ubt.GenerateVSProjectFilesAsync("file.uproject").GetAwaiter().GetResult();

            Assert.IsTrue(result.IsError);
        }


    }
}
