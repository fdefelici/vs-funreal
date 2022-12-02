using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FUnreal
{
    public class FUnrealCSharpFile
    {
        public string FilePath { get; private set; }

        public string Text { get; protected set; }
        public bool IsOpened { get; private set; }

        public FUnrealCSharpFile(string filePath)
        {
            FilePath = filePath;
            Text = XFilesystem.FileRead(filePath);
            IsOpened = Text != null;
        }

        public bool Save()
        {
            return XFilesystem.FileWrite(FilePath, Text);
        }

        public bool ContainsStringLiteral(string literal)
        {
            return Text.Contains($"\"{literal}\"");
        }

        public void ReplaceStringLiteral(string literal, string newLiteral)
        {
            string oldValue = $"\"{literal}\"";
            string newValue = $"\"{newLiteral}\"";
            Text = Text.Replace(oldValue, newValue);
        }


        public bool RemoveAddStatementForStringLiteral(string collectionName, string literal)
        {
            string regexAddApi_Strict = $@"^\s*{collectionName}\s*.Add\(\s*""{literal}""\s*\)\s*;[^\S\r\n]*(?:\r\n|\r|\n)";
            //string regexAddApi_Strict = $@"^\s*{collectionName}\s*.Add\(\s*""{literal}""\s*\)\s*;";
            //string regexAddApi_Strict = $@"^\s*{collectionName}\s*.Add\(\s*""{literal}""\s*\)\s*;[^\S\r\n]*$(\r\n|\r|\n)";
            //string regexAddApi_General = $@"\s*{collectionName}\s*.Add\(\s*""{literal}""\s*\)\s*;";
            string regexAddApi_General = $@"[^\S\r\n]*{collectionName}\s*.Add\(\s*""{literal}""\s*\)\s*;";

            if (Regex.IsMatch(Text, regexAddApi_Strict, RegexOptions.Multiline))
            {
                Text = Regex.Replace(Text, regexAddApi_Strict, string.Empty, RegexOptions.Multiline);
                return true;
            }

            if (Regex.IsMatch(Text, regexAddApi_General, RegexOptions.Multiline))
            {
                Text = Regex.Replace(Text, regexAddApi_General, string.Empty, RegexOptions.Multiline);
                return true;
            }

            return false;
        }

        public bool RemoveStringLiteralFromArray(string literal)
        {
            string regexAddRange_Api = $@"(?<!,\s*)\s*""{literal}""\s*,|,{{0,1}}\s*""{literal}""";
            if (Regex.IsMatch(Text, regexAddRange_Api))
            {
                Text = Regex.Replace(Text, regexAddRange_Api, string.Empty);
                return true;
            }
            return false;
        }
    }
}
