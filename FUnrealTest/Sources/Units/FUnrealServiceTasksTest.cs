using Microsoft.VisualStudio.TestTools.UnitTesting;
using FUnreal;
using FUnreal.Sources.Core;

namespace FUnrealTest
{
    [TestClass]
    public class FUnrealServiceTasksTest
    {
        [TestMethod]
        public void ComputingHeaderIncludePath()
        {
            var project = new FUnrealProject(@"C:\MyProject\MyProject.uproject");
            var plugin01 = new FUnrealPlugin(project, @"Plugin01\Plugin01.uplugin");
            var module01 = new FUnrealModule(plugin01, @"Module01\Module01.Build.cs");

            //Public
            Assert.AreEqual("MyHeader.h", FUnrealServiceTasks.Module_ComputeHeaderIncludePath(module01, $@"{module01.PublicPath}\MyHeader.h"));
            Assert.AreEqual("Sub1/Sub2/MyHeader.h", FUnrealServiceTasks.Module_ComputeHeaderIncludePath(module01, $@"{module01.PublicPath}\Sub1\Sub2\MyHeader.h"));

            //Private
            Assert.AreEqual("MyHeader.h", FUnrealServiceTasks.Module_ComputeHeaderIncludePath(module01, $@"{module01.PrivatePath}\MyHeader.h"));
            Assert.AreEqual("Sub1/Sub2/MyHeader.h", FUnrealServiceTasks.Module_ComputeHeaderIncludePath(module01, $@"{module01.PrivatePath}\Sub1\Sub2\MyHeader.h"));

            //Custom
            Assert.AreEqual("MyHeader.h", FUnrealServiceTasks.Module_ComputeHeaderIncludePath(module01, $@"{module01.FullPath}\MyHeader.h"));
            Assert.AreEqual("Sub1/Sub2/MyHeader.h", FUnrealServiceTasks.Module_ComputeHeaderIncludePath(module01, $@"{module01.FullPath}\Sub1\Sub2\MyHeader.h"));

        }
    }
}
