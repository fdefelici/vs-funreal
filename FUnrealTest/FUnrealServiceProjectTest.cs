using FUnreal;
using Microsoft.VisualStudio.Package;
using System.Text.Json.Nodes;
namespace FUnrealTest
{
    [TestClass]
    public class FUnrealServiceProjectTest
    {
        [TestMethod]
        public void FindModuleByBelongingPath()
        {
            FUnrealProject project = new FUnrealProject("MyProject", @"C:\MyProject\MyProject.uproject");
            var plug01 = new FUnrealPlugin(project, "Plugin01", @"C:\MyProject\Plugins\PLUGCAT\Plugin01\Plugin01.uplugin");
            var mod01 = new FUnrealModule(plug01, "Module01", @"C:\MyProject\Plugins\PLUGCAT\Plugin01\Source\MODCAT\Module01\Module01.Build.cs");
            var mod02 = new FUnrealModule(plug01, "Module02", @"C:\MyProject\Plugins\PLUGCAT\Plugin01\Source\Module02\Module02.Build.cs");
            plug01.Modules.Add(mod01);
            plug01.Modules.Add(mod02);
            project.Plugins.Add(plug01);

            var mod03 = new FUnrealModule(project, "Module03", @"C:\MyProject\Source\Module03\Module03.Build.cs");
            project.GameModules.Add(mod03);

            project.AllModules.Add(mod01);
            project.AllModules.Add(mod02);
            project.AllModules.Add(mod03);


            var found = project.AllModules.FindByBelongingPath(@"C:\MyProject\Plugins\PLUGCAT\Plugin01\Source\MODCAT\Module01\Private");
            Assert.AreEqual("Module01", found.Name);
        }

    }
}