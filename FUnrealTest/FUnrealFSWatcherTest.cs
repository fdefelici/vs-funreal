using FUnreal;
using FUnreal.Sources.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FUnrealTest
{
    [TestClass]
    public class FUnrealFSWatcherTest
    {
        static int eventSleepMS = 300;

        private string basePath;
        private List<string> plugins;
        private List<string> modules;
        FUnrealFSWatcher Watcher;

        int pluginCreatedCount;
        int pluginDeletedCount;
        int pluginRenamedCount;


        int moduleCreatedCount;
        int moduleDeletedCount;
        int moduleRenamedCount;

        [TestInitialize]
        public void SetUp()
        {
            basePath = TestUtils.AbsPath("FUnrealFSWatcherTest");
            TestUtils.DeleteDir(basePath);

            string uprjFilePath = TestUtils.MakeFile(basePath, "MyProject/MyProject.uproject");

            plugins = new List<string>();
            modules = new List<string>();

            pluginCreatedCount = 0;
            pluginDeletedCount = 0;
            pluginRenamedCount = 0;
            moduleCreatedCount = 0;
            moduleDeletedCount = 0;
            moduleRenamedCount = 0;

            Watcher = new FUnrealFSWatcher(uprjFilePath);
            Watcher.OnPluginCreated = (plugFile) => { pluginCreatedCount++; plugins.Add(plugFile); };
            Watcher.OnPluginDeleted = (plugFile) => { pluginDeletedCount++; plugins.Remove(plugFile); };
            Watcher.OnPluginRenamed = (plugFileOld, plugFileNew) =>
            {
                pluginRenamedCount++;
                int index = plugins.IndexOf(plugFileOld);
                plugins.RemoveAt(index); plugins.Insert(index, plugFileNew);
            };

            Watcher.OnModuleCreated = (modFile) => { moduleCreatedCount++; modules.Add(modFile); };
            Watcher.OnModuleDeleted = (modFile) => { moduleDeletedCount++; modules.Remove(modFile); };
            Watcher.OnModuleRenamed = (modFileOld, modFileNew) =>
            {
                moduleRenamedCount++;
                int index = modules.IndexOf(modFileOld);
                modules.RemoveAt(index); modules.Insert(index, modFileNew);
            };
        }

        [TestCleanup]
        public void TearDown()
        {
            Watcher.Stop();
            TestUtils.DeleteDir(basePath);
        }


        [TestMethod]
        public void WithPluginsAlreadyExists()
        {
            TestUtils.MakeDir(basePath, "MyProject/Plugins");

            Watcher.Start();

            //The create Plugin
            string uplugin01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Plugin01.uplugin");

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous

            Assert.AreEqual(1, plugins.Count);
            Assert.AreEqual(uplugin01, plugins[0]);
        }

        [TestMethod]
        public void WithPluginsNotxistsBefore()
        {
            Watcher.Start();

            //Plugins folder created on the fly, simulating copy/paste
            string uplugin01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Plugin01.uplugin");

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous

            Assert.AreEqual(1, pluginCreatedCount);
            Assert.AreEqual(0, pluginRenamedCount);
            Assert.AreEqual(0, pluginDeletedCount);

            Assert.AreEqual(1, plugins.Count);
            Assert.AreEqual(uplugin01, plugins[0]);
        }

        [TestMethod]
        public void RenamePluginFile()
        {
            string uplugin01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Plugin01.uplugin");
            plugins.Add(uplugin01);

            Watcher.Start();

            string uplugin01Renamed = TestUtils.RenameFile(uplugin01, "Plugin01Renamed");


            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous

            Assert.AreEqual(0, pluginCreatedCount);
            Assert.AreEqual(1, pluginRenamedCount);
            Assert.AreEqual(0, pluginDeletedCount);

            Assert.AreEqual(1, plugins.Count);
            Assert.AreEqual(uplugin01Renamed, plugins[0]);
        }

        [TestMethod]
        public void RenamePluginFolder()
        {
            string uplugin01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Plugin01.uplugin");
            plugins.Add(uplugin01);

            Watcher.Start();

            string pluginFolder = TestUtils.PathCombine(basePath, "MyProject/Plugins/Plugin01");
            TestUtils.RenameFolder(pluginFolder, "Plugin01Renamed");
            string uplugin01Renamed = TestUtils.PathCombine(basePath, "MyProject/Plugins/Plugin01Renamed/Plugin01.uplugin");


            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous

            Assert.AreEqual(0, pluginCreatedCount);
            Assert.AreEqual(1, pluginRenamedCount);
            Assert.AreEqual(0, pluginDeletedCount);

            Assert.AreEqual(1, plugins.Count);
            Assert.AreEqual(uplugin01Renamed, plugins[0]);
        }

        [TestMethod]
        public void DeletePluginFile()
        {
            string uplugin01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Plugin01.uplugin");
            plugins.Add(uplugin01);

            Watcher.Start();

            TestUtils.DeleteFile(uplugin01);

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous

            Assert.AreEqual(0, pluginCreatedCount);
            Assert.AreEqual(0, pluginRenamedCount);
            Assert.AreEqual(1, pluginDeletedCount);

            Assert.AreEqual(0, plugins.Count);
        }

        [TestMethod]
        public void DeletePluginFolder()
        {
            string uplugin01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Plugin01.uplugin");
            plugins.Add(uplugin01);

            Watcher.Start();

            string pluginFolder = TestUtils.PathParent(uplugin01);
            TestUtils.DeleteDir(pluginFolder);

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous

            Assert.AreEqual(0, pluginCreatedCount);
            Assert.AreEqual(0, pluginRenamedCount);
            Assert.AreEqual(2, pluginDeletedCount); //2 because TestUtils.DeleteDir fire .uplugin delete event. Instead when dealing with File Eplorer, event on file is not raised!

            Assert.AreEqual(0, plugins.Count);
        }

        [TestMethod]
        public void CreatePluginModule()
        {
            string uplugin01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Plugin01.uplugin");
            //plugins.Add(uplugin01);

            Watcher.Start();

            string module01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Source/Module01/Module01.Build.cs");


            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous


            Assert.AreEqual(1, moduleCreatedCount);
            Assert.AreEqual(0, moduleRenamedCount);
            Assert.AreEqual(0, moduleDeletedCount);

            Assert.AreEqual(1, modules.Count);
            Assert.AreEqual(module01, modules[0]);
        }

        [TestMethod]
        public void RenamePluginModuleFile()
        {
            //string uplugin01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Plugin01.uplugin");
            string module01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Source/Module01/Module01.Build.cs");
            modules.Add(module01);

            Watcher.Start();

            string module01Renamed = TestUtils.RenameFile(module01, "Module01Renamed.Build");

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous


            Assert.AreEqual(0, moduleCreatedCount);
            Assert.AreEqual(1, moduleRenamedCount);
            Assert.AreEqual(0, moduleDeletedCount);

            Assert.AreEqual(1, modules.Count);
            Assert.AreEqual(module01Renamed, modules[0]);
        }

        [TestMethod]
        public void RenamePluginModuleFolder()
        {
            //string uplugin01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Plugin01.uplugin");
            string module01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Source/Module01/Module01.Build.cs");
            modules.Add(module01);

            Watcher.Start();

            string module01Path = TestUtils.PathParent(module01);
            TestUtils.RenameFolder(module01Path, "Module01Renamed");
            string module01Renamed = TestUtils.PathCombine(basePath, "MyProject/Plugins/Plugin01/Source/Module01Renamed/Module01.Build.cs");

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous


            Assert.AreEqual(0, moduleCreatedCount);
            Assert.AreEqual(1, moduleRenamedCount);
            Assert.AreEqual(0, moduleDeletedCount);

            Assert.AreEqual(1, modules.Count);
            Assert.AreEqual(module01Renamed, modules[0]);
        }



        [TestMethod]
        public void DeletePluginModuleFile()
        {
            //string uplugin01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Plugin01.uplugin");
            string module01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Source/Module01/Module01.Build.cs");
            modules.Add(module01);

            Watcher.Start();

            TestUtils.DeleteFile(module01);

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous


            Assert.AreEqual(0, moduleCreatedCount);
            Assert.AreEqual(0, moduleRenamedCount);
            Assert.AreEqual(1, moduleDeletedCount);

            Assert.AreEqual(0, modules.Count);
        }

        [TestMethod]
        public void DeletePluginModuleFolder()
        {
            //string uplugin01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Plugin01.uplugin");
            string module01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Source/Module01/Module01.Build.cs");
            modules.Add(module01);

            Watcher.Start();

            TestUtils.DeleteDir(XFilesystem.PathParent(module01));

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous


            Assert.AreEqual(0, moduleCreatedCount);
            Assert.AreEqual(0, moduleRenamedCount);
            Assert.AreEqual(2, moduleDeletedCount); //2 because TestUtils.DeleteDir fire .Build.cs delete event. Instead when dealing with File Eplorer, event on file is not raised!

            Assert.AreEqual(0, modules.Count);
        }

        [TestMethod]
        public void CreateGameModule()
        {
            TestUtils.MakeDir(basePath, "MyProject/Source");

            Watcher.Start();

            string module01 = TestUtils.MakeFile(basePath, "MyProject/Source/Module01/Module01.Build.cs");

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous

            Assert.AreEqual(1, moduleCreatedCount);
            Assert.AreEqual(0, moduleRenamedCount);
            Assert.AreEqual(0, moduleDeletedCount);

            Assert.AreEqual(1, modules.Count);
            Assert.AreEqual(module01, modules[0]);
        }

        [TestMethod]
        public void RenameGameModuleFile()
        {
            string module01 = TestUtils.MakeFile(basePath, "MyProject/Source/Module01/Module01.Build.cs");
            modules.Add(module01);

            Watcher.Start();

            string module01Renamed = TestUtils.RenameFile(module01, "Module01Renamed.Build");

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous

            Assert.AreEqual(0, moduleCreatedCount);
            Assert.AreEqual(1, moduleRenamedCount);
            Assert.AreEqual(0, moduleDeletedCount);

            Assert.AreEqual(1, modules.Count);
            Assert.AreEqual(module01Renamed, modules[0]);
        }

        [TestMethod]
        public void RenameGameModuleFolder()
        {
            string module01 = TestUtils.MakeFile(basePath, "MyProject/Source/Module01/Module01.Build.cs");
            modules.Add(module01);

            Watcher.Start();

            string module01Path = TestUtils.PathParent(module01);
            TestUtils.RenameFolder(module01Path, "Module01Renamed");
            string module01Renamed = TestUtils.PathCombine(basePath, "MyProject/Source/Module01Renamed/Module01.Build.cs");

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous

            Assert.AreEqual(0, moduleCreatedCount);
            Assert.AreEqual(1, moduleRenamedCount);
            Assert.AreEqual(0, moduleDeletedCount);

            Assert.AreEqual(1, modules.Count);
            Assert.AreEqual(module01Renamed, modules[0]);
        }



        [TestMethod]
        public void DeleteGameModuleFile()
        {
            string module01 = TestUtils.MakeFile(basePath, "MyProject/Source/Module01/Module01.Build.cs");
            modules.Add(module01);

            Watcher.Start();

            TestUtils.DeleteFile(module01);

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous


            Assert.AreEqual(0, moduleCreatedCount);
            Assert.AreEqual(0, moduleRenamedCount);
            Assert.AreEqual(1, moduleDeletedCount);

            Assert.AreEqual(0, modules.Count);
        }

        [TestMethod]
        public void DeleteGameModuleFolder()
        {
            string module01 = TestUtils.MakeFile(basePath, "MyProject/Source/Module01/Module01.Build.cs");
            modules.Add(module01);

            Watcher.Start();

            TestUtils.DeleteDir(XFilesystem.PathParent(module01));

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous


            Assert.AreEqual(0, moduleCreatedCount);
            Assert.AreEqual(0, moduleRenamedCount);
            Assert.AreEqual(2, moduleDeletedCount); //2 because TestUtils.DeleteDir fire .Build.cs delete event. Instead when dealing with File Eplorer, event on file is not raised!

            Assert.AreEqual(0, modules.Count);
        }


        //Creare Test tipo di Move Plugin che contine Moduli 


        [TestMethod]
        public void CopyPastePluginFolder()
        {
            string sourcePath = TestUtils.PathCombine(basePath, "SourcePath");

            string uplugin01 = TestUtils.MakeFile(sourcePath, "Plugin01/Plugin01.uplugin");
            string module01 = TestUtils.MakeFile(sourcePath,  "Plugin01/Source/Module01/Module01.Build.cs");
            string module02 = TestUtils.MakeFile(sourcePath,  "Plugin01/Source/Module02/Module02.Build.cs");

            string destPath = TestUtils.MakeDir(basePath, "MyProject/Plugins");

            Watcher.Start();


            TestUtils.DeepCopy(sourcePath, destPath);

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous


            Assert.AreEqual(1, pluginCreatedCount);
            Assert.AreEqual(0, pluginRenamedCount);
            Assert.AreEqual(0, pluginDeletedCount);
            Assert.AreEqual(TestUtils.PathCombine(basePath, "MyProject/Plugins/Plugin01/Plugin01.uplugin"), plugins[0]);

            Assert.AreEqual(2, moduleCreatedCount);
            Assert.AreEqual(0, moduleRenamedCount);
            Assert.AreEqual(0, moduleDeletedCount);

            Assert.AreEqual(TestUtils.PathCombine(basePath, "MyProject/Plugins/Plugin01/Source/Module01/Module01.Build.cs"), modules[0]);
            Assert.AreEqual(TestUtils.PathCombine(basePath, "MyProject/Plugins/Plugin01/Source/Module02/Module02.Build.cs"), modules[1]);
        }

        [TestMethod]
        public void RenamePluginFolderWithModules()
        {
            string uplugin01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Plugin01.uplugin");
            string module01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Source/Module01/Module01.Build.cs");
            string module02 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Source/Module02/Module02.Build.cs");

            plugins.Add(uplugin01);
            modules.Add(module01);
            modules.Add(module02);

            Watcher.Start();


            TestUtils.RenameFolder(XFilesystem.PathParent(uplugin01), "Plugin01Renamed");

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous


            Assert.AreEqual(0, pluginCreatedCount);
            Assert.AreEqual(1, pluginRenamedCount);
            Assert.AreEqual(0, pluginDeletedCount);
            Assert.AreEqual(TestUtils.PathCombine(basePath, "MyProject/Plugins/Plugin01Renamed/Plugin01.uplugin"), plugins[0]);

            Assert.AreEqual(0, moduleCreatedCount);
            Assert.AreEqual(0, moduleRenamedCount);
            Assert.AreEqual(0, moduleDeletedCount);

            //Right that module path are not updated. (this will be business logic
            //Assert.AreEqual(TestUtils.PathCombine(basePath, "MyProject/Plugins/Plugin01Renamed/Source/Module01/Module01.Build.cs"), modules[0]);
            //Assert.AreEqual(TestUtils.PathCombine(basePath, "MyProject/Plugins/Plugin01Renamed/Source/Module02/Module02.Build.cs"), modules[1]);
        }

        [TestMethod]
        public void DeletePluginsFolder()
        {
            string uplugin01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Plugin01.uplugin");
            string module01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Source/Module01/Module01.Build.cs");
            string module02 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Source/Module02/Module02.Build.cs");

            plugins.Add(uplugin01);
            modules.Add(module01);
            modules.Add(module02);

            Watcher.Start();


            TestUtils.DeleteDir(basePath, "MyProject/Plugins");

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous


            Assert.AreEqual(0, pluginCreatedCount);
            Assert.AreEqual(0, pluginRenamedCount);
            Assert.AreEqual(2, pluginDeletedCount); //2 because TestUtils.DeleteDir fire .Build.cs delete event. Instead when dealing with File Eplorer, event on file is not raised!

            Assert.AreEqual(0, moduleCreatedCount);
            Assert.AreEqual(0, moduleRenamedCount);
            Assert.AreEqual(4, moduleDeletedCount); //4 because of TestUtils.DeleteDir
        }

        [TestMethod]
        public void WatcherPauseAndRestart()
        {
            string uplugin01 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin01/Plugin01.uplugin");
            plugins.Add(uplugin01);
            
            Watcher.Start();

            Watcher.Pause();
            TestUtils.DeleteDir(basePath, "MyProject/Plugins/Plugin01");

            Watcher.Resume();
            string uplugin02 = TestUtils.MakeFile(basePath, "MyProject/Plugins/Plugin02/Plugin02.uplugin");

            Thread.Sleep(eventSleepMS); //Wait for events to be fired, because are Asyncronous

            Assert.AreEqual(1, pluginCreatedCount);
            Assert.AreEqual(0, pluginRenamedCount);
            Assert.AreEqual(0, pluginDeletedCount); 

            Assert.AreEqual(2, plugins.Count);
            Assert.AreEqual(uplugin02, plugins[1]);

            Assert.AreEqual(0, moduleCreatedCount);
            Assert.AreEqual(0, moduleRenamedCount);
            Assert.AreEqual(0, moduleDeletedCount);
        }




        [TestMethod]
        public void FromWatcher_Event_CopyPastePluginFolder()
        {
            string basePath = TestUtils.AbsPath("FUnrealProjectFactoryTest");
            TestUtils.DeleteDir(basePath);

            string uprjFilePath = TestUtils.MakeFile(basePath, "UProject01/UProject01.uproject");

            var project = new FUnrealProject("UProject01", uprjFilePath);


            FUnrealProjectUpdater updater = new FUnrealProjectUpdater(project);

            //Copy/Paste Plugin01 folder containing .uplugin to Plugins folder
            string plugin01File = TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Plugin01.uplugin");
            string plugin01Dir = TestUtils.PathParent(plugin01File);
            var event01 = Task.Run(() => updater.HandleCreatePluginFolder(plugin01Dir));
            var event02 = Task.Run(() => updater.HandleCreatePluginFile(plugin01File));

            Task.WaitAll(event01, event02);

            Assert.AreEqual(1, project.Plugins.Count);
            Assert.IsTrue(project.Plugins.Exists("Plugin01"));

            project.Clear();

            //Inverting events should have same result;
            event01 = Task.Run(() => updater.HandleCreatePluginFile(plugin01File));
            event02 = Task.Run(() => updater.HandleCreatePluginFolder(plugin01Dir));

            Task.WaitAll(event01, event02);

            Assert.AreEqual(1, project.Plugins.Count);
            Assert.IsTrue(project.Plugins.Exists("Plugin01"));
        }

        [TestMethod]
        public void FromWatcher_Event_DeletePluginFolder_ForExistentPlugin()
        {
            string basePath = TestUtils.AbsPath("FUnrealProjectFactoryTest");
            TestUtils.DeleteDir(basePath);

            string uprjFilePath = TestUtils.MakeFile(basePath, "UProject01/UProject01.uproject");
            string plugin01File = TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Plugin01.uplugin");

            var project = new FUnrealProject("UProject01", uprjFilePath);
            var plugin01 = new FUnrealPlugin(project, "Plugin01", plugin01File);
            project.Plugins.Add(plugin01);


            FUnrealProjectUpdater updater = new FUnrealProjectUpdater(project);

            //Delete Plugin01 folder containing .uplugin to Plugins folder
            string plugin01Dir = TestUtils.PathParent(plugin01File);
            TestUtils.DeleteDir(plugin01Dir);
            updater.HandleDeletePluginFolder(plugin01Dir);

            Assert.AreEqual(0, project.Plugins.Count);
        }

        [TestMethod]
        public void FromWatcher_Event_RenamePluginFolder_ForExistentPlugin()
        {
            string basePath = TestUtils.AbsPath("FUnrealProjectFactoryTest");
            TestUtils.DeleteDir(basePath);

            string uprjFilePath = TestUtils.MakeFile(basePath, "UProject01/UProject01.uproject");
            string plugin01File = TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Plugin01.uplugin");

            var project = new FUnrealProject("UProject01", uprjFilePath);
            var plugin01 = new FUnrealPlugin(project, "Plugin01", plugin01File);
            project.Plugins.Add(plugin01);


            FUnrealProjectUpdater updater = new FUnrealProjectUpdater(project);

            //Rename Plugin01 folder containing .uplugin 
            string plugin01Dir = TestUtils.PathParent(plugin01File);
            string Plugin01DirRenamed = TestUtils.RenameFolder(plugin01Dir, "Plugin01Renamed");
            updater.HandleRenamePluginFolder(plugin01Dir, Plugin01DirRenamed);

            Assert.AreEqual(1, project.Plugins.Count);
            Assert.AreEqual(TestUtils.PathCombine(basePath, "UProject01/Plugins/Plugin01Renamed/Plugin01.uplugin"), project.Plugins["Plugin01"].DescriptorFilePath);
        }

        [TestMethod]
        public void FromWatcher_Event_Strange_AddingOneMorePluginFile()
        {
            string basePath = TestUtils.AbsPath("FUnrealProjectFactoryTest");
            TestUtils.DeleteDir(basePath);

            string uprjFilePath = TestUtils.MakeFile(basePath, "UProject01/UProject01.uproject");
            string plugin01File = TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Plugin01.uplugin");

            var project = new FUnrealProject("UProject01", uprjFilePath);
            var plugin01 = new FUnrealPlugin(project, "Plugin01", plugin01File);
            project.Plugins.Add(plugin01);


            FUnrealProjectUpdater updater = new FUnrealProjectUpdater(project);

            //Add another .uplugin to Plugin01 folder 
            string plugin01Dir = TestUtils.PathParent(plugin01File);
            string plugin01File2 = TestUtils.MakeFile(plugin01Dir, "Plugin01 - Copy.uplugin");
            updater.HandleCreatePluginFile(plugin01File2);

            Assert.AreEqual(1, project.Plugins.Count);
            Assert.AreEqual(TestUtils.PathCombine(basePath, "UProject01/Plugins/Plugin01Renamed/Plugin01.uplugin"), project.Plugins["Plugin01"].DescriptorFilePath);
        }

        [TestMethod]
        public void FromWatcher_Event_MovePluginFolderFromOutside()
        {
            string basePath = TestUtils.AbsPath("FUnrealProjectFactoryTest");
            TestUtils.DeleteDir(basePath);

            string uprjFilePath = TestUtils.MakeFile(basePath, "UProject01/UProject01.uproject");

            var project = new FUnrealProject("UProject01", uprjFilePath);


            FUnrealProjectUpdater updater = new FUnrealProjectUpdater(project);

            //Moving Plugin01 folder containing .uplugin to Plugins folder
            string plugin01File = TestUtils.MakeFile(basePath, "UProject01/Plugins/Plugin01/Plugin01.uplugin");
            string plugin01Dir = TestUtils.PathParent(plugin01File);
            updater.HandleCreatePluginFolder(plugin01Dir);

            Assert.AreEqual(1, project.Plugins.Count);
            Assert.IsTrue(project.Plugins.Exists("Plugin01"));
        }
    }
}
