using FUnreal;
using System.Text.Json.Nodes;

namespace FUnrealTest
{
    [TestClass]
    public class FUnrealServiceUtilsTest
    {
        [TestMethod]
        public void checkIfIsSourceCodePath()
        {
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);
            FUnrealService service = new FUnrealService(eng, @"C:\MyProject\MyProject.uproject", "MyProject", "template/path");

            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private\Folder1"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private\Folder1\Folder2"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public\Folder1"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public\Folder1\Folder2"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Source\Module01"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Source\Module01\Folder1"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Source\Module01\Folder2"));
            Assert.IsTrue(service.IsSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01"));

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
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);
            FUnrealService service = new FUnrealService(eng, @"C:\MyProject\MyProject.uproject", "MyProject", "template/path");

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
        public void GetPModuleNameFromSourceCode()
        {
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);
            FUnrealService service = new FUnrealService(eng, @"C:\MyProject\MyProject.uproject", "MyProject", "template/path");

            Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private"));
            Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private\Folder1"));
            Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private\Folder1\Folder2"));
            Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public"));
            Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public\Folder1"));
            Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Public\Folder1\Folder2"));
            Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Source\Module01"));
            Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Source\Module01\Folder1"));
            Assert.AreEqual("Module01", service.ModuleNameFromSourceCodePath(@"C:\MyProject\Source\Module01\Folder2"));
            Assert.AreEqual(null,       service.ModuleNameFromSourceCodePath(@"C:\MyProject\Source"));
        }

        [TestMethod]
        public void GetModulePathFromSourceCode()
        {
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);
            FUnrealService service = new FUnrealService(eng, @"C:\MyProject\MyProject.uproject", "MyProject", "template/path");

            Assert.AreEqual(@"C:\MyProject\Plugins\Plugin01\Source\Module01", service.ModulePathFromSourceCodePath(@"C:\MyProject\Plugins\Plugin01\Source\Module01\Private"));
            Assert.AreEqual(@"C:\MyProject\Source\Module01", service.ModulePathFromSourceCodePath(@"C:\MyProject\Source\Module01"));
            Assert.AreEqual(@"C:\MyProject\Source\Module01", service.ModulePathFromSourceCodePath(@"C:\MyProject\Source\Module01\Folder1"));
            Assert.AreEqual(null, service.ModulePathFromSourceCodePath(@"C:\MyProject\Source"));
        }

        [TestMethod]
        public void ComputeSources()
        {
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);
            FUnrealService service = new FUnrealService(eng, @"C:\MyProject\MyProject.uproject", "MyProject", "template/path");

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