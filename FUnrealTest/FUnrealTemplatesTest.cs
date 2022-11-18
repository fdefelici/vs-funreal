using FUnreal;

namespace FUnrealTest
{
    [TestClass]
    public class FUnrealTemplatesTest
    {
        [TestMethod]
        public void LoadFromFileThreeTemplates()
        {
            string basePath = TestUtils.AbsPath("FUnrealTemplatesTest");
            string descPath = TestUtils.PathCombine(basePath, "desc.xml");
            TestUtils.WriteFile(descPath, descXML);

            FUnrealTemplates templates = FUnrealTemplates.Load(descPath);

            Assert.AreEqual(3, templates.Count);

            FUnrealTemplate tpl1 = templates.GetTemplate("plugins", "4", "tpl_plugin_blank");
            Assert.AreEqual("tpl_plugin_blank", tpl1.Name);
            Assert.AreEqual(TestUtils.PathCombine(basePath, "UE5/Plugins/Blank"), tpl1.BasePath);
            Assert.AreEqual("Blank", tpl1.Label);
            Assert.AreEqual("Create a blank plugin", tpl1.Description);
            Assert.AreEqual("true", tpl1.GetMeta("has_module"));

            FUnrealTemplate tpl2 = templates.GetTemplate("plugins", "5", "tpl_plugin_blank");
            Assert.AreEqual("tpl_plugin_blank", tpl2.Name);
            Assert.AreEqual(TestUtils.PathCombine(basePath, "UE5/Plugins/Blank"), tpl2.BasePath);
            Assert.AreEqual("Blank", tpl2.Label);
            Assert.AreEqual("Create a blank plugin", tpl2.Description);
            Assert.AreEqual("true", tpl2.GetMeta("has_module"));

            FUnrealTemplate tpl3 = templates.GetTemplate("other", "5", "tpl_other");
            Assert.AreEqual("tpl_other", tpl3.Name);
            Assert.AreEqual(TestUtils.PathCombine(basePath, "UE5/Other"), tpl3.BasePath);
            Assert.AreEqual("OtherUi", tpl3.Label);
            Assert.AreEqual("OtherUiDesc", tpl3.Description);
            Assert.AreEqual("false", tpl2.GetMeta("has_module"));

            TestUtils.DeleteDir(basePath);
        }


        private const string descXML = @"<?xml version=""1.0"" encoding=""utf-8""?>
<templates>
	<template context=""plugins"" name=""tpl_plugin_blank"" ue=""4,5"" path=""UE5/Plugins/Blank"">
        <meta has_module=""true""/>
        <ui label=""Blank"" desc=""Create a blank plugin""/>
	</template>
    <template context=""other"" name=""tpl_other"" ue=""5"" path=""UE5/Other"">
        <meta has_module=""false""/>
        <ui label=""OtherUi"" desc=""OtherUiDesc""/>
	</template>
</templates>
";
    }
}