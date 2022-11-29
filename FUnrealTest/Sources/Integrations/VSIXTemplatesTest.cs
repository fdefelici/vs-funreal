using Microsoft.VisualStudio.TestTools.UnitTesting;

using FUnreal;
using System.IO;
using System;
using System.Collections.Generic;

namespace FUnrealTest.Integrations
{
    [TestClass]
    public class VISXTemplateTest
    {
        private string tmpPath;

        [TestInitialize]
        public void SetUp()
        {
            tmpPath = TestUtils.AbsPath("VISXTemplateTest");
        }

        [TestCleanup]
        public void TearDown()
        {
            TestUtils.DeleteDir(tmpPath);
        }


        [TestMethod]
        public void Simple()
        {
            string vsixPath = @"C:\Users\fdf82\AppData\Local\Microsoft\VisualStudio\17.0_ee860280Exp\Extensions\Federico De Felici\FUnreal\1.0";

            string tplPath = TestUtils.PathCombine(vsixPath, @"Templates\UE5\Plugins\ThirdPartyLibrary");

            string filePath = @"\\?\C:\Users\fdf82\AppData\Local\Microsoft\VisualStudio\17.0_ee860280Exp\Extensions\Federico De Felici\FUnreal\1.0\Templates\UE5\Plugins\ThirdPartyLibrary\@{TPL_PLUG_NAME}\Source\ThirdParty\@{TPL_MODU_NAME}Library\ExampleLibrary\ExampleLibrary.xcodeproj\project.pbxproj";
            
            Assert.IsTrue(File.Exists(filePath));

            bool success = XFilesystem.DirDeepCopy(tplPath, tmpPath);
            Assert.IsTrue(success);
        }

    }
}
