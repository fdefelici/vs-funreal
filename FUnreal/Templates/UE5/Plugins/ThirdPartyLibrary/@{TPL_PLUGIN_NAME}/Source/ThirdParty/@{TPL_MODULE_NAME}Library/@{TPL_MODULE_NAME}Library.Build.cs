// Fill out your copyright notice in the Description page of Project Settings.

using System.IO;
using UnrealBuildTool;

public class @{TPL_MODULE_NAME}Library : ModuleRules
{
	public @{TPL_MODULE_NAME}Library(ReadOnlyTargetRules Target) : base(Target)
	{
		Type = ModuleType.External;

		if (Target.Platform == UnrealTargetPlatform.Win64)
		{
			// Add the import library
			PublicAdditionalLibraries.Add(Path.Combine(ModuleDirectory, "x64", "Release", "ExampleLibrary.lib"));

			// Delay-load the DLL, so we can load it from the right place first
			PublicDelayLoadDLLs.Add("ExampleLibrary.dll");

			// Ensure that the DLL is staged along with the executable
			//RuntimeDependencies.Add("$(PluginDir)/Binaries/ThirdParty/@{TPL_MODULE_NAME}Library/Win64/ExampleLibrary.dll");
			//FUnreal fix respect to the original template
			string srcDir = Path.Combine(ModuleDirectory, "x64", "Release");
			string binDir = Path.Combine(PluginDirectory, "Binaries", "ThirdParty", "@{TPL_MODULE_NAME}Library", "Win64");
			RuntimeDependencies.Add(Path.Combine(binDir, "ExampleLibrary.dll"), Path.Combine(srcDir, "ExampleLibrary.dll"));
        }
        else if (Target.Platform == UnrealTargetPlatform.Mac)
        {
            PublicDelayLoadDLLs.Add(Path.Combine(ModuleDirectory, "Mac", "Release", "libExampleLibrary.dylib"));
            RuntimeDependencies.Add("$(PluginDir)/Source/ThirdParty/@{TPL_MODULE_NAME}Library/Mac/Release/libExampleLibrary.dylib");
        }
        else if (Target.Platform == UnrealTargetPlatform.Linux)
		{
			string ExampleSoPath = Path.Combine("$(PluginDir)", "Binaries", "ThirdParty", "@{TPL_MODULE_NAME}Library", "Linux", "x86_64-unknown-linux-gnu", "libExampleLibrary.so");
			PublicAdditionalLibraries.Add(ExampleSoPath);
			PublicDelayLoadDLLs.Add(ExampleSoPath);
			RuntimeDependencies.Add(ExampleSoPath);
		}
	}
}
