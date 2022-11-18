using FUnreal;
using FUnreal.Sources.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FUnrealTest
{
    [TestClass]
    public class XStringTest
    {
        [TestMethod]
        public void FormatReturnOriginalStringInCaseOfException()
        {
            string wrongFormattedString = @"og file: C:\Users\fdf82\AppData\Local\UnrealBuildTool\Log_GPF.txt
Discovering modules, targets and source code for project...
WARNING: Errors generated while parsing 'C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs'
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(5,14): error CS1001: Identifier expected
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(5,14): error CS1514: { expected
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(5,14): error CS1513: } expected
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(5,14): error CS1646: Keyword, identifier, or string expected after verbatim specifier: @
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(5,15): error CS8652: The feature 'top-level statements' is currently in Preview and *unsupported*. To use Preview features, use the 'preview' language version.
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(5,15): error CS8803: Top-level statements must precede namespace and type declarations.
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(5,28): error CS1002: ; expected
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(5,30): error CS1022: Type or namespace definition, or end-of-file expected
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(5,32): error CS0116: A namespace cannot directly contain members such as fields or methods
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(6,2): error CS1513: } expected
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(7,2): error CS0116: A namespace cannot directly contain members such as fields or methods
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(7,9): error CS1646: Keyword, identifier, or string expected after verbatim specifier: @
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(7,23): error CS1002: ; expected
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(7,45): error CS1026: ) expected
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(7,45): error CS1002: ; expected
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(7,45): error CS0116: A namespace cannot directly contain members such as fields or methods
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(7,51): error CS1022: Type or namespace definition, or end-of-file expected
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(7,67): error CS1002: ; expected
C:\_fdf\workspace_unreal\ws_unreal\UENLOpt\Plugins\NewPlugin\Source\MyModule\MyModule.Build.cs(53,1): error CS1022: Type or namespace definition, or end-of-file expected
ERROR: Expecting to find a type to be declared in a target rules named 'UENLOptTarget'.  This type must derive from the 'TargetRules' type defined by Unreal Build Tool.";
            
            var result = XString.Format(wrongFormattedString);
            Assert.AreEqual(wrongFormattedString, result);
        }
    }
}
