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


        class ContainerList
        {
            public Internal[] list;
        }


        [TestMethod]
        public void InternalSerializableClassFailure()
        {
            var container = new Container();
            container.intern = new Internal();

            var r = XObjectValidator.Validate(container);
            Assert.IsTrue(r.IsFailure);
        }

        [TestMethod]
        public void InternalSerializableClassSuccess()
        {
            var container = new Container();
            container.intern = new Internal();
            container.intern.name = "Hello";

            var r = XObjectValidator.Validate(container);
            Assert.IsTrue(r.IsSuccess);
        }

        [TestMethod]
        public void InternalNullSerializableClassSuccess()
        {
            var container = new Container();
            container.intern = null;

            var r = XObjectValidator.Validate(container);
            Assert.IsTrue(r.IsSuccess);
        }

        [TestMethod]
        public void ContainerListValidation()
        {
            var container = new ContainerList();
            container.list = new Internal[1];
            container.list[0] = new Internal();

            var r = XObjectValidator.Validate(container);
            Assert.IsTrue(r.IsFailure);

            container.list[0].name = "Hello";
            r = XObjectValidator.Validate(container);
            Assert.IsTrue(r.IsSuccess);
        }

        [TestMethod]

        public void ConstrainedStringAttrDescription()
        {
            var attr = new XStringContrainedValueAttrValidator("value1", "value2");
            Assert.AreEqual("One of [value1, value2]", attr.Description());
        }
    }
}
