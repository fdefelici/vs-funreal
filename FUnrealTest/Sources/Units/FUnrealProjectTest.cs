using Microsoft.VisualStudio.TestTools.UnitTesting;
using FUnreal;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace FUnrealTest
{
    [TestClass]
    public class FUnrealProjectTest
    {
        [TestMethod]
        public void ProjectTest()
        {
            var project = new FUnrealProject(@"C:\Projects\Hello\Hello.uproject");

            Assert.AreEqual(@"Hello", project.Name);
            Assert.AreEqual(@"C:\Projects\Hello", project.FullPath);
            Assert.AreEqual(@"C:\Projects\Hello\Hello.uproject", project.DescriptorFilePath);
            Assert.AreEqual(@"C:\Projects\Hello\Plugins", project.PluginsPath);
            Assert.AreEqual(@"C:\Projects\Hello\Source", project.SourcePath);
        }


        [TestMethod]
        public void PluginTest()
        {
            var project = new FUnrealProject(@"C:\Projects\Hello\Hello.uproject");
            var plugin = new FUnrealPlugin(project, @"Plugin01\Plugin01.uplugin");

            Assert.AreEqual(@"Plugin01", plugin.Name);
            Assert.AreEqual(@"C:\Projects\Hello\Plugins\Plugin01", plugin.FullPath);
            Assert.AreEqual(@"C:\Projects\Hello\Plugins\Plugin01\Plugin01.uplugin", plugin.DescriptorFilePath);
            Assert.AreEqual(@"C:\Projects\Hello\Plugins\Plugin01\Source", plugin.SourcePath);
            Assert.AreEqual(@"C:\Projects\Hello\Plugins\Plugin01\Content", plugin.ContentPath);
            Assert.AreEqual(@"C:\Projects\Hello\Plugins\Plugin01\Shaders", plugin.ShadersPath);
        }


        [TestMethod]
        public void PluginModuleTest()
        {
            var project = new FUnrealProject(@"C:\Projects\Hello\Hello.uproject");
            var plugin = new FUnrealPlugin(project, @"Plugin01\Plugin01.uplugin");
            var module = new FUnrealModule(plugin, @"Module01\Module01.Build.cs");

            Assert.AreEqual("Module01", module.Name);
            Assert.AreEqual(@"Module01\Module01.Build.cs", module.BuildFileRelPath);
            Assert.AreEqual(@"C:\Projects\Hello\Plugins\Plugin01\Source\Module01\Module01.Build.cs", module.BuildFilePath);
            Assert.AreEqual(@"C:\Projects\Hello\Plugins\Plugin01\Source\Module01", module.FullPath);
            Assert.AreEqual(@"C:\Projects\Hello\Plugins\Plugin01\Source\Module01\Public", module.PublicPath);
            Assert.AreEqual(@"C:\Projects\Hello\Plugins\Plugin01\Source\Module01\Private", module.PrivatePath);


            plugin.SetDescriptorFileRelPath(@"Plugin01Ren\\Plugin01Ren.uplugin");
            Assert.AreEqual(@"C:\Projects\Hello\Plugins\Plugin01Ren\Source", plugin.SourcePath);

            Assert.AreEqual(@"C:\Projects\Hello\Plugins\Plugin01Ren\Source\Module01\Module01.Build.cs", module.BuildFilePath);
            Assert.AreEqual(@"C:\Projects\Hello\Plugins\Plugin01Ren\Source\Module01", module.FullPath);
            Assert.AreEqual(@"C:\Projects\Hello\Plugins\Plugin01Ren\Source\Module01\Public", module.PublicPath);
            Assert.AreEqual(@"C:\Projects\Hello\Plugins\Plugin01Ren\Source\Module01\Private", module.PrivatePath);
        }

        [TestMethod]
        public void GameModuleTest()
        {
            var project = new FUnrealProject(@"C:\Projects\Hello\Hello.uproject");

            var module = new FUnrealModule(project, @"Module01\Module01.Build.cs");

            Assert.AreEqual("Module01", module.Name);
            Assert.AreEqual(@"Module01\Module01.Build.cs", module.BuildFileRelPath);
            Assert.AreEqual(@"C:\Projects\Hello\Source\Module01\Module01.Build.cs", module.BuildFilePath);
            Assert.AreEqual(@"C:\Projects\Hello\Source\Module01", module.FullPath);
            Assert.AreEqual(@"C:\Projects\Hello\Source\Module01\Public", module.PublicPath);
            Assert.AreEqual(@"C:\Projects\Hello\Source\Module01\Private", module.PrivatePath);
        }
    }
}
