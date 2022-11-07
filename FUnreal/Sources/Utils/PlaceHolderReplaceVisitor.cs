using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.Collections.Generic;

namespace FUnreal
{
    public class PlaceHolderReplaceStrategy
    {
        Dictionary<string, string> placeholder;
        private List<string> fileExts;

        public PlaceHolderReplaceStrategy()
        {
            placeholder = new Dictionary<string, string>();
            fileExts = new List<string>();
        }

        public void AddFileExtension(params string[] exts)
        {
            fileExts.AddRange(exts);    
        }

        public void AddPlaceholder(string key, string value)
        {
            placeholder[key] = value;
        }

        public string HandlePath(string fsPath)
        {
           return ReplacePlaceholder(fsPath);
        }

        public void HandleFileContent(string targetFile)
        {
            if (! XFilesystem.HasExtension(targetFile, fileExts) ) return;

            string contents = XFilesystem.ReadFile(targetFile);
            string newContents = ReplacePlaceholder(contents);
            XFilesystem.WriteFile(targetFile, newContents);
        }

        private string ReplacePlaceholder(string text)
        {
            string result = text;
            foreach (KeyValuePair<string, string> pair in placeholder)
            {
                result = result.Replace(pair.Key, pair.Value);
            }
            return result;
        }
    }
}