using Microsoft.VisualStudio.TestTools.UnitTesting;

using FUnreal;

namespace FUnrealTest
{
    [TestClass]
    public class FUnrealTemplatesTest
    {
        [TestMethod]
        public void LoadTemplates_WithMissingPlugins()
        {
            string basePath = TestUtils.AbsPath("FUnrealTemplatesTest");
            string descPath = TestUtils.PathCombine(basePath, "descr_0_plugins.json");
            TestUtils.WriteFile(descPath, descr_0_plugins);

            FUnrealTemplatesRules rules = new FUnrealTemplatesRules();
            FUnrealTemplates templates;
            bool success;

            rules.MustHavePlugins = true;
            success = FUnrealTemplates.TryLoad_V1_0(descPath, rules, out templates);
            Assert.IsFalse(success);
            Assert.IsNull(templates);

            rules.MustHavePlugins = false;
            success = FUnrealTemplates.TryLoad_V1_0(descPath, rules, out templates);
            Assert.IsTrue(success);
            Assert.AreEqual(0, templates.Count);

            TestUtils.DeleteDir(basePath);
        }

        [TestMethod]
        public void LoadTemplates_With3Plugins()
        {
            string basePath = TestUtils.AbsPath("FUnrealTemplatesTest");
            string descPath = TestUtils.PathCombine(basePath, "descr_3_plugins.json");
            TestUtils.WriteFile(descPath, descr_3_plugins);

            FUnrealTemplatesRules rules = new FUnrealTemplatesRules();
            rules.MustHavePlugins = true;

            bool success = FUnrealTemplates.TryLoad_V1_0(descPath, rules, out FUnrealTemplates templates);
            Assert.IsTrue(success);

            Assert.AreEqual(3, templates.Count);

            FUnrealPluginTemplate tpl1 = templates.GetPlugin("4", "tpl_plugin_blank");
            Assert.AreEqual("tpl_plugin_blank", tpl1.Name);
            Assert.AreEqual(TestUtils.PathCombine(basePath, "UE5/Plugins/Blank"), tpl1.BasePath);
            Assert.AreEqual("Blank", tpl1.Label);
            Assert.AreEqual("Create a blank plugin", tpl1.Description);
            Assert.AreEqual(true, tpl1.HasModule);

            FUnrealPluginTemplate tpl2 = templates.GetPlugin("5", "tpl_plugin_blank");
            Assert.AreEqual("tpl_plugin_blank", tpl2.Name);
            Assert.AreEqual(TestUtils.PathCombine(basePath, "UE5/Plugins/Blank"), tpl2.BasePath);
            Assert.AreEqual("Blank", tpl2.Label);
            Assert.AreEqual("Create a blank plugin", tpl2.Description);
            Assert.AreEqual(true, tpl2.HasModule);

            FUnrealPluginTemplate tpl3 = templates.GetPlugin("5", "tpl_plugin_other");
            Assert.AreEqual("tpl_plugin_other", tpl3.Name);
            Assert.AreEqual(TestUtils.PathCombine(basePath, "UE5/Other"), tpl3.BasePath);
            Assert.AreEqual("OtherUi", tpl3.Label);
            Assert.AreEqual("OtherUiDesc", tpl3.Description);
            Assert.AreEqual(false, tpl3.HasModule);

            TestUtils.DeleteDir(basePath);
        }

        [TestMethod]
        public void LoadTemplates_With2PluginModules()
        {
            string basePath = TestUtils.AbsPath("FUnrealTemplatesTest");
            string descPath = TestUtils.PathCombine(basePath, "descr_2_plugin_modules.json");
            TestUtils.WriteFile(descPath, descr_2_plugin_modules);

            FUnrealTemplatesRules rules = new FUnrealTemplatesRules();
            rules.MustHavePluginModules = true;

            bool success = FUnrealTemplates.TryLoad_V1_0(descPath, rules, out FUnrealTemplates templates);
            Assert.IsTrue(success);

            Assert.AreEqual(2, templates.Count);

            FUnrealPluginModuleTemplate tpl1 = templates.GetPluginModule("4", "tpl_module_blank");
            Assert.AreEqual("tpl_module_blank", tpl1.Name);
            Assert.AreEqual(TestUtils.PathCombine(basePath, "UE5/Modules/Blank"), tpl1.BasePath);
            Assert.AreEqual("Blank", tpl1.Label);
            Assert.AreEqual("Create a blank module", tpl1.Description);
            Assert.AreEqual("Runtime", tpl1.Type);
            Assert.AreEqual("Default", tpl1.Phase);

            FUnrealPluginModuleTemplate tpl2 = templates.GetPluginModule("5", "tpl_module_blank");
            Assert.AreEqual("tpl_module_blank", tpl2.Name);
            Assert.AreEqual(TestUtils.PathCombine(basePath, "UE5/Modules/Blank"), tpl2.BasePath);
            Assert.AreEqual("Blank", tpl2.Label);
            Assert.AreEqual("Create a blank module", tpl2.Description);
            Assert.AreEqual("Runtime", tpl2.Type);
            Assert.AreEqual("Default", tpl2.Phase);

            TestUtils.DeleteDir(basePath);
        }

        [TestMethod]
        public void LoadTemplates_With1GameModule()
        {
            string basePath = TestUtils.AbsPath("FUnrealTemplatesTest");
            string descPath = TestUtils.PathCombine(basePath, "descr_1_game_module.json");
            TestUtils.WriteFile(descPath, descr_1_game_module);

            FUnrealTemplatesRules rules = new FUnrealTemplatesRules();
            rules.MustHaveGameModules = true;

            bool success = FUnrealTemplates.TryLoad_V1_0(descPath, rules, out FUnrealTemplates templates);
            Assert.IsTrue(success);

            Assert.AreEqual(1, templates.Count);

            FUnrealGameModuleTemplate tpl1 = templates.GetGameModule("4", "tpl_module_blank");
            Assert.AreEqual("tpl_module_blank", tpl1.Name);
            Assert.AreEqual(TestUtils.PathCombine(basePath, "UE4/Modules/Blank"), tpl1.BasePath);
            Assert.AreEqual("Blank", tpl1.Label);
            Assert.AreEqual("Create a blank module", tpl1.Description);
            Assert.AreEqual("Runtime", tpl1.Type);
            Assert.AreEqual("Default", tpl1.Phase);
            Assert.AreEqual("Editor", tpl1.Target);

            TestUtils.DeleteDir(basePath);
        }

        [TestMethod]
        public void LoadTemplates_With1Source()
        {
            string basePath = TestUtils.AbsPath("FUnrealTemplatesTest");
            string descPath = TestUtils.PathCombine(basePath, "descr_1_source.json");
            TestUtils.WriteFile(descPath, descr_1_source);

            FUnrealTemplatesRules rules = new FUnrealTemplatesRules();
            rules.MustHaveSources = true;

            bool success = FUnrealTemplates.TryLoad_V1_0(descPath, rules, out FUnrealTemplates templates);
            Assert.IsTrue(success);

            Assert.AreEqual(1, templates.Count);

            FUnrealSourceTemplate tpl1 = templates.GetSource("4", "tpl_class_empty");
            Assert.AreEqual("tpl_class_empty", tpl1.Name);
            Assert.AreEqual(TestUtils.PathCombine(basePath, "UE4/Sources/Classes"), tpl1.BasePath);
            Assert.AreEqual("Empty", tpl1.Label);
            Assert.AreEqual("An empty class", tpl1.Description);
            Assert.AreEqual("Empty.h", tpl1.Header);
            Assert.AreEqual("Empty.cpp", tpl1.Source);

            TestUtils.DeleteDir(basePath);
        }


        private const string descr_0_plugins = @"
        {
           ""version"" : ""1.0"",
           ""templates"" : {
        
           } 
        }
        ";
        private const string descr_3_plugins = @"
        {
           ""version"" : ""1.0"",
           ""templates"" : {
                ""plugins"" : [
                    { ""name"":""tpl_plugin_blank"", ""ue"":""4,5"", ""path"":""UE5/Plugins/Blank"",
                      ""ui"": { ""label"":""Blank"", ""desc"":""Create a blank plugin"" },
                      ""meta"": { ""has_module"": true }
                    },
                    { ""name"":""tpl_plugin_other"", ""ue"":""5"", ""path"":""UE5/Other"",
                      ""ui"": { ""label"":""OtherUi"", ""desc"":""OtherUiDesc"" },
                      ""meta"": { ""has_module"": false }
                    }
                ]
           } 
        }
        ";
        private const string descr_2_plugin_modules = @"
        {
           ""version"" : ""1.0"",
           ""templates"" : {
                ""plugin_modules"" : [
                    { ""name"":""tpl_module_blank"", ""ue"":""4,5"", ""path"":""UE5/Modules/Blank"",
                      ""ui"": { ""label"":""Blank"", ""desc"":""Create a blank module"" },
                      ""meta"": { ""type"": ""Runtime"", ""phase"": ""Default"" }
                    }
                ]
           } 
        }
        ";
        private const string descr_1_game_module = @"
        {
           ""version"" : ""1.0"",
           ""templates"" : {
                ""game_modules"" : [
                    { ""name"":""tpl_module_blank"", ""ue"":""4"", ""path"":""UE4/Modules/Blank"",
                      ""ui"": { ""label"":""Blank"", ""desc"":""Create a blank module"" },
                      ""meta"": { ""type"": ""Runtime"", ""phase"": ""Default"", ""target"": ""Editor"" }
                    }
                ]
           } 
        }
        ";
        private const string descr_1_source = @"
        {
           ""version"" : ""1.0"",
           ""templates"" : {
                ""sources"" : [
                    { ""name"":""tpl_class_empty"", ""ue"":""4"", ""path"":""UE4/Sources/Classes"",
                      ""ui"": { ""label"":""Empty"", ""desc"":""An empty class"" },
                      ""meta"": { ""header"": ""Empty.h"", ""source"": ""Empty.cpp"" }
                    }
                ]
           } 
        }
        ";
    }
}