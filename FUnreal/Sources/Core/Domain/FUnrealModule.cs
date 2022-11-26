using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace FUnreal
{
    public class FUnrealModule : IFunrealCollectionItem
    {
        public FUnrealModule(IFUnrealModuleContainer owner, string moduleBuildFileRelPath)
        {            
            Parent = owner;
            BuildFileRelPath = moduleBuildFileRelPath;
            ApiMacro = $"{Name.ToUpper()}_API";
        }
        public void SetBuilFileRelPath(string newRelFile)
        {
            BuildFileRelPath = newRelFile;
        }

        public IFUnrealModuleContainer Parent { get; }
        public string Name { get => XFilesystem.GetFilenameNoExt(BuildFileRelPath, true); }
        public string BuildFileRelPath { get; internal set; }
        public string FullPath { get => XFilesystem.PathCombine(Parent.SourcePath, XFilesystem.PathParent(BuildFileRelPath)); }
        public string BuildFilePath { get => XFilesystem.PathCombine(Parent.SourcePath, BuildFileRelPath); }
        public string PublicPath { get => XFilesystem.PathCombine(FullPath, "Public"); }
        public string PrivatePath { get => XFilesystem.PathCombine(FullPath, "Private"); }

        public string ApiMacro { get; }

        public bool Exists { get { return XFilesystem.FileExists(BuildFilePath); } }

        public bool IsPrimaryGameModule { get; set; }
       
        public override bool Equals(object obj)
        {
            if (!(obj is FUnrealModule)) return false;
            FUnrealModule mod = obj as FUnrealModule;
            return Name == mod.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}