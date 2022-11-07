using FUnreal;
using FUnreal.Sources.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace FUnrealTest
{
    [TestClass]
    public class MiscTest
    {
        struct MyStruct
        {
            public string Name { get; set; }
        }


        [TestMethod]
        public void Simple()
        {
            string jsonstr = "{ 'list' : [ { 'name' : 'fdf' } ] }";

            JObject json = JObject.Parse(jsonstr);

            JToken fdfJson = json.SelectToken("$.list[?(@.name1 == 'fdfrdgf')]");
            Assert.IsNull(fdfJson);
            //JObject fdfJson = (JObject)json.SelectToken("$.list[?(@.name == 'fdfrdgf')]");

            JToken array = json?["list"]!;

            MyStruct myStruct;


            Console.WriteLine(array.Parent);
        }

        [TestMethod]
        public void Simple2()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("MiscTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjOnePlug";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");

            FUnrealUProjectFile file = new FUnrealUProjectFile(uprojectFile);

            Assert.AreEqual(1, file.Plugins.Count);
            //Assert.IsTrue();

            var plug01 = file.Plugins["Plugin01"];
            Assert.IsTrue(plug01);

            var plugNE = file.Plugins["Plugin01_NOT_EXISTENT"];
            Assert.IsFalse(plugNE);

            Assert.AreEqual("Plugin01", plug01.Name);

            Assert.IsNull(plugNE.Name);

            plug01.Name = "Hello";
            Assert.AreEqual("Hello", plug01.Name);


            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void CSharpRegex()
        {
            string regex = @"(?<=class\s+?)" + "Hello" + @"(?=[\s\S]*?\{)";

            string csText = "class Hello { }";
            string newCsText = Regex.Replace(csText, regex, "HelloNew");
            Assert.AreEqual("class HelloNew { }", newCsText);

            csText = "class Hello : ModuleRules\n{ }";
            newCsText = Regex.Replace(csText, regex, "HelloNew");
            Assert.AreEqual("class HelloNew : ModuleRules\n{ }", newCsText);

            string ctorRegex = @"(?<=public\s*?)" + "Hello" + @"(?=\s*?\()";

            string ctorText = "public Hello() { }";
            string newCtorText = Regex.Replace(ctorText, ctorRegex, "HelloNew");
            Assert.AreEqual("public HelloNew() { }", newCtorText);

            ctorText = "public Hello(ReadOnlyTargetRules Target) : base(Target)\n{ }";
            newCtorText = Regex.Replace(ctorText, ctorRegex, "HelloNew");
            Assert.AreEqual("public HelloNew(ReadOnlyTargetRules Target) : base(Target)\n{ }", newCtorText);


            //Scenario 1
            string regex1 = @"(?<=,)\s*""Hello""(\s*,|)"; //replace con ""

            string scenario1 = "PrivateDependencyModuleNames.AddRange(\r\n\t\t\tnew string[]\r\n\t\t\t{\r\n\t\t\t\t\"CoreUObject\",\r\n\t\t\t\t\"Hello\",\r\n\t\t\t\t\"Slate\"\r\n}";
            string scenario1R = Regex.Replace(scenario1, regex1, "");
            string scenario1Exp = "PrivateDependencyModuleNames.AddRange(\r\n\t\t\tnew string[]\r\n\t\t\t{\r\n\t\t\t\t\"CoreUObject\",\r\n\t\t\t\t\"Slate\"\r\n}";
            Assert.AreEqual(scenario1Exp, scenario1R);

            string regex2 = @"(?<!,\s*)\s*""Hello""\s*,"; //replace con ""
            string scenario2 = "PrivateDependencyModuleNames.AddRange(\r\n\t\t\tnew string[]\r\n\t\t\t{\r\n\t\t\t\t\"Hello\" ,\r\n\t\t\t\t\"Slate\"\r\n}";
            string scenario2R = Regex.Replace(scenario2, regex2, "");
            string scenario2Exp = "PrivateDependencyModuleNames.AddRange(\r\n\t\t\tnew string[]\r\n\t\t\t{\r\n\t\t\t\t\"Slate\"\r\n}";
            Assert.AreEqual(scenario2Exp, scenario2R);


            string finalRegex = @"(?<!,\s*)\s*""Hello""\s*,|,{0,1}\s*""Hello""\s*";
            scenario1R = Regex.Replace(scenario1, finalRegex, "");
            Assert.AreEqual(scenario1Exp, scenario1R);

            scenario2R = Regex.Replace(scenario2, finalRegex, "");
            Assert.AreEqual(scenario2Exp, scenario2R);

            string scenario3 = "new string[] { \"prova\", \"Hello\" }";
            string scenario3R = Regex.Replace(scenario3, finalRegex, "");
            string scenario3Exp = "new string[] { \"prova\"}";
            Assert.AreEqual(scenario3Exp, scenario3R);

            string scenario4 = "PrivateDependencyModuleNames.AddRange(\r\n\t\t\tnew string[]\r\n\t\t\t{\r\n\t\t\t\t\"Hello\"\r\n}";
            string scenario4R = Regex.Replace(scenario4, finalRegex, "");
            string scenario4Exp = "PrivateDependencyModuleNames.AddRange(\r\n\t\t\tnew string[]\r\n\t\t\t{}";
            Assert.AreEqual(scenario4Exp, scenario4R);
        }

        [TestMethod]
        public void CppRegex()
        {
            string implModText = "IMPLEMENT_MODULE(FHelloModule, Hello)";
            string implModRegex = @"(?<=IMPLEMENT_MODULE\s*\([\s\S]+?,\s*)Hello(?=\s*?\))";
            string newImplModText = Regex.Replace(implModText, implModRegex, "HelloNew");
            Assert.AreEqual("IMPLEMENT_MODULE(FHelloModule, HelloNew)", newImplModText);
        }

        [TestMethod]
        public void Path()
        {
            string path = @"c:\part1\part2\..\part3";
            string actual = System.IO.Path.GetFullPath(path);
            Assert.AreEqual(@"c:\part1\part3", actual);
        }
    }
}
