using Microsoft.VisualStudio.TestTools.UnitTesting;

using FUnreal;
using System.IO;
using System;
using System.Collections.Generic;
using System.Security.Policy;
using System.IO.Ports;

namespace FUnrealTest.Integrations
{
    [TestClass]
    public class ProductionTemplateTest
    {
        [TestMethod]
        public void CheckSourcesTemplate()
        {
            string prodTpls = TestUtils.AbsPath("../../../FUnreal/Templates/descriptor.xml");

            var list = new List<string>()
            {
                "Empty", "Character", "Pawn", "Actor", "Actor Component",
                "Scene Component", "Player Camera Manager", "Player Controller", "Game Mode Base",
                "World Settings", "HUD", "Player State", "Game State Base", "Blueprint Function Library",
                "Slate Widget", "Slate Widget Style", "Unreal Interface", "UObject"
            };

            Action<string, FUnrealTemplate> test = (label, tpl) =>
            {
                string wikiName = label.Replace(" ", "");

                Assert.AreEqual(label, tpl.Label);
                Assert.IsNotNull(tpl.Description);

                Assert.AreEqual($"tpl_class_{wikiName.ToLower()}", tpl.Name);
                Assert.IsTrue(tpl.BasePath.EndsWith(@"\UEC\Sources\Classes"));
                Assert.AreEqual($"{wikiName}.h", tpl.GetMeta("header"));
                Assert.AreEqual($"{wikiName}.cpp", tpl.GetMeta("source"));

                string heaFullPath = TestUtils.PathCombine(tpl.BasePath, tpl.GetMeta("header"));
                string cppFullPath = TestUtils.PathCombine(tpl.BasePath, tpl.GetMeta("source"));
                Assert.IsTrue(TestUtils.ExistsFile(heaFullPath), heaFullPath);
                Assert.IsTrue(TestUtils.ExistsFile(cppFullPath), cppFullPath);
            };


            var tpls = FUnrealTemplates.Load(prodTpls);

            var classTpls = tpls.GetTemplates(FUnrealTemplateCtx.SOURCES, "4");
            Assert.AreEqual(list.Count, classTpls.Count);

            for(int i=0; i < list.Count; i++)
            {
                var label = list[i];
                var tpl = classTpls[i];
                test(label, tpl);
            }

            classTpls = tpls.GetTemplates(FUnrealTemplateCtx.SOURCES, "5");
            Assert.AreEqual(list.Count, classTpls.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var label = list[i];
                var tpl = classTpls[i];
                test(label, tpl);
            }
        }

        Action<string, string, FUnrealTemplate> TestPluginModule = (label, root, tpl) =>
        {
            string wikiName = label.Replace(" ", "");

            Assert.AreEqual(label, tpl.Label);
            Assert.IsNotNull(tpl.Description);

            Assert.AreEqual($"tpl_module_{wikiName.ToLower()}", tpl.Name);
            Assert.IsTrue(tpl.BasePath.EndsWith($@"\{root}\Plugins\{wikiName}\@{{TPL_PLUG_NAME}}\Source"), $"tpl_module_{wikiName.ToLower()} wrong path: {tpl.BasePath}");
            Assert.IsTrue(TestUtils.ExistsDir(tpl.BasePath), $"tpl_module_{wikiName.ToLower()} path not exists: {tpl.BasePath}");
            Assert.IsNotNull(tpl.GetMeta("type"));
            Assert.IsNotNull(tpl.GetMeta("phase"));
            Assert.IsNotNull(tpl.GetMeta("target"));
        };

        Action<string, string, FUnrealTemplate> TestPlugin = (label, root, tpl) =>
        {
            string wikiName = label.Replace(" ", "");

            Assert.AreEqual(label, tpl.Label);
            Assert.IsNotNull(tpl.Description);

            Assert.AreEqual($"tpl_plugin_{wikiName.ToLower()}", tpl.Name);
            Assert.IsTrue(tpl.BasePath.EndsWith($@"\{root}\Plugins\{wikiName}"), $"tpl_module_{wikiName.ToLower()} wrong path: {tpl.BasePath}");
            Assert.IsTrue(TestUtils.ExistsDir(tpl.BasePath), $"tpl_module_{wikiName.ToLower()} path not exists: {tpl.BasePath}");
            Assert.IsNotNull(tpl.GetMeta("has_module"));
        };

        [TestMethod]
        public void CheckModules_ForUE5()
        {
            string prodTpls = TestUtils.AbsPath("../../../FUnreal/Templates/descriptor.xml");
            var tpls = FUnrealTemplates.Load(prodTpls);
            var ctxTpls = tpls.GetTemplates(FUnrealTemplateCtx.MODULES, "5");

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
        public void CheckModules_ForUE4()
        {
            string prodTpls = TestUtils.AbsPath("../../../FUnreal/Templates/descriptor.xml");
            var tpls = FUnrealTemplates.Load(prodTpls);
            var ctxTpls = tpls.GetTemplates(FUnrealTemplateCtx.MODULES, "4");

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
            string prodTpls = TestUtils.AbsPath("../../../FUnreal/Templates/descriptor.xml");
            var tpls = FUnrealTemplates.Load(prodTpls);
            var ctxTpls = tpls.GetTemplates(FUnrealTemplateCtx.PLUGINS, "5");

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
            string prodTpls = TestUtils.AbsPath("../../../FUnreal/Templates/descriptor.xml");
            var tpls = FUnrealTemplates.Load(prodTpls);
            var ctxTpls = tpls.GetTemplates(FUnrealTemplateCtx.PLUGINS, "4");

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
