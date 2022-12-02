using Microsoft.VisualStudio.TestTools.UnitTesting;
using FUnreal;


namespace FUnrealTest
{
    [TestClass]
    public class FUnrealTargetFileTest
    {
        string tmpPath;

        [TestInitialize]
        public void SetUp() 
        {
            tmpPath = TestUtils.AbsPath("FUnrealServiceTest");

            TestUtils.DeleteDir(tmpPath);
        }

        [TestCleanup]
        public void TearDown()
        {
            TestUtils.DeleteDir(tmpPath);
        }

        [TestMethod]
        public void ReadFile_AddRange()
        {
            string targetFile = TestUtils.PathCombine(tmpPath, "MyBuild.Target.cs");
            string targetContents = Target01_AddRange;
            TestUtils.WriteFile(targetFile, targetContents);

            FUnrealTargetFile file = new FUnrealTargetFile(targetFile);
            Assert.AreEqual(targetContents, file.Text);

            Assert.IsTrue(file.HasExtraModule("UPrjGame"));
            Assert.IsTrue(file.HasExtraModule("MyMod"));
        }

        [TestMethod]
        public void ReadFile_AddApi()
        {
            string targetFile = TestUtils.PathCombine(tmpPath, "MyBuild.Target.cs");
            string targetContents = Target02_Add;
            TestUtils.WriteFile(targetFile, targetContents);

            FUnrealTargetFile file = new FUnrealTargetFile(targetFile);
            Assert.AreEqual(targetContents, file.Text);

            Assert.IsTrue(file.HasExtraModule("UPrjGame"));
            Assert.IsTrue(file.HasExtraModule("MyMod"));
        }

        [TestMethod]
        public void AddModule_with_AddRange_api()
        {
            string targetFile = TestUtils.PathCombine(tmpPath, "MyBuild.Target.cs");
            string targetContents = Target01_AddRange;
            TestUtils.WriteFile(targetFile, targetContents);

            FUnrealTargetFile file = new FUnrealTargetFile(targetFile);
            file.AddExtraModule("NewMod");

            Assert.IsTrue(file.Save());
            Assert.AreEqual(Target01_AddRange_AddExpected, TestUtils.ReadFile(targetFile));
        }

        [TestMethod]
        public void AddModule_with_Add_api()
        {
            string targetFile = TestUtils.PathCombine(tmpPath, "MyBuild.Target.cs");
            string targetContents = Target02_Add;
            TestUtils.WriteFile(targetFile, targetContents);

            FUnrealTargetFile file = new FUnrealTargetFile(targetFile);
            file.AddExtraModule("NewMod");

            Assert.IsTrue(file.Save());
            Assert.AreEqual(Target02_Add_AddExpected, TestUtils.ReadFile(targetFile));
        }

        [TestMethod]
        public void RenameModule_with_AddRange_api()
        {
            string targetFile = TestUtils.PathCombine(tmpPath, "MyBuild.Target.cs");
            string targetContents = Target01_AddRange;
            TestUtils.WriteFile(targetFile, targetContents);

            FUnrealTargetFile file = new FUnrealTargetFile(targetFile);
            file.RenameExtraModule("MyMod", "MyModRenamed");

            Assert.IsTrue(file.Save());
            Assert.AreEqual(Target01_AddRange_RenameExpected, TestUtils.ReadFile(targetFile));
        }

        [TestMethod]
        public void RenameModule_with_Add_api()
        {
            string targetFile = TestUtils.PathCombine(tmpPath, "MyBuild.Target.cs");
            string targetContents = Target02_Add;
            TestUtils.WriteFile(targetFile, targetContents);

            FUnrealTargetFile file = new FUnrealTargetFile(targetFile);
            file.RenameExtraModule("MyMod", "MyModRenamed");

            Assert.IsTrue(file.Save());
            Assert.AreEqual(Target02_Add_RenameExpected, TestUtils.ReadFile(targetFile));
        }


        [TestMethod]
        public void RemoveModule_with_AddRange_api()
        {
            string targetFile = TestUtils.PathCombine(tmpPath, "MyBuild.Target.cs");
            string targetContents = Target01_AddRange;
            TestUtils.WriteFile(targetFile, targetContents);

            FUnrealTargetFile file = new FUnrealTargetFile(targetFile);
            file.RemoveExtraModule("MyMod");

            Assert.IsTrue(file.Save());
            Assert.AreEqual(Target01_AddRange_RemoveExpected, TestUtils.ReadFile(targetFile));
        }

        [TestMethod]
        public void RemoveModule_with_Add_api()
        {
            string targetFile = TestUtils.PathCombine(tmpPath, "MyBuild.Target.cs");
            string targetContent = Target02_Add;
            TestUtils.WriteFile(targetFile, targetContent);

            FUnrealTargetFile file = new FUnrealTargetFile(targetFile);
            file.RemoveExtraModule("MyMod");

            Assert.IsTrue(file.Save());
            Assert.AreEqual(Target02_Add_RemoveExpected, TestUtils.ReadFile(targetFile));
        }

        private const string Target02_Add = @"
public class UPrjGameTarget : TargetRules
{
    public UPrjGameTarget(TargetInfo Target) : base(Target)
    {
        Type = TargetType.Game;
        DefaultBuildSettings = BuildSettingsVersion.V2;
        ExtraModuleNames.Add(""UPrjGame"");
        ExtraModuleNames.Add(""MyMod"");
    }
}";

        private const string Target02_Add_AddExpected = @"
public class UPrjGameTarget : TargetRules
{
    public UPrjGameTarget(TargetInfo Target) : base(Target)
    {
        Type = TargetType.Game;
        DefaultBuildSettings = BuildSettingsVersion.V2;
        ExtraModuleNames.Add(""UPrjGame"");
        ExtraModuleNames.Add(""MyMod"");
        ExtraModuleNames.Add(""NewMod"");
    }
}";

        private const string Target02_Add_RenameExpected = @"
public class UPrjGameTarget : TargetRules
{
    public UPrjGameTarget(TargetInfo Target) : base(Target)
    {
        Type = TargetType.Game;
        DefaultBuildSettings = BuildSettingsVersion.V2;
        ExtraModuleNames.Add(""UPrjGame"");
        ExtraModuleNames.Add(""MyModRenamed"");
    }
}";

        private const string Target02_Add_RemoveExpected = @"
public class UPrjGameTarget : TargetRules
{
    public UPrjGameTarget(TargetInfo Target) : base(Target)
    {
        Type = TargetType.Game;
        DefaultBuildSettings = BuildSettingsVersion.V2;
        ExtraModuleNames.Add(""UPrjGame"");
    }
}";

        private const string Target01_AddRange = @"
public class UPrjGameTarget : TargetRules
{
    public UPrjGameTarget(TargetInfo Target) : base(Target)
    {
        Type = TargetType.Game;
        DefaultBuildSettings = BuildSettingsVersion.V2;
        ExtraModuleNames.AddRange( new string[] { ""UPrjGame"", ""MyMod"" });
    }
}";

        private const string Target01_AddRange_AddExpected = @"
public class UPrjGameTarget : TargetRules
{
    public UPrjGameTarget(TargetInfo Target) : base(Target)
    {
        Type = TargetType.Game;
        DefaultBuildSettings = BuildSettingsVersion.V2;
        ExtraModuleNames.AddRange( new string[] { ""UPrjGame"", ""MyMod"", ""NewMod"" });
    }
}";

        private const string Target01_AddRange_RenameExpected = @"
public class UPrjGameTarget : TargetRules
{
    public UPrjGameTarget(TargetInfo Target) : base(Target)
    {
        Type = TargetType.Game;
        DefaultBuildSettings = BuildSettingsVersion.V2;
        ExtraModuleNames.AddRange( new string[] { ""UPrjGame"", ""MyModRenamed"" });
    }
}";

        private const string Target01_AddRange_RemoveExpected = @"
public class UPrjGameTarget : TargetRules
{
    public UPrjGameTarget(TargetInfo Target) : base(Target)
    {
        Type = TargetType.Game;
        DefaultBuildSettings = BuildSettingsVersion.V2;
        ExtraModuleNames.AddRange( new string[] { ""UPrjGame"" });
    }
}";
    }
}