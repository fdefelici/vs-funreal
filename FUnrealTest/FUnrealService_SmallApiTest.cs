using FUnreal;
using Microsoft.VisualStudio.Package;
using System.Text.Json.Nodes;
namespace FUnrealTest
{
    [TestClass]
    public class FUnrealService_SmallApiTest
    {

        private FUnrealProject ProjectWithCategoryPath()
        {
            FUnrealProject project = new FUnrealProject("MyProject", @"C:\MyProject\MyProject.uproject");
            var plug01 = new FUnrealPlugin(project, "Plugin01", @"C:\MyProject\Plugins\PLUGCAT\Plugin01\Plugin01.uplugin");
            var mod01 = new FUnrealModule(plug01, "Module01", @"C:\MyProject\Plugins\PLUGCAT\Plugin01\Source\MODCAT\Module01\Module01.Build.cs");
            var mod02 = new FUnrealModule(plug01, "Module02", @"C:\MyProject\Plugins\PLUGCAT\Plugin01\Source\Module02\Module02.Build.cs");
            plug01.Modules.Add(mod01);
            plug01.Modules.Add(mod02);
            project.Plugins.Add(plug01);

            var mod03 = new FUnrealModule(project, "Module03", @"C:\MyProject\Source\Module03\Module03.Build.cs");
            project.GameModules.Add(mod03);

            project.AllModules.Add(mod01);
            project.AllModules.Add(mod02);
            project.AllModules.Add(mod03);
            return project;
        }

        private FUnrealProject ProjectWithStandardPath()
        {
            FUnrealProject project = new FUnrealProject("MyProject", @"C:\MyProject\MyProject.uproject");
            var plug01 = new FUnrealPlugin(project, "Plugin01", @"C:\MyProject\Plugins\Plugin01\Plugin01.uplugin");
            var mod01 = new FUnrealModule(plug01, "Module01", @"C:\MyProject\Plugins\Plugin01\Source\Module01\Module01.Build.cs");
            var mod02 = new FUnrealModule(plug01, "Module02", @"C:\MyProject\Plugins\Plugin01\Source\Module02\Module02.Build.cs");
            plug01.Modules.Add(mod01);
            plug01.Modules.Add(mod02);
            project.Plugins.Add(plug01);

            var mod03 = new FUnrealModule(project, "Module03", @"C:\MyProject\Source\Module03\Module03.Build.cs");
            project.GameModules.Add(mod03);

            project.AllModules.Add(mod01);
            project.AllModules.Add(mod02);
            project.AllModules.Add(mod03);
            return project;
        }

        private FUnrealService ServiceWithStandardPath()
        {
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);
            var project = ProjectWithStandardPath();
            FUnrealService service = new FUnrealService(eng, project, null);
            return service;
        }

        private FUnrealService ServiceWithCategoryPath()
        {
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);
            var project = ProjectWithCategoryPath();
            FUnrealService service = new FUnrealService(eng, project, null);
            return service;
        }


        [TestMethod]
        public void ComputeRelPluginPath()
        {
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealProject project = new FUnrealProject("MyProject", @"C:\MyProject\MyProject.uproject");
            project.Plugins.Add(new FUnrealPlugin(project, "Plugin01", @"C:\MyProject\Plugins\CATEGORY\Plugin01\Plugin01.uplugin"));

            FUnrealService service = new FUnrealService(eng, project, null);

            //Rel Path for existent plugin retrive the real path
            Assert.AreEqual(@"MyProject\Plugins\CATEGORY\Plugin01", service.ProjectRelativePathForPlugin("Plugin01"));

            //Rel Path for not existent plugin compute a theorical path
            Assert.AreEqual(@"MyProject\Plugins\Plugin02", service.ProjectRelativePathForPlugin("Plugin02"));
        }

        [TestMethod]
        public void CheckPluginExistence()
        {
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealProject project = new FUnrealProject("MyProject", @"C:\MyProject\MyProject.uproject");
            project.Plugins.Add(new FUnrealPlugin(project, "Plugin01", @"C:\MyProject\Plugins\CATEGORY\Plugin01\Plugin01.uplugin"));

            FUnrealService service = new FUnrealService(eng, project, null);

            Assert.IsTrue(service.ExistsPlugin("Plugin01"));
            Assert.IsFalse(service.ExistsPlugin("Plugin02"));
        }


        [TestMethod]
        public void checkIfIsSourceCodePath()
        {
            var service = ServiceWithStandardPath();

            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private\Folder1"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private\Folder1\Folder2"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public\Module01.h"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public\Folder1"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public\Folder1\Folder2"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Source\Module03"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Source\Module03\Folder1"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Source\Module03\Folder2"));

            Assert.IsFalse(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source"));
            Assert.IsFalse(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01"));
            Assert.IsFalse(service.IsSourceCodePath(@"C:\MyProject\Plugins"));
            Assert.IsFalse(service.IsSourceCodePath(@"C:\MyProject\Source"));
            Assert.IsFalse(service.IsSourceCodePath(@"C:\MyProject"));
            Assert.IsFalse(service.IsSourceCodePath(@"C:\Other\Plugins\Plugin01\Source\Module01\Private"));
            Assert.IsFalse(service.IsSourceCodePath(@"C:\Other\Plugins\Plugin01\Source\Module01\Public"));
            Assert.IsFalse(service.IsSourceCodePath(@"C:\Other\Source\Module01"));
        }

        [TestMethod]
        public void GetPluginNameFromSourceCode()
        {
            var service = ServiceWithStandardPath();

            Assert.AreEqual("Plugin01", service.PluginNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private"));
            Assert.AreEqual("Plugin01", service.PluginNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private\Folder1"));
            Assert.AreEqual("Plugin01", service.PluginNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private\Folder1\Folder2"));
            Assert.AreEqual("Plugin01", service.PluginNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public"));
            Assert.AreEqual("Plugin01", service.PluginNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public\Folder1"));
            Assert.AreEqual("Plugin01", service.PluginNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public\Folder1\Folder2"));
            Assert.AreEqual("Plugin01", service.PluginNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Module01.Build.cs"));
            Assert.AreEqual(null, service.PluginNameFromSourceCodePath(@"C:\MyProject\Source\Module01"));
            Assert.AreEqual(null, service.PluginNameFromSourceCodePath(@"C:\MyProject\Source\Module01\Folder1"));
            Assert.AreEqual(null, service.PluginNameFromSourceCodePath(@"C:\MyProject\Source\Module01\Folder2"));
        }

        [TestMethod]
        public void GetModuleNameFromSourceCode()
        {
            var service = ServiceWithStandardPath();

            Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private"));
            Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private\Folder1"));
            Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private\Folder1\Folder2"));
            Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public"));
            Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public\Folder1"));
            Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public\Folder1\Folder2"));
            Assert.AreEqual("Module03", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Source\Module03"));
            Assert.AreEqual("Module03", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Source\Module03\Folder1"));
            Assert.AreEqual("Module03", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Source\Module03\Folder1\Folder2\File.txt"));
            Assert.AreEqual(null, service.ModuleNameFromSourceCodePath(@"C:\MyProject\Source"));
        }

        [TestMethod]
        public void GetModulePathFromSourceCode()
        {
            var service = ServiceWithCategoryPath();

            Assert.AreEqual(@"C:\MyProject\Plugins\PLUGCAT\Plugin01\Source\MODCAT\Module01", service.ModulePathFromSourceCodePath(@"C:\MyProject\Plugins\PLUGCAT\Plugin01\Source\MODCAT\Module01\Private"));
            Assert.AreEqual(@"C:\MyProject\Plugins\PLUGCAT\Plugin01\Source\Module02", service.ModulePathFromSourceCodePath(@"C:\MyProject\Plugins\PLUGCAT\Plugin01\Source\Module02\Public"));
            Assert.AreEqual(@"C:\MyProject\Source\Module03", service.ModulePathFromSourceCodePath(@"C:\MyProject\Source\Module03"));
            Assert.AreEqual(@"C:\MyProject\Source\Module03", service.ModulePathFromSourceCodePath(@"C:\MyProject\Source\Module03\Folder1"));
            Assert.AreEqual(null, service.ModulePathFromSourceCodePath(@"C:\MyProject\Source"));
        }

        [TestMethod]
        public void ComputeSources()
        {
            var service = ServiceWithStandardPath();

            string headerPath, sourcePath, sourceRelPath;

            service.ComputeSourceCodePaths(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private", "MyClass", 0, out headerPath, out sourcePath, out sourceRelPath);
            Assert.AreEqual(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public\MyClass.h", headerPath);
            Assert.AreEqual(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private\MyClass.cpp", sourcePath);
            Assert.AreEqual(@"", sourceRelPath);

            service.ComputeSourceCodePaths(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private\Folder1", "MyClass", 0, out headerPath, out sourcePath, out sourceRelPath);
            Assert.AreEqual(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public\Folder1\MyClass.h", headerPath);
            Assert.AreEqual(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private\Folder1\MyClass.cpp", sourcePath);
            Assert.AreEqual(@"Folder1", sourceRelPath);
            /*
                      
        Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private\Folder1\Folder2"));
        Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public"));
        Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public\Folder1"));
        Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public\Folder1\Folder2"));
        Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Source\Module01"));
        Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Source\Module01\Folder1"));
        Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Source\Module01\Folder2"));
        Assert.AreEqual(null, service.ModuleNameFromSourceCodePath(@"C:\MyProject\Source"));
            */
        }
    }
}