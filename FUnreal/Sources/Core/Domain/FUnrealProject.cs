using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace FUnreal
{
    public class FUnrealProject : IFUnrealModuleContainer
    {
        public FUnrealProject(string uprojectFilePath)
        {
            Name = XFilesystem.GetFilenameNoExt(uprojectFilePath);
            DescriptorFilePath = uprojectFilePath;
            FullPath = XFilesystem.PathParent(uprojectFilePath);

            PluginsPath = XFilesystem.PathCombine(FullPath, "Plugins");
            SourcePath = XFilesystem.PathCombine(FullPath, "Source");

            Plugins = new FUnrealCollection<FUnrealPlugin>();
            GameModules = new FUnrealCollection<FUnrealModule>();
            AllModules = new FUnrealCollection<FUnrealModule>();
        }

        public string Name { get; }
        public string DescriptorFilePath { get; }
        public string FullPath { get; }
        public string PluginsPath { get; }
        public string SourcePath { get; internal set; }

        public FUnrealCollection<FUnrealModule> GameModules { get; }
        public FUnrealCollection<FUnrealPlugin> Plugins { get; set; }

        public FUnrealCollection<FUnrealModule> AllModules { get; }

        public List<string> TargetFiles
        {
            get
            {
                List<string> paths = XFilesystem.FindFilesEnum(SourcePath, false, "*.Target.cs").ToList();
                return paths;
            }
        }

        public void Clear()
        {
            Plugins.Clear();
            GameModules.Clear();
        }
    }
}