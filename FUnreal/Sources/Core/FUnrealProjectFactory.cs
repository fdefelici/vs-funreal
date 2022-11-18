using System.Threading.Tasks;
using System.Collections.Generic;

namespace FUnreal
{
    public class FUnrealProjectFactory
    {
        public FUnrealProject Create(string uprojectFilePath)
        {
            if (!XFilesystem.FileExists(uprojectFilePath)) return null;

            string prjName = XFilesystem.GetFilenameNoExt(uprojectFilePath);
            string prjPath = XFilesystem.PathParent(uprojectFilePath);

            FUnrealProject uproject = new FUnrealProject(prjName, uprojectFilePath);

            //Look for Plugins and Modules
            string pluginsPath = XFilesystem.PathCombine(prjPath, "Plugins");

            if (XFilesystem.DirectoryExists(pluginsPath)) 
            {
                List<string> plugFiles = XFilesystem.FindFilesAtLevel(pluginsPath, 1, "*.uplugin");
                foreach (string plugFile in plugFiles)
                {
                    string plugName = XFilesystem.GetFilenameNoExt(plugFile);
                    string plugPath = XFilesystem.PathParent(plugFile);
                    string modulesSources = XFilesystem.PathCombine(plugPath, "Source");

                    FUnrealPlugin plug = new FUnrealPlugin(uproject, plugName, plugFile);

                    uproject.Plugins.Add(plug);

                    List<string> modulesPathI = XFilesystem.FindFilesAtLevel(modulesSources, 1, "*.Build.cs");
                    foreach (string modFile in modulesPathI)
                    {
                        string modName = XFilesystem.GetFilenameNoExt(modFile, true);
                        string modPath = XFilesystem.PathParent(modName);

                        var mod = new FUnrealModule(plug, modName, modFile);

                        plug.Modules.Add(mod);
                    }
                }
            }

            //Look for Game Modules
            string modulesPath = XFilesystem.PathCombine(prjPath, "Source");

            List<string> modFiles = XFilesystem.FindFilesAtLevel(modulesPath, 1, "*.Build.cs");
            bool primaryFound = false;
            foreach (string modFile in modFiles)
            {
                string modName = XFilesystem.GetFilenameNoExt(modFile, true);
                string modPath = XFilesystem.PathParent(modName);

                FUnrealModule mod = new FUnrealModule(uproject, modName, modFile);
                uproject.GameModules.Add(mod);

                //Standard
                if (!primaryFound)
                {
                    //Parallel?!
                    string found = XFilesystem.FindFile(modPath, true, "*.cpp", file =>
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

            return uproject;
        }

        public FUnrealProject CreateV2(string uprojectFilePath)
        {
            if (!XFilesystem.FileExists(uprojectFilePath)) return null;

            string prjName = XFilesystem.GetFilenameNoExt(uprojectFilePath);
            string prjPath = XFilesystem.PathParent(uprojectFilePath);

            FUnrealProject uproject = new FUnrealProject(prjName, uprojectFilePath);

            //Look for Plugins and Modules
            string pluginsPath = XFilesystem.PathCombine(prjPath, "Plugins");

            if (XFilesystem.DirectoryExists(pluginsPath))
            {
                List<string> plugFiles = XFilesystem.FindFiles(pluginsPath, true, "*.uplugin");
                foreach (string plugFile in plugFiles)
                {
                    string plugName = XFilesystem.GetFilenameNoExt(plugFile);
                    string plugPath = XFilesystem.PathParent(plugFile);
                    string modulesSources = XFilesystem.PathCombine(plugPath, "Source");

                    FUnrealPlugin plug = new FUnrealPlugin(uproject, plugName, plugFile);

                    uproject.Plugins.Add(plug);

                    if (XFilesystem.DirectoryExists(modulesSources)) 
                    { 
                        List<string> modulesPathI = XFilesystem.FindFiles(modulesSources, true, "*.Build.cs");
                        foreach (string modFile in modulesPathI)
                        {
                            string modName = XFilesystem.GetFilenameNoExt(modFile, true);
                            string modPath = XFilesystem.PathParent(modName);

                            var mod = new FUnrealModule(plug, modName, modFile);

                            plug.Modules.Add(mod);
                        }
                    }
                }
            }

            //Look for Game Modules
            string modulesPath = XFilesystem.PathCombine(prjPath, "Source");

            List<string> modFiles = XFilesystem.FindFiles(modulesPath, true, "*.Build.cs");
            bool primaryFound = false;
            foreach (string modFile in modFiles)
            {
                string modName = XFilesystem.GetFilenameNoExt(modFile, true);
                string modPath = XFilesystem.PathParent(modName);

                FUnrealModule mod = new FUnrealModule(uproject, modName, modFile);
                uproject.GameModules.Add(mod);

                //Standard
                if (!primaryFound)
                {
                    //Parallel?!
                    string found = XFilesystem.FindFile(modPath, true, "*.cpp", file =>
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

            return uproject;
        }

        private object pluginLock = new object();
        private object moduleLock = new object();

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


        public async Task<FUnrealProject> CreateV4Async(string uprojectFilePath, FUnrealNotifier notifier)
        {
            if (!XFilesystem.FileExists(uprojectFilePath)) return null;

            string prjName = XFilesystem.GetFilenameNoExt(uprojectFilePath);
            string prjPath = XFilesystem.PathParent(uprojectFilePath);

            FUnrealProject uproject = new FUnrealProject(prjName, uprojectFilePath);

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
                            string plugName = XFilesystem.GetFilenameNoExt(plugFile);
                            string plugPath = XFilesystem.PathParent(plugFile);
                            string modulesSources = XFilesystem.PathCombine(plugPath, "Source");

                            FUnrealPlugin plug = null;
                            lock (pluginLock)
                            {
                                if (TryFindPlugineByNameOrPath(uproject, plugName, plugPath, out plug))
                                {
                                    var prevPlugFile = plug.DescriptorFilePath;

                                    notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, "Multiple plugin definition! First at {0}. Second at {1}", prevPlugFile, plugFile);

                                    IsProjectValid = false;
                                    return;
                                }

                                plug = new FUnrealPlugin(uproject, plugName, plugFile);
                                uproject.Plugins.Add(plug);
                            }

                            if (XFilesystem.DirectoryExists(modulesSources))
                            {
                                List<string> modulesPathI = XFilesystem.FindFilesStoppingDepth(modulesSources, "*.Build.cs");
                                foreach (string modFile in modulesPathI)
                                {
                                    string modName = XFilesystem.GetFilenameNoExt(modFile, true);
                                    string modPath = XFilesystem.PathParent(modName);

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

                                        var mod = new FUnrealModule(plug, modName, modFile);
                                        plug.Modules.Add(mod);

                                        uproject.AllModules.Add(mod);
                                    }
                                }
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

                        FUnrealModule mod = new FUnrealModule(uproject, modName, modFile);

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

    }
}