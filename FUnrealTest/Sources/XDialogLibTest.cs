using Microsoft.VisualStudio.TestTools.UnitTesting;
using FUnreal;

namespace FUnrealTest
{
    [TestClass]
    public class XDialogLibTest
    {
        [TestMethod]
        public void CheckValidFilename()
        {
            Assert.IsTrue(XDialogLib.IsValidFileNameWitExt(".valid"));
            Assert.IsTrue(XDialogLib.IsValidFileNameWitExt(".valid.one"));
            Assert.IsTrue(XDialogLib.IsValidFileNameWitExt(".valid.one."));
            Assert.IsTrue(XDialogLib.IsValidFileNameWitExt("_Hello_"));

            Assert.IsTrue(XDialogLib.IsValidFileNameWitExt("._________"));

            Assert.IsFalse(XDialogLib.IsValidFileNameWitExt("."));
            Assert.IsFalse(XDialogLib.IsValidFileNameWitExt(".."));
            Assert.IsFalse(XDialogLib.IsValidFileNameWitExt("..Hello"));
            Assert.IsFalse(XDialogLib.IsValidFileNameWitExt(".........."));
            Assert.IsFalse(XDialogLib.IsValidFileNameWitExt("__________"));
            Assert.IsFalse(XDialogLib.IsValidFileNameWitExt("..________"));

        }
    }
}
