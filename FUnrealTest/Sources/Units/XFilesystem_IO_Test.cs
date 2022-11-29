using Microsoft.VisualStudio.TestTools.UnitTesting;
using FUnreal;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System;

namespace FUnrealTest
{
    [TestClass]
    public class XFilesystem_IO_Test
    {
        private string tmpPath;

        [TestInitialize]
        public void SetUp()
        {
            tmpPath = TestUtils.AbsPath("XFilesystem_IO_Test");
            TestUtils.DeleteDir(tmpPath);
            TestUtils.MakeDir(tmpPath);
        }

        [TestCleanup]
        public void TearDown()
        {
            TestUtils.DeleteDir(tmpPath);
        }


        [TestMethod]
        public void JsonWrite()
        {
            string jsonFilePath = TestUtils.PathCombine(tmpPath, "file.json");
            string jsonStr = "{ \"First\" : 1, \"Second\" : 2 }";

            XFilesystem.JsonFileWrite(jsonFilePath, JObject.Parse(jsonStr));

            string jsonExpected = "{\r\n\t\"First\": 1,\r\n\t\"Second\": 2\r\n}";
            Assert.AreEqual(jsonExpected, TestUtils.ReadFile(jsonFilePath));
        }

        [TestMethod]
        public void RenameFileSameNameButDifferentCase()
        {
            string filePath = TestUtils.PathCombine(tmpPath, "file.txt");
            TestUtils.MakeFile(filePath);

            XFilesystem.FileRename(filePath, "FILE");
            var files = XFilesystem.FindFilesEnum(tmpPath, false, "*.txt").ToList();
            Assert.AreEqual(1, files.Count);
            Assert.IsTrue(files[0].EndsWith("FILE.txt"));
        }

        [TestMethod]
        public void FindEmptyDirs()
        {
            var path01 = TestUtils.MakeDir(tmpPath, @"Hello");
            var path02 = TestUtils.MakeDir(tmpPath, @"Hello\Hello2");
            var path03 = TestUtils.MakeDir(tmpPath, @"Hello\Hello2\Hello3");
            var path04 = TestUtils.MakeDir(tmpPath, @"Other");
            var path05 = TestUtils.MakeDir(tmpPath, @"Other\Other2");
                         TestUtils.MakeFile(tmpPath, @"Other\Other2\file.txt");

            var result = XFilesystem.FindEmptyFoldersAsync(tmpPath).GetAwaiter().GetResult();
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(path03, result[0]);
        }

        [TestMethod]
        public void IsFileLocked()
        {
            string filePath = TestUtils.MakeFile(tmpPath, @"file.txt");

            TestUtils.FileLock(filePath);

            Assert.IsTrue(XFilesystem.FileIsLocked(filePath));

            TestUtils.FileUnlock(filePath);
        }

        [TestMethod]
        public void DirHasAnyFileLocked()
        {
            string filePath = TestUtils.MakeFile(tmpPath, @"locked.txt");
            TestUtils.MakeFile(tmpPath, @"free.txt");

            TestUtils.FileLock(filePath);

            Assert.IsTrue(XFilesystem.DirContainsAnyFileLocked(tmpPath, false, "*.txt", out string firstFileLocked));
            Assert.AreEqual(filePath, firstFileLocked);

            TestUtils.FileUnlock(filePath);
        }

        [TestMethod]
        public void FileExistsWithSearch() 
        {
            TestUtils.MakeFile(tmpPath, @"file1.txt");
            TestUtils.MakeFile(tmpPath, @"file2.txt");
            TestUtils.MakeFile(tmpPath, @"sub/file3.txt");

            Assert.IsTrue(XFilesystem.FileExists(tmpPath, true, "*.txt", file => file.EndsWith("3.txt")));
            Assert.IsFalse(XFilesystem.FileExists(tmpPath, true, "*.txt", file => file.EndsWith("NOT_EXIST.txt")));
        }

        [TestMethod]
        public void FileWithSearch()
        {
            TestUtils.MakeFile(tmpPath, @"file1.txt");
            TestUtils.MakeFile(tmpPath, @"file2.txt");
            string file3 = TestUtils.MakeFile(tmpPath, @"sub/file3.txt");

            Assert.AreEqual(file3, XFilesystem.FindFile(tmpPath, true, "*.txt", file => file.EndsWith("3.txt")));
            Assert.IsNull(XFilesystem.FindFile(tmpPath, true, "*.txt", file => file.EndsWith("NOT_EXIST.txt")));
        }

        [TestMethod]
        public void FindFilesEnum()
        {
            var files = XFilesystem.FindFilesEnum(tmpPath, true, "*.*", file => true);

            Assert.IsFalse(files.Any());
            Assert.AreEqual(0, files.Count());
        }

        [TestMethod]
        public void FilesForEach()
        {
            TestUtils.MakeFile(tmpPath, @"file1.txt");
            TestUtils.MakeFile(tmpPath, @"file2.txt");
            TestUtils.MakeFile(tmpPath, @"sub/file3.txt");

            int count = 0;
            XFilesystem.FilesForEach(tmpPath, true, "*.*", file => count++);

            Assert.AreEqual(3, count);
        }

        private string LongFile()
        {
            string longPathFile = "";
            for (int i = 0; i < 30; i++)
            {
                longPathFile = XFilesystem.PathCombine(longPathFile, $"{i}ABCDEFGHI");
            }
            longPathFile = XFilesystem.PathCombine(longPathFile, "file.txt");

            longPathFile = XFilesystem.PathCombine(tmpPath, longPathFile);
            return longPathFile;
        }

        private string LongDir()
        {
            return XFilesystem.PathParent(LongFile());
        }


        [TestMethod]
        public void LongFilePath_DirCrud()
        {
            string longPathDir = LongDir();
   
            Assert.IsTrue(XFilesystem.DirCreate(longPathDir));
            Assert.IsTrue(XFilesystem.DirExists(longPathDir));
            Assert.IsTrue(XFilesystem.DirIsEmpty(longPathDir));

            string renamed = XFilesystem.RenameDirAsync(longPathDir, "renamed").GetAwaiter().GetResult();
            Assert.IsNotNull(renamed);
            Assert.IsTrue(XFilesystem.DirDelete(renamed));
        }

        [TestMethod]
        public void LongFilePath_DirDeepCopy()
        {
            string longDir = LongDir();

            TestUtils.MakeFile(longDir, "file1.txt");
            TestUtils.MakeFile(longDir, "sub/file2.txt");
            TestUtils.MakeFile(longDir, "sub/sub/file3.txt");

            string targetDir = TestUtils.PathCombine(TestUtils.PathParent(longDir), "target");

            bool success;

            TestUtils.DeleteDir(targetDir);
            success = XFilesystem.DirDeepCopy(longDir, targetDir);
            Assert.IsTrue(success);
            Assert.IsTrue(TestUtils.ExistsFile(targetDir, "file1.txt"));
            Assert.IsTrue(TestUtils.ExistsFile(targetDir, "sub/file2.txt"));
            Assert.IsTrue(TestUtils.ExistsFile(targetDir, "sub/sub/file3.txt"));

            TestUtils.DeleteDir(targetDir);
            success = XFilesystem.DirDeepCopy(longDir, targetDir, new NullDeepCopyVisitor());
            Assert.IsTrue(success);
            Assert.IsTrue(TestUtils.ExistsFile(targetDir, "file1.txt"));
            Assert.IsTrue(TestUtils.ExistsFile(targetDir, "sub/file2.txt"));
            Assert.IsTrue(TestUtils.ExistsFile(targetDir, "sub/sub/file3.txt"));

            TestUtils.DeleteDir(targetDir);
            success = XFilesystem.DirDeepCopyAsync(longDir, targetDir).GetAwaiter().GetResult();
            Assert.IsTrue(success);
            Assert.IsTrue(TestUtils.ExistsFile(targetDir, "file1.txt"));
            Assert.IsTrue(TestUtils.ExistsFile(targetDir, "sub/file2.txt"));
            Assert.IsTrue(TestUtils.ExistsFile(targetDir, "sub/sub/file3.txt"));

            TestUtils.DeleteDir(targetDir);
            success = XFilesystem.DirDeepCopyAsync(longDir, targetDir, new NullDeepCopyVisitor()).GetAwaiter().GetResult();
            Assert.IsTrue(success);
            Assert.IsTrue(TestUtils.ExistsFile(targetDir, "file1.txt"));
            Assert.IsTrue(TestUtils.ExistsFile(targetDir, "sub/file2.txt"));
            Assert.IsTrue(TestUtils.ExistsFile(targetDir, "sub/sub/file3.txt"));
        }

        [TestMethod]
        public void LongFilePath_FileCrud()
        {
            string longPathFile = LongFile();

            Assert.IsTrue(XFilesystem.FileCreate(longPathFile));
            Assert.IsTrue(XFilesystem.FileExists(longPathFile));
            Assert.IsTrue(XFilesystem.FileDelete(longPathFile));

            Assert.IsTrue(XFilesystem.FileWrite(longPathFile, "Hello"));
            Assert.AreEqual("Hello", XFilesystem.FileRead(longPathFile));

            string expRenamed = TestUtils.PathCombine(TestUtils.PathParent(longPathFile), "renamed.txt");
            Assert.AreEqual(expRenamed, XFilesystem.FileRename(longPathFile, "renamed"));

            TestUtils.DeleteFile(expRenamed);
            TestUtils.MakeFile(longPathFile);
            Assert.AreEqual(expRenamed, XFilesystem.FileRenameWithExt(longPathFile, "renamed.txt"));

            TestUtils.DeleteFile(expRenamed);
            TestUtils.MakeFile(longPathFile);
            Assert.IsTrue(XFilesystem.FileCopy(longPathFile, expRenamed));
        }

        [TestMethod]
        public void LongFilePath_FileLocking()
        {
            string longPathFile = LongFile();
            string parePath = XFilesystem.PathParent(longPathFile);

            TestUtils.MakeFile(longPathFile);

            Assert.IsFalse(XFilesystem.FileIsLocked(longPathFile));
            Assert.IsFalse(XFilesystem.DirContainsAnyFileLocked(parePath, true, "*.*", out string _));

            TestUtils.FileLock(longPathFile);
            Assert.IsTrue(XFilesystem.FileIsLocked(longPathFile));
            Assert.IsTrue(XFilesystem.DirContainsAnyFileLocked(parePath, true, "*.*", out string firstFileLocked));
            Assert.AreEqual(longPathFile, firstFileLocked);

            TestUtils.FileUnlock(longPathFile);
        }

        [TestMethod]
        public void LongFilePath_JsonFile()
        {
            string longPathFile = LongDir();
            TestUtils.WriteFile(longPathFile, "{ }");

            var json = XFilesystem.JsonFileRead(longPathFile);
            Assert.IsNotNull(json);

            string parent = TestUtils.PathParent(longPathFile);
            string newFile = TestUtils.PathCombine(parent, "newFile.json");
            XFilesystem.JsonFileWrite(newFile, json);

            Assert.IsTrue(TestUtils.ExistsFile(newFile));
        }


        [TestMethod]
        public void LongFilePath_DirFinds()
        {
            string longPathDir = LongDir();
            TestUtils.MakeDir(longPathDir);
            
            string parentPath = XFilesystem.PathParent(longPathDir);

            IEnumerable<string> found;
            
            found = XFilesystem.FindDirsEnum(parentPath);
            Assert.AreEqual(1, found.Count());
            Assert.AreEqual(longPathDir, found.First());

            found = XFilesystem.FindEmptyFoldersAsync(parentPath).GetAwaiter().GetResult();
            Assert.AreEqual(1, found.Count());
            Assert.AreEqual(longPathDir, found.First());


            TestUtils.MakeFile(longPathDir, "file1.txt");
            Assert.IsTrue(XFilesystem.DirContainsAnyFile(parentPath, true));
        }

        [TestMethod]
        public void LongFilePath_FileFinds()
        {
            string longDir = LongDir();
            string file1 = TestUtils.MakeFile(longDir, "file1.txt");
            string file2 = TestUtils.MakeFile(longDir, "sub/file2.txt");
            string file3 = TestUtils.MakeFile(longDir, "sub/sub/file3.txt");
            string dire1 = TestUtils.MakeDir(longDir, "empty");

            string parentPath = XFilesystem.PathParent(longDir);

            Assert.AreEqual(file3, XFilesystem.FindFile(parentPath, true, "file3.txt"));
            Assert.AreEqual(file3, XFilesystem.FindFile(parentPath, true, "*.txt", file => file.EndsWith("file3.txt")));

            IEnumerable<string> found;
            
            found = XFilesystem.FindFilesEnum(parentPath, true, "*.txt");
            Assert.AreEqual(3, found.Count());
            Assert.AreEqual(file3, found.ElementAt(2));

            found = XFilesystem.FindFilesEnum(parentPath, true, "*.txt", file => true);
            Assert.AreEqual(3, found.Count());
            Assert.AreEqual(file3, found.ElementAt(2));

            found = XFilesystem.FindFilesEnumAsync(parentPath, true, "*.txt").GetAwaiter().GetResult();
            Assert.AreEqual(3, found.Count());
            Assert.AreEqual(file3, found.ElementAt(2));

            found = XFilesystem.FindFilesStoppingDepth(parentPath, "*.txt");
            Assert.AreEqual(1, found.Count());
            Assert.AreEqual(file1, found.First());

            int count = 0;
            XFilesystem.FilesForEach(parentPath, true, "*.*", file => count++);
            Assert.AreEqual(3, count);
        }
    }
}
