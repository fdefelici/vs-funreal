using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FUnreal
{
    public class FUnrealTargetFile : FUnrealCSharpFile
    {
        public FUnrealTargetFile(string filePath) 
            : base(filePath) 
        { }

        public bool HasExtraModule(string moduleName) 
        {
            return base.ContainsStringLiteral(moduleName);
        }

        public void RenameExtraModule(string moduleName, string newModuleName)
        {
            base.ReplaceStringLiteral(moduleName, newModuleName);
        }


        public void RemoveExtraModule(string moduleName)
        {
            //Add Scenario
            if (!HasExtraModule(moduleName)) return;
            RemoveAddStatementForStringLiteral("ExtraModuleNames", moduleName);

            //AddRange Scenario
            if (!HasExtraModule(moduleName)) return;
            RemoveStringLiteralFromArray(moduleName);
        }

       
        public void AddExtraModule(string moduleName)
        {
            //Capture Group1 for all module names such as: ("Mod1", "Mod2") and replacing with ("Mod1", "Mod2", "ModuleName")
            string regex = @"ExtraModuleNames\s*\.AddRange\s*\(\s*new\s*string\[\]\s*\{\s*(\"".+\"")\s*\}\s*\)\s*;";
            var match = Regex.Match(Text, regex);  //this return the first match, just in case multiple .AddRange are used
            if (match.Success && match.Groups.Count == 2)
            {
                string moduleList = match.Groups[1].Value;
                Text = Text.Replace(moduleList, $"{moduleList}, \"{moduleName}\"");
                return;
            }

            string regexAddApi = $@"[^\S\r\n]*ExtraModuleNames\s*.Add\(\s*""\w+""\s*\)\s*;";
            match = Regex.Match(Text, regexAddApi, RegexOptions.RightToLeft);
            if (match.Success)
            {
                string lastAddApi = match.Value;

                int startExtraModuleNames = lastAddApi.IndexOf("ExtraModuleNames");
                string startSpaces = lastAddApi.Substring(0, startExtraModuleNames);
                string subst = $"{lastAddApi}{XFilesystem.NewLineStr}{startSpaces}ExtraModuleNames.Add(\"{moduleName}\");";
                Text = Text.Replace(lastAddApi, subst);
            }
        }
    }
}
