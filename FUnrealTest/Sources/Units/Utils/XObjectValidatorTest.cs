using Microsoft.VisualStudio.TestTools.UnitTesting;
using FUnreal;
using System;

namespace FUnrealTest
{
    [TestClass]
    public class XObjectValidatorTest
    {
        
        class Container
        {
            public Internal intern;
            public int value = 0;
        }

        class Internal
        {
            [XNotNullAttrValidator]
            public string name = null;
        }


        [TestMethod]
        public void InternalSerializableClassFailure()
        {
            var container = new Container();
            container.intern = new Internal();

            Assert.IsFalse(XObjectValidator.Validate(container, true));
        }

        [TestMethod]
        public void InternalSerializableClassSuccess()
        {
            var container = new Container();
            container.intern = new Internal();
            container.intern.name = "Hello";

            Assert.IsTrue(XObjectValidator.Validate(container));
        }

        [TestMethod]
        public void InternalNullSerializableClassSuccess()
        {
            var container = new Container();
            container.intern = null;

            Assert.IsTrue(XObjectValidator.Validate(container));
        }
    }
}
