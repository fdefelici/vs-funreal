using Microsoft.VisualStudio.TestTools.UnitTesting;
using FUnreal;

namespace FUnrealTest
{
    [TestClass]
    public class FUnrealUProjectDirsFileTest
    {
        string tmpPath;

        [TestInitialize]
        public void SetUp() 
        {
            tmpPath = TestUtils.AbsPath("FUnrealUProjectDirsFileTest");

            TestUtils.DeleteDir(tmpPath);
        }

        [TestCleanup]
        public void TearDown()
        {
            TestUtils.DeleteDir(tmpPath);
        }

        private string CreateFile(string contents)
        {
            string targetFile = TestUtils.PathCombine(tmpPath, "myfile.uprojectdirs");
            string targetContents = contents;
            TestUtils.WriteFile(targetFile, targetContents);
            return targetFile;
        }


        [TestMethod]
        public void ReadFile_WithCommentsAndEmptiesAnd3Paths()
        {
            var contents = File01_Comments_Empty_3Paths;
            var absFilePath = CreateFile(contents);

            var file = new FUnrealUProjectDirsFile(absFilePath);

            Assert.AreEqual(3, file.Paths.Count);
            Assert.AreEqual("./", file.Paths[0]);
            Assert.AreEqual("Engine/Source/", file.Paths[1]);
            Assert.AreEqual("Engine/Programs/", file.Paths[2]);

        }


        private const string File01_Comments_Empty_3Paths = @"
; These folders will be searched 1 level deep in order to find projects
; UnrealBuildTool will store the following information:
; - Project name
; - Location of project
; - Whether it has code or not
; - TargetNames contains at the project location
;
./
Engine/Source/
Engine/Programs/

";
    }
}