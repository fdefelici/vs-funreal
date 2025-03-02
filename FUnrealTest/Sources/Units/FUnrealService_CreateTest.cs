using Microsoft.VisualStudio.TestTools.UnitTesting;
using FUnreal;
using System;

namespace FUnrealTest
{
    [TestClass]
    public class FUnrealService_CreateTest
    {
        string rootPath;

        [TestInitialize]
        public void SetUp()
        {
            rootPath = TestUtils.AbsPath("Resources/FUnrealService_CreateTest");
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
            private FUnrealUEProjectType _projectType;
            private FUnrealTemplateOptionsPage _options;

            public FUnrealVsMock(
                string uprojectFilePath,
                string engineRootPath,
                string vsixDllPath,
                FUnrealUEProjectType projectType,
                FUnrealTemplateOptionsPage options)
            {
                _uprojectFilePath = uprojectFilePath;
                _engineRootPath = engineRootPath;
                _vsixDllPath = vsixDllPath;
                _projectType = projectType;
                _options = options;
                Output = new LoggerMock();
            }

            //public IFUnrealLogger Output { get; private set; }

            public override FUnrealTemplateOptionsPage GetOptions()
            {
                return _options;
            }

            public override FUnrealUEProjectType GetUEProjectType()
            {
                return _projectType;
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
        public void CreateFor_ForeignProject_And_UE_v4_27_2()
        {
            string scenarioPath = TestUtils.PathCombine(rootPath, "Scenario_Foreign");
            string uprojectFilePath = TestUtils.PathCombine(scenarioPath, "Projects/UE4Prj/UE4Prj.uproject");
            string engineRootPath = TestUtils.PathCombine(scenarioPath, "Engines/UE_4.27/Engine");
            string vsixDllPath = TestUtils.PathCombine(rootPath, "faked-funreal.dll"); //faked dll, just used to get is base path and locate templates

            var expVersion = new XVersion(4, 27, 2);
            var expUbtPath = TestUtils.PathCombine(engineRootPath, "Binaries/DotNET/UnrealBuildTool.exe");
            var options = new FUnrealTemplateOptionsPage();
            options.TemplatesMode = TemplateMode.BuiltIn;

            var service = FUnrealService.Create(new FUnrealVsMock(uprojectFilePath, engineRootPath, vsixDllPath, FUnrealUEProjectType.Foreign, options));

            var engine = service.Engine;
            Assert.AreEqual(expVersion, engine.Version);
            Assert.AreEqual(expUbtPath, engine.UnrealBuildTool.BinPath);

            var plugins = service.PluginTemplates();
            Assert.AreEqual(2, service.PluginTemplates().Count);
            Assert.AreEqual("builtin_plugin_0", plugins[0].Name);
       
            Assert.AreEqual(1, service.PluginModuleTemplates().Count);
            Assert.AreEqual(1, service.GameModuleTemplates().Count);
            Assert.AreEqual(2, service.SourceTemplates().Count);
        }

        [TestMethod]
        public void CreateFor_ForeignProject_And_UE_v5_1_0()
        {
            string scenarioPath = TestUtils.PathCombine(rootPath, "Scenario_Foreign");
            string uprojectFilePath = TestUtils.PathCombine(scenarioPath, "Projects/UE5Prj/UE5Prj.uproject");
            string engineRootPath = TestUtils.PathCombine(scenarioPath, "Engines/UE_5.1/Engine");
            string vsixDllPath = TestUtils.PathCombine(rootPath, "faked-funreal.dll");

            var expVersion = new XVersion(5, 1, 0);
            var expUbtPath = TestUtils.PathCombine(engineRootPath, "Binaries/DotNET/UnrealBuildTool/UnrealBuildTool.exe");
            var options = new FUnrealTemplateOptionsPage();
            options.TemplatesMode = TemplateMode.BuiltIn;

            var service = FUnrealService.Create(new FUnrealVsMock(uprojectFilePath, engineRootPath, vsixDllPath, FUnrealUEProjectType.Foreign, options));

            var engine = service.Engine;
            Assert.AreEqual(expVersion, engine.Version);
            Assert.AreEqual(expUbtPath, engine.UnrealBuildTool.BinPath);

            var plugins = service.PluginTemplates();
            Assert.AreEqual(2, service.PluginTemplates().Count);
            Assert.AreEqual("builtin_plugin_0", plugins[0].Name);

            Assert.AreEqual(1, service.PluginModuleTemplates().Count);
            Assert.AreEqual(1, service.GameModuleTemplates().Count);
            Assert.AreEqual(2, service.SourceTemplates().Count);
        }

        [TestMethod]
        public void CreateFor_NativeProject_And_UE_v5_1_0()
        {
            string scenarioPath = TestUtils.PathCombine(rootPath, "Scenario_Native");
            string uprojectFilePath = TestUtils.PathCombine(scenarioPath, "UE_5.1/UE5Prj/UE5Prj.uproject");
            string engineRootPath = TestUtils.PathCombine(scenarioPath, "UE_5.1/Engine");
            string vsixDllPath = TestUtils.PathCombine(rootPath, "faked-funreal.dll");

            var expVersion = new XVersion(5, 1, 0);
            var expUbtPath = TestUtils.PathCombine(scenarioPath, "UE_5.1/GenerateProjectFiles.bat");
            var options = new FUnrealTemplateOptionsPage();
            options.TemplatesMode = TemplateMode.BuiltIn;

            var service = FUnrealService.Create(new FUnrealVsMock(uprojectFilePath, engineRootPath, vsixDllPath, FUnrealUEProjectType.Native, options));

            var engine = service.Engine;
            Assert.AreEqual(expVersion, engine.Version);
            Assert.AreEqual(expUbtPath, engine.UnrealBuildTool.BinPath);

            var plugins = service.PluginTemplates();
            Assert.AreEqual(2, service.PluginTemplates().Count);
            Assert.AreEqual("builtin_plugin_0", plugins[0].Name);

            Assert.AreEqual(1, service.PluginModuleTemplates().Count);
            Assert.AreEqual(1, service.GameModuleTemplates().Count);
            Assert.AreEqual(2, service.SourceTemplates().Count);
        }

    }
}