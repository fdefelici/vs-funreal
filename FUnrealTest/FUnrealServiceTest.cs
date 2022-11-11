using FUnreal;
using System.Text.Json.Nodes;

namespace FUnrealTest
{
    [TestClass]
    public class FUnrealServiceTest
    {

        [TestMethod]
        public void AddPluginBlankToAnEmptyProject()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjEmpty";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.AddPluginAsync("tpl_plugin_blank", "MyPlug", "MyMod", new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsTrue(TestUtils.ExistsDir(uprojectPath,  "Plugins/MyPlug"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/MyPlug/Resources/Icon128.png"));
            Assert.IsTrue(TestUtils.ExistsDir(uprojectPath,  "Plugins/MyPlug/Source/MyMod"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/MyPlug/Source/MyMod/Private/MyMod.cpp"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/MyPlug/Source/MyMod/Public/MyMod.h"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/MyPlug/Source/MyMod/MyMod.Build.cs"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/MyPlug/MyPlug.uplugin"));

            string fileHea = TestUtils.ReadFile(uprojectPath, "Plugins/MyPlug/Source/MyMod/Public/MyMod.h");
            Assert.IsTrue(fileHea.Contains("class FMyModModule"));
            
            string fileCpp = TestUtils.ReadFile(uprojectPath, "Plugins/MyPlug/Source/MyMod/Private/MyMod.cpp");
            Assert.IsTrue(fileCpp.Contains("#include \"MyMod.h\""));
            Assert.IsTrue(fileCpp.Contains("#define LOCTEXT_NAMESPACE \"FMyModModule\""));
            Assert.IsTrue(fileCpp.Contains("void FMyModModule::StartupModule()"));
            Assert.IsTrue(fileCpp.Contains("void FMyModModule::ShutdownModule()"));
            Assert.IsTrue(fileCpp.Contains("IMPLEMENT_MODULE(FMyModModule, MyMod)"));

            string fileCs  = TestUtils.ReadFile(uprojectPath, "Plugins/MyPlug/Source/MyMod/MyMod.Build.cs");
            Assert.IsTrue(fileCs.Contains("public class MyMod"));
            Assert.IsTrue(fileCs.Contains("public MyMod(ReadOnlyTargetRules Target)"));

            string filePlu = TestUtils.ReadFile(uprojectPath, "Plugins/MyPlug/MyPlug.uplugin");
            Assert.IsTrue(filePlu.Contains("\"FriendlyName\": \"MyPlug\""));
            Assert.IsTrue(filePlu.Contains("\"Name\": \"MyMod\""));
            
            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void AddPluginContentOnlyToAnEmptyProject()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjEmpty";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.AddPluginAsync("tpl_plugin_contentonly", "MyPlug", null, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsTrue(TestUtils.ExistsDir(uprojectPath, "Plugins/MyPlug"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/MyPlug/Resources/Icon128.png"));
            Assert.IsTrue(TestUtils.ExistsDir(uprojectPath, "Plugins/MyPlug/Content"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/MyPlug/MyPlug.uplugin"));

            string filePlu = TestUtils.ReadFile(uprojectPath, "Plugins/MyPlug/MyPlug.uplugin");
            Assert.IsTrue(filePlu.Contains("\"FriendlyName\": \"MyPlug\""));

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void RemovePluginToProjectWithOnePluginInUProjectFile()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjOnePlug";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");
            string upluginName = "Plugin01";
            FUnrealBuildToolMock ubt = new();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.DeletePluginAsync(upluginName, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            string uprojectFileExpected = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject_expected");
            string jsonExpected = TestUtils.ReadFile(uprojectFileExpected);
            string jsonActual   = TestUtils.ReadFile(uprojectFile);
            Assert.AreEqual(jsonExpected, jsonActual);

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void RenanePluginToProjectWithOnePluginInUProjectFile()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjOnePlug";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");
            string upluginName = "Plugin01";
            string upluginNewName = "Plugin01Renamed";
            FUnrealBuildToolMock ubt = new();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.RenamePluginAsync(upluginName, upluginNewName, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            string exp_upluginPath = TestUtils.PathCombine(uprojectPath, "Plugins", upluginNewName);
            string exp_upluginFile = TestUtils.PathCombine(exp_upluginPath, $"{upluginNewName}.uplugin");
            Assert.IsTrue(TestUtils.ExistsFile(exp_upluginFile));

            string uprojectFileExpected = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject_renamed_expected");
            string jsonExpected = TestUtils.ReadFile(uprojectFileExpected);
            string jsonActual = TestUtils.ReadFile(uprojectFile);
            Assert.AreEqual(jsonExpected, jsonActual);

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void AddModuleBlankToExistentPlugin()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjOnePlug";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.AddPluginModuleAsync("tpl_module_blank", "Plugin01", "MyMod", new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsTrue(TestUtils.ExistsDir(uprojectPath,  "Plugins/Plugin01/Source/MyMod"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/Plugin01/Source/MyMod/Private/MyMod.cpp"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/Plugin01/Source/MyMod/Public/MyMod.h"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/Plugin01/Source/MyMod/MyMod.Build.cs"));
            
            string fileHea = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/MyMod/Public/MyMod.h");
            Assert.IsTrue(fileHea.Contains("class FMyModModule"));

            string fileCpp = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/MyMod/Private/MyMod.cpp");
            Assert.IsTrue(fileCpp.Contains("#include \"MyMod.h\""));
            Assert.IsTrue(fileCpp.Contains("#define LOCTEXT_NAMESPACE \"FMyModModule\""));
            Assert.IsTrue(fileCpp.Contains("void FMyModModule::StartupModule()"));
            Assert.IsTrue(fileCpp.Contains("void FMyModModule::ShutdownModule()"));
            Assert.IsTrue(fileCpp.Contains("IMPLEMENT_MODULE(FMyModModule, MyMod)"));

            string fileCs = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/MyMod/MyMod.Build.cs");
            Assert.IsTrue(fileCs.Contains("public class MyMod"));
            Assert.IsTrue(fileCs.Contains("public MyMod(ReadOnlyTargetRules Target)"));

            string filePlu = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Plugin01.uplugin");
            Assert.IsTrue(filePlu.Contains("\"Name\": \"MyMod\""));
            Assert.IsTrue(filePlu.Contains("\"Type\": \"Runtime\""));
            Assert.IsTrue(filePlu.Contains("\"LoadingPhase\": \"Default\""));

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void RenameExistentModuleWithCppFiles()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjOnePlugMod";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");

            string expectedPath = TestUtils.PathCombine(tmpPath, "Expected/UPrjOnePlugMod_RenameModule01");
            
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.RenamePluginModuleAsync("Plugin01", "Module01", "Module01Ren", true, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsTrue(TestUtils.ExistsDir(uprojectPath,  "Plugins/Plugin01/Source/Module01Ren"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/Plugin01/Source/Module01Ren/Private/Module01RenModule.cpp"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/Plugin01/Source/Module01Ren/Public/Module01RenModule.h"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/Plugin01/Source/Module01Ren/Module01Ren.Build.cs"));


            string fileHeaExp = TestUtils.ReadFile(expectedPath, "Plugins/Plugin01/Source/Module01Ren/Public/Module01RenModule.h");
            string fileHeaAct = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/Module01Ren/Public/Module01RenModule.h");
            FAssert.AreEqualNN(fileHeaExp, fileHeaAct);

            string fileCppExp = TestUtils.ReadFile(expectedPath, "Plugins/Plugin01/Source/Module01Ren/Private/Module01RenModule.cpp");
            string fileCppAct = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/Module01Ren/Private/Module01RenModule.cpp");
            FAssert.AreEqualNN(fileCppExp, fileCppAct);

            string fileCsExp = TestUtils.ReadFile(expectedPath, "Plugins/Plugin01/Source/Module01Ren/Module01Ren.Build.cs");
            string fileCsAct = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/Module01Ren/Module01Ren.Build.cs");
            FAssert.AreEqualNN(fileCsExp, fileCsAct);

            string filePlugExp = TestUtils.ReadFile(expectedPath, "Plugins/Plugin01/Plugin01.uplugin");
            string filePlugAct = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Plugin01.uplugin");
            FAssert.AreEqualNN(filePlugExp, filePlugAct);

            //Check MODULENAME_API macro is updated
            string fileApiExp = TestUtils.ReadFile(expectedPath, "Plugins/Plugin01/Source/Module01Ren/Public/Actor01.h");
            string fileApiAct = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/Module01Ren/Public/Actor01.h");
            FAssert.AreEqualNN(fileApiExp, fileApiAct);

            //Check Module02 dependency to Module01 (now became NewModule01)
            string module02CsExp = TestUtils.ReadFile(expectedPath, "Plugins/Plugin01/Source/Module02/Module02.Build.cs");
            string module02CsAct = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/Module02/Module02.Build.cs");
            FAssert.AreEqualNN(module02CsExp, module02CsAct);

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void RenameExistentModuleWithoutCppFiles()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjOnePlugMod";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.RenamePluginModuleAsync("Plugin01", "Module01", "NewModule01", false, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsTrue(TestUtils.ExistsDir(uprojectPath, "Plugins/Plugin01/Source/NewModule01"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/Plugin01/Source/NewModule01/Private/Module01.cpp"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/Plugin01/Source/NewModule01/Public/Module01.h"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/Plugin01/Source/NewModule01/NewModule01.Build.cs"));

            string fileHea = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/NewModule01/Public/Module01.h");
            Assert.IsTrue(fileHea.Contains("class FModule01Module"));

            string fileCpp = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/NewModule01/Private/Module01.cpp");
            Assert.IsTrue(fileCpp.Contains("#include \"Module01.h\""));
            Assert.IsTrue(fileCpp.Contains("#define LOCTEXT_NAMESPACE \"FModule01Module\""));
            Assert.IsTrue(fileCpp.Contains("void FModule01Module::StartupModule()"));
            Assert.IsTrue(fileCpp.Contains("void FModule01Module::ShutdownModule()"));
            Assert.IsTrue(fileCpp.Contains("IMPLEMENT_MODULE(FModule01Module, NewModule01)")); //Only ModuleName is updated

            string fileCs = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/NewModule01/NewModule01.Build.cs");
            Assert.IsTrue(fileCs.Contains("public class NewModule01"));
            Assert.IsTrue(fileCs.Contains("public NewModule01(ReadOnlyTargetRules Target)"));

            string filePlu = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Plugin01.uplugin");
            Assert.IsTrue(filePlu.Contains("\"Name\": \"NewModule01\""));
            Assert.IsTrue(filePlu.Contains("\"Type\": \"Editor\""));
            Assert.IsTrue(filePlu.Contains("\"LoadingPhase\": \"Default\""));

            //Check Module02 dependency to Module01 (now became NewModule01)
            string module02Cs = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/Module02/Module02.Build.cs");
            Assert.IsTrue(module02Cs.Contains("\"NewModule01\""));

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void RenameExistentModuleWithCppFilesNotAligned()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjOnePlugMod";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.RenamePluginModuleAsync("Plugin01", "Module03", "NewModule03", true, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsTrue(TestUtils.ExistsDir(uprojectPath, "Plugins/Plugin01/Source/NewModule03"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/Plugin01/Source/NewModule03/Private/Module03NotAligned.cpp"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/Plugin01/Source/NewModule03/Public/Module03NotAligned.h"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Plugins/Plugin01/Source/NewModule03/NewModule03.Build.cs"));

            string fileHea = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/NewModule03/Public/Module03NotAligned.h");
            Assert.IsTrue(fileHea.Contains("class FModule03Module"));

            string fileCpp = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/NewModule03/Private/Module03NotAligned.cpp");
            Assert.IsTrue(fileCpp.Contains("#include \"Module03NotAligned.h\""));
            Assert.IsTrue(fileCpp.Contains("#define LOCTEXT_NAMESPACE \"FModule03Module\""));
            Assert.IsTrue(fileCpp.Contains("void FModule03Module::StartupModule()"));
            Assert.IsTrue(fileCpp.Contains("void FModule03Module::ShutdownModule()"));
            Assert.IsTrue(fileCpp.Contains("IMPLEMENT_MODULE(FModule03Module, Module03)")); 

            string fileCs = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/NewModule03/NewModule03.Build.cs");
            Assert.IsTrue(fileCs.Contains("public class NewModule03"));
            Assert.IsTrue(fileCs.Contains("public NewModule03(ReadOnlyTargetRules Target)"));

            string filePlu = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Plugin01.uplugin");
            Assert.IsTrue(filePlu.Contains("\"Name\": \"NewModule03\""));
            Assert.IsTrue(filePlu.Contains("\"Type\": \"Editor\""));
            Assert.IsTrue(filePlu.Contains("\"LoadingPhase\": \"Default\""));

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void DeleteExistentModuleWithDependency()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjOnePlugMod";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.DeletePluginModuleAsync("Plugin01", "Module01", new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsFalse(TestUtils.ExistsDir(uprojectPath, "Plugins/Plugin01/Source/Module01"));
          
            string filePlu = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Plugin01.uplugin");
            Assert.IsFalse(filePlu.Contains("\"Name\": \"Module01\""));

            //Check Module02 dependency to Module01 (now became NewModule01)
            string module02Cs = TestUtils.ReadFile(uprojectPath, "Plugins/Plugin01/Source/Module02/Module02.Build.cs");
            Assert.IsFalse(module02Cs.Contains("\"Module01\""));

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void AddSourceClassPublicPrivate()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjOnePlugMod";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");
            string expectedPath = TestUtils.PathCombine(tmpPath, "Expected/Module01_Public");


            string selectedSourcePath = TestUtils.PathCombine(uprojectPath, "Plugins/Plugin01/Source/Module01/Private");
            FUnrealSourceType selectedType = FUnrealSourceType.PUBLIC;
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.AddSourceClassAsync("tpl_class_actor", selectedSourcePath, "MyActor", selectedType, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsTrue(TestUtils.ExistsFile(TestUtils.PathParent(selectedSourcePath), "Public/MyActor.h"));
            Assert.IsTrue(TestUtils.ExistsFile(TestUtils.PathParent(selectedSourcePath), "Private/MyActor.cpp"));
          
            string fileHeaExp = TestUtils.ReadFile(expectedPath, "MyActor.h");
            string fileHea = TestUtils.ReadFile(TestUtils.PathParent(selectedSourcePath), "Public/MyActor.h");
            Assert.AreEqual(fileHeaExp, fileHea);

            string fileCppExp = TestUtils.ReadFile(expectedPath, "MyActor.cpp");
            string fileCpp = TestUtils.ReadFile(TestUtils.PathParent(selectedSourcePath), "Private/MyActor.cpp");
            Assert.AreEqual(fileCppExp, fileCpp);

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void AddSourceClassPublicPrivateWithSubFolder()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjOnePlugMod";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");
            string expectedPath = TestUtils.PathCombine(tmpPath, "Expected/Module01_PublicSubFolder");


            string selectedSourcePath = TestUtils.PathCombine(uprojectPath, "Plugins/Plugin01/Source/Module01/Private/SubFolder");
            FUnrealSourceType selectedType = FUnrealSourceType.PUBLIC;
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.AddSourceClassAsync("tpl_class_actor", selectedSourcePath, "MyActor", selectedType, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsTrue(TestUtils.ExistsFile(TestUtils.PathParent(selectedSourcePath, 2), "Public/SubFolder/MyActor.h"));
            Assert.IsTrue(TestUtils.ExistsFile(TestUtils.PathParent(selectedSourcePath, 2), "Private/SubFolder/MyActor.cpp"));

            string fileHeaExp = TestUtils.ReadFile(expectedPath, "MyActor.h");
            string fileHea = TestUtils.ReadFile(TestUtils.PathParent(selectedSourcePath, 2), "Public/SubFolder/MyActor.h");
            Assert.AreEqual(fileHeaExp, fileHea);

            string fileCppExp = TestUtils.ReadFile(expectedPath, "MyActor.cpp");
            string fileCpp = TestUtils.ReadFile(TestUtils.PathParent(selectedSourcePath, 2), "Private/SubFolder/MyActor.cpp");
            Assert.AreEqual(fileCppExp, fileCpp);

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void AddSourceClassPrivateOnly()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjOnePlugMod";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");
            string expectedPath = TestUtils.PathCombine(tmpPath, "Expected/Module01_Private");


            string selectedSourcePath = TestUtils.PathCombine(uprojectPath, "Plugins/Plugin01/Source/Module01/Private");
            FUnrealSourceType selectedType = FUnrealSourceType.PRIVATE;
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.AddSourceClassAsync("tpl_class_actor", selectedSourcePath, "MyActor", selectedType, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsTrue(TestUtils.ExistsFile(TestUtils.PathParent(selectedSourcePath), "Private/MyActor.h"));
            Assert.IsTrue(TestUtils.ExistsFile(TestUtils.PathParent(selectedSourcePath), "Private/MyActor.cpp"));

            string fileHeaExp = TestUtils.ReadFile(expectedPath, "MyActor.h");
            string fileHea = TestUtils.ReadFile(TestUtils.PathParent(selectedSourcePath), "Private/MyActor.h");
            Assert.AreEqual(fileHeaExp, fileHea);

            string fileCppExp = TestUtils.ReadFile(expectedPath, "MyActor.cpp");
            string fileCpp = TestUtils.ReadFile(TestUtils.PathParent(selectedSourcePath), "Private/MyActor.cpp");
            Assert.AreEqual(fileCppExp, fileCpp);

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void AddSourceClassFreePath()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjOnePlugMod";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");
            string expectedPath = TestUtils.PathCombine(tmpPath, "Expected/Module01_Private");


            string selectedSourcePath = TestUtils.PathCombine(uprojectPath, "Plugins/Plugin01/Source/Module01");
            FUnrealSourceType selectedType = FUnrealSourceType.FREE;
            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.AddSourceClassAsync("tpl_class_actor", selectedSourcePath, "MyActor", selectedType, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsTrue(TestUtils.ExistsFile(selectedSourcePath, "MyActor.h"));
            Assert.IsTrue(TestUtils.ExistsFile(selectedSourcePath, "MyActor.cpp"));

            string fileHeaExp = TestUtils.ReadFile(expectedPath, "MyActor.h");
            string fileHea = TestUtils.ReadFile(selectedSourcePath, "MyActor.h");
            Assert.AreEqual(fileHeaExp, fileHea);

            string fileCppExp = TestUtils.ReadFile(expectedPath, "MyActor.cpp");
            string fileCpp = TestUtils.ReadFile(selectedSourcePath, "MyActor.cpp");
            Assert.AreEqual(fileCppExp, fileCpp);

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void AddSourceFile()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjOnePlugMod";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");

            string selectedSourcePath = TestUtils.PathCombine(uprojectPath, "Plugins/Plugin01/Source/Module01");

            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.AddSourceFileAsync(selectedSourcePath, "MyFile.txt", new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsTrue(TestUtils.ExistsFile(selectedSourcePath, "MyFile.txt"));

            TestUtils.DeleteDir(tmpPath);
        }


        [TestMethod]
        public void DeleteSources_OneFolder()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjOnePlugMod";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");

            List<string> selectedPaths = new List<string>();
            selectedPaths.Add(TestUtils.PathCombine(uprojectPath, "Plugins/Plugin01/Source/Module01/Public"));


            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.DeleteSourceDirectoryAsync(selectedPaths, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsFalse(TestUtils.ExistsDir(selectedPaths[0]));
          
            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void AddGameModuleBlank()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjGame";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");

            string expectedPath = TestUtils.PathCombine(tmpPath, "Expected/UPrjGame");


            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.AddGameModuleAsync("tpl_module_blank", "MyMod", new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsTrue(TestUtils.ExistsDir(uprojectPath,  "Source/MyMod"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Source/MyMod/Private/MyMod.cpp"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Source/MyMod/Public/MyMod.h"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Source/MyMod/MyMod.Build.cs"));

            string fileHeaExp = TestUtils.ReadFile(expectedPath, "Source/MyMod/Public/MyMod.h");
            string fileHea    = TestUtils.ReadFile(uprojectPath, "Source/MyMod/Public/MyMod.h");
            Assert.AreEqual(fileHeaExp, fileHea);

            string fileCppExp = TestUtils.ReadFile(expectedPath, "Source/MyMod/Private/MyMod.cpp");
            string fileCpp    = TestUtils.ReadFile(uprojectPath, "Source/MyMod/Private/MyMod.cpp");
            Assert.AreEqual(fileCppExp, fileCpp);

            string fileCsExp = TestUtils.ReadFile(expectedPath, "Source/MyMod/MyMod.Build.cs");
            string fileCs    = TestUtils.ReadFile(uprojectPath, "Source/MyMod/MyMod.Build.cs");
            Assert.AreEqual(fileCsExp, fileCs);

            string fileTargetExp = TestUtils.ReadFile(expectedPath, "Source/UPrjGame.Target.cs");
            string fileTarget    = TestUtils.ReadFile(uprojectPath, "Source/UPrjGame.Target.cs");
            Assert.AreEqual(fileTargetExp, fileTarget);

            string filePrjExp = TestUtils.ReadFile(expectedPath, "UPrjGame.uproject");
            string filePrj    = TestUtils.ReadFile(uprojectPath, "UPrjGame.uproject");
            Assert.AreEqual(filePrjExp, filePrj);

            TestUtils.DeleteDir(tmpPath);
        }


        [TestMethod]
        public void RenameGameModuleWithCppFiles()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjGameOneMod";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");

            string expectedPath = TestUtils.PathCombine(tmpPath, "Expected/UPrjGame_RenameModule");


            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.RenameGameModuleAsync("MyMod", "MyModRenamed", true, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsTrue(TestUtils.ExistsDir(uprojectPath,  "Source/MyModRenamed"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Source/MyModRenamed/Private/MyModRenamedModule.cpp"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Source/MyModRenamed/Public/MyModRenamedModule.h"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Source/MyModRenamed/MyModRenamed.Build.cs"));

            string fileHeaExp = TestUtils.ReadFile(expectedPath, "Source/MyModRenamed/Public/MyModRenamedModule.h");
            string fileHea = TestUtils.ReadFile(uprojectPath, "Source/MyModRenamed/Public/MyModRenamedModule.h");
            FAssert.AreEqualNN(fileHeaExp, fileHea);

            string fileCppExp = TestUtils.ReadFile(expectedPath, "Source/MyModRenamed/Private/MyModRenamedModule.cpp");
            string fileCpp = TestUtils.ReadFile(uprojectPath, "Source/MyModRenamed/Private/MyModRenamedModule.cpp");
            FAssert.AreEqualNN(fileCppExp, fileCpp);

            string fileCsExp = TestUtils.ReadFile(expectedPath, "Source/MyModRenamed/MyModRenamed.Build.cs");
            string fileCs = TestUtils.ReadFile(uprojectPath, "Source/MyModRenamed/MyModRenamed.Build.cs");
            FAssert.AreEqualNN(fileCsExp, fileCs);

            string fileTargetExp = TestUtils.ReadFile(expectedPath, "Source/UPrjGameOneMod.Target.cs");
            string fileTarget = TestUtils.ReadFile(uprojectPath, "Source/UPrjGameOneMod.Target.cs");
            FAssert.AreEqualNN(fileTargetExp, fileTarget);

            string fileEdTargetExp = TestUtils.ReadFile(expectedPath, "Source/UPrjGameOneModEditor.Target.cs");
            string fileEdTarget = TestUtils.ReadFile(uprojectPath, "Source/UPrjGameOneModEditor.Target.cs");
            FAssert.AreEqualNN(fileEdTargetExp, fileEdTarget);


            string filePrjExp = TestUtils.ReadFile(expectedPath, "UPrjGameOneMod.uproject");
            string filePrj = TestUtils.ReadFile(uprojectPath, "UPrjGameOneMod.uproject");
            FAssert.AreEqualNN(filePrjExp, filePrj);

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void RenamePrimaryGameModule()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjGame";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");

            string expectedPath = TestUtils.PathCombine(tmpPath, "Expected/UPrjGame_RenamePrimaryModule");


            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.RenameGameModuleAsync("UPrjGame", "UPrjGameRenamed", true, new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsTrue(TestUtils.ExistsDir(uprojectPath,  "Source/UPrjGameRenamed"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Source/UPrjGameRenamed/UPrjGameRenamedModule.cpp"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Source/UPrjGameRenamed/UPrjGameRenamedModule.h"));
            Assert.IsTrue(TestUtils.ExistsFile(uprojectPath, "Source/UPrjGameRenamed/UPrjGameRenamed.Build.cs"));

            string fileHeaExp = TestUtils.ReadFile(expectedPath, "Source/UPrjGameRenamed/UPrjGameRenamedModule.h");
            string fileHea = TestUtils.ReadFile(uprojectPath,    "Source/UPrjGameRenamed/UPrjGameRenamedModule.h");
            FAssert.AreEqualNN(fileHeaExp, fileHea);

            string fileCppExp = TestUtils.ReadFile(expectedPath, "Source/UPrjGameRenamed/UPrjGameRenamedModule.cpp");
            string fileCpp = TestUtils.ReadFile(uprojectPath,    "Source/UPrjGameRenamed/UPrjGameRenamedModule.cpp");
            FAssert.AreEqualNN(fileCppExp, fileCpp);

            string fileCsExp = TestUtils.ReadFile(expectedPath, "Source/UPrjGameRenamed/UPrjGameRenamed.Build.cs");
            string fileCs = TestUtils.ReadFile(uprojectPath,    "Source/UPrjGameRenamed/UPrjGameRenamed.Build.cs");
            FAssert.AreEqualNN(fileCsExp, fileCs);

            string fileTargetExp = TestUtils.ReadFile(expectedPath, "Source/UPrjGame.Target.cs");
            string fileTarget = TestUtils.ReadFile(uprojectPath,    "Source/UPrjGame.Target.cs");
            FAssert.AreEqualNN(fileTargetExp, fileTarget);

            string fileEdTargetExp = TestUtils.ReadFile(expectedPath, "Source/UPrjGameEditor.Target.cs");
            string fileEdTarget = TestUtils.ReadFile(uprojectPath,    "Source/UPrjGameEditor.Target.cs");
            FAssert.AreEqualNN(fileEdTargetExp, fileEdTarget);


            string filePrjExp = TestUtils.ReadFile(expectedPath, "UPrjGame.uproject");
            string filePrj = TestUtils.ReadFile(uprojectPath, "UPrjGame.uproject");
            FAssert.AreEqualNN(filePrjExp, filePrj);

            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void DeleteGameModuleWithCppFiles()
        {
            string resPath = TestUtils.AbsPath("Resources", "FUnrealServiceTest");
            string tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
            TestUtils.DeepCopy(resPath, tmpPath);

            string uprojectName = "UPrjGameOneMod";
            string uprojectPath = TestUtils.PathCombine(tmpPath, "Projects", uprojectName);
            string uprojectFile = TestUtils.PathCombine(uprojectPath, $"{uprojectName}.uproject");
            string templatePath = TestUtils.PathCombine(tmpPath, "Templates");

            string expectedPath = TestUtils.PathCombine(tmpPath, "Expected/UPrjGame_DeleteModule");


            FUnrealBuildToolMock ubt = new FUnrealBuildToolMock();
            FUnrealEngine eng = new FUnrealEngine(new XVersion(5, 0), "engine/5.0", ubt);

            FUnrealService service = new FUnrealService(eng, uprojectFile, uprojectName, templatePath);
            bool taskResult = service.DeleteGameModuleAsync("MyMod", new FUnrealNotifier()).GetAwaiter().GetResult();

            Assert.IsTrue(taskResult);
            Assert.IsTrue(ubt.Called);
            Assert.AreEqual(uprojectFile, ubt.UProjectFilePath);

            Assert.IsFalse(TestUtils.ExistsDir(uprojectPath,  "Source/MyMod"));

            string fileTargetExp = TestUtils.ReadFile(expectedPath, "Source/UPrjGameOneMod.Target.cs");
            string fileTarget    = TestUtils.ReadFile(uprojectPath, "Source/UPrjGameOneMod.Target.cs");
            FAssert.AreEqualNN(fileTargetExp, fileTarget);

            string fileEdTargetExp = TestUtils.ReadFile(expectedPath, "Source/UPrjGameOneModEditor.Target.cs");
            string fileEdTarget    = TestUtils.ReadFile(uprojectPath, "Source/UPrjGameOneModEditor.Target.cs");
            FAssert.AreEqualNN(fileEdTargetExp, fileEdTarget);

            string filePrjExp = TestUtils.ReadFile(expectedPath, "UPrjGameOneMod.uproject");
            string filePrj    = TestUtils.ReadFile(uprojectPath, "UPrjGameOneMod.uproject");
            FAssert.AreEqualNN(filePrjExp, filePrj);

            TestUtils.DeleteDir(tmpPath);
        }

    }
}