using Microsoft.VisualStudio.TestTools.UnitTesting;
using FUnreal;
using System;

namespace FUnrealTest
{
    [TestClass]
    public class FUnrealNativeProjectScannerTest
    {
        string resPath;

        [TestInitialize]
        public void SetUp()
        {
            resPath = TestUtils.AbsPath("Resources/FUnrealNativeProjectScannerTest");
        }

        [TestMethod]
        public void Scenario01_NoUProjectDirsFile()
        {
            var rootPath = TestUtils.PathCombine(resPath, "Scenario01");
            var scanner = new FUnrealNativeProjectScanner(rootPath);

            var filePaths = scanner.RetrieveUProjectFilePathsAsync().GetAwaiter().GetResult();
            Assert.AreEqual(0, filePaths.Count);
        }

        [TestMethod]
        public void Scenario02_NoUProjectFiles()
        {
            var rootPath = TestUtils.PathCombine(resPath, "Scenario02");
            var scanner = new FUnrealNativeProjectScanner(rootPath);

            var filePaths = scanner.RetrieveUProjectFilePathsAsync().GetAwaiter().GetResult();
            Assert.AreEqual(0, filePaths.Count);
        }

        [TestMethod]
        public void Scenario03_OneUProjectFile()
        {
            var rootPath = TestUtils.PathCombine(resPath, "Scenario03");
            var scanner = new FUnrealNativeProjectScanner(rootPath);

            var filePaths = scanner.RetrieveUProjectFilePathsAsync().GetAwaiter().GetResult();
            Assert.AreEqual(1, filePaths.Count);
            string expectedPath = TestUtils.PathCombine(rootPath, "MyProject/MyProject.uproject");
            Assert.AreEqual(expectedPath, filePaths[0]);
        }

        [TestMethod]
        public void Scenario03_TwoUProjectFiles()
        {
            var rootPath = TestUtils.PathCombine(resPath, "Scenario04");
            var scanner = new FUnrealNativeProjectScanner(rootPath);

            var filePaths = scanner.RetrieveUProjectFilePathsAsync().GetAwaiter().GetResult();
            Assert.AreEqual(2, filePaths.Count);

            string expectedPath;
            expectedPath = TestUtils.PathCombine(rootPath, "MyProject01/MyProject01.uproject");
            Assert.AreEqual(expectedPath, filePaths[0]);
            expectedPath = TestUtils.PathCombine(rootPath, "MyProject02/MyProject02.uproject");
            Assert.AreEqual(expectedPath, filePaths[1]);
        }

        [TestMethod]
        public void Scenario05_TwoProjectDirs_OneUProjectFile()
        {
            var rootPath = TestUtils.PathCombine(resPath, "Scenario05");
            var scanner = new FUnrealNativeProjectScanner(rootPath);

            var filePaths = scanner.RetrieveUProjectFilePathsAsync().GetAwaiter().GetResult();
            Assert.AreEqual(1, filePaths.Count);

            string expectedPath = TestUtils.PathCombine(rootPath, "Games/MyProject01/MyProject01.uproject");
            Assert.AreEqual(expectedPath, filePaths[0]);
        }

    }
}