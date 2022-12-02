using Microsoft.VisualStudio.TestTools.UnitTesting;
using FUnreal;


namespace FUnrealTest
{
    [TestClass]
    public class FUnrealBuildFileTest
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

        private FUnrealBuildFile CreateFile(string contents)
        {
            string targetFile = TestUtils.PathCombine(tmpPath, "MyBuild.Target.cs");
            string targetContents = contents;
            TestUtils.WriteFile(targetFile, targetContents);

            var file = new FUnrealBuildFile(targetFile);
            return file;
        }


        [TestMethod]
        public void ReadFile_AddRange()
        {
            var contents = Build01_AddRange;
            var file = CreateFile(contents);
            Assert.AreEqual(contents, file.Text);

            Assert.IsTrue(file.HasDependency("PublicMod"));
            Assert.IsTrue(file.HasDependency("PrivateMod"));
            Assert.IsTrue(file.HasDependency("AllTypeMod"));
            Assert.IsFalse(file.HasDependency("NotExists"));
        }

        [TestMethod]
        public void ReadFile_AddApi()
        {
            var contents = Build02_Add;
            var file = CreateFile(contents);
            Assert.AreEqual(contents, file.Text);

            Assert.IsTrue(file.HasDependency("PublicMod"));
            Assert.IsTrue(file.HasDependency("PrivateMod"));
            Assert.IsTrue(file.HasDependency("AllTypeMod"));
            Assert.IsFalse(file.HasDependency("NotExists"));
        }

        [TestMethod]
        public void RenameModule_with_AddRange_api()
        {
            var contents = Build01_AddRange;
            var file = CreateFile(contents);

            file.RenameDependency("AllTypeMod", "AllTypeModRenamed");

            Assert.IsTrue(file.Save());
            Assert.AreEqual(Build01_AddRange_RenameExpected, TestUtils.ReadFile(file.FilePath));
        }


        [TestMethod]
        public void RenameModule_with_Add_api()
        {
            var contents = Build02_Add;
            var file = CreateFile(contents);

            file.RenameDependency("AllTypeMod", "AllTypeModRenamed");

            Assert.IsTrue(file.Save());
            Assert.AreEqual(Build02_Add_RenameExpected, TestUtils.ReadFile(file.FilePath));
        }

        [TestMethod]
        public void RemoveModule_with_AddRange_api()
        {
            var contents = Build01_AddRange;
            var file = CreateFile(contents);

            file.RemoveDependency("AllTypeMod");

            Assert.IsTrue(file.Save());
            Assert.AreEqual(Build01_AddRange_RemoveExpected, TestUtils.ReadFile(file.FilePath));
        }

        [TestMethod]
        public void RemoveModule_with_Add_api()
        {
            var contents = Build02_Add;
            var file = CreateFile(contents);

            file.RemoveDependency("AllTypeMod");

            Assert.IsTrue(file.Save());
            Assert.AreEqual(Build02_Add_RemoveExpected, TestUtils.ReadFile(file.FilePath));
        }


        private const string Build02_Add = @"
public class Win01 : ModuleRules
{
	public Win01(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;
		
		PublicDependencyModuleNames.Add(""Core"");
        PublicDependencyModuleNames.Add(""PublicMod"");
        PublicDependencyModuleNames.Add(""AllTypeMod"");
			
		PrivateDependencyModuleNames.Add(""UnrealEd"");
        PrivateDependencyModuleNames.Add(""PrivateMod"");
        PrivateDependencyModuleNames.Add(""AllTypeMod"");

        DynamicallyLoadedModuleNames.Add(""AllTypeMod"");
	}
}
";

        private const string Build02_Add_RenameExpected = @"
public class Win01 : ModuleRules
{
	public Win01(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;
		
		PublicDependencyModuleNames.Add(""Core"");
        PublicDependencyModuleNames.Add(""PublicMod"");
        PublicDependencyModuleNames.Add(""AllTypeModRenamed"");
			
		PrivateDependencyModuleNames.Add(""UnrealEd"");
        PrivateDependencyModuleNames.Add(""PrivateMod"");
        PrivateDependencyModuleNames.Add(""AllTypeModRenamed"");

        DynamicallyLoadedModuleNames.Add(""AllTypeModRenamed"");
	}
}
";

        private const string Build02_Add_RemoveExpected = @"
public class Win01 : ModuleRules
{
	public Win01(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;
		
		PublicDependencyModuleNames.Add(""Core"");
        PublicDependencyModuleNames.Add(""PublicMod"");
			
		PrivateDependencyModuleNames.Add(""UnrealEd"");
        PrivateDependencyModuleNames.Add(""PrivateMod"");
	}
}
";

        private const string Build01_AddRange = @"
public class Win01 : ModuleRules
{
	public Win01(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;
		
		PublicDependencyModuleNames.AddRange(
			new string[]
			{
				""Core"",
				// ... add other public dependencies that you statically link with here ...
                ""PublicMod"",
                ""AllTypeMod""
			}
			);
			
		PrivateDependencyModuleNames.AddRange(
			new string[]
			{
				""UnrealEd"",
				// ... add private dependencies that you statically link with here ...	
                ""PrivateMod"",
                ""AllTypeMod""
			}
			);

		DynamicallyLoadedModuleNames.AddRange(
			new string[]
			{
                ""AllTypeMod""
				// ... add any modules that your module loads dynamically here ...
			}
			);
	}
}
";

        private const string Build01_AddRange_RenameExpected = @"
public class Win01 : ModuleRules
{
	public Win01(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;
		
		PublicDependencyModuleNames.AddRange(
			new string[]
			{
				""Core"",
				// ... add other public dependencies that you statically link with here ...
                ""PublicMod"",
                ""AllTypeModRenamed""
			}
			);
			
		PrivateDependencyModuleNames.AddRange(
			new string[]
			{
				""UnrealEd"",
				// ... add private dependencies that you statically link with here ...	
                ""PrivateMod"",
                ""AllTypeModRenamed""
			}
			);

		DynamicallyLoadedModuleNames.AddRange(
			new string[]
			{
                ""AllTypeModRenamed""
				// ... add any modules that your module loads dynamically here ...
			}
			);
	}
}
";

        private const string Build01_AddRange_RemoveExpected = @"
public class Win01 : ModuleRules
{
	public Win01(ReadOnlyTargetRules Target) : base(Target)
	{
		PCHUsage = ModuleRules.PCHUsageMode.UseExplicitOrSharedPCHs;
		
		PublicDependencyModuleNames.AddRange(
			new string[]
			{
				""Core"",
				// ... add other public dependencies that you statically link with here ...
                ""PublicMod""
			}
			);
			
		PrivateDependencyModuleNames.AddRange(
			new string[]
			{
				""UnrealEd"",
				// ... add private dependencies that you statically link with here ...	
                ""PrivateMod""
			}
			);

		DynamicallyLoadedModuleNames.AddRange(
			new string[]
			{
				// ... add any modules that your module loads dynamically here ...
			}
			);
	}
}
";
    }
}