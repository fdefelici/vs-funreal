using Microsoft.VisualStudio.TestTools.UnitTesting;
using FUnreal;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

namespace FUnrealTest
{
    [TestClass]
    public class XFilesystem_Path_Test
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
        public void PathChild()
        {
            Assert.AreEqual(@"Plugin01\Source\Module01", XFilesystem.PathChild(@"Plugins\Plugin01\Source\Module01", 1));
            Assert.AreEqual(@"Source\Module01",          XFilesystem.PathChild(@"Plugins\Plugin01\Source\Module01", 2));
            Assert.AreEqual("Module01",                  XFilesystem.PathChild(@"Plugins\Plugin01\Source\Module01", 3));
            Assert.AreEqual("",                          XFilesystem.PathChild(@"Plugins\Plugin01\Source\Module01", 4));
            Assert.AreEqual("",                          XFilesystem.PathChild(@"Plugins\Plugin01\Source\Module01", 5));
        }

        [TestMethod]
        public void IsChildPath()
        {
            Assert.IsTrue(XFilesystem.IsChildPath(@"aa\bb\cc", @"aa\bb"));
            Assert.IsFalse(XFilesystem.IsChildPath(@"aa\bb", @"a\b"));
            Assert.IsFalse(XFilesystem.IsChildPath(@"aa\b", @"aa\b"));
            Assert.IsTrue(XFilesystem.IsChildPath(@"aa\b", @"aa\b", true));

            Assert.IsTrue(XFilesystem.IsChildPath(@"C:\aa\bb\cc", @"C:\aa\bb"));
        }

        [TestMethod]
        public void PathParent()
        {
            Assert.AreEqual("", XFilesystem.PathParent("file.txt"));
            Assert.AreEqual("", XFilesystem.PathParent(""));

            Assert.AreEqual(@"C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Source", XFilesystem.PathParent(@"C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Source\file.txt"));
        }


        [TestMethod]
        public void SelectCommonBasePath()
        {
            var paths = new List<string>
            {
                @"a\b\c\d\e\f\g\h",  //reference string - longest
                @"a\b\c\d",
                @"a\b\c\d\e",
                @"a\bb\cc\dd\e"
            };

            Assert.AreEqual(@"a", XFilesystem.SelectCommonBasePath(paths));

            paths = new List<string>
            {
                @"a\b\c\d",      //reference string - shortest
                @"a\b\c\d\e\f",  
                @"a\b\c\d\e",
                @"a\bb\cc\dd\e"
            };

            Assert.AreEqual(@"a", XFilesystem.SelectCommonBasePath(paths));

            paths = new List<string>
            {
                @"a\b\c\d",      //reference string - shortest
                @"a\b\c\d\e\f",
                @"a\b\c\d\e",
                @"a\b\c\dd\e"
            };

            Assert.AreEqual(@"a\b\c", XFilesystem.SelectCommonBasePath(paths));


            paths = new List<string>
            {
                @"a\b\c\d",      //reference string scanned untile the end
                @"a\b\c\d\e\f",
            };

            Assert.AreEqual(@"a\b\c\d", XFilesystem.SelectCommonBasePath(paths));

            paths = new List<string>
            {
                @"C:\MyProject\Source\NewGameModule4\Prova1\Prova2\NewFile.ext",     
                @"C:\MyProject\Source\NewGameModule4\Prova1\Prova2\Prova3\NewFile.ext"
            };
            Assert.AreEqual(@"C:\MyProject\Source\NewGameModule4\Prova1\Prova2", XFilesystem.SelectCommonBasePath(paths));
        }

        [TestMethod]
        public void PathBase()
        {
            var path = @"a\b\c\d\e\f";

            Assert.AreEqual(@"",            XFilesystem.PathBase(path, 0));
            Assert.AreEqual(@"a",           XFilesystem.PathBase(path, 1));
            Assert.AreEqual(@"a\b",         XFilesystem.PathBase(path, 2));
            Assert.AreEqual(@"a\b\c",       XFilesystem.PathBase(path, 3));
            Assert.AreEqual(@"a\b\c\d\e\f", XFilesystem.PathBase(path, 100));

            Assert.AreEqual(@"C:", XFilesystem.PathBase(@"C:\some\other", 1));
            Assert.AreEqual(@"C:\some", XFilesystem.PathBase(@"C:\some\other", 2));
        }

        [TestMethod]
        public void IsParentPath()
        {

            var path01 = @"Hello";
            var path02 = @"Hello\Hello2";
            var path03 = @"Hello\Hello2\Hello3";
            var path04 = @"Other";
            
            Assert.IsTrue(XFilesystem.IsParentPath(path01, path02));
            Assert.IsTrue(XFilesystem.IsParentPath(path02, path03));
            Assert.IsTrue(XFilesystem.IsParentPath(path01, path03));
            Assert.IsFalse(XFilesystem.IsParentPath(path01, path04));
            Assert.IsFalse(XFilesystem.IsParentPath(path01, path01));
            Assert.IsTrue(XFilesystem.IsParentPath(path01, path01, true));
        }

        [TestMethod]
        public void PathCombine()
        {
            Assert.AreEqual(@"C:\some", XFilesystem.PathCombine("C:", "some"));

            Assert.AreEqual(@"Hello", XFilesystem.PathCombine("Hello", "", ""));

            Assert.AreEqual(@"Hello", XFilesystem.PathCombine("", "Hello", ""));

            Assert.AreEqual(@"C:\some", XFilesystem.PathCombine("C:\\some", "."));
            Assert.AreEqual(@"C:\some", XFilesystem.PathCombine("C:\\some", "./"));
            Assert.AreEqual(@"C:\some\other", XFilesystem.PathCombine("C:\\some", "./other"));
        }

        [TestMethod]
        public void ChangeFilePathName()
        {
           Assert.AreEqual("HelloRenamed.h", XFilesystem.ChangeFilePathName("Hello.h", "HelloRenamed"));
        }

    }
}
