using FUnreal;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnrealTest
{
    [TestClass]
    public class XVersionTest
    {

        [TestMethod]
        public void FromUEStandardVersionWithMajorMinor()
        {
            XVersion version = XVersion.FromSemVer("5.0");
            Assert.AreEqual(5, version.Major);
            Assert.AreEqual(0, version.Minor);
            Assert.AreEqual("5.0", version.AsString());
        }

        [TestMethod]
        public void FromUEStandardVersionWithMajorMinorPatch()
        {
            XVersion version;

            version = XVersion.FromSemVer("4.26.2");
            Assert.AreEqual(4, version.Major);
            Assert.AreEqual(26, version.Minor);
            Assert.AreEqual(2, version.Patch);
            Assert.AreEqual("4.26.2", version.AsString());
        }


        [TestMethod]
        public void TestCustomUEVersion()
        {
            var version = XVersion.FromSemVer("4.26.2-ABC");
            Assert.AreEqual(4, version.Major);
            Assert.AreEqual(26, version.Minor);
            Assert.AreEqual(2, version.Patch);
            Assert.AreEqual("ABC", version.Label);
            Assert.AreEqual("4.26.2-ABC", version.AsString());

            version = XVersion.FromSemVer("4.26.2-A.B.C-OTHER");
            Assert.AreEqual(4, version.Major);
            Assert.AreEqual(26, version.Minor);
            Assert.AreEqual(2, version.Patch);
            Assert.AreEqual("A.B.C-OTHER", version.Label);
            Assert.AreEqual("4.26.2-A.B.C-OTHER", version.AsString());

            version = XVersion.FromSemVer("4.26.2-");
            Assert.AreEqual(4, version.Major);
            Assert.AreEqual(26, version.Minor);
            Assert.AreEqual(2, version.Patch);
            Assert.AreEqual("", version.Label);
            Assert.AreEqual("4.26.2", version.AsString());
        }
    }
}
