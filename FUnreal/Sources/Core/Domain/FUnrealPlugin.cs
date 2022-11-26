using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace FUnreal
{
    public class FUnrealPlugin : IFUnrealModuleContainer, IFunrealCollectionItem
    {
        private readonly FUnrealProject Parent;

        public FUnrealPlugin(FUnrealProject uproject, string plugFileRelPath)
        {
            Parent = uproject;

            SetDescriptorFileRelPath(plugFileRelPath);

            Modules = new FUnrealCollection<FUnrealModule>();
        }

        public string Name { get; internal set; }

        public string FullPath { get; internal set; }

        public string SourcePath { get; internal set; }
        public string DescriptorFilePath { get; internal set; }

        public string ContentPath { get; internal set; }
        public string ResourcesPath { get; internal set; }
        public string ShadersPath { get; internal set; }

        public FUnrealCollection<FUnrealModule> Modules { get; }

        public bool Exists
        {
            get { return XFilesystem.FileExists(DescriptorFilePath); }
        }

        public void SetDescriptorFileRelPath(string uplugFileRelPath)
        {
            DescriptorFilePath = XFilesystem.PathCombine(Parent.PluginsPath, uplugFileRelPath);

            Name = XFilesystem.GetFilenameNoExt(DescriptorFilePath);
            FullPath = XFilesystem.PathParent(DescriptorFilePath);
            SourcePath = XFilesystem.PathCombine(FullPath, "Source");
            ContentPath = XFilesystem.PathCombine(FullPath, "Content");
            ResourcesPath = XFilesystem.PathCombine(FullPath, "Resources");
            ShadersPath = XFilesystem.PathCombine(FullPath, "Shaders");
        }

        public override string ToString()
        {
            return Name;
        }
        
    }
}