using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FUnreal
{
    public class FUnrealTargetFile
    {
        private string _filePath;

        public string Text { get; private set; }
        public bool IsOpened { get; private set; }

        public FUnrealTargetFile(string filePath) 
        {
            _filePath = filePath;   
            Text = XFilesystem.FileRead(filePath);
            IsOpened = Text != null;
        }

        public bool Save()
        {
            return XFilesystem.FileWrite(_filePath, Text);
        }

        public bool HasExtraModule(string moduleName) 
        {
            string moduleDepend = $"\"{moduleName}\"";
            return Text.Contains(moduleDepend);
        }

        public void RemoveExtraModule(string moduleName)
        {
            if (!HasExtraModule(moduleName)) return;

            //ExtraModuleNames.Add scenario
            string regexAddApi_Strict =  $@"^\s*ExtraModuleNames\s*.Add\(\s*""{moduleName}""\s*\)\s*;\s*$(\r\n|\r|\n)";
            string regexAddApi_General = $@"\s*ExtraModuleNames\s*.Add\(\s*""{moduleName}""\s*\)\s*;";
            string regexAddRange_Api = $@"(?<!,\s*)\s*""{moduleName}""\s*,|,{{0,1}}\s*""{moduleName}""";

            string selectedRegex = null;
            RegexOptions options;
            if (Regex.IsMatch(Text, regexAddApi_Strict, RegexOptions.Multiline))
            {
                selectedRegex = regexAddApi_Strict;
                options = RegexOptions.Multiline;
            }
            else if (Regex.IsMatch(Text, regexAddApi_General, RegexOptions.Multiline))
            {
                selectedRegex = regexAddApi_General;
                options = RegexOptions.Multiline;
            }
            else //Try AddRange
            {
                selectedRegex = regexAddRange_Api;
                options = RegexOptions.None;
            }

            Text = Regex.Replace(Text, selectedRegex, string.Empty, options);
        }

        public void RenameExtraModule(string moduleName, string newModuleName)
        {
            string dependency = $"\"{moduleName}\"";
            string newDependency = $"\"{newModuleName}\"";
            Text = Text.Replace(dependency, newDependency);
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
