using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections;
using System.Collections.Generic;

namespace FUnreal
{
    public class FUnrealProject
    {
        public FUnrealProject(string uprojectName, string uprojectFilePath)
        {
            Name = uprojectName;
            FilePath = uprojectFilePath;
            FullPath = XFilesystem.PathParent(uprojectFilePath);

            PluginsPath = XFilesystem.PathCombine(FullPath, "Plugins");
        }

        public string Name { get; }
        public string FilePath { get; }
        public string FullPath { get; }
        public string PluginsPath { get; }

        public FUnrealPlugins Plugins
        {
            get
            {
                var result = new FUnrealPlugins();
                List<string> paths = XFilesystem.FindDirectories(PluginsPath);
                foreach (string path in paths)
                {
                    string dirName = XFilesystem.GetLastPathToken(path);
                    FUnrealPlugin plug = new FUnrealPlugin(this, dirName);
                    if (plug.Exists)
                    {
                        result.Add(plug);
                    }
                }
                return result;
            }
        }
    }

    public class FUnrealPlugins : IEnumerable<FUnrealPlugin>
    {
        private List<FUnrealPlugin> _modules = new List<FUnrealPlugin>();
        private Dictionary<string, FUnrealPlugin> _byName = new Dictionary<string, FUnrealPlugin>();

        public void Add(FUnrealPlugin item)
        {
            _modules.Add(item);
            _byName[item.Name] = item;
        }

        public IEnumerator<FUnrealPlugin> GetEnumerator()
        {
            return _modules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public FUnrealPlugin this[string name]
        {
            get
            {
                if (_byName.TryGetValue(name, out var found))
                {
                    return found;
                }
                return null;
            }
        }

        public int Count { get { return _modules.Count; } }

    }
        public class FUnrealPlugin
    {
        private string pluginsPath;

        public FUnrealPlugin(FUnrealProject project, string pluginName)
        {
            this.pluginsPath = project.PluginsPath;
            Name = pluginName;
            this.FullPath = XFilesystem.PathCombine(pluginsPath, pluginName);
            this.SourcePath = XFilesystem.PathCombine(FullPath, "Source");
            this.DescriptorFilePath = XFilesystem.PathCombine(FullPath, $"{pluginName}.uplugin");
        }
        public string Name { get; internal set; }

        public string FullPath { get; internal set; }

        public string SourcePath { get; internal set; }
        public string DescriptorFilePath { get; internal set; }

        public FUnrealPluginModules Modules
        {
            get 
            {
                var result = new FUnrealPluginModules();
                List<string> modulePaths = XFilesystem.FindDirectories(SourcePath);
                foreach(string modulePath in modulePaths)
                {
                    string dirName = XFilesystem.GetLastPathToken(modulePath);
                    FUnrealPluginModule module = new FUnrealPluginModule(this, dirName);
                    if (module.Exists)
                    {
                        result.Add(module);
                    }
                }
                return result; 
            }
        }

        public bool Exists 
            { 
                get { return XFilesystem.FileExists(DescriptorFilePath); }  
            }

        
    }
}