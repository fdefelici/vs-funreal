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
            DescriptorFilePath = uprojectFilePath;
            FullPath = XFilesystem.PathParent(uprojectFilePath);

            PluginsPath = XFilesystem.PathCombine(FullPath, "Plugins");
            SourcePath = XFilesystem.PathCombine(FullPath, "Source");
        }

        public string Name { get; }
        public string DescriptorFilePath { get; }
        public string FullPath { get; }
        public string PluginsPath { get; }
        public string SourcePath { get; internal set; }

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

        public FUnrealGameModules GameModules
        {
            get
            {
                var result = new FUnrealGameModules();
                List<string> paths = XFilesystem.FindDirectories(SourcePath);
                foreach (string path in paths)
                {
                    string dirName = XFilesystem.GetLastPathToken(path);
                    FUnrealGameModule mod = new FUnrealGameModule(this, dirName);
                    if (mod.Exists)
                    {
                        result.Add(mod);
                    }
                }
                return result;
            }
        }
        public List<string> TargetFiles
        {
            get
            {
                List<string> paths = XFilesystem.FindFiles(SourcePath, false, "*.Target.cs");
                return paths;
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
                foreach (string modulePath in modulePaths)
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


    public class FUnrealGameModules : IEnumerable<FUnrealGameModule>
    {
        private List<FUnrealGameModule> _modules = new List<FUnrealGameModule>();
        private Dictionary<string, FUnrealGameModule> _byName = new Dictionary<string, FUnrealGameModule>();

        public void Add(FUnrealGameModule module)
        {
            _modules.Add(module);
            _byName[module.Name] = module;
        }

        public IEnumerator<FUnrealGameModule> GetEnumerator()
        {
            return _modules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public FUnrealGameModule this[string name]
        {
            get
            {
                if (_byName.TryGetValue(name, out var module))
                {
                    return module;
                }
                return null;
            }
        }

        public int Count { get { return _modules.Count; } }
    }

    public class FUnrealGameModule
    {
        public FUnrealGameModule(FUnrealProject project, string moduleName)
        {
            Name = moduleName;
            FullPath = XFilesystem.PathCombine(project.SourcePath, moduleName);
            BuildFilePath = XFilesystem.PathCombine(FullPath, $"{moduleName}.Build.cs");
            PublicPath = XFilesystem.PathCombine(FullPath, "Public");
            ApiMacro = $"{moduleName.ToUpper()}_API";
        }

        public string Name { get; }
        public string FullPath { get; }
        public string BuildFilePath { get; }

        public bool Exists { get { return XFilesystem.FileExists(BuildFilePath); } }

        public bool IsPrimaryGame
        {
            get
            {
                string found = XFilesystem.FindFile(FullPath, true, "*.cpp", file =>
                {
                    string text = XFilesystem.ReadFile(file);
                    if (text.Contains("PRIMARY_GAME_MODULE")) return true;
                    return false;
                });

                if (found == null) return false;
                return true;
            }
        }

        public string PublicPath { get; internal set; }
        public string ApiMacro { get; internal set; }

        public override bool Equals(object obj)
        {
            if (!(obj is FUnrealGameModule)) return false;
            FUnrealGameModule mod = obj as FUnrealGameModule;
            return Name == mod.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}