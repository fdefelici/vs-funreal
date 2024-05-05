using Microsoft.VisualStudio.TestTools.UnitTesting;

using FUnreal;
using System.Collections.Generic;
using System.Threading;
using System.IO;

namespace FUnrealTest
{
    [TestClass]
    public class FUnrealService_CreateTest
    {
        string tmpPath;

        [TestInitialize]
        public void SetUp()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealService_CreateTest");
            tmpPath = TestUtils.AbsPath("FUnrealService_CreateTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);
        }

        [TestCleanup]
        public void TearDown()
        {
            TestUtils.DeleteDir(tmpPath);
        }

        class LoggerMock : IFUnrealLogger
        {
            public void Erro(string format, params string[] args) { }
            public void ForceFocus() { }
            public void Info(string format, params string[] args) { }
            public void PlainText(string str) { }
            public void Warn(string format, params string[] args) { }
        }

        class FUnrealVsMock : IFUnrealVS
        {
            private string _uprojectFilePath;
            private string _engineRootPath;
            private string _vsixDllPath;

            public FUnrealVsMock(
                string uprojectFilePath,
                string engineRootPath,
                string vsixDllPath)
            {
                _uprojectFilePath = uprojectFilePath;
                _engineRootPath = engineRootPath;
                _vsixDllPath = vsixDllPath;
                Output = new LoggerMock();
            }

            //public IFUnrealLogger Output { get; private set; }

            public override FUnrealTemplateOptionsPage GetOptions()
            {
                throw new System.NotImplementedException();
            }

            public override string GetUnrealEnginePath()
            {
                return _engineRootPath;
            }

            public override string GetUProjectFilePath()
            {
                return _uprojectFilePath;
            }

            public override string GetVSixDllPath()
            {
                return _vsixDllPath;
            }
        }

        [TestMethod]
        public void CreateForUE_v4_27_2()
        {
            string uprojectFilePath = TestUtils.PathCombine(tmpPath, "Projects/UE4Prj/UE4Prj.uproject");
            string engineRootPath = TestUtils.PathCombine(tmpPath, "Engines/UE_4.27/Engine");
            string vsixDllPath = TestUtils.PathCombine(tmpPath, "faked-funreal.dll");

            var expVersion = new XVersion(4, 27, 2);
            string expUbtPath = TestUtils.PathCombine(engineRootPath, "Binaries/DotNET/UnrealBuildTool.exe");

            var service = FUnrealService.Create(new FUnrealVsMock(uprojectFilePath, engineRootPath, vsixDllPath));

            var engine = service.Engine;
            Assert.AreEqual(expVersion, engine.Version);
            Assert.AreEqual(expUbtPath, engine.UnrealBuildTool.BinPath);
        }

        [TestMethod]
        public void CreateForUE_v5_1_0()
        {
            string uprojectFilePath = TestUtils.PathCombine(tmpPath, "Projects/UE5Prj/UE5Prj.uproject");
            string engineRootPath = TestUtils.PathCombine(tmpPath, "Engines/UE_5.1/Engine");
            string vsixDllPath = TestUtils.PathCombine(tmpPath, "faked-funreal.dll");

            var expVersion = new XVersion(5, 1, 0);
            string expUbtPath = TestUtils.PathCombine(engineRootPath, "Binaries/DotNET/UnrealBuildTool/UnrealBuildTool.exe");

            var service = FUnrealService.Create(new FUnrealVsMock(uprojectFilePath, engineRootPath, vsixDllPath));

            var engine = service.Engine;
            Assert.AreEqual(expVersion, engine.Version);
            Assert.AreEqual(expUbtPath, engine.UnrealBuildTool.BinPath);
        }

    }
}