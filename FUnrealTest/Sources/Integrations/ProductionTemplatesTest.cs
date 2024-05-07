using Microsoft.VisualStudio.TestTools.UnitTesting;

using FUnreal;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Diagnostics;

namespace FUnrealTest.Integrations
{
    [TestClass]
    public class ProductionTemplateTest
    {
        Action<string, string, FUnrealPluginModuleTemplate> TestPluginModule = (label, root, tpl) =>
        {
            string wikiName = label.Replace(" ", "");

            Assert.AreEqual(label, tpl.Label);
            Assert.IsNotNull(tpl.Description);

            Assert.IsTrue(tpl.Name.StartsWith("tpl_pluginmodule_"));
            Assert.IsTrue(tpl.BasePath.EndsWith($@"\{root}\Plugins\{wikiName}\@{{TPL_PLUG_NAME}}\Source"), $"tpl_module_{wikiName.ToLower()} wrong path: {tpl.BasePath}");
            Assert.IsTrue(TestUtils.ExistsDir(tpl.BasePath), $"tpl_module_{wikiName.ToLower()} path not exists: {tpl.BasePath}");
            Assert.IsNotNull(tpl.Type);
            Assert.IsNotNull(tpl.Phase);
        };

        Action<string, string, FUnrealGameModuleTemplate> TestGameModule = (label, root, tpl) =>
        {
            string wikiName = label.Replace(" ", "");

            Assert.AreEqual(label, tpl.Label);
            Assert.IsNotNull(tpl.Description);

            Assert.IsTrue(tpl.Name.StartsWith("tpl_gamemodule_"));
            Assert.IsTrue(tpl.BasePath.EndsWith($@"\{root}\Plugins\{wikiName}\@{{TPL_PLUG_NAME}}\Source"), $"tpl_module_{wikiName.ToLower()} wrong path: {tpl.BasePath}");
            Assert.IsTrue(TestUtils.ExistsDir(tpl.BasePath), $"tpl_module_{wikiName.ToLower()} path not exists: {tpl.BasePath}");
            Assert.IsNotNull(tpl.Type);
            Assert.IsNotNull(tpl.Phase);
            Assert.IsNotNull(tpl.Target);
        };

        Action<string, string, FUnrealPluginTemplate> TestPlugin = (label, root, tpl) =>
        {
            string wikiName = label.Replace(" ", "");

            Assert.AreEqual(label, tpl.Label);
            Assert.IsNotNull(tpl.Description);

            Assert.IsTrue(tpl.Name.StartsWith("tpl_plugin_"));
            Assert.IsTrue(tpl.BasePath.EndsWith($@"\{root}\Plugins\{wikiName}"), $"tpl_module_{wikiName.ToLower()} wrong path: {tpl.BasePath}");
            Assert.IsTrue(TestUtils.ExistsDir(tpl.BasePath), $"tpl_module_{wikiName.ToLower()} path not exists: {tpl.BasePath}");
            //Assert.IsNotNull(tpl.GetMeta("has_module"));
        };

        Action<string, FUnrealSourceTemplate> testSource = (label, tpl) =>
        {
            string wikiName = label.Replace(" ", "");

            Assert.AreEqual(label, tpl.Label);
            Assert.IsNotNull(tpl.Description);

            Assert.IsTrue(tpl.Name.StartsWith("tpl_source_"));
            Assert.IsTrue(tpl.BasePath.EndsWith(@"\UEC\Sources\Classes"));
            Assert.AreEqual($"{wikiName}.h", tpl.Header);
            Assert.AreEqual($"{wikiName}.cpp", tpl.Source);

            string heaFullPath = TestUtils.PathCombine(tpl.BasePath, tpl.Header);
            string cppFullPath = TestUtils.PathCombine(tpl.BasePath, tpl.Source);
            Assert.IsTrue(TestUtils.ExistsFile(heaFullPath), heaFullPath);
            Assert.IsTrue(TestUtils.ExistsFile(cppFullPath), cppFullPath);
        };

        FUnrealTemplatesRules rules;

        [TestInitialize]
        public void SetUp()
        {
            rules = new FUnrealTemplatesRules();
            rules.LoadPlugins = FUnrealTemplateLoadRule.MustLoad;
            rules.LoadPluginModules = FUnrealTemplateLoadRule.MustLoad;
            rules.LoadGameModules = FUnrealTemplateLoadRule.MustLoad;
            rules.LoadSources = FUnrealTemplateLoadRule.MustLoad;
            rules.TemplatePrefix = "tpl";
        }

        [TestMethod]
        public void CheckSourcesTemplate()
        {
            string prodTpls = TestUtils.AbsPath("../../../FUnreal/Templates/descriptor.json");

            var list = new List<string>()
            {
                "Empty", "Character", "Pawn", "Actor", "Actor Component",
                "Scene Component", "Player Camera Manager", "Player Controller", "Game Mode Base",
                "World Settings", "HUD", "Player State", "Game State Base", "Blueprint Function Library",
                "Slate Widget", "Slate Widget Style", "Unreal Interface", "UObject"
            };

            bool success = FUnrealTemplates.TryLoad_V1_0(prodTpls, rules, out FUnrealTemplates tpls);
            Assert.IsTrue(success);

            var classTpls = tpls.GetSources("4");
            Assert.AreEqual(list.Count, classTpls.Count);

            for(int i=0; i < list.Count; i++)
            {
                var label = list[i];
                var tpl = classTpls[i];
                testSource(label, tpl);
            }

            classTpls = tpls.GetSources("5");
            Assert.AreEqual(list.Count, classTpls.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var label = list[i];
                var tpl = classTpls[i];
                testSource(label, tpl);
            }
        }

       

        [TestMethod]
        public void CheckPluginModules_ForUE5()
        {
            string prodTpls = TestUtils.AbsPath("../../../FUnreal/Templates/descriptor.json");
            bool success = FUnrealTemplates.TryLoad_V1_0(prodTpls, rules, out FUnrealTemplates tpls);
            Assert.IsTrue(success);

            var ctxTpls = tpls.GetPluginModules("5");

            var modulePerRoot = new Dictionary<string, string>();
            modulePerRoot["Blank"] = "UEC";
            modulePerRoot["Blueprint Library"] = "UEC";
            modulePerRoot["Editor Mode"] = "UE5";
            modulePerRoot["Editor Standalone Window"] = "UE5";
            modulePerRoot["Editor Toolbar Button"] = "UE5";
            modulePerRoot["Third Party Library"] = "UE5";
    
            Assert.AreEqual(modulePerRoot.Count, ctxTpls.Count);

            int index = 0;
            foreach ( var pair in modulePerRoot )
            {
                var label = pair.Key;
                var root = pair.Value;
                var tpl = ctxTpls[index++];
                TestPluginModule(label, root, tpl);
            }

        }

        [TestMethod]
        public void CheckGameModules_ForUE4()
        {
            string prodTpls = TestUtils.AbsPath("../../../FUnreal/Templates/descriptor.json");
            bool success = FUnrealTemplates.TryLoad_V1_0(prodTpls, rules, out FUnrealTemplates tpls);
            Assert.IsTrue(success);
            var ctxTpls = tpls.GetGameModules("4");

            var modulePerRoot = new Dictionary<string, string>();
            modulePerRoot["Blank"] = "UEC";
            modulePerRoot["Blueprint Library"] = "UEC";
            modulePerRoot["Editor Mode"] = "UE4";
            modulePerRoot["Editor Standalone Window"] = "UE4";
            modulePerRoot["Editor Toolbar Button"] = "UE4";
            modulePerRoot["Third Party Library"] = "UE4";

            Assert.AreEqual(modulePerRoot.Count, ctxTpls.Count);

            int index = 0;
            foreach (var pair in modulePerRoot)
            {
                var label = pair.Key;
                var root = pair.Value;
                var tpl = ctxTpls[index++];
                TestGameModule(label, root, tpl);
            }
        }

        [TestMethod]
        public void CheckGameModules_ForUE5()
        {
            string prodTpls = TestUtils.AbsPath("../../../FUnreal/Templates/descriptor.json");
            bool success = FUnrealTemplates.TryLoad_V1_0(prodTpls, rules, out FUnrealTemplates tpls);
            Assert.IsTrue(success);

            var ctxTpls = tpls.GetGameModules("5");

            var modulePerRoot = new Dictionary<string, string>();
            modulePerRoot["Blank"] = "UEC";
            modulePerRoot["Blueprint Library"] = "UEC";
            modulePerRoot["Editor Mode"] = "UE5";
            modulePerRoot["Editor Standalone Window"] = "UE5";
            modulePerRoot["Editor Toolbar Button"] = "UE5";
            modulePerRoot["Third Party Library"] = "UE5";

            Assert.AreEqual(modulePerRoot.Count, ctxTpls.Count);

            int index = 0;
            foreach (var pair in modulePerRoot)
            {
                var label = pair.Key;
                var root = pair.Value;
                var tpl = ctxTpls[index++];
                TestGameModule(label, root, tpl);
            }

        }

        [TestMethod]
        public void CheckPluginModules_ForUE4()
        {
            string prodTpls = TestUtils.AbsPath("../../../FUnreal/Templates/descriptor.json");
            bool success = FUnrealTemplates.TryLoad_V1_0(prodTpls, rules, out FUnrealTemplates tpls);
            Assert.IsTrue(success);
            var ctxTpls = tpls.GetPluginModules("4");

            var modulePerRoot = new Dictionary<string, string>();
            modulePerRoot["Blank"] = "UEC";
            modulePerRoot["Blueprint Library"] = "UEC";
            modulePerRoot["Editor Mode"] = "UE4";
            modulePerRoot["Editor Standalone Window"] = "UE4";
            modulePerRoot["Editor Toolbar Button"] = "UE4";
            modulePerRoot["Third Party Library"] = "UE4";

            Assert.AreEqual(modulePerRoot.Count, ctxTpls.Count);

            int index = 0;
            foreach (var pair in modulePerRoot)
            {
                var label = pair.Key;
                var root = pair.Value;
                var tpl = ctxTpls[index++];
                TestPluginModule(label, root, tpl);
            }
        }


        [TestMethod]
        public void CheckPlugins_ForUE5()
        {
            string prodTpls = TestUtils.AbsPath("../../../FUnreal/Templates/descriptor.json");
            bool success = FUnrealTemplates.TryLoad_V1_0(prodTpls, rules, out FUnrealTemplates tpls);
            Assert.IsTrue(success);

            var ctxTpls = tpls.GetPlugins("5");

            var pluginPerRoot = new Dictionary<string, string>();
            pluginPerRoot["Blank"] = "UEC";
            pluginPerRoot["Content Only"] = "UEC";
            pluginPerRoot["Blueprint Library"] = "UEC";
            pluginPerRoot["Editor Mode"] = "UE5";
            pluginPerRoot["Editor Standalone Window"] = "UE5";
            pluginPerRoot["Editor Toolbar Button"] = "UE5";
            pluginPerRoot["Third Party Library"] = "UE5";

            Assert.AreEqual(pluginPerRoot.Count, ctxTpls.Count);

            int index = 0;
            foreach (var pair in pluginPerRoot)
            {
                var label = pair.Key;
                var root = pair.Value;
                var tpl = ctxTpls[index++];
                TestPlugin(label, root, tpl);
            }

        }

        [TestMethod]
        public void CheckPlugins_ForUE4()
        {
            string prodTpls = TestUtils.AbsPath("../../../FUnreal/Templates/descriptor.json");
            bool success = FUnrealTemplates.TryLoad_V1_0(prodTpls, rules, out FUnrealTemplates tpls);
            Assert.IsTrue(success);
            var ctxTpls = tpls.GetPlugins("4");

            var pluginPerRoot = new Dictionary<string, string>();
            pluginPerRoot["Blank"] = "UEC";
            pluginPerRoot["Content Only"] = "UEC";
            pluginPerRoot["Blueprint Library"] = "UEC";
            pluginPerRoot["Editor Mode"] = "UE4";
            pluginPerRoot["Editor Standalone Window"] = "UE4";
            pluginPerRoot["Editor Toolbar Button"] = "UE4";
            pluginPerRoot["Third Party Library"] = "UE4";

            Assert.AreEqual(pluginPerRoot.Count, ctxTpls.Count);

            int index = 0;
            foreach (var pair in pluginPerRoot)
            {
                var label = pair.Key;
                var root = pair.Value;
                var tpl = ctxTpls[index++];
                TestPlugin(label, root, tpl);
            }

        }

    }
}
