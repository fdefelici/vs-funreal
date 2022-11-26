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
    public class FUnrealProjectFactoryTest
    {
        /*
        [TestMethod]
        public void TrackEmptyFolders()
        {
            FUnrealProjectFactory fact = new FUnrealProjectFactory();

            var list = new List<string>()
            {
                @"a\b\c\d",
                @"a\b\e",
                @"c\d\e\f",
            };

            fact.TrackEmptyFolders(list);

            list = new List<string>()
            {
                @"a\b\c\d\e",
                @"c\d",
            };

            fact.TrackEmptyFolders(list);

            Assert.AreEqual(3, fact.EmptyFolderPaths.Count);
            Assert.AreEqual(@"a\b\c\d\e", fact.EmptyFolderPaths[0]);
            Assert.AreEqual(@"a\b\e", fact.EmptyFolderPaths[1]);
            Assert.AreEqual(@"c\d\e\f", fact.EmptyFolderPaths[2]);
        }
        */

        [TestMethod]
        public void ScanEmptyDirs()
        {
            string basePath = TestUtils.AbsPath("FUnrealProjectFactoryTest");
            TestUtils.DeleteDir(basePath);

            string uprjFilePath = TestUtils.MakeFile(basePath, "UProject01/UProject01.uproject");

            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Plugin01.uplugin");
            TestUtils.MakeDir(basePath, "UProject01/Plugins/Plugin01/Content");
            var dir = TestUtils.MakeDir(basePath, "UProject01/Plugins/Plugin01/Resources");

            FUnrealProjectFactory factory = new FUnrealProjectFactory();
            var project = factory.CreateAsync(uprjFilePath, new FUnrealNotifier()).GetAwaiter().GetResult();


            factory.ScanEmptyFoldersAsync(project).GetAwaiter().GetResult();
            Assert.AreEqual(1, factory.EmptyFolderPaths.Count);
            Assert.AreEqual(dir, factory.EmptyFolderPaths[0]);

            TestUtils.DeleteDir(basePath);
        }


        [TestMethod]
        public void Simple()
        {
            string basePath = TestUtils.AbsPath("FUnrealProjectFactoryTest");
            TestUtils.DeleteDir(basePath);

            string uprjFilePath = TestUtils.MakeFile(basePath, "UProject01/UProject01.uproject");

            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Plugin01.uplugin");
            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Source/Module10/Module10.Build.cs");
            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Source/Module12/Module12.Build.cs");

            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin02/Plugin02.uplugin");
            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin02/Source/Module20/Module20.Build.cs");

            TestUtils.MakeFile(basePath, "UProject01/Source/Module30/Module30.Build.cs");


            FUnrealProjectFactory factory = new FUnrealProjectFactory();
            var project = factory.CreateAsync(uprjFilePath, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.AreEqual(2, project.Plugins.Count);
            Assert.IsNotNull(project.Plugins["Plugin01"]);
            Assert.IsNotNull(project.Plugins["Plugin02"]);

            Assert.AreEqual(1, project.GameModules.Count);

            Assert.AreEqual(4, project.AllModules.Count);


            Assert.IsTrue(project.Plugins.Exists("Plugin01"));
            Assert.IsFalse(project.Plugins.Exists("NotExists"));

            TestUtils.DeleteDir(basePath);
        }

        [TestMethod]
        public void DuplicatedPlugin_InDifferentFolder()
        {
            string basePath = TestUtils.AbsPath("FUnrealProjectFactoryTest");
            TestUtils.DeleteDir(basePath);

            string uprjFilePath = TestUtils.MakeFile(basePath, "UProject01/UProject01.uproject");
            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Plugin01.uplugin");
            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin02/Plugin01.uplugin");


            FUnrealProjectFactory factory = new FUnrealProjectFactory();
            var project = factory.CreateAsync(uprjFilePath, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsNull(project);


            TestUtils.DeleteDir(basePath);
        }

        [TestMethod]
        public void DuplicatedPlugin_InSameFolder_ShouldFail()
        {
            string basePath = TestUtils.AbsPath("FUnrealProjectFactoryTest");
            TestUtils.DeleteDir(basePath);

            string uprjFilePath = TestUtils.MakeFile(basePath, "UProject01/UProject01.uproject");

            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Plugin01.uplugin");
            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Plugin01_Bis.uplugin");


            FUnrealProjectFactory factory = new FUnrealProjectFactory();
            var project = factory.CreateAsync(uprjFilePath, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsNull(project);

            TestUtils.DeleteDir(basePath);
        }

        [TestMethod]
        public void NestedPluginShouldNotBeDiscovered()
        {
            string basePath = TestUtils.AbsPath("FUnrealProjectFactoryTest");
            TestUtils.DeleteDir(basePath);

            string uprjFilePath = TestUtils.MakeFile(basePath, "UProject01/UProject01.uproject");

            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Plugin01.uplugin");
            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/SubFolder/Other.uplugin");


            FUnrealProjectFactory factory = new FUnrealProjectFactory();
            var project = factory.CreateAsync(uprjFilePath, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.AreEqual(1, project.Plugins.Count);

            TestUtils.DeleteDir(basePath);
        }

        [TestMethod]
        public void ModuleFolderWithMultipleBuildCs_ShouldBeThreatedLikeMultipleModules()
        {
            string basePath = TestUtils.AbsPath("FUnrealProjectFactoryTest");
            TestUtils.DeleteDir(basePath);

            string uprjFilePath = TestUtils.MakeFile(basePath, "UProject01/UProject01.uproject");

            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Plugin01.uplugin");
            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Source/Module01/Module01.Build.cs");
            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Source/Module01/Module01_Bis.Build.cs");

            FUnrealProjectFactory factory = new FUnrealProjectFactory();
            var project = factory.CreateAsync(uprjFilePath, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.AreEqual(1, project.Plugins.Count);
            Assert.AreEqual(2, project.AllModules.Count);

            TestUtils.DeleteDir(basePath);
        }

        [TestMethod]
        public void NestedBuildCs_ShouldJustBeIgnored()
        {
            string basePath = TestUtils.AbsPath("FUnrealProjectFactoryTest");
            TestUtils.DeleteDir(basePath);

            string uprjFilePath = TestUtils.MakeFile(basePath, "UProject01/UProject01.uproject");

            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Plugin01.uplugin");
            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Source/Module01/Module01.Build.cs");
            TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Source/Module01/SubFolder/Module01_Bis.Build.cs");

            FUnrealProjectFactory factory = new FUnrealProjectFactory();
            var project = factory.CreateAsync(uprjFilePath, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.AreEqual(1, project.Plugins.Count);
            Assert.AreEqual(1, project.AllModules.Count);

            TestUtils.DeleteDir(basePath);
        }

        [TestMethod]
        public void ProjectWithPrimaryGameModule()
        {
            string basePath = TestUtils.AbsPath("FUnrealProjectFactoryTest");
            TestUtils.DeleteDir(basePath);

            string uprjFilePath = TestUtils.MakeFile(basePath, "UProject01/UProject01.uproject");
            TestUtils.MakeFile(basePath, "UProject01/Source/Module01/Module01.Build.cs");
            var modFile = TestUtils.MakeFile(basePath, "UProject01/Source/Module01/Module01.cpp");
            TestUtils.WriteFile(modFile, "PRIMARY_GAME_MODULE(....)");

            FUnrealProjectFactory factory = new FUnrealProjectFactory();
            var project = factory.CreateAsync(uprjFilePath, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.AreEqual(0, project.Plugins.Count);
            Assert.AreEqual(1, project.AllModules.Count);
            Assert.AreEqual(1, project.GameModules.Count);
            Assert.IsTrue(project.GameModules["Module01"].IsPrimaryGameModule);

            TestUtils.DeleteDir(basePath);
        }
    }
}
