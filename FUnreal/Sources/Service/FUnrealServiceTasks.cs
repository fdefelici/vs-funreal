using EnvDTE90;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FUnreal.Sources.Core
{
    public static class FUnrealServiceTasks
    {
        public static bool Module_UpdateAndRenameBuildCs(FUnrealModule module, string newModuleName, FUnrealNotifier notifier)
        {
            notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_UpdatingModuleTargetFile, module.BuildFilePath);
            {
                string moduleFilePath = module.BuildFilePath;
                string csText = XFilesystem.FileRead(moduleFilePath);

                string classRegex = @"(?<=class\s+?)SEARCH(?=[\s\S]*?\{)";
                classRegex = classRegex.Replace("SEARCH", module.Name);
                csText = Regex.Replace(csText, classRegex, newModuleName);

                string ctorRegex = $@"(?<=public\s*?){module.Name}(?=\s*?\()";
                csText = Regex.Replace(csText, ctorRegex, newModuleName);

                XFilesystem.FileWrite(moduleFilePath, csText);
                XFilesystem.FileRename(moduleFilePath, $"{newModuleName}.Build");
            }

            return true;
        }

        public static async Task<bool> Module_UpdateAndRenameSourcesAsync(FUnrealModule module, string newModuleName, bool renameSourceFiles, FUnrealCollection<FUnrealModule> allModules, FUnrealNotifier notifier, bool IsPrimaryGameModule = false)
        {
            bool sourcesFound = TryFindModuleSources(module, out string heaFilePath, out string cppFilePath);
            if (!sourcesFound)
            {
                notifier.Warn(XDialogLib.Ctx_UpdatingModule, XDialogLib.Warn_ModuleSourcesNotFound, module.FullPath);
                return true;
            }
            else
            {
                string moduleName = module.Name;

                //Computing new file name
                string newFileName = newModuleName;
                if (!newFileName.EndsWith("Module"))
                {
                    newFileName = $"{newFileName}Module";
                }

                //Computing new header include directive path
                string heaFileNameExt = XFilesystem.GetFileNameWithExt(heaFilePath);
                bool isPublic = XFilesystem.IsChildPath(heaFilePath, module.PublicPath);
                string basePath = isPublic ? module.PublicPath : module.FullPath;
                string heaRelPath = XFilesystem.PathSubtract(heaFilePath, basePath);
                string newHeaRelPath = XFilesystem.ChangeFilePathName(heaRelPath, newFileName);
                string heaIncPath = XFilesystem.PathToUnixStyle(heaRelPath);
                string newHeaIncPath = XFilesystem.PathToUnixStyle(newHeaRelPath);

                //2.1 update .cpp
                notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_UpdatingFile, cppFilePath);
                string cppText = XFilesystem.FileRead(cppFilePath);

              
                if (IsPrimaryGameModule)
                {
                    //IMPLEMENT_PRIMARY_GAME_MODULE( FDefaultGameModuleImpl, ModuleName, "ModuleName" );
                    string regexFirst = $@"(?<=IMPLEMENT_PRIMARY_GAME_MODULE\s*\([\s\S]+?,\s*){moduleName}(?=\s*?,[\s\S]+?\))";
                    string regexSecon = $@"(?<=IMPLEMENT_PRIMARY_GAME_MODULE\s*\([\s\S]+?,[\s\S]+?,\s*""){moduleName}(?=""\s*?\))";

                    cppText = Regex.Replace(cppText, regexFirst, newModuleName);
                    cppText = Regex.Replace(cppText, regexSecon, newModuleName);
                } else //Game Module or Plugin Module
                {
                    //IMPLEMENT_MODULE(F<ModuleName>Module, <ModuleName>)
                    //string implModRegex = $@"(?<=IMPLEMENT_MODULE\s*\([\s\S]+?,\s*){moduleName}(?=\s*?\))";
                    string implModRegex = $@"(?<=(?:IMPLEMENT_MODULE|IMPLEMENT_GAME_MODULE)\s*\([\s\S]+?,\s*){moduleName}(?=\s*?\))";
                    cppText = Regex.Replace(cppText, implModRegex, newModuleName);
                }

                if (renameSourceFiles)
                {
                    //#include "**/<FileName>.h" 
                    cppText = cppText.Replace(heaIncPath, newHeaRelPath);
                }
                XFilesystem.FileWrite(cppFilePath, cppText);


                if (renameSourceFiles)
                {
                    //2.2 Renaming .cpp and .h
                    notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_RenamingFile, cppFilePath, $"{newFileName}.cpp");
                    XFilesystem.FileRename(cppFilePath, newFileName);

                    notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_RenamingFile, heaFilePath, $"{newFileName}.h");
                    XFilesystem.FileRename(heaFilePath, newFileName);

                    //2.3 scan dependent modules sources to replace #include directive
                    var dependentModules = new List<FUnrealModule>();
                    dependentModules.Add(module);
                    foreach (var other in allModules)
                    {
                        if (other == module) continue;
                        string csFile = other.BuildFilePath;
                        string buildText = XFilesystem.FileRead(csFile);
                        string dependency = $"\"{moduleName}\"";
                        string newDependency = $"\"{newModuleName}\"";
                        if (buildText.Contains(dependency)) dependentModules.Add(other);
                    }

                    var task1 = Task.Run(() =>
                    {
                        Parallel.ForEach(dependentModules, eachModule =>
                        {
                            XFilesystem.FilesForEach(eachModule.FullPath, true, ".h", file =>
                            {
                                string text = XFilesystem.FileRead(file);
                                if (text.Contains(heaIncPath))
                                {
                                    notifier.Info(XDialogLib.Ctx_UpdatingModuleDependency, XDialogLib.Info_UpdatingFile, file);
                                    text = text.Replace(heaIncPath, newHeaIncPath);
                                    XFilesystem.FileWrite(file, text);
                                }
                            });
                        });
                    });

                    var task2 = Task.Run(() =>
                    {
                        Parallel.ForEach(dependentModules, eachModule =>
                        {
                            XFilesystem.FilesForEach(eachModule.FullPath, true, ".cpp", file =>
                            {
                                string text = XFilesystem.FileRead(file);
                                if (text.Contains(heaIncPath))
                                {
                                    notifier.Info(XDialogLib.Ctx_UpdatingModuleDependency, XDialogLib.Info_UpdatingFile, file);
                                    text = text.Replace(heaIncPath, newHeaIncPath);
                                    XFilesystem.FileWrite(file, text);
                                }
                            });
                        });
                    });

                    await Task.WhenAll(task1, task2);

                }
                return true;
            }
        }


        //EMPOWER Api Macro: Look recursively under Public or Custom folders
        public static bool Module_UpdateApiMacro(FUnrealModule module, string newModuleName, FUnrealNotifier notifier) 
        {
            string modulePublicPath = module.PublicPath;

            var publicHeaderFiles = XFilesystem.FindFilesEnum(modulePublicPath, true, "*.h");
            string moduleApi = module.ApiMacro;
            string newModuleApi = $"{newModuleName.ToUpper()}_API";

            //Parallel?
            foreach (var file in publicHeaderFiles)
            {
                string text = XFilesystem.FileRead(file);
                if (text.Contains(moduleApi))
                {
                    notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_UpdatingApiMacroInFile, file);
                    text = text.Replace(moduleApi, newModuleApi);
                    XFilesystem.FileWrite(file, text);
                }
            }
            return true;
        }

        public static async Task<bool> Module_RenameFolderAsync(FUnrealModule module, string newModuleName, FUnrealNotifier notifier)
        {
            string modulePath = module.FullPath;

            notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_RenamingFolder, modulePath, newModuleName);
            string newModulePath = await XFilesystem.RenameDirAsync(modulePath, newModuleName);
            if (newModulePath == null)
            {
                notifier.Erro(XDialogLib.Ctx_UpdatingModule, XDialogLib.Error_FailureRenamingFolder);
                return false;
            }
            return true;
        }

        public static bool Module_UpdateDependencyInOtherModules(FUnrealModule module, string newModuleName, FUnrealCollection<FUnrealModule> allModules, FUnrealNotifier notifier)
        {
            var others = allModules.Except(module);

            //Parallel?
            foreach (var other in others)
            {
                var csFile = new FUnrealBuildFile(other.BuildFilePath);
                if (csFile.HasDependency(module.Name))
                {
                    notifier.Info(XDialogLib.Ctx_UpdatingModuleDependency, XDialogLib.Info_UpdatingDependencyFromFile, other.BuildFilePath);
                    csFile.RenameDependency(module.Name, newModuleName);
                    csFile.Save();
                }
            }
            return true;
        }

        public static bool Module_DeleteDependencyInOtherModules(FUnrealModule module, IEnumerable<FUnrealModule> allModules, FUnrealNotifier notifier)
        {
            string moduleName = module.Name;

            //TODO: Create Except method that takes 1 element
            var others = allModules.Except(new List<FUnrealModule>() { module });

            //Parallel?
            foreach (var other in others)
            {
                var csFile = new FUnrealBuildFile(other.BuildFilePath);
                if (csFile.HasDependency(moduleName))
                {
                    notifier.Info(XDialogLib.Ctx_UpdatingModuleDependency, XDialogLib.Info_UpdatingDependencyFromFile, other.BuildFilePath);
                    csFile.RemoveDependency(moduleName);
                    csFile.Save();
                }
            }
            return true;
        }

        public static bool Plugin_RenameModuleInDescriptor(FUnrealPlugin plugin, FUnrealModule module, string newModuleName, FUnrealNotifier notifier)
        {
            string moduleName = module.Name;
            string upluginFilePath = plugin.DescriptorFilePath;
            notifier.Info(XDialogLib.Ctx_UpdatingPlugin, XDialogLib.Info_UpdatingPluginDescriptorFile, upluginFilePath);

            FUnrealUPluginJsonFile upluginFile = new FUnrealUPluginJsonFile(upluginFilePath);
            var moduleJson = upluginFile.Modules[moduleName];
            if (moduleJson)
            {
                moduleJson.Name = newModuleName;
                upluginFile.Save();
            }
            return true;
        }

        public static bool Project_RenameModuleInTargets(FUnrealProject project, FUnrealModule module, string newModuleName, FUnrealNotifier notifier)
        {
            string moduleName = module.Name;
            foreach (var csFilePath in project.TargetFiles)
            {
                var csFile = new FUnrealTargetFile(csFilePath);
                if (!csFile.IsOpened)
                {
                    notifier.Warn(XDialogLib.Ctx_UpdatingProject, XDialogLib.Info_CannotOpenFile, csFilePath);
                    continue;
                }

                if (csFile.HasExtraModule(moduleName))
                {
                    notifier.Info(XDialogLib.Ctx_UpdatingModuleDependency, XDialogLib.Info_UpdatingDependencyFromFile, csFilePath);
                    csFile.RenameExtraModule(moduleName, newModuleName);
                    csFile.Save();
                }
            }
            return true;
        }

        public static bool Project_RenameModuleInDescriptor(FUnrealProject project, FUnrealModule module, string newModuleName, FUnrealNotifier notifier)
        {
            string moduleName = module.Name;
            string descrFilePath = project.DescriptorFilePath;
            notifier.Info(XDialogLib.Ctx_UpdatingProject, XDialogLib.Info_UpdatingProjectDescriptorFile, descrFilePath);

            var prjFile = new FUnrealUProjectFile(descrFilePath);
            var moduleJson = prjFile.Modules[moduleName];
            if (moduleJson)
            {
                moduleJson.Name = newModuleName;
                prjFile.Save();
            }
            return true;
        }

        public static async Task<bool> Project_RegenSolutionFilesAsync(FUnrealProject project, IFUnrealBuildTool ubt, FUnrealNotifier notifier)
        {
            notifier.Info(XDialogLib.Ctx_RegenSolutionFiles);
            XProcessResult ubtResult = await ubt.GenerateVSProjectFilesAsync(project.DescriptorFilePath);
            if (ubtResult.IsError)
            {
                notifier.Erro(XDialogLib.Ctx_RegenSolutionFiles, ubtResult.StdOut);
                return false;
            }
            return true;
        }

        public static List<FUnrealModule> Module_DependentModules(FUnrealModule module, IEnumerable<FUnrealModule> allModules, FUnrealNotifier notifier)
        {
            notifier.Info(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Info_CheckingModuleDependency, module.Name);
            var result = new List<FUnrealModule>();

            var others = allModules.Except(new List<FUnrealModule>() { module });

            foreach (var other in others)
            {
                var csFile = new FUnrealBuildFile(other.BuildFilePath);
                if (!csFile.IsOpened)
                {
                    notifier.Info(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Info_CannotOpenFile, other.BuildFilePath);
                    continue;
                }

                if (csFile.HasDependency(module.Name))
                {
                    notifier.Info(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Info_DependentModule, other.Name);
                    result.Add(other);
                }
            }
            return result;
        }

        public static async Task<bool> Modules_FixIncludeDirectiveBasePathAsync(List<FUnrealModule> modules, string includeBasePath, string newIncludeBasePath, FUnrealNotifier notifier)
        {
            await Task.Run(async () =>
            {
                foreach (var module in modules)
                {
                    await Module_FixIncludeDirectiveBasePathAsync(module, includeBasePath, newIncludeBasePath, notifier);
                }
            });
            return true;
        }
        public static async Task<bool> Module_FixIncludeDirectiveBasePathAsync(FUnrealModule module, string incOldPath, string incNewPath, FUnrealNotifier notifier)
        {
            string incRegex = $@"(?<=#include\s+(?:""|<)){incOldPath}(?=(?:/\w+)+\.h(?:""|>))";
            Action<string> replaceAction = (path) =>
            {
                string text = XFilesystem.FileRead(path);

                if (Regex.IsMatch(text, incRegex))
                {
                    notifier.Info(XDialogLib.Ctx_UpdatingFiles, XDialogLib.info_UpdatingFile, path);
                    text = Regex.Replace(text, incRegex, incNewPath);
                    XFilesystem.FileWrite(path, text);
                }
            };

            await Task.Run( async () =>
            {
                string modulePath = module.FullPath;
                var headerPaths = await XFilesystem.FindFilesEnumAsync(modulePath, true, "*.h");
                Parallel.ForEach(headerPaths, replaceAction);

                var sourcePaths = await XFilesystem.FindFilesEnumAsync(modulePath, true, "*.cpp");
                Parallel.ForEach(sourcePaths, replaceAction);
            });

            return true;
        }

        public static async Task<bool> Modules_FixIncludeDirectiveFullPathAsync(List<FUnrealModule> modules, string includePath, string newIncludePath, FUnrealNotifier notifier)
        {
            await Task.Run(async () =>
            {
                foreach (var module in modules)
                {
                    await Module_FixIncludeDirectiveFullPathAsync(module, includePath, newIncludePath, notifier);
                }
            });
            return true;
        }
        public static async Task<bool> Module_FixIncludeDirectiveFullPathAsync(FUnrealModule module, string incOldPath, string incNewPath, FUnrealNotifier notifier)
        {
            string incRegex = $@"(?<=#include\s+(?:""|<)){incOldPath}(?=(?:""|>))";
            Action<string> replaceAction = (path) =>
            {
                string text = XFilesystem.FileRead(path);

                if (Regex.IsMatch(text, incRegex))
                {
                    notifier.Info(XDialogLib.Ctx_UpdatingFiles, XDialogLib.info_UpdatingFile, path);
                    text = Regex.Replace(text, incRegex, incNewPath);
                    XFilesystem.FileWrite(path, text);
                }
            };

            await Task.Run(async () =>
            {
                string modulePath = module.FullPath;
                var headerPaths = await XFilesystem.FindFilesEnumAsync(modulePath, true, "*.h");
                Parallel.ForEach(headerPaths, replaceAction);

                var sourcePaths = await XFilesystem.FindFilesEnumAsync(modulePath, true, "*.cpp");
                Parallel.ForEach(sourcePaths, replaceAction);
            });

            return true;
        }

        public static async Task<bool> Source_RenameFolderAsync(string folderPath, string newFolderName, FUnrealNotifier notifier)
        {
            notifier.Info(XDialogLib.Ctx_UpdatingFiles, XDialogLib.Info_RenamingFolder, newFolderName);
            string newPath = await XFilesystem.RenameDirAsync(folderPath, newFolderName);
            bool result = newPath != null;
            if (!result)
            {
                notifier.Erro(XDialogLib.Ctx_UpdatingFiles, XDialogLib.Error_FailureRenamingFolder);
            }
            return result;
        }
        
        public static string Module_ComputeHeaderIncludePath(FUnrealModule module, string headerPath)
        {
            bool isPublic = XFilesystem.IsChildPath(headerPath, module.PublicPath);
            bool isPrivate = XFilesystem.IsChildPath(headerPath, module.PrivatePath);

            string basePath;
            if (isPublic) basePath = module.PublicPath;
            else if (isPrivate) basePath = module.PrivatePath;
            else basePath = module.FullPath;

            string heaRelPath = XFilesystem.PathSubtract(headerPath, basePath);
            return XFilesystem.PathToUnixStyle(heaRelPath);
        }

        public static void Module_ComputeSourceCodePaths(FUnrealModule module, string currentPath, string className, FUnrealSourceType classType,
          out string headerPath, out string sourcePath, out string sourceRelPath)
        {
            string modulePath = module.FullPath;

            bool isPublicPath = XFilesystem.IsChildPath(currentPath, module.PublicPath, true);
            bool isPrivatePath = XFilesystem.IsChildPath(currentPath, module.PrivatePath, true);

            if (isPublicPath) sourceRelPath = XFilesystem.PathSubtract(currentPath, module.PublicPath);
            else if (isPrivatePath) sourceRelPath = XFilesystem.PathSubtract(currentPath, module.PrivatePath);
            else sourceRelPath = XFilesystem.PathSubtract(currentPath, module.FullPath);

            string headerBasePath;
            string sourceBasePath;

            if (classType == FUnrealSourceType.PUBLIC) //Public/Private
            {
                if (isPublicPath)
                {
                    headerBasePath = currentPath;
                    sourceBasePath = XFilesystem.PathCombine(module.PrivatePath, sourceRelPath);
                }
                else if (isPrivatePath)
                {
                    headerBasePath = XFilesystem.PathCombine(module.PublicPath, sourceRelPath);
                    sourceBasePath = currentPath;
                }
                else
                {
                    headerBasePath = XFilesystem.PathCombine(module.PublicPath, sourceRelPath);
                    sourceBasePath = XFilesystem.PathCombine(module.PrivatePath, sourceRelPath);
                }
            }
            else if (classType == FUnrealSourceType.PRIVATE) //Private Only
            {
                if (isPublicPath)
                {
                    headerBasePath = XFilesystem.PathCombine(module.PrivatePath, sourceRelPath);
                    sourceBasePath = headerBasePath;
                }
                else if (isPrivatePath)
                {
                    headerBasePath = currentPath;
                    sourceBasePath = currentPath;
                }
                else
                {
                    headerBasePath = XFilesystem.PathCombine(module.PrivatePath, sourceRelPath);
                    sourceBasePath = headerBasePath;
                }
            }
            else // (classType == FUnrealSourceType.CUSTOM)
            {
                headerBasePath = currentPath;
                sourceBasePath = currentPath;

            }

            headerPath = XFilesystem.PathCombine(headerBasePath, $"{className}.h");
            sourcePath = XFilesystem.PathCombine(sourceBasePath, $"{className}.cpp");
        }


        private static bool TryFindModuleSources(FUnrealModule module, out string headerFilePath, out string sourceFilePath)
        {
            bool moduleCppFound = true;
            bool moduleHeadFound = true;
            string cppPath = null;
            string heaPath = null;

            string modulePath = module.FullPath;
            string moduleName = module.Name;

            cppPath = XFilesystem.PathCombine(module.PrivatePath, $"{moduleName}.cpp");
            if (!XFilesystem.FileExists(cppPath))
            {
                cppPath = XFilesystem.PathCombine(module.PrivatePath, $"{moduleName}Module.cpp");
                if (!XFilesystem.FileExists(cppPath))
                {
                    //FullScan
                    cppPath = XFilesystem.FindFile(modulePath, true, "*.cpp", file =>
                    {
                        string text = XFilesystem.FileRead(file);

                        string gameOrPlugModRx  = $@"(?<=(?:IMPLEMENT_MODULE|IMPLEMENT_GAME_MODULE)\s*\([\s\S]+?,\s*){moduleName}(?=\s*?\))";
                        string primaryGameModRx = $@"(?<=IMPLEMENT_PRIMARY_GAME_MODULE\s*\([\s\S]+?,\s*){moduleName}(?=\s*?,[\s\S]+?\))";

                        bool isModuleSource = Regex.IsMatch(text, gameOrPlugModRx) || Regex.IsMatch(text, primaryGameModRx);
                        return isModuleSource; 
                    });

                    moduleCppFound = cppPath != null;
                }
            }

            if (moduleCppFound) //Try to locate related header (TODO: could read #include directive eventually. But which one read?! At least file name shold be the same).
            {
                //By now search symmetrical file respect to cpp
                bool isCppUnderPrivate = XFilesystem.IsChildPath(cppPath, module.PrivatePath);
                if (isCppUnderPrivate)
                {
                    string cppPublic = XFilesystem.ChangePathBase(cppPath, module.PrivatePath, module.PublicPath);
                    heaPath = XFilesystem.ChangeFilePathExtension(cppPublic, ".h");
                } else
                {
                    heaPath = XFilesystem.ChangeFilePathExtension(cppPath, ".h");
                }
                moduleHeadFound = XFilesystem.FileExists(heaPath);
            }

            if (!moduleCppFound || !moduleHeadFound)
            {
                headerFilePath = null;
                sourceFilePath = null;
                return false;
            }

            headerFilePath = heaPath;
            sourceFilePath = cppPath;
            return true;
        }

        public static bool Project_AddModuleToTarget(FUnrealProject project, string targetName, string moduleName, FUnrealNotifier notifier)
        {
            if (!XString.IsEqualToAny(targetName, FUnrealTargets.ALL))
            {
                notifier.Warn(XDialogLib.Ctx_UpdatingProject, XDialogLib.Error_WrongTargetName, targetName);
                return false;
            }


            string targetFileName = $"{project.Name}{targetName}.Target.cs";
            string targetFilePath = XFilesystem.PathCombine(project.SourcePath, targetFileName);

            if (!XFilesystem.FileExists(targetFilePath))
            {
                notifier.Warn(XDialogLib.Ctx_UpdatingProject, XDialogLib.Info_UpdatingModuleTargetFile, targetFilePath);
                return false;
            }

            notifier.Info(XDialogLib.Ctx_UpdatingProject, XDialogLib.Info_UpdatingModuleTargetFile, targetFilePath);
            {
                var csFile = new FUnrealTargetFile(targetFilePath);
                if (!csFile.IsOpened)
                {
                    notifier.Warn(XDialogLib.Ctx_UpdatingProject, XDialogLib.Info_CannotOpenFile, targetFilePath);
                    return false;
                }

                csFile.AddExtraModule(moduleName);
                csFile.Save();
                /*
                string csText = XFilesystem.FileRead(targetFilePath);

                //Capture Group1 for all module names such as: ("Mod1", "Mod2") and replacing with ("Mod1", "Mod2", "ModuleName")
                string regex = @"ExtraModuleNames\s*\.AddRange\s*\(\s*new\s*string\[\]\s*\{\s*(\"".+\"")\s*\}\s*\)\s*;";
                var match = Regex.Match(csText, regex);
                if (match.Success && match.Groups.Count == 2)
                {
                    string moduleList = match.Groups[1].Value;
                    csText = csText.Replace(moduleList, $"{moduleList}, \"{moduleName}\"");
                    XFilesystem.FileWrite(targetFilePath, csText);
                }
                */
            }
            return true;
        }

        public static bool Plugin_CheckIfNotLockedByOtherProcess(FUnrealPlugin plugin, FUnrealNotifier notifier)
        {
            if (XFilesystem.DirContainsAnyFileLocked(plugin.BinariesPath, true, "*.dll", out string binFirstFileLocked))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_FileLockedByOtherProcess, binFirstFileLocked);
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_MaybeLockedByUnreal);
                return false;
            }
            if (XFilesystem.DirContainsAnyFileLocked(plugin.ContentPath, true, "*.*", out string cntFirstFileLocked))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_FileLockedByOtherProcess, cntFirstFileLocked);
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_MaybeLockedByUnreal);
                return false;
            }
            return true;
        }

        public static List<FUnrealPlugin> Plugin_DependentPlugins(FUnrealPlugin plugin, FUnrealCollection<FUnrealPlugin> allPlugins, FUnrealNotifier notifier)
        {
            notifier.Info(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Info_CheckingPluginDependency, plugin.Name);
            var result = new List<FUnrealPlugin>();
            foreach (var other in allPlugins)
            {
                if (other == plugin) continue;

                string descrFile = other.DescriptorFilePath;
                var descrJson = new FUnrealUPluginJsonFile(descrFile);
                var plugingJson = descrJson.Plugins[plugin.Name];
                if (plugingJson)
                {
                    result.Add(other);
                }
            }
            return result;
        }

        public static async Task<bool> Plugin_RenameDependencyAsync(FUnrealPlugin plugin, FUnrealCollection<FUnrealPlugin> allPlugins, string pluginNewName, FUnrealNotifier notifier)
        {
            await Task.Run(() =>
            {
                var dependentPlugins = Plugin_DependentPlugins(plugin, allPlugins, notifier);
                string pluginName = plugin.Name;

                foreach (var other in dependentPlugins)
                {
                    var descrJson = new FUnrealUPluginJsonFile(other.DescriptorFilePath);
                    var plugingJson = descrJson.Plugins[pluginName];
                    if (plugingJson)
                    {
                        notifier.Info(XDialogLib.Ctx_UpdatingPlugin, XDialogLib.Info_UpdatingPluginDescriptorFile, other.DescriptorFilePath);
                        plugingJson.Name = pluginNewName;
                        descrJson.Save();
                    }
                }
            });
            return true;
        }

        public static async Task<bool> Plugin_DeleteDependencyAsync(FUnrealPlugin plugin, FUnrealCollection<FUnrealPlugin> allPlugins, FUnrealNotifier notifier)
        {
            await Task.Run(() =>
            {
                var dependentPlugins = Plugin_DependentPlugins(plugin, allPlugins, notifier);
                string pluginName = plugin.Name;

                foreach (var other in dependentPlugins)
                {
                    var descrJson = new FUnrealUPluginJsonFile(other.DescriptorFilePath);
                    var plugingJson = descrJson.Plugins[pluginName];
                    if (plugingJson)
                    {
                        notifier.Info(XDialogLib.Ctx_UpdatingPlugin, XDialogLib.Info_UpdatingPluginDescriptorFile, other.DescriptorFilePath);
                        plugingJson.Remove();
                        descrJson.Plugins.RemoveIfEmpty();
                        descrJson.Save();
                    }
                }
            });
            return true;
        }

        public static async Task<bool> Plugin_DeleteModuleDependencyAsync(FUnrealPlugin plugin, FUnrealCollection<FUnrealModule> allModules, FUnrealNotifier notifier)
        {
            //NOTE: This method doesn't take advantage to reduce the scope of module dependency search based on dependency between Plugin,
            //      because in Games UE Project, Plugins developed are active by default. So a plugin module can depend to another one, without
            //      the need to declare dependency between plugins
            //      In this scenario, when building the project will give this warning:
            //          UnrealBuildTool : warning : Warning: Plugin 'NLOpt' does not list plugin 'NewPlugin12' as a dependency, but module 'NLOptMod01' depends on 'NewPlugin12'.

            //TODO: Eventually improving performance, avoiding to open Build.cs file twice when checking dependency and one for deleting dependency
            return await Task.Run(() =>
            {
                //Excluding plugin modules from the collection, to avoid iterating on more exists .Build.cs file (in Module_DependentModules)
                //if plugin has been already deleted.
                var otherModules = allModules.Except(plugin.Modules); 

                foreach (var plugModule in plugin.Modules)
                {
                    var moduleDependency = Module_DependentModules(plugModule, otherModules, notifier);
                    Module_DeleteDependencyInOtherModules(plugModule, moduleDependency, notifier);
                }
                return true;
            });
        }
    }
}

/*            
 *            
 *            OLD MODULE RENAME LOGIC, WITH CLASS RENAMING
            //Cerco filename ModuleNameModule.cpp o ModuleName.cpp
            //NOTA: il file .cpp potrebbe essere stato spostato dentro altra cartella...
            //      per ora lo cerco solo sotto Private/
           
            bool moduleCppFound = true;
            string cppPath = XFilesystem.PathCombine(modulePath, $"Private/{moduleName}.cpp");
            if (!XFilesystem.FileExists(cppPath))
            {
                cppPath = XFilesystem.PathCombine(modulePath, $"Private/{moduleName}Module.cpp");
                if (!XFilesystem.FileExists(cppPath))
                {
                    moduleCppFound = false;
                    notifier.Warn(XDialogLib.Ctx_UpdatingModule, XDialogLib.Warn_ModuleCppFileNotFound, $"Private/{moduleName}.cpp", $"Private/{moduleName}Module.cpp");
                }
            }

            if (moduleCppFound)
            {
                string cppText = XFilesystem.ReadFile(cppPath);

                //IMPLEMENT_MODULE(F<ModuleName>Module, <ModuleName>)
                notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_UpdatingModuleNameInCpp, cppPath);
                string implModRegex = @"(?<=IMPLEMENT_MODULE\s*\([\s\S]+?,\s*)SEARCH(?=\s*?\))";
                implModRegex = implModRegex.Replace("SEARCH", moduleName);
                cppText = Regex.Replace(cppText, implModRegex, newModuleName);

                if (updateCppFiles) {
                    notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_RenamingCppFiles, cppPath);
                    //#include "<ModuleName>.h" or "<ModuleName>Module.h"
                    string inclRegex = $@"""{moduleName}.h""|""{moduleName}Module.h""";
                    string newIncName = $"\"{newModuleName}Module.h\"";
                    cppText = Regex.Replace(cppText, inclRegex, newIncName);

                    //F<ModuleName>Module
                    string className = $"F{moduleName}Module";
                    string newClassName = $"F{newModuleName}Module";
                    cppText = cppText.Replace(className, newClassName);
                }

                XFilesystem.WriteFile(cppPath, cppText);

                if (updateCppFiles) XFilesystem.RenameFileName(cppPath, $"{newModuleName}Module");
            }

            //3. Rename .h and class (if still have same moduleName is configured there)
            if (moduleCppFound && updateCppFiles)
            {

                string cppFileName = XFilesystem.GetFilenameNoExt(cppPath);
                string hppPath = XFilesystem.PathCombine(modulePath, $"Public/{cppFileName}.h");
                if (XFilesystem.FileExists(hppPath))
                {
                    notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_RenamingCppFiles, hppPath);
                    string hppText = XFilesystem.ReadFile(hppPath);

                    //F<ModuleName>Module
                    string className = $"F{moduleName}Module";
                    string newClassName = $"F{newModuleName}Module";
                    hppText = hppText.Replace(className, newClassName);

                    XFilesystem.WriteFile(hppPath, hppText);
                    XFilesystem.RenameFileName(hppPath, $"{newModuleName}Module");
                }
            }
*/