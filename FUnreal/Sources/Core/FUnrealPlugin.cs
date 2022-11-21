using Microsoft.VisualStudio.TextManager.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FUnreal
{
    public class FUnrealProject : IFUnrealModuleContainer
    {
        public FUnrealProject(string uprojectName, string uprojectFilePath)
        {
            Name = uprojectName;
            DescriptorFilePath = uprojectFilePath;
            FullPath = XFilesystem.PathParent(uprojectFilePath);

            PluginsPath = XFilesystem.PathCombine(FullPath, "Plugins");
            SourcePath = XFilesystem.PathCombine(FullPath, "Source");

            Plugins = new FUnrealPlugins();
            GameModules = new FUnrealCollection<FUnrealModule>();
            /*
            PluginModules = new FUnrealCollection<FUnrealModule>();
            */
            AllModules = new FUnrealCollection<FUnrealModule>();
        }

        public string Name { get; }
        public string DescriptorFilePath { get; }
        public string FullPath { get; }
        public string PluginsPath { get; }
        public string SourcePath { get; internal set; }
        public FUnrealCollection<FUnrealModule> GameModules { get; }
        public FUnrealPlugins Plugins { get; set; }

        public FUnrealCollection<FUnrealModule> AllModules { get; }


        /*
        public FUnrealCollection<FUnrealModule> AllModules { get 
            {
                var result  = new FUnrealCollection<FUnrealModule>();
                foreach(var plug in Plugins)
                {
                    result.AddAll(plug.Modules);
                }

                result.AddAll(GameModules);
                return result;
            } 
        }
        */

        public List<string> TargetFiles
        {
            get
            {
                List<string> paths = XFilesystem.FindFiles(SourcePath, false, "*.Target.cs");
                return paths;
            }
        }

        public void Clear()
        {
            Plugins.Clear();
            GameModules.Clear();
        }
    }

    public interface IFunrealCollectionItem
    {
        string Name { get; }
        string FullPath { get; }
    }

    public class FUnrealCollection<T> : IEnumerable<T> where T : IFunrealCollectionItem
    {
        private List<T> list = new List<T>();
        private Dictionary<string, T> dict = new Dictionary<string, T>();

        public int Count { get { return list.Count; } }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void AddAll(FUnrealCollection<T> items)
        {
            foreach(var item in items)
            {
                Add(item);
            }
        }

        public void Add(T item)
        {
            this[item.Name] = item;
        }

        public void Clear()
        {
            list.Clear();
            dict.Clear();   
        }

        public T FindByPath(string fullPath)
        {
            foreach(var item in list)
            {
                if (item.FullPath == fullPath) return item;
            }
            return default(T);
        }

        public T FindByBelongingPath(string innerFullPath)
        {
            foreach (var item in list)
            {
                if (innerFullPath.Contains(item.FullPath)) return item;
            }
            return default(T);
        }

        public T this[string name]
        {
            get
            {
                if (dict.TryGetValue(name, out var found))
                {
                    return found;
                }
                return default(T);
            }

            set
            {
                int listPost = list.Count;
                if (dict.ContainsKey(name))
                {
                    var item = dict[name];
                    listPost = list.IndexOf(item);
                    list.RemoveAt(listPost);
                }

                dict[name] = value;
                list.Insert(listPost, value);
            }
        }
    }


    public class FUnrealPlugins : IEnumerable<FUnrealPlugin>
    {
        private List<FUnrealPlugin> _list = new List<FUnrealPlugin>();
        private Dictionary<string, FUnrealPlugin> _byName = new Dictionary<string, FUnrealPlugin>();

        public void Add(FUnrealPlugin item)
        {
            _list.Add(item);
            _byName[item.Name] = item;
        }

        public IEnumerator<FUnrealPlugin> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Exists(string name)
        {
            return this[name] != null;  
        }

        public void Clear()
        {
            _list.Clear();
            _byName.Clear();
        }

        public FUnrealPlugin FindByPath(string fullPath)
        {
            foreach(var plug in _list)
            {
                if (plug.FullPath == fullPath) return plug;
            }
            return null;
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

        public int Count { get { return _list.Count; } }

        public void Remove(FUnrealPlugin plug)
        {
            _list.Remove(plug);
            _byName.Remove(plug.Name);
        }

        public FUnrealPlugin FindByBelongingPath(string innerFullPath)
        {
            foreach(var item in _list)
            {
                if (innerFullPath.Contains(item.FullPath)) return item; 
            }
            return null;
        }
    }
    public class FUnrealPlugin : IFUnrealModuleContainer
    {
        private readonly FUnrealProject uproject;

        public FUnrealPlugin(FUnrealProject uproject, string plugName, string plugFile)
        {
            this.uproject = uproject;
            
            SetDescriptorFilePath(plugFile);

            Modules = new FUnrealCollection<FUnrealModule>();
        }

        public string Name { get; internal set; }

        public string FullPath { get; internal set; }

        public string SourcePath { get; internal set; }
        public string DescriptorFilePath { get; internal set; }

        public FUnrealCollection<FUnrealModule> Modules { get; }

        public bool Exists
        {
            get { return XFilesystem.FileExists(DescriptorFilePath); }
        }

        public void SetDescriptorFilePath(string newPath)
        {
            DescriptorFilePath = newPath;

            Name = XFilesystem.GetFilenameNoExt(DescriptorFilePath);
            FullPath = XFilesystem.PathParent(DescriptorFilePath);
            SourcePath = XFilesystem.PathCombine(FullPath, "Source");
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public interface IFUnrealModuleContainer
    {
        string FullPath { get;}
    }

    public class FUnrealModule : IFunrealCollectionItem
    {
        public FUnrealModule(IFUnrealModuleContainer owner, string name, string moduleBuildFilePath)
        {
            Name = name;
            FullPath = XFilesystem.PathParent(moduleBuildFilePath);
            BuildFilePath = moduleBuildFilePath;
            PublicPath = XFilesystem.PathCombine(FullPath, "Public");
            PrivatePath = XFilesystem.PathCombine(FullPath, "Private");
            ApiMacro = $"{Name.ToUpper()}_API";
        }

        public string Name { get; }
        public string FullPath { get; }
        public string BuildFilePath { get; }
        public string PublicPath { get; }
        public string PrivatePath { get; internal set; }

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