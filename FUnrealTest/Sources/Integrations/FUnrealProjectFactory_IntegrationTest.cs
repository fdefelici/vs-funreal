﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using FUnreal;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System;

namespace FUnrealTest.Integrations
{
    [TestClass]
    public class FUnrealProjectFactory_IntegrationTest
    {

        [TestMethod]
        public void LoadUE5()
        {
            string uprjFilePath = @"C:\_fdf\tools\epic\Epic Games\UE_5.0\Engine\FUnrealIntegrationTest.uproject";

            FUnrealProjectFactory factory = new FUnrealProjectFactory();
            FUnrealProject project = factory.CreateAsync(uprjFilePath, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.AreEqual(433, project.Plugins.Count);
            Assert.AreEqual(1415, project.AllModules.Count);
        }


        [TestMethod]
        public void LoadUE5_Asynchr()
        {
            string uprjFilePath = @"C:\Program Files\Epic Games\UE_5.0\Engine\Engine.uproject";

            FUnrealProjectFactory factory = new FUnrealProjectFactory();
            FUnrealProject project = factory.CreateAsync(uprjFilePath, new FUnrealNotifier()).ConfigureAwait(false).GetAwaiter().GetResult();

            Assert.AreEqual(434, project.Plugins.Count);
            Assert.AreEqual(1417, project.AllModules.Count);

            /*
            StringBuilder buffer = new StringBuilder();
            var ordered = project.AllModules.OrderBy(each => each.FullPath);

            foreach(var o in ordered)
            {
                buffer.Append(o.FullPath).Append("\n");
            }
            XFilesystem.WriteFile(@"cmodules.txt", buffer.ToString());
            */
        }

        //[TestMethod]

        public void LoadUE5_Perf()
        {
            string uprjFilePath = @"C:\Program Files\Epic Games\UE_5.0\Engine\Engine.uproject";

            FUnrealProjectFactory factory = new FUnrealProjectFactory();
            FUnrealProject project = null;

            for (int i = 0; i < 100; i++)
            {
                project = factory.CreateAsync(uprjFilePath, new FUnrealNotifier()).GetAwaiter().GetResult();
            }

            Assert.AreEqual(434, project.Plugins.Count);

            Assert.AreEqual(1417, project.AllModules.Count);
        }

        [TestMethod]
        public void LoadUE5_FindFilesEnumPerf() 
        {
            string uprjFilePath = @"C:\Program Files\Epic Games\UE_5.0\Engine";

            var files = XFilesystem.FindFilesEnum(uprjFilePath, true, "*.*");
            Assert.IsTrue(files.Any()); 
           
        }

        [TestMethod]
        public void LoadUE5_FindFilesEnumByFilterPerf()
        {
            string uprjFilePath = @"C:\Program Files\Epic Games\UE_5.0\Engine";

            var files = XFilesystem.FindFilesEnum(uprjFilePath, true, "*.h", file => { string text = XFilesystem.FileRead(file); return text!=null; });
            Assert.IsTrue(files.Any());
        }


        [TestMethod]
        public void LoadUE5_CountDirPerf()
        {
            string uprjFilePath = @"C:\Program Files\Epic Games\UE_5.0\Engine";

            var pluginsPath = TestUtils.PathCombine(uprjFilePath, "Plugins");
            var sourcePath = TestUtils.PathCombine(uprjFilePath, "Source");

            var plugDirs = XFilesystem.FindDirsEnum(pluginsPath, true);
            var sourceDirs = XFilesystem.FindDirsEnum(sourcePath, true);

            Assert.AreEqual(15536, plugDirs.Count()); 
            Assert.AreEqual(6190, sourceDirs.Count());
        }

        [TestMethod]
        public void LoadUE5_FindEmptyDirs()
        {
            string uprjFilePath = @"C:\Program Files\Epic Games\UE_5.0\Engine";

            var pluginsPath = TestUtils.PathCombine(uprjFilePath, "Plugins");
            var sourcePath = TestUtils.PathCombine(uprjFilePath, "Source");

            var plugDirs = XFilesystem.FindEmptyFoldersAsync(pluginsPath).GetAwaiter().GetResult();
            var sourceDirs = XFilesystem.FindEmptyFoldersAsync(sourcePath).GetAwaiter().GetResult();

            Assert.AreEqual(10, plugDirs.Count); //6 seconds con trasformazione in List
            Assert.AreEqual(0, sourceDirs.Count);
        }

        [TestMethod]
        public void LoadUE5_ScanEmptyDirs()
        {
            string uprjFilePath = @"C:\Program Files\Epic Games\UE_5.0\Engine\Engine.uproject";

            FUnrealProjectFactory factory = new FUnrealProjectFactory();
            FUnrealProject project = factory.CreateAsync(uprjFilePath, new FUnrealNotifier()).ConfigureAwait(false).GetAwaiter().GetResult();


            Stopwatch watch = Stopwatch.StartNew();
            factory.ScanEmptyFoldersAsync(project).ConfigureAwait(false).GetAwaiter().GetResult(); 
            watch.Stop();

            Console.WriteLine($"Scan duration: {watch.ElapsedMilliseconds} ms");

            Assert.AreEqual(0, factory.EmptyFolderPaths.Count);
        }


        [TestMethod]
        public void LoadUE5_FindEmptyDirsMulti()
        {
            string uprjFilePath = @"C:\Program Files\Epic Games\UE_5.0\Engine";

            var pluginsPath = TestUtils.PathCombine(uprjFilePath, "Plugins");
            var sourcePath = TestUtils.PathCombine(uprjFilePath, "Source");

            var emptyDirs = XFilesystem.FindEmptyFoldersAsync(pluginsPath, sourcePath).GetAwaiter().GetResult();

            Assert.AreEqual(10, emptyDirs.Count); 
        }

        [TestMethod]
        public void LoadUE5_FindEmptyDirsMultiParallel()
        {
            string uprjFilePath = @"C:\Program Files\Epic Games\UE_5.0\Engine";

            var pluginsPath = TestUtils.PathCombine(uprjFilePath, "Plugins");
            var sourcePath = TestUtils.PathCombine(uprjFilePath, "Source");

            var emptyDirs = XFilesystem.FindEmptyFoldersAsync(pluginsPath, sourcePath).GetAwaiter().GetResult();

            Assert.AreEqual(10, emptyDirs.Count);
        }

        [TestMethod]
        public void tryes()
        {
            string uprjFilePath = @"C:\Program Files\Epic Games\UE_5.0\Engine\Engine.uproject";

            FUnrealProjectFactory factory = new FUnrealProjectFactory();
            FUnrealProject project = factory.CreateAsync(uprjFilePath, new FUnrealNotifier()).ConfigureAwait(false).GetAwaiter().GetResult();

            
            foreach(var plug in project.Plugins)
            {
                string text = XFilesystem.FileRead(plug.DescriptorFilePath);
                if (text.Contains("Plugins"))
                {
                    Console.WriteLine(plug.DescriptorFilePath);
                }
            }
            
        }

    }
}
