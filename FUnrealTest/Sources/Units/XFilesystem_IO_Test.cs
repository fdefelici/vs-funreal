using Microsoft.VisualStudio.TestTools.UnitTesting;
using FUnreal;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;

namespace FUnrealTest
{
    [TestClass]
    public class XFilesystem_IO_Test
    {

        [TestMethod]
        public void JsonWrite()
        {
            string tmpPath = TestUtils.AbsPath("XFilesystemTest");
            TestUtils.DeleteDir(tmpPath);

            string jsonFilePath = TestUtils.PathCombine(tmpPath, "file.json");
            string jsonStr = "{ \"First\" : 1, \"Second\" : 2 }";

            XFilesystem.WriteJsonFile(jsonFilePath, JObject.Parse(jsonStr));

            string jsonExpected = "{\r\n\t\"First\": 1,\r\n\t\"Second\": 2\r\n}";
            Assert.AreEqual(jsonExpected, TestUtils.ReadFile(jsonFilePath));
            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void RenameFileSameNameButDifferentCase()
        {
            string tmpPath = TestUtils.AbsPath("XFileSystemTest");
            TestUtils.DeleteDir(tmpPath);

            string filePath = TestUtils.PathCombine(tmpPath, "file.txt");
            TestUtils.MakeFile(filePath);

            XFilesystem.RenameFileName(filePath, "FILE");
            var files = XFilesystem.FindFilesEnum(tmpPath, false, "*.txt").ToList();
            Assert.AreEqual(1, files.Count);
            Assert.IsTrue(files[0].EndsWith("FILE.txt"));

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void FindEmptyDirs()
        {
            string tmpPath = TestUtils.AbsPath("XFileSystemTest");
            TestUtils.DeleteDir(tmpPath);


            var path01 = TestUtils.MakeDir(tmpPath, @"Hello");
            var path02 = TestUtils.MakeDir(tmpPath, @"Hello\Hello2");
            var path03 = TestUtils.MakeDir(tmpPath, @"Hello\Hello2\Hello3");
            var path04 = TestUtils.MakeDir(tmpPath, @"Other");
            var path05 = TestUtils.MakeDir(tmpPath, @"Other\Other2");
                         TestUtils.MakeFile(tmpPath, @"Other\Other2\file.txt");

            var result = XFilesystem.FindEmptyFoldersAsync(tmpPath).GetAwaiter().GetResult();

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(path03, result[0]);

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void IsFileLocked()
        {
            string tmpPath = TestUtils.AbsPath("XFileSystemTest");
            TestUtils.DeleteDir(tmpPath);

            string filePath = TestUtils.MakeFile(tmpPath, @"file.txt");

            TestUtils.FileLock(filePath);

            Assert.IsTrue(XFilesystem.FileIsLocked(filePath));

            TestUtils.FileUnlock(filePath);

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void DirHasAnyFileLocked()
        {
            string tmpPath = TestUtils.AbsPath("XFileSystemTest");
            TestUtils.DeleteDir(tmpPath);

            string filePath = TestUtils.MakeFile(tmpPath, @"locked.txt");
            TestUtils.MakeFile(tmpPath, @"free.txt");

            var streamBusy = System.IO.File.OpenWrite(filePath);

            Assert.IsTrue(XFilesystem.DirectoryHasAnyFileLocked(tmpPath, false, "*.txt", out string firstFileLocked));
            Assert.AreEqual(filePath, firstFileLocked);

            streamBusy.Close();

           
        }

        [TestMethod]
        public void FileExistsWithSearch() 
        {
            string tmpPath = TestUtils.AbsPath("XFileSystemTest");
            TestUtils.DeleteDir(tmpPath);

            TestUtils.MakeFile(tmpPath, @"file1.txt");
            TestUtils.MakeFile(tmpPath, @"file2.txt");
            TestUtils.MakeFile(tmpPath, @"sub/file3.txt");

            Assert.IsTrue(XFilesystem.FileExists(tmpPath, true, "*.txt", file => file.EndsWith("3.txt")));
            Assert.IsFalse(XFilesystem.FileExists(tmpPath, true, "*.txt", file => file.EndsWith("NOT_EXIST.txt")));

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void FileWithSearch()
        {
            string tmpPath = TestUtils.AbsPath("XFileSystemTest");
            TestUtils.DeleteDir(tmpPath);

            TestUtils.MakeFile(tmpPath, @"file1.txt");
            TestUtils.MakeFile(tmpPath, @"file2.txt");
            string file3 = TestUtils.MakeFile(tmpPath, @"sub/file3.txt");

            Assert.AreEqual(file3, XFilesystem.FindFile(tmpPath, true, "*.txt", file => file.EndsWith("3.txt")));
            Assert.IsNull(XFilesystem.FindFile(tmpPath, true, "*.txt", file => file.EndsWith("NOT_EXIST.txt")));

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void FindFilesEnum()
        {
            string tmpPath = TestUtils.AbsPath("XFileSystemTest");
            TestUtils.DeleteDir(tmpPath);

            var files = XFilesystem.FindFilesEnum(tmpPath, true, "*.*", file => true);

            Assert.IsFalse(files.Any());
            Assert.AreEqual(0, files.Count());

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void FilesForEach()
        {
            string tmpPath = TestUtils.AbsPath("XFileSystemTest");
            TestUtils.DeleteDir(tmpPath);

            TestUtils.MakeFile(tmpPath, @"file1.txt");
            TestUtils.MakeFile(tmpPath, @"file2.txt");
            TestUtils.MakeFile(tmpPath, @"sub/file3.txt");

            int count = 0;
            XFilesystem.FilesForEach(tmpPath, true, "*.*", file => count++);

            Assert.AreEqual(3, count);

            TestUtils.DeleteDir(tmpPath);
        }
    }
}
