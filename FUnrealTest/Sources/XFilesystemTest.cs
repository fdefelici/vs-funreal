using Microsoft.VisualStudio.TestTools.UnitTesting;
using FUnreal;
using Newtonsoft.Json.Linq;

namespace FUnrealTest
{
    [TestClass]
    public class XFilesystemTest
    {
        [TestMethod]
        public void PathCombineWithSpaces()
        {
            string result = XFilesystem.PathCombine("c:/mypath", "with space");

            string expected = "c:\\mypath\\with space";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void PathSubtract()
        {
            Assert.AreEqual("path", XFilesystem.PathSubtract("c:/base/middle/path", "c:/base/middle"));
            Assert.AreEqual("middle\\path", XFilesystem.PathSubtract("c:/base/middle/path", "c:/base/middle", true));
            Assert.AreEqual("path", XFilesystem.PathSubtract("c:/base/middle/path/", "c:/base/middle"));
        }

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
            var files = XFilesystem.FindFiles(tmpPath, false, "*.txt");
            Assert.AreEqual(1, files.Count);
            Assert.IsTrue(files[0].EndsWith("FILE.txt"));

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void PathChild()
        {
            Assert.AreEqual(@"Plugin01\Source\Module01", XFilesystem.PathChild(@"Plugins\Plugin01\Source\Module01", 1));
            Assert.AreEqual(@"Source\Module01",          XFilesystem.PathChild(@"Plugins\Plugin01\Source\Module01", 2));
            Assert.AreEqual("Module01",                  XFilesystem.PathChild(@"Plugins\Plugin01\Source\Module01", 3));
            Assert.AreEqual("",                          XFilesystem.PathChild(@"Plugins\Plugin01\Source\Module01", 4));
            Assert.AreEqual("",                          XFilesystem.PathChild(@"Plugins\Plugin01\Source\Module01", 5));
        } 
    }
}
