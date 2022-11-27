using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            string uprjFilePath = @"C:\Program Files\Epic Games\UE_5.0\Engine\Engine.uproject";

            FUnrealProjectFactory factory = new FUnrealProjectFactory();
            FUnrealProject project = factory.CreateAsync(uprjFilePath, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.AreEqual(434, project.Plugins.Count);
            Assert.AreEqual(1417, project.AllModules.Count);
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
        public void LoadUE5_CountFilesPerf() 
        {
            string uprjFilePath = @"C:\Program Files\Epic Games\UE_5.0\Engine";

            var files = XFilesystem.FindFiles(uprjFilePath, true, "*.*");
            
            Assert.AreEqual(166104, files.Count); //6 seconds con trasformazione in List
        }

        [TestMethod]
        public void LoadUE5_CountDirPerf()
        {
            string uprjFilePath = @"C:\Program Files\Epic Games\UE_5.0\Engine";

            var pluginsPath = TestUtils.PathCombine(uprjFilePath, "Plugins");
            var sourcePath = TestUtils.PathCombine(uprjFilePath, "Source");

            var plugDirs = XFilesystem.FindDirectories(pluginsPath, true);
            var sourceDirs = XFilesystem.FindDirectories(sourcePath, true);

            Assert.AreEqual(15536, plugDirs.Count); //6 seconds con trasformazione in List
            Assert.AreEqual(6190, sourceDirs.Count);
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

    }
}
