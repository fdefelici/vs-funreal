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
    public class XFilesystemEnumerableTest
    {
        [TestMethod]
        public void ExploringEnumerableApi()
        {
            Func<string, string> identity = item => item;
            var enume = new XFilesystemEnumerable(new List<string>() { "1", "2", "3" }, identity);

            Assert.AreEqual(3, enume.Count());
            Assert.AreEqual("1", enume.ElementAt(0));
            Assert.AreEqual("2", enume.ElementAt(1));
            Assert.AreEqual("3", enume.ElementAt(2));

            Assert.AreEqual("1", enume.First());
            Assert.AreEqual("3", enume.Last());

            string str = "";
            foreach (var each in enume)
            {
                str = str + each;
            }
            Assert.AreEqual("123", str);
        }


        [TestMethod]
        public void AdaptToLong()
        {
            var enume = XFilesystemEnumerable.AdaptToLong(new List<string>() { "1", @"\\?\2", "3" });

            Assert.AreEqual(3, enume.Count());
            Assert.AreEqual(@"\\?\1", enume.ElementAt(0));
            Assert.AreEqual(@"\\?\2", enume.ElementAt(1));
            Assert.AreEqual(@"\\?\3", enume.ElementAt(2));
        }

        [TestMethod]
        public void AdaptToNormal()
        {
            var enume = XFilesystemEnumerable.AdaptToNormal(new List<string>() { @"\\?\1", "2", @"\\?\3" });

            Assert.AreEqual(3, enume.Count());
            Assert.AreEqual("1", enume.ElementAt(0));
            Assert.AreEqual("2", enume.ElementAt(1));
            Assert.AreEqual("3", enume.ElementAt(2));
        }

    }
}
