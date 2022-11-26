using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace FUnreal
{
    public class FUnrealProjectFactory
    {
        private object pluginLock = new object();
        private object moduleLock = new object();

        public async Task<FUnrealProject> CreateAsync(string uprojectFilePath, FUnrealNotifier notifier)
        {
            if (!XFilesystem.FileExists(uprojectFilePath)) return null;

            string prjName = XFilesystem.GetFilenameNoExt(uprojectFilePath);
            string prjPath = XFilesystem.PathParent(uprojectFilePath);

            FUnrealProject uproject = new FUnrealProject(uprojectFilePath);

            bool IsProjectValid = true;

            //Look for Plugins and Modules
            var task1 = Task.Run(() =>
            {
                string pluginsPath = XFilesystem.PathCombine(prjPath, "Plugins");

                if (XFilesystem.DirectoryExists(pluginsPath))
                {
                    List<string> pluginSubPaths = XFilesystem.FindDirectories(pluginsPath, false);

                    Parallel.ForEach(pluginSubPaths, path =>
                    {
                        List<string> plugFiles = XFilesystem.FindFilesStoppingDepth(path, "*.uplugin");
                        foreach (string plugFile in plugFiles)
                        {
                            bool success = createPlugin(uproject, plugFile, notifier);
                            if (!success)
                            {
                                //concurrency is not a problem here, by the fact are just setting at false
                                //Alternative: use IsProjectValid as createPluging output parameter and update it under lock
                                IsProjectValid = false; 
                            }
                        }
                    });
                }
            });

            //Look for Game Modules
            var task2 = Task.Run(() =>
            {
                string modulesPath = XFilesystem.PathCombine(prjPath, "Source");

                List<string> modulesSubPath = XFilesystem.FindDirectories(modulesPath, false);

                Parallel.ForEach(modulesSubPath, path =>
                {
                    List<string> modFiles = XFilesystem.FindFilesStoppingDepth(path, "*.Build.cs");
                    bool primaryFound = false;
                    foreach (string modFile in modFiles)
                    {
                        string modName = XFilesystem.GetFilenameNoExt(modFile, true);
                        string modFileRel = XFilesystem.PathSubtract(modFile, uproject.SourcePath);

                        FUnrealModule mod = new FUnrealModule(uproject, modFileRel);

                        lock (moduleLock)
                        {
                            var prevMod = uproject.AllModules[modName];
                            if (prevMod != null)
                            {
                                var prevModFile = prevMod.BuildFilePath;

                                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, "Duplicated module detected! First at {0}. Second at {1}", prevModFile, modFile);

                                IsProjectValid = false;
                                return;
                            }

                            uproject.GameModules.Add(mod);

                            uproject.AllModules.Add(mod);
                        }

                        //Standard
                        if (!primaryFound)
                        {
                            //Parallel?!
                            string found = XFilesystem.FindFile(mod.FullPath, true, "*.cpp", file =>
                            {
                                string text = XFilesystem.ReadFile(file);
                                if (text.Contains("PRIMARY_GAME_MODULE")) return true;
                                return false;
                            });

                            if (found != null)
                            {
                                primaryFound = true;
                                mod.IsPrimaryGameModule = true;
                            }
                        }
                    }
                });
            });

            await Task.WhenAll(task1, task2);

            if (!IsProjectValid) return null;
            return uproject;
        }

        private bool createPlugin(FUnrealProject uproject, string plugFile, FUnrealNotifier notifier)
        {
            string plugName = XFilesystem.GetFilenameNoExt(plugFile);
            string plugPath = XFilesystem.PathParent(plugFile);
            string modulesSources = XFilesystem.PathCombine(plugPath, "Source");
            string plugRelFile = XFilesystem.PathSubtract(plugFile, uproject.PluginsPath);

            FUnrealPlugin plug = null;
            lock (pluginLock)
            {
                if (TryFindPlugineByNameOrPath(uproject, plugName, plugPath, out plug))
                {
                    var prevPlugFile = plug.DescriptorFilePath;

                    notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, "Multiple plugin definition! First at {0}. Second at {1}", prevPlugFile, plugFile);

                    //IsProjectValid = false;
                    return false;
                }

                plug = new FUnrealPlugin(uproject, plugRelFile);
                uproject.Plugins.Add(plug);
            }

            if (XFilesystem.DirectoryExists(modulesSources))
            {
                List<string> modulesPathI = XFilesystem.FindFilesStoppingDepth(modulesSources, "*.Build.cs");
                foreach (string modFile in modulesPathI)
                {
                    string modName = XFilesystem.GetFilenameNoExt(modFile, true);
                    string modPath = XFilesystem.PathParent(modName);
                    string modFileRel = XFilesystem.PathSubtract(modFile, plug.SourcePath);

                    lock (moduleLock)
                    {
                        var prevMod = uproject.AllModules[modName];
                        if (prevMod != null)
                        {
                            var prevModFile = prevMod.BuildFilePath;

                            notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, "Duplicated module detected! First at {0}. Second at {1}", prevModFile, modFile);

                            //IsProjectValid = false;
                            return false;
                        }

                        var mod = new FUnrealModule(plug, modFileRel);
                        plug.Modules.Add(mod);

                        uproject.AllModules.Add(mod);
                    }
                }
            }
            return true;
        }

        private bool TryFindPlugineByNameOrPath(FUnrealProject uproject, string name, string path, out FUnrealPlugin result)
        {
            var prev = uproject.Plugins[name];

            if (prev == null)
            {
                prev = uproject.Plugins.FindByPath(path);
            }

            if (prev == null)
            {
                result = null;
                return false;
            }

            result = prev;
            return true;
        }

        public FUnrealModule AddGameModule(FUnrealProject projectModel, string modulePath)
        {
            string moduleName = XFilesystem.GetLastPathToken(modulePath);
            string moduleFile = XFilesystem.PathCombine(modulePath, $"{moduleName}.Build.cs");
            string moduleFileRel = XFilesystem.PathSubtract(moduleFile, projectModel.SourcePath);

            FUnrealModule module = new FUnrealModule(projectModel, moduleFileRel);

            projectModel.AllModules.Add(module);
            projectModel.GameModules.Add(module);
            return module;
        }

        public FUnrealPlugin AddPlugin(FUnrealProject projectModel, string pluginPath)
        {
            var pluginFilePath = XFilesystem.FindFile(pluginPath, false, "*.uplugin");
            if (pluginFilePath == null) return null;

            bool created = createPlugin(projectModel, pluginFilePath, new FUnrealNotifier());
            if (!created) return null;

            string pluginName = XFilesystem.GetLastPathToken(pluginPath);
            return projectModel.Plugins[pluginName];
        }

        public FUnrealPlugin RenamePlugin(FUnrealProject projectModel, FUnrealPlugin plugin, string pluginNewName)
        {
            projectModel.Plugins.Remove(plugin);

            string uplugFile = plugin.DescriptorFilePath;
            string fileName = XFilesystem.GetFileNameWithExt(uplugFile);
            string plugPath = XFilesystem.PathParent(uplugFile);

            string newFileName = XFilesystem.ChangeFilePathName(fileName, pluginNewName);
            string newPlugPath = XFilesystem.ChangeDirName(plugPath, pluginNewName);
            string newUplugFile = XFilesystem.PathCombine(newPlugPath, newFileName);

            string uplugFileRelPath = XFilesystem.PathSubtract(newUplugFile, projectModel.PluginsPath);

            plugin.SetDescriptorFileRelPath(uplugFileRelPath);

            projectModel.Plugins.Add(plugin);

            //Plugin modules have no impact because all models path are relative to the Parent.
            return plugin;
        }

        public FUnrealModule RenameGameModule(FUnrealProject projectModel, FUnrealModule module, string newModuleName)
        {
            projectModel.AllModules.Remove(module);
            projectModel.GameModules.Remove(module);

            string relFile = module.BuildFileRelPath;
            string fileName = XFilesystem.GetFileNameWithExt(relFile);
            string relBase = XFilesystem.PathParent(relFile);

            string newFileName = XFilesystem.ChangeFilePathName(fileName, newModuleName, true);
            string newRelBase = XFilesystem.ChangeDirName(relBase, newModuleName);
            string newRelFile = XFilesystem.PathCombine(newRelBase, newFileName);

            module.SetBuilFileRelPath(newRelFile);

            projectModel.AllModules.Add(module);
            projectModel.GameModules.Add(module);
            return module;
        }

        public FUnrealModule RenamePluginModule(FUnrealProject projectModel, FUnrealPlugin plugin, FUnrealModule module, string newModuleName)
        {
            projectModel.AllModules.Remove(module);
            plugin.Modules.Remove(module);

            string relFile = module.BuildFileRelPath;
            string fileName = XFilesystem.GetFileNameWithExt(relFile);
            string relBase = XFilesystem.PathParent(relFile);

            string newFileName = XFilesystem.ChangeFilePathName(fileName, newModuleName, true);
            string newRelBase = XFilesystem.ChangeDirName(relBase, newModuleName);
            string newRelFile = XFilesystem.PathCombine(newRelBase, newFileName);

            module.SetBuilFileRelPath(newRelFile);

            projectModel.AllModules.Add(module);
            plugin.Modules.Add(module);
            return module;
        }

        public FUnrealModule AddPluginModule(FUnrealProject projectModel, FUnrealPlugin plugin, string modulePath)
        {
            string moduleName = XFilesystem.GetLastPathToken(modulePath);
            string moduleFile = XFilesystem.PathCombine(modulePath, $"{moduleName}.Build.cs");
            string moduleFileRel = XFilesystem.PathSubtract(moduleFile, plugin.SourcePath);

            FUnrealModule module = new FUnrealModule(plugin, moduleFileRel);

            projectModel.AllModules.Add(module);
            plugin.Modules.Add(module);
            return module;
        }

        public void DeletePluginModule(FUnrealProject project, FUnrealPlugin plugin, FUnrealModule module)
        {
            plugin.Modules.Remove(module);
            project.AllModules.Remove(module);
        }

        public void DeleteGameModule(FUnrealProject project, FUnrealModule module)
        {
            project.AllModules.Remove(module);
            project.GameModules.Remove(module);
        }

        public void RemovePlugin(FUnrealProject project, FUnrealPlugin plugin)
        {
            project.AllModules.RemoveAll(plugin.Modules);
            project.Plugins.Remove(plugin);
        }

        //TODO: Replace with Set?
        private List<string> _emptyFolders = new List<string>();

        public List<string> EmptyFolderPaths { get { return _emptyFolders; } }

        public async Task ScanEmptyFoldersAsync(FUnrealProject project)
        {
            EmptyFolderPaths.Clear();
            foreach(var plugin in project.Plugins)
            {
                //For Plugin the only interesting folder if empty are: "Resources", "Shaders"
                var emptyFolders = await XFilesystem.FindEmptyFoldersAsync(plugin.ResourcesPath, plugin.ShadersPath);
                EmptyFolderPaths.AddRange(emptyFolders);
            }

            foreach (var module in project.AllModules)
            {
                var emptyFolders = await XFilesystem.FindEmptyFoldersAsync(module.FullPath);
                EmptyFolderPaths.AddRange(emptyFolders);
            }
        }

        /*
                public void TrackEmptyFolders(List<string> paths)
                {
                    return;

                    foreach(var input in paths)
                    {
                        bool canBeAdded = true;
                        for(int i=0; i < _emptyFolders.Count; ++i)
                        {
                            string stored = _emptyFolders[i];
                            if (XFilesystem.IsChildPath(input, stored))
                            {
                                _emptyFolders[i] = input;
                                canBeAdded = false;
                                break;
                            } else if (XFilesystem.IsParentPath(input, stored, true))
                            {
                                canBeAdded = false;
                                break;
                            }
                        }
                        if (canBeAdded) _emptyFolders.Add(input);
                    }
                }

                public void TrackFileAdded(string filePath)
                {
                    return;

                    var parentPath = XFilesystem.PathParent(filePath);
                    EmptyFolderPaths.Remove(parentPath);
                }

                public void UntrackEmptyFolders(List<string> dirs)
                {
                    return;

                    foreach (var dir in dirs)
                    {
                        EmptyFolderPaths.Remove(dir);
                    }
                }
        */
    }
}