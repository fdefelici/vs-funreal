using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace FUnreal
{

    public class XFSWatcher 
    {
        private FileSystemWatcher _watcher;

        public XFSWatcher()
        {
            _watcher = new FileSystemWatcher();
            Name = $"{this.GetType().Name}_{GetHashCode()}";
        }
        public string Path { get { return _watcher.Path; } set { _watcher.Path = value; } }
        public string Filter { get { return _watcher.Filter; } set { _watcher.Filter = value; } }
        public NotifyFilters NotifyFilter { get { return _watcher.NotifyFilter; } set { _watcher.NotifyFilter = value; } }
        public bool IncludeSubdirectories { get { return _watcher.IncludeSubdirectories; } set { _watcher.IncludeSubdirectories = value; } }

        public Action<string> OnCreated { get;  set; }
        public Action<string> OnDeleted { get; set; }

        public Action<string> OnChanged { get; set; }
        public Action<string, string> OnRenamed { get; set; }
        public string Name { get; internal set; }

        private void _OnCreated(object sender, FileSystemEventArgs e)
        {
           OnCreated?.Invoke(e.FullPath);
        }

        private void _OnDeleted(object sender, FileSystemEventArgs e)
        {
            OnDeleted?.Invoke(e.FullPath);
        }

        private void _OnRenamed(object sender, RenamedEventArgs e)
        {
            OnRenamed?.Invoke(e.OldFullPath, e.FullPath);
        }

        private void _OnChanged(object sender, FileSystemEventArgs e)
        {
            if (e.ChangeType != WatcherChangeTypes.Changed) return;

            OnChanged?.Invoke(e.FullPath);
        }

        public void Start()
        {
            _watcher.Created += _OnCreated;
            _watcher.Deleted += _OnDeleted;
            _watcher.Renamed += _OnRenamed;
            _watcher.Changed += _OnChanged;
            _watcher.EnableRaisingEvents = true;
        }

        public void Pause()
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Created -= _OnCreated;
            _watcher.Deleted -= _OnDeleted;
            _watcher.Renamed -= _OnRenamed;
            _watcher.Changed -= _OnChanged;
        }

        public void Resume()
        {
            Start();
        }

        public void Destroy()
        {
            Pause();
            _watcher.Dispose();
        }
    }

    internal class FUnrealFSWatcher
    {
        private string uprjFilePath;
        private XFSWatcher _projectWatcher;
        private XFSWatcher _pluginFilesWatcher;
        private XFSWatcher _pluginFoldersWatcher;
        private XFSWatcher _pluginModulesWatcher;
        private XFSWatcher _pluginModuleFoldersWatcher;
        private XFSWatcher _gameModulesWatcher;
        private XFSWatcher _gameModuleFoldersWatcher;

        private string pluginsPath;
        private string sourcePath;
        public Action<string> OnPluginCreated;
        public Action<string> OnPluginDeleted;
        public Action<string, string> OnPluginRenamed;

        public Action<string> OnModuleCreated;
        public Action<string> OnModuleDeleted;
        public Action<string, string> OnModuleRenamed;

        string projectPath;
        public FUnrealFSWatcher(string uprjFilePath)
        {
            this.uprjFilePath = uprjFilePath;
            projectPath = XFilesystem.PathParent(uprjFilePath);

            

            pluginsPath = XFilesystem.PathCombine(projectPath, "Plugins");
            sourcePath = XFilesystem.PathCombine(projectPath, "Source");

        }

        private void SetupPluginsWatcherIfNull()
        {
            if (!XFilesystem.DirectoryExists(pluginsPath)) return;
            if (_pluginFilesWatcher != null) return;

            /* 
                Address following scenario:
                - file created
                - file deleted
                - file renamed
                - folder containing file copied => fire file created
            */
            _pluginFilesWatcher = new XFSWatcher();
            _pluginFilesWatcher.Path = pluginsPath;
            _pluginFilesWatcher.Filter = "*.uplugin";
            _pluginFilesWatcher.NotifyFilter = NotifyFilters.FileName; //| NotifyFilters.Security; //Security trigger OnChanged on CTRL+Z
            _pluginFilesWatcher.IncludeSubdirectories = true;
            _pluginFilesWatcher.OnCreated = (fullPath) =>
            {
                OnPluginCreated(fullPath);
            };

            _pluginFilesWatcher.OnDeleted = (fullPath) =>
            {
                OnPluginDeleted(fullPath);
            };

            _pluginFilesWatcher.OnRenamed = (oldPath, newPath) =>
            {
                bool oldIsUplug = XFilesystem.HasExtension(oldPath, ".uplugin");
                bool newIsUplug = XFilesystem.HasExtension(newPath, ".uplugin");

                if (oldIsUplug && !newIsUplug)      OnPluginDeleted(oldPath);
                else if (!oldIsUplug && newIsUplug) OnPluginCreated(newPath);
                else if (oldIsUplug && newIsUplug)  OnPluginRenamed(oldPath, newPath); //or Deleted + Created
            };

            //_pluginFilesWatcher.OnChanged = OnPluginCreated;


            _pluginFilesWatcher.Start();


            /* 
                Address following scenario (when user play with Windows "File Explorer":
                - fire file deleted when a plugin folder is deleted
                - fire file renamed when a plugin folder is renamed
            */
            _pluginFoldersWatcher = new XFSWatcher();
            _pluginFoldersWatcher.Path = pluginsPath;
            _pluginFoldersWatcher.Filter = "";
            _pluginFoldersWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.Security; //Security trigger OnChanged on CTRL+Z
            _pluginFoldersWatcher.IncludeSubdirectories = false;   //Change event is notified only for plugin folder, because watcher is not recursive

            _pluginFoldersWatcher.OnDeleted = (fullPath) =>
            {
                string name = XFilesystem.GetLastPathToken(fullPath);
                string upluginPath = XFilesystem.PathCombine(fullPath, $"{name}.uplugin"); //Here can only guess the plugin name from the dir
                OnPluginDeleted(upluginPath);
            };

            _pluginFoldersWatcher.OnRenamed = (oldPath, newPath) =>
            {
                string upluginFilePath = XFilesystem.FindFile(newPath, false, "*.uplugin");
                if (upluginFilePath == null) return;

                string fileName = XFilesystem.GetFileNameWithExt(upluginFilePath);

                string foldPath = XFilesystem.PathCombine(oldPath, fileName);
                string fnewPath = XFilesystem.PathCombine(newPath, fileName);

                OnPluginRenamed(foldPath, fnewPath); //Convert to Moved?
            };

            //CTRL+Z from delete
            _pluginFoldersWatcher.OnChanged = (fullPath) =>
            {
                string upluginFilePath = XFilesystem.FindFile(fullPath, false, "*.uplugin");
                //Debug.Print($"CHANGED: {upluginFilePath}");
                //OnPluginDeleted(upluginPath);
                if (upluginFilePath == null) return;
                OnPluginCreated("[CHANGED1] " + upluginFilePath);
            };

            _pluginFoldersWatcher.Start();


            //Plugins Module Watcher
            _pluginModulesWatcher = new XFSWatcher();
            _pluginModulesWatcher.Path = pluginsPath;
            _pluginModulesWatcher.Filter = "*.Build.cs";
            _pluginModulesWatcher.NotifyFilter = NotifyFilters.FileName;
            _pluginModulesWatcher.IncludeSubdirectories = true;
            _pluginModulesWatcher.OnCreated += (fullPath) =>
            {
                OnModuleCreated(fullPath);
            };

            _pluginModulesWatcher.OnDeleted += (fullPath) =>
            {
                OnModuleDeleted(fullPath);
            };

            _pluginModulesWatcher.OnRenamed += (oldPath, newPath) =>
            {
                bool oldIsBuild = XFilesystem.HasExtension(oldPath, ".Build.cs");
                bool newIsBuild = XFilesystem.HasExtension(newPath, ".Build.cs");

                if (oldIsBuild && !newIsBuild) OnModuleDeleted(oldPath);
                else if (!oldIsBuild && newIsBuild) OnModuleCreated(newPath);
                else if (oldIsBuild && newIsBuild) OnModuleRenamed(oldPath, newPath); //or Deleted + Created
            };
            _pluginModulesWatcher.Start();


            /* 
               Address following scenario (when user play with Windows "File Explorer":
               - fire file deleted when a plugin folder is deleted
               - fire file renamed when a plugin folder is renamed
           */
            _pluginModuleFoldersWatcher = new XFSWatcher();
            _pluginModuleFoldersWatcher.Path = pluginsPath;
            _pluginModuleFoldersWatcher.Filter = "";
            _pluginModuleFoldersWatcher.NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.Security; //Changed event for CTRL+Z
            _pluginModuleFoldersWatcher.IncludeSubdirectories = true;  //Change event is notified also for files, because watcher is recursive


            _pluginModuleFoldersWatcher.OnDeleted = (fullPath) =>
            {
                //Replace with Regex
                string relPath = XFilesystem.PathSubtract(fullPath, projectPath);
                string[] parts = XFilesystem.PathSplit(relPath);
                if (parts.Count() != 4) return; //Not a module Path: Plugins/<Plugin>/Source/<Module>


                string name = XFilesystem.GetLastPathToken(fullPath);
                string buildPath = XFilesystem.PathCombine(fullPath, $"{name}.Build.cs"); //Here can only guess the module name from the dir
                OnModuleDeleted(buildPath);
            };

            _pluginModuleFoldersWatcher.OnRenamed = (oldFullPath, newFullPath) =>
            {
                string buildFilePath = XFilesystem.FindFile(newFullPath, false, "*.Build.cs");
                if (buildFilePath == null) return;

                string fileName = XFilesystem.GetFileNameWithExt(buildFilePath);

                string oldPath = XFilesystem.PathCombine(oldFullPath, fileName);
                string newPath = XFilesystem.PathCombine(newFullPath, fileName);

                OnModuleRenamed(oldPath, newPath); //Convert to Moved?
            };

            //CTRL+Z from delete
            _pluginModuleFoldersWatcher.OnChanged = (fullPath) =>
            {
                //Strange: here can arrive event related to file!?!?!? But should be only folder....
                //Event seems fired recursively for any file contained in the module folder.
                //So wait for event related to a .Build.cs 
                if (XFilesystem.HasExtension(fullPath, ".Build.cs"))
                {
                    OnModuleCreated("[CHANGED2] " + fullPath);
                    return;
                }

                /*
                string buildFilePath = XFilesystem.FindFile(fullPath, false, "*.Build.cs");
                //Debug.Print($"CHANGED: {upluginFilePath}");
                //OnPluginDeleted(upluginPath);
                if (buildFilePath == null) return;
                OnModuleCreated(buildFilePath);
                */
            };

            _pluginModuleFoldersWatcher.Start();
        }


        private void SetupSourceWatcherIfNull()
        {
            if (!XFilesystem.DirectoryExists(sourcePath)) return;
            if (_gameModulesWatcher != null) return;


                //Game Module Watcher
            _gameModulesWatcher = new XFSWatcher();
            _gameModulesWatcher.Path = sourcePath;
            _gameModulesWatcher.Filter = "*.Build.cs";
            _gameModulesWatcher.NotifyFilter = NotifyFilters.FileName;
            _gameModulesWatcher.IncludeSubdirectories = true;
            _gameModulesWatcher.OnCreated = (fullPath) =>
            {
                OnModuleCreated(fullPath);
            };

            _gameModulesWatcher.OnDeleted = (fullPath) =>
            {
                OnModuleDeleted(fullPath);
            };

            _gameModulesWatcher.OnRenamed += (oldPath, newPath) =>
            {
                OnModuleRenamed(oldPath, newPath);
            };
            _gameModulesWatcher.Start();


            /* 
               Address following scenario (when user play with Windows "File Explorer":
               - fire file deleted when a plugin folder is deleted
               - fire file renamed when a plugin folder is renamed
           */
            _gameModuleFoldersWatcher = new XFSWatcher();
            _gameModuleFoldersWatcher.Path = sourcePath;
            _gameModuleFoldersWatcher.Filter = "";
            _gameModuleFoldersWatcher.NotifyFilter = NotifyFilters.DirectoryName;
            _gameModuleFoldersWatcher.IncludeSubdirectories = true;

            _gameModuleFoldersWatcher.OnDeleted = (fullPath) =>
            {
                //Replace with Regex
                string relPath = XFilesystem.PathSubtract(fullPath, projectPath);
                string[] parts = XFilesystem.PathSplit(relPath);
                if (parts.Count() != 2) return; //Not a module Path: Source/<Module>


                string name = XFilesystem.GetLastPathToken(fullPath);
                string buildPath = XFilesystem.PathCombine(fullPath, $"{name}.Build.cs"); //Here can only guess the module name from the dir
                OnModuleDeleted(buildPath);
            };

            _gameModuleFoldersWatcher.OnRenamed = (oldFullPath, newFullPath) =>
            {
                string buildFilePath = XFilesystem.FindFile(newFullPath, false, "*.Build.cs");
                if (buildFilePath == null) return;

                string fileName = XFilesystem.GetFileNameWithExt(buildFilePath);

                string oldPath = XFilesystem.PathCombine(oldFullPath, fileName);
                string newPath = XFilesystem.PathCombine(newFullPath, fileName);

                OnModuleRenamed(oldPath, newPath); //Convert to Moved?
            };

            _gameModuleFoldersWatcher.Start();
        }
        private void _OnPluginsFolderDeleted(string fullPath)
        {
            string name = XFilesystem.GetLastPathToken(fullPath);
            if (name == "Plugins")
            {
                _pluginFilesWatcher.Destroy();
                _pluginFoldersWatcher.Destroy();
                _pluginModulesWatcher.Destroy();
                _pluginModuleFoldersWatcher.Destroy();

                _pluginFilesWatcher = null;
                _pluginFoldersWatcher = null;
                _pluginModulesWatcher = null;
                _pluginModuleFoldersWatcher = null;
            }
            else if (name == "Source")
            {
                _gameModulesWatcher.Destroy();
                _gameModuleFoldersWatcher.Destroy();

                _gameModulesWatcher = null;
                _gameModuleFoldersWatcher = null;
            }
        }

        private void _OnPluginsFolderCreated(string fullPath)
        {
            string name = XFilesystem.GetLastPathToken(fullPath);

            if (name == "Plugins")
            {
                //Start Watcher for Plugins
                SetupPluginsWatcherIfNull();

                var files = XFilesystem.FindFilesAtLevel(pluginsPath, 1, "*.uplugin");
                foreach (var file in files)
                {
                    OnPluginCreated(file);
                }
            }
            else if (name == "Source")
            {
                SetupSourceWatcherIfNull();
                var files = XFilesystem.FindFilesAtLevel(sourcePath, 1, "*.Build.cs");
                foreach (var file in files)
                {
                    OnModuleCreated(file);
                }
            }
        }

        public void Start()
        {
            _projectWatcher = new XFSWatcher();
            _projectWatcher.Path = projectPath;
            _projectWatcher.Filter = ""; //look for dir Plugin & Source
            _projectWatcher.NotifyFilter = NotifyFilters.DirectoryName;
            _projectWatcher.IncludeSubdirectories = false;
            _projectWatcher.OnCreated += _OnPluginsFolderCreated;
            _projectWatcher.OnDeleted += _OnPluginsFolderDeleted;

            _projectWatcher.Start();

            SetupPluginsWatcherIfNull();

            SetupSourceWatcherIfNull();
        }

        public void Pause()
        {
            _projectWatcher.Pause();

            if (_pluginFilesWatcher != null)           _pluginFilesWatcher.Pause();
            if (_pluginFoldersWatcher != null)         _pluginFoldersWatcher.Pause();
            if (_pluginModulesWatcher != null)         _pluginModulesWatcher.Pause();
            if (_pluginModuleFoldersWatcher != null) _pluginModuleFoldersWatcher.Pause();
            if (_gameModulesWatcher != null) _gameModulesWatcher.Pause();
            if (_gameModuleFoldersWatcher != null) _gameModuleFoldersWatcher.Pause();
        }

        public void Stop()
        {
            Pause();
        }

        public void Resume()
        {
            _projectWatcher.Resume();

            if (_pluginFilesWatcher != null) _pluginFilesWatcher.Resume();
            if (_pluginFoldersWatcher != null) _pluginFoldersWatcher.Resume();
            if (_pluginModulesWatcher != null) _pluginModulesWatcher.Resume();
            if (_pluginModuleFoldersWatcher != null) _pluginModuleFoldersWatcher.Resume();
            if (_gameModulesWatcher != null) _gameModulesWatcher.Resume();
            if (_gameModuleFoldersWatcher != null) _gameModuleFoldersWatcher.Resume();

            //In the meantime "Plugins" and "Source" could have been created.
            SetupPluginsWatcherIfNull();
            SetupSourceWatcherIfNull();
        }
    }
}