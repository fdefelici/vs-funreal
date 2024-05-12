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
   
            string vsixPath = @"C:\Users\fdf82\AppData\Local\Microsoft\VisualStudio\17.0_b7638a25Exp\Extensions\Federico De Felici\FUnreal\0.1.0";

            string tplPath = TestUtils.PathCombine(vsixPath, @"Templates\UE5\Plugins\ThirdPartyLibrary");

            //string filePath = @"\\?\C:\Users\fdf82\AppData\Local\Microsoft\VisualStudio\17.0_b7638a25Exp\Extensions\Federico De Felici\FUnreal\0.1.0\Templates\UE5\Plugins\ThirdPartyLibrary\@{TPL_PLUGIN_NAME}\Source\ThirdParty\@{TPL_MODULE_NAME}Library\x64\Release\ExampleLibrary.dll";

            string filePath = TestUtils.PathCombine(tplPath, @"@{TPL_PLUGIN_NAME}\Source\ThirdParty\@{TPL_MODULE_NAME}Library\x64\Release\ExampleLibrary.dll");

            Assert.IsTrue(File.Exists(filePath));

            bool success = XFilesystem.DirDeepCopy(tplPath, tmpPath);
            Assert.IsTrue(success);
        }

    }
}
