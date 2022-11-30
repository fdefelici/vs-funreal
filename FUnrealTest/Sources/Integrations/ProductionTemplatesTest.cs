using Microsoft.VisualStudio.TestTools.UnitTesting;

using FUnreal;
using System.IO;
using System;
using System.Collections.Generic;

namespace FUnrealTest.Integrations
{
    [TestClass]
    public class ProductionTemplateTest
    {
        [TestMethod]
        public void CheckSourcesTemplate()
        {
            string prodTpls = TestUtils.AbsPath("../../../FUnreal/Templates/descriptor.xml");

            var tpls = FUnrealTemplates.Load(prodTpls);

            //Assert.AreEqual(0, tpls.Count);

            var classTpls = tpls.GetTemplates(FUnrealTemplateCtx.SOURCES, "5");

            Action<string, FUnrealTemplate> test = (label, tpl) => 
            {
                string wikiName = label.Replace(" ", "");

                Assert.AreEqual(label, tpl.Label);
                Assert.IsNotNull(tpl.Description);

                Assert.AreEqual($"tpl_class_{wikiName.ToLower()}", tpl.Name);
                Assert.IsTrue(tpl.BasePath.EndsWith(@"\UE5\Sources\Classes"));
                Assert.AreEqual($"{wikiName}.h", tpl.GetMeta("header"));
                Assert.AreEqual($"{wikiName}.cpp", tpl.GetMeta("source"));
            };

            var list = new List<string>() 
            {  
                "Empty", "Character", "Pawn", "Actor", "Actor Component",
                "Scene Component", "Player Camera Manager", "Player Controller", "Game Mode Base",
                "World Settings", "HUD", "Player State", "Game State Base", "Blueprint Function Library",
                "Slate Widget", "Slate Widget Style", "Unreal Interface"
            };

            Assert.AreEqual(list.Count, classTpls.Count);

            for(int i=0; i < list.Count; i++)
            {
                var label = list[i];
                var tpl = classTpls[i];
                test(label, tpl);
            }
        }

        [TestMethod]
        public void CheckModules()
        {
            string prodTpls = TestUtils.AbsPath("../../../FUnreal/Templates/descriptor.xml");

            var tpls = FUnrealTemplates.Load(prodTpls);

            //Assert.AreEqual(0, tpls.Count);

            var ctxTpls = tpls.GetTemplates(FUnrealTemplateCtx.MODULES, "5");

            Action<string, FUnrealTemplate> test = (label, tpl) =>
            {
                string wikiName = label.Replace(" ", "");

                Assert.AreEqual(label, tpl.Label);
                Assert.IsNotNull(tpl.Description);
                
                Assert.AreEqual($"tpl_module_{wikiName.ToLower()}", tpl.Name);
                Assert.IsTrue(tpl.BasePath.EndsWith($@"\UE5\Plugins\{wikiName}\@{{TPL_PLUG_NAME}}\Source"));
                Assert.IsNotNull(tpl.GetMeta("type"));
                Assert.IsNotNull(tpl.GetMeta("phase"));
                Assert.IsNotNull(tpl.GetMeta("target"));
            };

            var list = new List<string>()
            {
                "Blank", "Blueprint Library", "Editor Mode", "Editor Standalone Window",
                "Editor Toolbar Button", "Third Party Library"
            };

            Assert.AreEqual(list.Count, ctxTpls.Count);

            for (int i = 0; i < list.Count; i++)
            {
                var label = list[i];
                var tpl = ctxTpls[i];
                test(label, tpl);
            }
        }

    }
}
