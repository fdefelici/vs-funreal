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
        public void Simple()
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

    }
}
