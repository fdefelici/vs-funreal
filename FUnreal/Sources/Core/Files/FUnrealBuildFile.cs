using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FUnreal
{
    public class FUnrealBuildFile : FUnrealCSharpFile
    {
        public FUnrealBuildFile(string filePath) 
            : base(filePath)
        { }

        public bool HasDependency(string moduleName)
        {
            return ContainsStringLiteral(moduleName);
        }
        public void RenameDependency(string moduleName, string newModuleName)
        {
            ReplaceStringLiteral(moduleName, newModuleName);
        }

        public void RemoveDependency(string moduleName)
        {
            //Add Scenario
            if (!HasDependency(moduleName)) return;
            RemoveAddStatementForStringLiteral("PublicDependencyModuleNames", moduleName);
            RemoveAddStatementForStringLiteral("PrivateDependencyModuleNames", moduleName);
            RemoveAddStatementForStringLiteral("DynamicallyLoadedModuleNames", moduleName);

            //AddRange Scenario
            if (!HasDependency(moduleName)) return;
            RemoveStringLiteralFromArray(moduleName);
        }
    }
}
