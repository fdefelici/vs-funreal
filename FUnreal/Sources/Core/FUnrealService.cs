using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using System.Diagnostics;
using System.Xml;
using Newtonsoft.Json.Linq;
using FUnreal.Sources.Core;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Package;

namespace FUnreal
{
    public enum FUnrealSourceType { INVALID = -1, PUBLIC, PRIVATE, FREE }

    public class FUnrealService
    {
        public static FUnrealService SetUp_OnUIThread()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE2 dTE2 = Package.GetGlobalService(typeof(DTE)) as DTE2;

            Solution sol = dTE2.Solution;
            Debug.Print("SOLUTION NAME: {0}", sol.FullName);
            Debug.Print("SOLUTION FILENAME: {0}", sol.FileName);
            Debug.Print("Projects Count: {0}", sol.Projects.Count);

            string solAbsPath = sol.FileName;
            string uprjFilePath = Path.ChangeExtension(solAbsPath, "uproject");
            bool found = XFilesystem.FileExists(uprjFilePath);

            if (!found) return null;

            Debug.Print("UProject Path: {0}", uprjFilePath);
            Debug.Print("UProject found: {0}", found);

            string enginePath = null;
            string gameProjectName = null;
            foreach (Project project in sol.Projects)
            {
                Debug.Print("Project Full: {0}", project.FullName);
                Debug.Print("Project Name: {0}", project.Name);
                Debug.Print("Project File: {0}", project.FileName);
                Debug.Print("        Kind: {0}", project.Kind);
                Debug.Print("       Items: {0}", project.ProjectItems.Count);

                //if (project.Name != "Games") continue;
                /*
                 *  Skip:
                 *  - "Engine" Folder project and related UEXX subproject
                 *  - "Visualizer" Folder project 
                 */

                //Microsoft.VisualStudio.CommonIDE.Solutions
                //DteMiscProject
                if (project.Name.Equals("Visualizers", StringComparison.OrdinalIgnoreCase))
                {
                    if (project.ProjectItems.Count == 1)
                    {
                        ProjectItem item = project.ProjectItems.Item(1); //Collection 1-based (not starting from 0!!!)

                        if (item.FileCount == 1)
                        {
                            string absFilePath = item.FileNames[1];
                            Debug.Print(" Natvis file path: {0}", absFilePath);

                            string visualStudioDebuggingPath = Path.GetDirectoryName(absFilePath);
                            string extrasPath = Path.GetDirectoryName(visualStudioDebuggingPath);
                            enginePath = Path.GetDirectoryName(extrasPath);

                            Debug.Print(" Engine Path: {0}", enginePath);
                        }
                    }

                }
                else if (project.Name.Equals("Games", StringComparison.OrdinalIgnoreCase))
                {
                    //foreach (ProjectItem item in project.ProjectItems)
                    if (project.ProjectItems.Count == 1)
                    {
                        ProjectItem item = project.ProjectItems.Item(1); //Collection 1-based (not starting from 0!!!)

                        Debug.Print("    Item: {0}", item.Name);
                        Project SubPrj = item.SubProject;
                        //if (SubPrj == null) continue; 
                        Debug.Print("      Full: {0}", SubPrj.FullName);
                        Debug.Print("      Name: {0}", SubPrj.Name);
                        Debug.Print("      File: {0}", SubPrj.FileName);
                        Debug.Print("      Kind: {0}", SubPrj.Kind);
                        //games.Add(SubPrj);

                        gameProjectName = item.Name;
                    }
                }
            }

            string vsixDllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string vsixBasePath = Path.GetDirectoryName(vsixDllPath);
            string templatePath = Path.Combine(vsixBasePath, "Templates");
            Debug.Print("VSIX Dll Path: {0}", vsixDllPath);
            Debug.Print("VSIX Base Path: {0}", vsixBasePath);


            var uprojectFile = new FUnrealUProjectFile(uprjFilePath);
            string versionStr = uprojectFile.EngineAssociation;
            var version = XVersion.FromSemVer(versionStr);

            string ubtBin;
            if (version.Major == 4)
            {
                //Example UE4: C:\Program Files\Epic Games\UE_4.27\Engine\Binaries\DotNET\UnrealBuildTool.exe
                ubtBin = XFilesystem.PathCombine(enginePath, "Binaries/DotNET/UnrealBuildTool.exe");
            } else //5+
            {
                //Example UE5: C:\Program Files\Epic Games\UE_5.0\Engine\Binaries\DotNET\UnrealBuildTool\UnrealBuildTool.exe
                ubtBin = XFilesystem.PathCombine(enginePath, "Binaries/DotNET/UnrealBuildTool/UnrealBuildTool.exe");
            }

            FUnrealEngine engine = new FUnrealEngine(version, enginePath, new FUnrealBuildTool(ubtBin));
            //FUnrealProject projec1t = new FUnrealProject();
            return new FUnrealService(engine, uprjFilePath, gameProjectName, templatePath);
        }

        private FUnrealEngine _engine;
        private IFUnrealBuildTool _engineUbt;
        private string _engineMajorVer;

        private string _uprjFileAbsPath;
        
        private string _prjPath;
        private string _pluginsPath;
        private string _sourcePath;
        private FUnrealTemplates _templates;
       

        public FUnrealService(FUnrealEngine engine, string uprjAbsPath, string prjName, string templatePath)
        {
            _engine = engine;
            _engineUbt = engine.UnrealBuildTool;
            _engineMajorVer = engine.Version.Major.ToString();

            _uprjFileAbsPath = uprjAbsPath;
            ProjectName = prjName;
            _templates = FUnrealTemplates.Load(XFilesystem.PathCombine(templatePath, "descriptor.xml"));
        
            _prjPath = XFilesystem.PathParent(uprjAbsPath);
            _pluginsPath = XFilesystem.PathCombine(_prjPath, "Plugins");
            _sourcePath = XFilesystem.PathCombine(_prjPath, "Source");
        }

        public string ProjectName { get;  }

        public bool IsSourceCodePath(string fullPath, bool afterModuleDir=false)
        {
            //         ProjectType       .uplugin        .Build.cs
            //Path Es: <Project>\Plugins\<Plugin>\Source\<Module>\{Private,Public}
            //         <Project>\Source\<Module>\{Private,Public}

            //Plugins\<Plugin>\Source\<Module>\{Private,Public}
            //Source\<Module>\{Private,Public}

            /*
            //Not satisfy Source/Module1 and conflict with Plugin/Source/Module1/{Private,Public}
            //string regex = @"^(?:Plugins\\[^\\\r\n\t]+\\){0,1}Source\\[^\\\r\n\t]+(?:\\[^\\\r\n\t]+){0,}$";
            string regexPluginPath = @"^Plugins\\[^\\\r\n\t]+\\Source\\[^\\\r\n\t]+\\(?:Public|Private)(?:\\[^\\\r\n\t]+){0,}$";
            string regexGamePath = @"^Source\\[^\\\r\n\t]+(?:\\[^\\\r\n\t]+){0,}$";
            string regex = $"{regexPluginPath}|{regexGamePath}";
            */
           
            string moduleRegex = @"PRJ_PATH\\(?:Plugins\\[^\\]+?\\){0,1}Source\\[^\\]+";
            moduleRegex = moduleRegex.Replace("PRJ_PATH", Regex.Escape(_prjPath));
            string afterModuleRegex = $@"{moduleRegex}\\[^\\]+";

            bool regexSatisfied = false;
            if (afterModuleDir)
            {
                regexSatisfied = Regex.IsMatch(fullPath, afterModuleRegex);
            } else
            {
                regexSatisfied =  Regex.IsMatch(fullPath, moduleRegex);
            }
            return regexSatisfied && !fullPath.EndsWith(".Build.cs");
        }

        public FUnrealSourceType TypeForSourcePath(string path)
        {
            if (!IsSourceCodePath(path)) return FUnrealSourceType.INVALID;

            string modulePath = ModulePathFromSourceCodePath(path);

            string afterModulePath = XFilesystem.PathSubtract(path, modulePath);

            if (afterModulePath == "Public" || afterModulePath.StartsWith("Public\\"))
            {
                return FUnrealSourceType.PUBLIC;
            }

            if (afterModulePath == "Private" || afterModulePath.StartsWith("Private\\"))
            {
                return FUnrealSourceType.PRIVATE;
            }

            return FUnrealSourceType.FREE;
        }

        /*
                public bool IsSourceCodePublicPath(string fullPath)
                {
                    string regex = @"^(?:Plugins\\[^\\\r\n\t]+\\){0,1}Source\\[^\\\r\n\t]+\\Public(?:\\[^\\\r\n\t]+){0,}$";
                    string relPath = XFilesystem.PathSubtract(fullPath, _prjPath);
                    return Regex.IsMatch(relPath, regex);
                }

                public bool IsSourceCodePrivatePath(string fullPath)
                {
                    string regex = @"^(?:Plugins\\[^\\\r\n\t]+\\){0,1}Source\\[^\\\r\n\t]+\\Private(?:\\[^\\\r\n\t]+){0,}$";
                    string relPath = XFilesystem.PathSubtract(fullPath, _prjPath);
                    return Regex.IsMatch(relPath, regex);
                }
        */
        public void ComputeSourceCodePaths(string currentPath, string className, FUnrealSourceType classType,
            out string headerPath, out string sourcePath, out string sourceRelPath)
        {

            string relPath = XFilesystem.PathSubtract(currentPath, _prjPath);

            //- Plugins\Plugin01\Source\Module01
            //- Plugins\Plugin01\Source\Module01\Folder1
            //- Plugins\Plugin01\Source\Module01\Public
            //- Source\Module01\Public\Folder1
            //- Source\Module01\Private\Folder1

            string startWithSourceFolder;
            if (relPath.StartsWith("Plugins\\"))
            {
                startWithSourceFolder = XFilesystem.PathChild(relPath, 2);
            } else
            {
                startWithSourceFolder = relPath;
            }

            //Now should have relpath like these:
            //- Source\Module01
            //- Source\Module01\Folder1
            //- Source\Module01\Public
            //- Source\Module01\Public\Folder1
            //- Source\Module01\Private\Folder1

            string afterModulePath = XFilesystem.PathChild(startWithSourceFolder, 2);

            //Now should have relpath like these:
            //- ""
            //- Folder1
            //- Public
            //- Public\Folder1
            //- Private\Folder1

            bool isPublicPath = false;
            bool isPrivatePath = false;
            //bool isFreePath = false;

            //string pluginNameOrNull = PluginNameFromSourceCodePath(currentPath);
            //string moduleName = ModuleNameFromSourceCodePath(currentPath);

            string modulePath = ModulePathFromSourceCodePath(currentPath);

            if (afterModulePath == "Public" || afterModulePath.StartsWith("Public\\"))
            {
                sourceRelPath = XFilesystem.PathSubtract(afterModulePath, "Public");
                isPublicPath = true;
            }
            else if (afterModulePath == "Private" || afterModulePath.StartsWith("Private\\"))
            {
                sourceRelPath = XFilesystem.PathSubtract(afterModulePath, "Private");
                isPrivatePath = true;
            } else
            {
                sourceRelPath = afterModulePath;
                //isFreePath = true;
            }
            
            string headerBasePath;
            string sourceBasePath;

            if (classType == FUnrealSourceType.PUBLIC) //Public/Private
            {
                if (isPublicPath)
                {
                    headerBasePath = currentPath;
                    sourceBasePath = XFilesystem.PathCombine(modulePath, "Private", sourceRelPath); 
                }
                else if (isPrivatePath)
                {
                    headerBasePath = XFilesystem.PathCombine(modulePath, "Public", sourceRelPath);
                    sourceBasePath = currentPath;
                } else
                {
                    headerBasePath = XFilesystem.PathCombine(modulePath, "Public", sourceRelPath); 
                    sourceBasePath = XFilesystem.PathCombine(modulePath, "Private", sourceRelPath);
                }
            }
            else if (classType == FUnrealSourceType.PRIVATE) //Private Only
            {
                if (isPublicPath)
                {
                    headerBasePath = XFilesystem.PathCombine(modulePath, "Private", sourceRelPath);
                    sourceBasePath = headerBasePath;
                }
                else if (isPrivatePath)
                {
                    headerBasePath = currentPath;
                    sourceBasePath = currentPath;
                }
                else
                {
                    headerBasePath = XFilesystem.PathCombine(modulePath, "Private", sourceRelPath);
                    sourceBasePath = headerBasePath;
                }
            }
            else // (classType == FUnrealSourceType.FREE) //Free
            {
                headerBasePath = currentPath;
                sourceBasePath = currentPath;

            }

            headerPath = XFilesystem.PathCombine(headerBasePath, $"{className}.h");
            sourcePath = XFilesystem.PathCombine(sourceBasePath, $"{className}.cpp");
        }


        private FUnrealProject GetUProject()
        {
            //NOTA: Per ora lo ricreo sempre per essere sicuro di intercettare modifiche fatte dall'utente senza usare FUnreal.
            return new FUnrealProject(ProjectName, _uprjFileAbsPath);
        }

        public FUnrealPluginModule GetPluginModule(string pluginName, string moduleName)
        {
            var project = GetUProject();
            var plugin = project.Plugins[pluginName];
            var module = plugin.Modules[moduleName];
            return module;
        }

        public List<FUnrealTemplate> PluginTemplates()
        {
            return _templates.GetTemplates("plugins", _engineMajorVer);
        }

        public List<FUnrealTemplate> SourceTemplates()
        {
            return _templates.GetTemplates("sources", _engineMajorVer);
        }

        public List<FUnrealTemplate> ModuleTemplates()
        {
            return _templates.GetTemplates("modules", _engineMajorVer);
        }

        public string AbsPluginPath(string plugName)
        {
            return XFilesystem.PathCombine(_pluginsPath, plugName);
        }

        public string AbsPluginModulePath(string plugName, string modName)
        {
            string plugPath = AbsPluginPath(plugName);
            return XFilesystem.PathCombine(plugPath, "Source", modName);
        }

        public string AbsGameModulePath(string modName)
        {
            return XFilesystem.PathCombine(AbsProjectPath(), "Source", modName);
        }

        public string AbsProjectPath()
        {
            return _prjPath;
        }

        public string AbsProjectSourceFolderPath()
        {
            return _sourcePath;
        }

        public string RelPluginPath(string pluginName)
        {
            string prjPath = AbsProjectPath();
            string absPath = AbsPluginPath(pluginName);
            string relPath = XFilesystem.PathSubtract(absPath, prjPath, true);
            return relPath;
        }

        public string RelPluginModulePath(string plugName, string modName)
        {
            string prjPath = AbsProjectPath();
            string absPath = AbsPluginModulePath(plugName, modName);
            string relPath = XFilesystem.PathSubtract(absPath, prjPath, true);
            return relPath;
        }

        public string RelGameModulePath(string moduleName)
        {
            string prjPath = AbsProjectPath();
            string absPath = AbsGameModulePath(moduleName);
            string relPath = XFilesystem.PathSubtract(absPath, prjPath, true);
            return relPath;
        }

        public string RelPath(string fullPath)
        {
            string prjPath = AbsProjectPath();
            string absPath = fullPath;
            string relPath = XFilesystem.PathSubtract(absPath, prjPath, true);
            return relPath;
        }

        public string RelPathToModule(string fullPath)
        {
            string modPath = ModulePathFromSourceCodePath(fullPath);
            string relPath = XFilesystem.PathSubtract(fullPath, modPath, true);
            return relPath;
        }

        public bool ExistsPlugin(string pluginName)
        {
            string pluginPath = AbsPluginPath(pluginName);
            return Directory.Exists(pluginPath);
        }

        public bool ExistsPluginModule(string pluginName, string moduleName)
        {
            string modulePath = AbsPluginModulePath(pluginName, moduleName);
            return Directory.Exists(modulePath);
        }

        public bool ExistsGameModule(string modName)
        {
            string modulePath = AbsGameModulePath(modName);
            return Directory.Exists(modulePath);
        }

       
       
        public string ModuleNameFromSourceCodePath(string path)
        {
            string moduleGroupRegex = @"PRJ_PATH\\(?:Plugins\\[^\\]+?\\){0,1}Source\\([^\\]+)";
            string prjExcape = Regex.Escape(_prjPath);
            moduleGroupRegex = moduleGroupRegex.Replace("PRJ_PATH", prjExcape);

            var match = Regex.Match(path, moduleGroupRegex);
            if (match.Success && match.Groups.Count == 2)
            {
                var group = match.Groups[1];
                return group.Value;
            }
            return null;
        }

        public string ModulePathFromSourceCodePath(string path)
        {
            string moduleGroupRegex = @"PRJ_PATH\\(?:Plugins\\[^\\]+?\\){0,1}Source\\[^\\]+";
            string prjExcape = Regex.Escape(_prjPath);
            moduleGroupRegex = moduleGroupRegex.Replace("PRJ_PATH", prjExcape);

            var match = Regex.Match(path, moduleGroupRegex);
            if (match.Success)
            {
                return match.Value;
            }
            return null;
        }

        public string PluginNameFromSourceCodePath(string path)
        {   
            string moduleGroupRegex = @"PRJ_PATH\\Plugins\\([^\\]+?)(?:\\[^\\]+){0,}$";
            string prjExcape = Regex.Escape(_prjPath);
            moduleGroupRegex = moduleGroupRegex.Replace("PRJ_PATH", prjExcape);

            var match = Regex.Match(path, moduleGroupRegex);
            if (match.Success && match.Groups.Count == 2)
            {
                var group = match.Groups[1];
                return group.Value;
            }
            return null;
        }


        public bool ExistsSourceDirectory(string sourcePath)
        {
            if (!IsSourceCodePath(sourcePath)) return false;
            return XFilesystem.DirectoryExists(sourcePath);
        }

        public bool ExistsSourceFile(string sourcePath)
        {
            if (!IsSourceCodePath(sourcePath)) return false;
            return XFilesystem.FileExists(sourcePath);
        }

        public bool IsModulePathOrTargetFile(string fullPath)
        {
            string moduleName = ModuleNameFromSourceCodePath(fullPath);
            if (moduleName == null) return false;

            //Improve with regex
            if (fullPath.EndsWith(moduleName)) return true;
            if (fullPath.EndsWith($"{moduleName}.Build.cs")) return true;
            return false;
        }

        public bool IsPluginFolder(string fullPath)
        {
            string pluginName = PluginNameFromSourceCodePath(fullPath);
            if (pluginName == null) return false;

            string expectedPath = AbsPluginPath(pluginName);
            return expectedPath == fullPath;
        }

        public bool IsPluginDescriptorFile(string fullPath)
        {
            return fullPath.EndsWith(".uplugin");
        }

        public bool IsModuleTargetFile(string fullPath)
        {
            return fullPath.EndsWith(".Build.cs");
        }

        public bool IsProjectDescriptorFile(string fullPath)
        {
            return _uprjFileAbsPath == fullPath;
        }

        public bool IsPluginModulePath(string fullPath)
        {
            //Basterebbe il match con la regex che calcola il module name

            string pluginName = PluginNameFromSourceCodePath(fullPath);
            string moduleName = ModuleNameFromSourceCodePath(fullPath);
            
            bool isTrue = pluginName != null && moduleName != null;
            return isTrue;
        }

        public bool IsGameModulePath(string fullPath)
        {
            //Basterebbe il match con la regex che calcola il module name

            string pluginName = PluginNameFromSourceCodePath(fullPath);
            string moduleName = ModuleNameFromSourceCodePath(fullPath);

            bool isTrue = pluginName == null && moduleName != null;
            return isTrue;
        }

        public bool IsPrimaryGameModulePath(string fullPath)
        {
            string moduleName = ModuleNameFromSourceCodePath(fullPath);
            if (moduleName == null) return false;

            var project = GetUProject();
            var module = project.GameModules[moduleName];
            if (module == null) return false;
            return module.IsPrimaryGame;
        }

        public async Task<bool> AddPluginAsync(string templeName, string pluginName, string moduleNameOrNull, FUnrealNotifier notifier)
        {
            string context = "plugins";
            string engine = _engineMajorVer;
            string name = templeName;

            if (ExistsPlugin(pluginName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.ErrorMsg_PluginNotExists, pluginName);
                return false;
            }
            if (moduleNameOrNull != null && ExistsPluginModule(pluginName, moduleNameOrNull))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginModuleAlreadyExists, pluginName, moduleNameOrNull);
                return false;
            }

            FUnrealTemplate tpl = _templates.GetTemplate(context, engine, name);
            if (tpl == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateNotFound, context, engine, name);
                return false;
            }

            string pluginNamePH = tpl.GetPlaceHolder("PluginName"); //Mandatory
            if (pluginNamePH == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateWrongConfig, context, engine, name);
                return false;
            }

            string moduleNamePH = tpl.GetPlaceHolder("ModuleName"); //Optional

            notifier.Info(XDialogLib.Ctx_ConfiguringTemplate, XDialogLib.Info_TemplateCopyingFiles, _pluginsPath);
            PlaceHolderReplaceStrategy strategy = new PlaceHolderReplaceStrategy();
            strategy.AddFileExtension(".cpp", ".h", ".cs", ".uplugin");
            strategy.AddPlaceholder(pluginNamePH, pluginName);
            if (moduleNamePH != null) 
            { 
                strategy.AddPlaceholder(moduleNamePH, moduleNameOrNull);
            }
            await XFilesystem.DeepCopyAsync(tpl.BasePath, _pluginsPath, strategy);

            notifier.Info(XDialogLib.Ctx_RegenSolutionFiles);
            XProcessResult ubtResult = await _engineUbt.GenerateVSProjectFilesAsync(_uprjFileAbsPath);
            if (ubtResult.IsError)
            {
                notifier.Erro(XDialogLib.Ctx_RegenSolutionFiles, ubtResult.StdOut);
                return false;
            }
            return true;
        }

        public async Task<bool> DeletePluginAsync(string pluginName, FUnrealNotifier notifier)
        {
            var project = GetUProject();
            var plugin = project.Plugins[pluginName];
            if (!plugin.Exists)
            {
                notifier.Erro($"Plugin not found: ${pluginName}");
                return false;
            }

            notifier.Info($"Detecting plugin: {pluginName} ...");
            if (!plugin.Exists)
            {
                notifier.Erro($"Plugin not found at path: {plugin.FullPath}");
                return false;
            }

            string plugPath = plugin.FullPath;
            notifier.Info($"Deleting plugin: {plugin.FullPath} ...");
            if (!XFilesystem.DeleteDir(plugPath))
            {
                notifier.Erro($"Delete failed!");
                return false;
            }

            if (XFilesystem.IsEmptyDir(_pluginsPath))
            {
                notifier.Info($"Deleting plugins folder because empty: {_pluginsPath} ...");
                if (!XFilesystem.DeleteDir(_pluginsPath))
                {
                    notifier.Warn($"Delete failed!");
                }
            }

            notifier.Info($"Checking .uproject for plugin declaration: {_uprjFileAbsPath} ...");
            var uprojectJson = new FUnrealUProjectFile(_uprjFileAbsPath);
            if (uprojectJson.Plugins) {
                var pluginJson = uprojectJson.Plugins[pluginName];
                if (pluginJson) {
                    notifier.Info($"Removing plugin config from .uproject file...");
                    pluginJson.Remove();
                    uprojectJson.Plugins.RemoveIfEmpty();
                    uprojectJson.Save();
                }
            }
            
            notifier.Info($"Regenerating VS project files ...");
            XProcessResult ubtResult = await _engineUbt.GenerateVSProjectFilesAsync(_uprjFileAbsPath);
            if (ubtResult.IsError)
            {
                notifier.Erro(XDialogLib.Ctx_RegenSolutionFiles, ubtResult.StdOut);
                return false;
            }
            return true;
        }

        public async Task<bool> RenamePluginAsync(string pluginName, string pluginNewName, FUnrealNotifier notifier)
        {
            if (!ExistsPlugin(pluginName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginNotFound, pluginName);
                return false;
            }
            if (ExistsPlugin(pluginNewName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginAlreadyExists, pluginNewName);
                return false;
            }

            var project = GetUProject();
            var plugin = project.Plugins[pluginName];

            //1. Rename .uplugin file and replace "FriendlyName" (only if is the same as pluginName)
            string upluginFilePath = plugin.DescriptorFilePath;
            notifier.Info(XDialogLib.Ctx_UpdatingPlugin, XDialogLib.Info_UpdatingPluginDescriptorFile, upluginFilePath);

            JObject upluginJson = XFilesystem.ReadJsonFile(upluginFilePath);
            string upluginName = (string)upluginJson["FriendlyName"];
            if (pluginName.Equals(upluginName))
            {
                upluginJson["FriendlyName"] = pluginNewName;
                XFilesystem.WriteJsonFile(upluginFilePath, upluginJson);
            }
            notifier.Info(XDialogLib.Ctx_UpdatingPlugin, XDialogLib.Info_RenamingPluginDescriptorFile, upluginFilePath, pluginNewName);
            XFilesystem.RenameFileName(upluginFilePath, pluginNewName);

            //2. Rename Plugin Folder
            notifier.Info(XDialogLib.Ctx_UpdatingPlugin, XDialogLib.Info_RenamingFolder, plugin.FullPath, pluginNewName);
            XFilesystem.RenameDir(plugin.FullPath, pluginNewName);

            //3. Rename plugin in .uproject (if plugin is configured there)
            FUnrealUProjectFile uprojectJson = new FUnrealUProjectFile(_uprjFileAbsPath);
            var plugingJson = uprojectJson.Plugins[pluginName];
            if (plugingJson)
            {
                notifier.Info(XDialogLib.Ctx_UpdatingProject, XDialogLib.Info_UpdatingProjectDescriptorFile, _uprjFileAbsPath);
                plugingJson.Name = pluginNewName;
                uprojectJson.Save();
            }

            notifier.Info(XDialogLib.Ctx_RegenSolutionFiles);
            XProcessResult ubtResult = await _engineUbt.GenerateVSProjectFilesAsync(_uprjFileAbsPath);
            if (ubtResult.IsError)
            {
                notifier.Erro(XDialogLib.Ctx_RegenSolutionFiles, ubtResult.StdOut);
                return false;
            }
            return true;
        }

        public async Task<bool> AddPluginModuleAsync(string templeName, string pluginName, string moduleName, FUnrealNotifier notifier)
        {
            string context = "modules";
            string engine = _engineMajorVer;
            string name = templeName;

            var project = GetUProject();
            var plugin = project.Plugins[pluginName];
            
            if (plugin == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.ErrorMsg_PluginNotExists, pluginName);
                return false;
            }
            var module = plugin.Modules[moduleName];
            if (module != null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginModuleAlreadyExists, pluginName, moduleName);
                return false;
            }

            FUnrealTemplate tpl = _templates.GetTemplate(context, engine, name);
            if (tpl == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateNotFound, context, engine, name);
                return false;
            }

            string moduleNamePH = tpl.GetPlaceHolder("ModuleName");
            if (moduleNamePH == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateWrongConfig, context, engine, name);
                return false;
            }

            string metaType = tpl.GetMeta("type");
            string metaPhase = tpl.GetMeta("phase");
            if (metaType == null || metaPhase == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateWrongConfig, context, engine, name);
                return false;
            }

            notifier.Info(XDialogLib.Ctx_ConfiguringTemplate, XDialogLib.Info_TemplateCopyingFiles, plugin.SourcePath);
            PlaceHolderReplaceStrategy strategy = new PlaceHolderReplaceStrategy();
            strategy.AddFileExtension(".cpp", ".h", ".cs", ".uplugin");
            strategy.AddPlaceholder(moduleNamePH, moduleName);

            //string pluginPath = AbsPluginPath(pluginName);
            //string sourcePath = XFilesystem.PathCombine(pluginPath, "Source");
            string sourcePath = plugin.SourcePath;
            await XFilesystem.DeepCopyAsync(tpl.BasePath, sourcePath, strategy);

            //Update .uplugin
            string upluginFilePath = plugin.DescriptorFilePath;
            notifier.Info(XDialogLib.Ctx_UpdatingPlugin, XDialogLib.Info_UpdatingPluginDescriptorFile, upluginFilePath);
            var upluginFile = new FUnrealUPluginJsonFile(upluginFilePath);
            upluginFile.Modules.Add(new FUnrealUPluginModuleJson() { 
                Name = moduleName,
                Type = metaType,
                LoadingPhase = metaPhase
            });
            upluginFile.Save(); //todo: SaveAsync

            notifier.Info(XDialogLib.Ctx_RegenSolutionFiles);
            XProcessResult ubtResult = await _engineUbt.GenerateVSProjectFilesAsync(_uprjFileAbsPath);
            if (ubtResult.IsError)
            {
                notifier.Erro(XDialogLib.Ctx_RegenSolutionFiles, ubtResult.StdOut);
                return false;
            }
            return true;
        }

        public async Task<bool> RenamePluginModuleAsync(string pluginName, string moduleName, string newModuleName, bool updateCppFiles, FUnrealNotifier notifier)
        {
            if (!ExistsPlugin(pluginName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginNotFound, pluginName);
                return false;
            }
            if (!ExistsPluginModule(pluginName, moduleName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginModuleNotFound, pluginName, moduleName);
                return false;
            }
            if (ExistsPluginModule(pluginName, newModuleName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginModuleAlreadyExists, pluginName, moduleName);
                return false;
            }

            var project = GetUProject();
            var plugin = project.Plugins[pluginName];
            var module = plugin.Modules[moduleName];
            string modulePath = module.FullPath;

            //1. Rename .Build.cs file and replace "class ModuleName" and constructor (only if is the same as moduleName)
            notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_UpdatingModuleTargetFile, module.BuildFilePath);
            {
                string moduleFilePath = module.BuildFilePath;
                string csText = XFilesystem.ReadFile(moduleFilePath);

                string classRegex = @"(?<=class\s+?)SEARCH(?=[\s\S]*?\{)";
                classRegex = classRegex.Replace("SEARCH", moduleName);
                csText = Regex.Replace(csText, classRegex, newModuleName);

                string ctorRegex = @"(?<=public\s*?)SEARCH(?=\s*?\()";
                ctorRegex = ctorRegex.Replace("SEARCH", moduleName);
                csText = Regex.Replace(csText, ctorRegex, newModuleName);

                XFilesystem.WriteFile(moduleFilePath, csText);
                XFilesystem.RenameFileName(moduleFilePath, $"{newModuleName}.Build");
            }

            //2. Rename .cpp and class (if still have same moduleName is configured there)
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
                //TODO: Add control if file exists before renaming?!
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
                    //TODO: Add control if file exists before renaming?!
                }
            }

            //4. Update MODULENAME_API macro in all .h files under Public/**
            { 
                string modulePublicPath = module.PublicPath;

                var publicHeaderFiles = XFilesystem.FindFiles(modulePublicPath, true, "*.h");
                string moduleApi = module.ApiMacro;
                string newModuleApi = $"{newModuleName.ToUpper()}_API";
                
                foreach(var file in publicHeaderFiles)
                {
                    string text = XFilesystem.ReadFile(file);
                    if (text.Contains(moduleApi))
                    {
                        notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_UpdatingApiMacroInFile, file);
                        text = text.Replace(moduleApi, newModuleApi);
                        XFilesystem.WriteFile(file, text);
                    }
                }
            }


            //5 Rename Module Folder
            notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_RenamingFolder, modulePath, newModuleName);
            string newModulePath = XFilesystem.RenameDir(modulePath, newModuleName);
            if (newModulePath == null)
            {
                notifier.Erro(XDialogLib.Ctx_UpdatingModule, XDialogLib.Error_FailureRenamingFolder);
                return false;
            }

            //6. Update module dependency in other module .Build.cs
            {
                foreach (var other in plugin.Modules)
                {
                    if (other == module) continue;

                    string csFile = other.BuildFilePath;
                    string buildText = XFilesystem.ReadFile(csFile);
                    string dependency = $"\"{moduleName}\"";
                    string newDependency = $"\"{newModuleName}\"";
                    if (buildText.Contains(dependency))
                    {
                        notifier.Info(XDialogLib.Ctx_UpdatingModuleDependency, XDialogLib.Info_UpdatingDependencyFromFile, other.BuildFilePath);
                        buildText = buildText.Replace(dependency, newDependency);
                        XFilesystem.WriteFile(csFile, buildText);
                    }
                }
            }


            //7. Rename module in .uplugin
            {
                string upluginFilePath = plugin.DescriptorFilePath;
                notifier.Info(XDialogLib.Ctx_UpdatingPlugin, XDialogLib.Info_UpdatingPluginDescriptorFile, upluginFilePath);

                FUnrealUPluginJsonFile upluginFile = new FUnrealUPluginJsonFile(upluginFilePath);
                var moduleJson = upluginFile.Modules[moduleName];
                if (moduleJson)
                {
                    moduleJson.Name = newModuleName;
                    upluginFile.Save();
                }
            }            
            
            notifier.Info(XDialogLib.Ctx_RegenSolutionFiles);
            XProcessResult ubtResult = await _engineUbt.GenerateVSProjectFilesAsync(_uprjFileAbsPath);
            if (ubtResult.IsError)
            {
                notifier.Erro(XDialogLib.Ctx_RegenSolutionFiles, ubtResult.StdOut);
                return false;
            }
            return true;
        }

        public async Task<bool> DeletePluginModuleAsync(string pluginName, string moduleName, FUnrealNotifier notifier)
        {
            var project = GetUProject();
            var plugin = project.Plugins[pluginName];
            if (!plugin.Exists)
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginNotFound, pluginName);
                return false;
            }

            FUnrealPluginModule module = plugin.Modules[moduleName];
            if (module == null) {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginModuleNotFound, pluginName, moduleName);
                return false;
            }

            //1. Remove dependency from other modules .Build.cs
            //notifier.Info(XDialogLib.Ctx_UpdatingModuleDependency);
            string moduleDepend = $"\"{moduleName}\"";
            //string regexDepend = @"(?<!,\s*)\s*""SEARCH""\s*,|,{0,1}\s*""SEARCH""\s*";
            string regexDepend = @"(?<!,\s*)\s*""SEARCH""\s*,|,{0,1}\s*""SEARCH""";
            regexDepend = regexDepend.Replace("SEARCH", moduleName);
            foreach(var other in plugin.Modules)
            {
                if (other.Name == module.Name) continue;

                string buildText = XFilesystem.ReadFile(other.BuildFilePath);
                if (buildText.Contains(moduleDepend))
                {
                    notifier.Info(XDialogLib.Ctx_UpdatingModuleDependency, XDialogLib.Info_CleaningDependencyFromFile, other.BuildFilePath);
                    buildText = Regex.Replace(buildText, regexDepend, "");
                    XFilesystem.WriteFile(other.BuildFilePath, buildText);
                }
            }


            //2. Delete module path
            notifier.Info(XDialogLib.Ctx_DeletingModule, XDialogLib.Info_DeletingModuleFolder, module.FullPath);
            XFilesystem.DeleteDir(module.FullPath);

            //3. Update .uplugin removing the module
            notifier.Info(XDialogLib.Ctx_UpdatingPlugin, XDialogLib.Info_UpdatingPluginDescriptorFile, plugin.DescriptorFilePath);
            FUnrealUPluginJsonFile upluginFile = new FUnrealUPluginJsonFile(plugin.DescriptorFilePath);
            var moduleJson = upluginFile.Modules[moduleName];
            if (moduleJson)
            {
                moduleJson.Remove();
                upluginFile.Save();
            }

            notifier.Info(XDialogLib.Ctx_RegenSolutionFiles);
            XProcessResult ubtResult = await _engineUbt.GenerateVSProjectFilesAsync(_uprjFileAbsPath);
            if (ubtResult.IsError)
            {
                notifier.Erro(XDialogLib.Ctx_RegenSolutionFiles, ubtResult.StdOut);
                return false;
            }
            return true;
        }

        public async Task<bool> AddSourceClassAsync(string templeName, string absBasePath, string className, FUnrealSourceType classType, FUnrealNotifier notifier)
        {
            string context = "sources";
            string engine = _engineMajorVer;
            string name = templeName;

            string modulePath = ModulePathFromSourceCodePath(absBasePath);
            string moduleName = XFilesystem.GetLastPathToken(modulePath);

            ComputeSourceCodePaths(absBasePath, className, classType, 
                out string headerPath, 
                out string sourcePath,
                out string sourceRelPath
                );

            if (XFilesystem.FileExists(headerPath) || XFilesystem.FileExists(sourcePath))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.ErrorMsg_FileAlreadyExists);
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, headerPath);
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, sourcePath);
                return false;
            }
            FUnrealTemplate tpl = _templates.GetTemplate(context, engine, name);
            if (tpl == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateNotFound, context, engine, name);
                return false;
            }

            string moduleApiPH = tpl.GetPlaceHolder("ModuleApi");
            string incluPathPH = tpl.GetPlaceHolder("IncluPath");
            string classNamePH = tpl.GetPlaceHolder("ClassName");
            string headerFileME = tpl.GetMeta("header");
            string sourceFileME = tpl.GetMeta("source");

            if (moduleApiPH == null || incluPathPH == null || classNamePH == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateWrongConfig, context, engine, name);
                return false;
            }

            string tplHeaderPath = XFilesystem.PathCombine(tpl.BasePath, headerFileME);
            string tplSourcePath = XFilesystem.PathCombine(tpl.BasePath, sourceFileME);
            string moduleApi = classType == FUnrealSourceType.PUBLIC ? $"{moduleName.ToUpper()}_API " : ""; //Final space to separate from Class Name
            string incluPath = XFilesystem.PathToUnixStyle(sourceRelPath);
            if (incluPath != "") incluPath += "/";             //Final Path separator to separate from Class Name

            notifier.Info(XDialogLib.Ctx_ConfiguringTemplate, XDialogLib.Info_TemplateCopyingFiles, headerPath);
            XFilesystem.FileCopy(tplHeaderPath, headerPath);

            notifier.Info(XDialogLib.Ctx_ConfiguringTemplate, XDialogLib.Info_TemplateCopyingFiles, sourcePath);
            XFilesystem.FileCopy(tplSourcePath, sourcePath);

            PlaceHolderReplaceStrategy strategy = new PlaceHolderReplaceStrategy();
            strategy.AddFileExtension(".h", ".cpp");
            strategy.AddPlaceholder(moduleApiPH, moduleApi); //Di fatto da mettere solo se la classe e' Public ?!?!
            strategy.AddPlaceholder(incluPathPH, incluPath);
            strategy.AddPlaceholder(classNamePH, className);
            strategy.HandleFileContent(headerPath);
            strategy.HandleFileContent(sourcePath);

            notifier.Info(XDialogLib.Ctx_RegenSolutionFiles);
            XProcessResult ubtResult = await _engineUbt.GenerateVSProjectFilesAsync(_uprjFileAbsPath);
            if (ubtResult.IsError)
            {
                notifier.Erro(XDialogLib.Ctx_RegenSolutionFiles, ubtResult.StdOut);
                return false;
            }
            return true;
        }

        public async Task<bool> DeleteSourceDirectoryAsync(List<string> sourcePaths, FUnrealNotifier notifier)
        {
            notifier.Info(XDialogLib.Ctx_CheckProjectPlayout);

            List<string> dirs = new List<string>();
            List<string> files = new List<string>();
            foreach(string sourcePath in sourcePaths)
            {
                if (ExistsSourceFile(sourcePath))
                {
                    files.Add(sourcePath);
                } 
                else if (ExistsSourceDirectory(sourcePath))
                {
                    dirs.Add(sourcePath);
                }
                else
                {
                    notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_SourcePathNotFound, sourcePath);
                    return false;
                }
            }

            //Note: possibile optimization skipping files that are under some dirs

            foreach (string filePath in files)
            {
                notifier.Info(XDialogLib.Ctx_DeletingFiles, XDialogLib.Info_DeletingFile, filePath);

                bool success = XFilesystem.DeleteFile(filePath);
                if (!success)
                {
                    notifier.Erro(XDialogLib.Ctx_DeletingFiles, XDialogLib.Error_Delete);
                    return false;
                }
            }

            foreach (string dirPath in dirs)
            {
                notifier.Info(XDialogLib.Ctx_DeletingDirectories, XDialogLib.Info_DeletingFolder, dirPath);

                bool success = XFilesystem.DeleteDir(dirPath);
                if (!success)
                {
                    notifier.Erro(XDialogLib.Ctx_DeletingDirectories, XDialogLib.Error_Delete);
                    return false;
                }
            }

            notifier.Info(XDialogLib.Ctx_RegenSolutionFiles);
            XProcessResult ubtResult = await _engineUbt.GenerateVSProjectFilesAsync(_uprjFileAbsPath);
            if (ubtResult.IsError)
            {
                notifier.Erro(XDialogLib.Ctx_RegenSolutionFiles, ubtResult.StdOut);
                return false;
            }
            return true;
        }

        public async Task<bool> AddGameModuleAsync(string templeName, string moduleName, FUnrealNotifier notifier)
        {
            string context = "game_modules";
            string engine = _engineMajorVer;
            string name = templeName;

            
            if (ExistsGameModule(moduleName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_GameModuleAlreadyExists, moduleName);
                return false;
            }

            FUnrealTemplate tpl = _templates.GetTemplate(context, engine, name);
            if (tpl == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateNotFound, context, engine, name);
                return false;
            }

            string moduleNamePH = tpl.GetPlaceHolder("ModuleName");
            if (moduleNamePH == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateWrongConfig, context, engine, name);
                return false;
            }

            string metaType = tpl.GetMeta("type");
            string metaPhase = tpl.GetMeta("phase");
            string metaTarget = tpl.GetMeta("target");
            if (metaType == null || metaPhase == null || metaTarget == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateWrongConfig, context, engine, name);
                return false;
            }

            string sourcePath = AbsProjectSourceFolderPath();

            notifier.Info(XDialogLib.Ctx_ConfiguringTemplate, XDialogLib.Info_TemplateCopyingFiles, sourcePath);
            PlaceHolderReplaceStrategy strategy = new PlaceHolderReplaceStrategy();
            strategy.AddFileExtension(".cpp", ".h", ".cs");
            strategy.AddPlaceholder(moduleNamePH, moduleName);

            await XFilesystem.DeepCopyAsync(tpl.BasePath, sourcePath, strategy);

            //Update Project [TARGET].Target.cs file
            string targetName;
            if (metaTarget == "Game")
            {
                targetName = "";
            } 
            else
            {
                targetName = metaTarget;
            }
            
            string targetFileName = $"{ProjectName}{targetName}.Target.cs";
            string targetFilePath = XFilesystem.PathCombine(AbsProjectSourceFolderPath(), targetFileName);

            if (XFilesystem.FileExists(targetFilePath)) { 
                notifier.Info(XDialogLib.Ctx_UpdatingProject, XDialogLib.Info_UpdatingModuleTargetFile, targetFilePath);
                {
                    string csText = XFilesystem.ReadFile(targetFilePath);

                    //Capture Group1 for all module names sucs as: "Mod1", "Mod2" and replacing with "Mod1", "Mod2", "ModuleName"
                    string regex = @"ExtraModuleNames\s*\.AddRange\s*\(\s*new\s*string\[\]\s*\{\s*(\"".+\"")\s*\}\s*\)\s*;";
                    var match = Regex.Match(csText, regex); 
                    if (match.Success && match.Groups.Count == 2)
                    {
                        string moduleList = match.Groups[1].Value;
                        csText = csText.Replace(moduleList, $"{moduleList}, \"{moduleName}\"");
                        XFilesystem.WriteFile(targetFilePath, csText);
                    }
                }
            } else
            {
                notifier.Warn(XDialogLib.Ctx_UpdatingProject, XDialogLib.Info_UpdatingModuleTargetFile, targetFilePath);
            }

            //Update .uproject
            notifier.Info(XDialogLib.Ctx_UpdatingProject, XDialogLib.Info_UpdatingProjectDescriptorFile, _uprjFileAbsPath);
            FUnrealUProjectFile uprojectJson = new FUnrealUProjectFile(_uprjFileAbsPath);
            uprojectJson.Modules.Add(new FUnrealUProjectModuleJson()
            {
                 Name = moduleName,
                 Type = metaType,
                 LoadingPhase = metaPhase
            });
            uprojectJson.Save();
           
            notifier.Info(XDialogLib.Ctx_RegenSolutionFiles);
            XProcessResult ubtResult = await _engineUbt.GenerateVSProjectFilesAsync(_uprjFileAbsPath);
            if (ubtResult.IsError)
            {
                notifier.Erro(XDialogLib.Ctx_RegenSolutionFiles, ubtResult.StdOut);
                return false;
            }
            return true;
        }

        public async Task<bool> RenameGameModuleAsync(string moduleName, string newModuleName, bool updateCppFiles, FUnrealNotifier notifier)
        {
            if (!ExistsGameModule(moduleName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_GameModuleNotFound, moduleName);
                return false;
            }
            if (ExistsGameModule(newModuleName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_GameModuleAlreadyExists, moduleName);
                return false;
            }

            var project = GetUProject();
            var module = project.GameModules[moduleName];
            bool IsPrimaryGameModule = module.IsPrimaryGame;


            //string modulePath = AbsGameModulePath(moduleName);
            //string moduleFilePath = XFilesystem.PathCombine(modulePath, $"{moduleName}.Build.cs");
            string modulePath = module.FullPath;
            string moduleFilePath = module.BuildFilePath;

            //1. Rename .Build.cs file and replace "class ModuleName" and constructor (only if is the same as moduleName)
            notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_UpdatingModuleTargetFile, modulePath);
            {
                string csText = XFilesystem.ReadFile(moduleFilePath);

                string classRegex = @"(?<=class\s+?)SEARCH(?=[\s\S]*?\{)";
                classRegex = classRegex.Replace("SEARCH", moduleName);
                csText = Regex.Replace(csText, classRegex, newModuleName);

                string ctorRegex = @"(?<=public\s*?)SEARCH(?=\s*?\()";
                ctorRegex = ctorRegex.Replace("SEARCH", moduleName);
                csText = Regex.Replace(csText, ctorRegex, newModuleName);

                XFilesystem.WriteFile(moduleFilePath, csText);
                XFilesystem.RenameFileName(moduleFilePath, $"{newModuleName}.Build");
            }


            if (IsPrimaryGameModule)
            {
                bool moduleCppFound = true;

                string cppPath = XFilesystem.FindFile(modulePath, true, $"{moduleName}.cpp");
                if (cppPath == null)
                {
                    cppPath = XFilesystem.FindFile(modulePath, true, $"{moduleName}Module.cpp");
                    if (cppPath == null)
                    {
                        moduleCppFound = false;
                        notifier.Warn(XDialogLib.Ctx_UpdatingModule, XDialogLib.Warn_ModuleCppFileNotFound, $"**/{moduleName}.cpp", $"**/{moduleName}Module.cpp");
                    }
                }

                if (moduleCppFound)
                {
                    string cppText = XFilesystem.ReadFile(cppPath);

                    //IMPLEMENT_PRIMARY_GAME_MODULE( FDefaultGameModuleImpl, ModuleName, "ModuleName" );
                    notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_UpdatingModuleNameInCpp, cppPath);
                    string regexFirst = $@"(?<=IMPLEMENT_PRIMARY_GAME_MODULE\s*\([\s\S]+?,\s*){moduleName}(?=\s*?,[\s\S]+?\))";
                    string regexSecon = $@"(?<=IMPLEMENT_PRIMARY_GAME_MODULE\s*\([\s\S]+?,[\s\S]+?,\s*""){moduleName}(?=""\s*?\))";

                    cppText = Regex.Replace(cppText, regexFirst, newModuleName);
                    cppText = Regex.Replace(cppText, regexSecon, newModuleName);

                    if (updateCppFiles)
                    {
                        notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_RenamingCppFiles, cppPath);
                        //#include "<ModuleName>.h" or "<ModuleName>Module.h"
                        string inclRegex = $@"""{moduleName}.h""|""{moduleName}Module.h""";
                        string newIncName = $"\"{newModuleName}Module.h\"";
                        cppText = Regex.Replace(cppText, inclRegex, newIncName);

                        // No class for Primary Game Module (when using the default one)
                    }

                    XFilesystem.WriteFile(cppPath, cppText);

                    if (updateCppFiles) XFilesystem.RenameFileName(cppPath, $"{newModuleName}Module");
                    //TODO: Add control if file exists before renaming?!
                }

                if (moduleCppFound && updateCppFiles)
                {
                    string cppFileName = XFilesystem.GetFilenameNoExt(cppPath);
                    string hppPath = XFilesystem.FindFile(modulePath, true, $"{cppFileName}.h");
                    if (hppPath != null)
                    {
                        notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_RenamingCppFiles, hppPath);
                        // No class for Primary Game Module (when using the default one)
                        XFilesystem.RenameFileName(hppPath, $"{newModuleName}Module");
                        //TODO: Add control if file exists before renaming?!
                    }
                }

                // Update MODULENAME_API macro in all .h files under Public/** or /
                {
                    string modulePublicPath = module.PublicPath;
                    bool recurse = true;
                    if (!XFilesystem.DirectoryExists(modulePublicPath))
                    {
                        modulePublicPath = module.FullPath;
                        recurse = false;
                    }

                    var publicHeaderFiles = XFilesystem.FindFiles(modulePublicPath, recurse, "*.h");
                    string moduleApi = module.ApiMacro;
                    string newModuleApi = $"{newModuleName.ToUpper()}_API";

                    foreach (var file in publicHeaderFiles)
                    {
                        string text = XFilesystem.ReadFile(file);
                        if (text.Contains(moduleApi))
                        {
                            notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_UpdatingApiMacroInFile, file);
                            text = text.Replace(moduleApi, newModuleApi);
                            XFilesystem.WriteFile(file, text);
                        }
                    }
                }
            } 
            else //GAME_MODULE 
            {  


                //2. Rename .cpp and class (if still have same moduleName is configured there)
                //Cerco filename ModuleNameModule.cpp o ModuleName.cpp
                //NOTA: il file .cpp potrebbe essere stato spostato dentro altra cartella...per ora lo cerco solo sotto Private/
            
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

                    //IMPLEMENT_MODULE(F<ModuleName>Module, <ModuleName>)  or IMPLEMENT_GAME_MODULE
                    notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_UpdatingModuleNameInCpp, cppPath);
                    //string implModRegex = @"(?<=IMPLEMENT_MODULE\s*\([\s\S]+?,\s*)SEARCH(?=\s*?\))";
                    string implModRegex = @"(?<=(?:IMPLEMENT_MODULE|IMPLEMENT_GAME_MODULE)\s*\([\s\S]+?,\s*)SEARCH(?=\s*?\))";
                    implModRegex = implModRegex.Replace("SEARCH", moduleName);
                    cppText = Regex.Replace(cppText, implModRegex, newModuleName);

                    if (updateCppFiles)
                    {
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
                    //TODO: Add control if file exists before renaming?!
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
                        //TODO: Add control if file exists before renaming?!
                    }
                }

                //4. Update MODULENAME_API macro in all .h files under Public/**
                {
                    string modulePublicPath = module.PublicPath;

                    var publicHeaderFiles = XFilesystem.FindFiles(modulePublicPath, true, "*.h");
                    string moduleApi = module.ApiMacro;
                    string newModuleApi = $"{newModuleName.ToUpper()}_API";

                    foreach (var file in publicHeaderFiles)
                    {
                        string text = XFilesystem.ReadFile(file);
                        if (text.Contains(moduleApi))
                        {
                            notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_UpdatingApiMacroInFile, file);
                            text = text.Replace(moduleApi, newModuleApi);
                            XFilesystem.WriteFile(file, text);
                        }
                    }
                }
            }

            //4. Rename Module Folder
            notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_RenamingFolder, modulePath, newModuleName);
            XFilesystem.RenameDir(modulePath, newModuleName);


            //NOTA: Far diventare il 6 => 5 cosi da allineare gli step con il RenamePluginModule
            //5. Update module dependency in other module .Build.cs
            {
                foreach (var other in project.GameModules)
                {
                    if (other == module) continue;

                    string csFile = other.BuildFilePath;
                    string buildText = XFilesystem.ReadFile(csFile);
                    string dependency = $"\"{moduleName}\"";
                    string newDependency = $"\"{newModuleName}\"";
                    if (buildText.Contains(dependency))
                    {
                        notifier.Info(XDialogLib.Ctx_UpdatingModuleDependency, XDialogLib.Info_UpdatingDependencyFromFile, other.BuildFilePath);
                        buildText = buildText.Replace(dependency, newDependency);
                        XFilesystem.WriteFile(csFile, buildText);
                    }
                }
            }

            //6. Rename module in all project [TARGET].Target.cs 
            {
                foreach (var csFile in project.TargetFiles)
                {
                    string buildText = XFilesystem.ReadFile(csFile);
                    string dependency = $"\"{moduleName}\"";
                    string newDependency = $"\"{newModuleName}\"";
                    if (buildText.Contains(dependency))
                    {
                        notifier.Info(XDialogLib.Ctx_UpdatingModuleDependency, XDialogLib.Info_UpdatingDependencyFromFile, csFile);
                        buildText = buildText.Replace(dependency, newDependency);
                        XFilesystem.WriteFile(csFile, buildText);
                    }
                }
            }

            //7. Rename module in .uproject
            {
                string descrFilePath = project.DescriptorFilePath;
                notifier.Info(XDialogLib.Ctx_UpdatingProject, XDialogLib.Info_UpdatingProjectDescriptorFile, descrFilePath);

                var prjFile = new FUnrealUProjectFile(descrFilePath);
                var moduleJson = prjFile.Modules[moduleName];
                if (moduleJson)
                {
                    moduleJson.Name = newModuleName;
                    prjFile.Save();
                }
            }

            //8. Regen VS Project
            notifier.Info(XDialogLib.Ctx_RegenSolutionFiles);
            XProcessResult ubtResult = await _engineUbt.GenerateVSProjectFilesAsync(_uprjFileAbsPath);
            if (ubtResult.IsError)
            {
                notifier.Erro(XDialogLib.Ctx_RegenSolutionFiles, ubtResult.StdOut);
                return false;
            }
            return true;
        }

        public async Task<bool> DeleteGameModuleAsync(string moduleName, FUnrealNotifier notifier)
        {
            var project = GetUProject();
            var module = project.GameModules[moduleName];
            if (module == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_GameModuleNotFound, moduleName);
                return false;
            }
            //TODO: Should stop in case moduleName is Primary Module.


            //1. Remove dependency from other modules .Build.cs
            { 
                string moduleDepend = $"\"{moduleName}\"";
                //string regexDepend = @"(?<!,\s*)\s*""SEARCH""\s*,|,{0,1}\s*""SEARCH""\s*";
                string regexDepend = @"(?<!,\s*)\s*""SEARCH""\s*,|,{0,1}\s*""SEARCH""";
                regexDepend = regexDepend.Replace("SEARCH", moduleName); //replace to keep "clean" the regex because contains graphs {0,1}
                foreach (var other in project.GameModules)
                {
                    if (other.Name == module.Name) continue;

                    string buildText = XFilesystem.ReadFile(other.BuildFilePath);
                    if (buildText.Contains(moduleDepend))
                    {
                        notifier.Info(XDialogLib.Ctx_UpdatingModuleDependency, XDialogLib.Info_CleaningDependencyFromFile, other.BuildFilePath);
                        buildText = Regex.Replace(buildText, regexDepend, "");
                        XFilesystem.WriteFile(other.BuildFilePath, buildText);
                    }
                }
            }

            //2. Delete module path
            { 
                notifier.Info(XDialogLib.Ctx_DeletingModule, XDialogLib.Info_DeletingModuleFolder, module.FullPath);
                XFilesystem.DeleteDir(module.FullPath);
            }

            //3. Update .uproject removing the module
            { 
                notifier.Info(XDialogLib.Ctx_UpdatingProject, XDialogLib.Info_UpdatingProjectDescriptorFile, project.DescriptorFilePath);
                var descrFile = new FUnrealUProjectFile(project.DescriptorFilePath);
                var moduleJson = descrFile.Modules[moduleName];
                if (moduleJson)
                {
                    moduleJson.Remove();
                    descrFile.Save();
                }
            }

            //4. Delete module in all project [TARGET].Target.cs 
            {
                string moduleDepend = $"\"{moduleName}\"";
                string regexDepend = @"(?<!,\s*)\s*""SEARCH""\s*,|,{0,1}\s*""SEARCH"""; //NOTE: Regex repeated in different parts
                regexDepend = regexDepend.Replace("SEARCH", moduleName); //replace to keep "clean" the regex because contains graphs {0,1}
                foreach (var csFile in project.TargetFiles)
                {
                    string buildText = XFilesystem.ReadFile(csFile);
                    if (buildText.Contains(moduleDepend))
                    {
                        notifier.Info(XDialogLib.Ctx_UpdatingModuleDependency, XDialogLib.Info_UpdatingDependencyFromFile, csFile);
                        buildText = Regex.Replace(buildText, regexDepend, "");
                        XFilesystem.WriteFile(csFile, buildText);
                    }
                }
            }

            //5. Regen VS Project
            { 
                notifier.Info(XDialogLib.Ctx_RegenSolutionFiles);
                XProcessResult ubtResult = await _engineUbt.GenerateVSProjectFilesAsync(_uprjFileAbsPath);
                if (ubtResult.IsError)
                {
                    notifier.Erro(XDialogLib.Ctx_RegenSolutionFiles, ubtResult.StdOut);
                    return false;
                }
            }
            return true;
        }


        public async Task<bool> AddSourceFileAsync(string absBasePath, string fileName, FUnrealNotifier notifier)
        {
            string modulePath = ModulePathFromSourceCodePath(absBasePath);
            string moduleName = XFilesystem.GetLastPathToken(modulePath);

            string filePath = XFilesystem.PathCombine(absBasePath, fileName);

            if (XFilesystem.FileExists(filePath))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_FileAlreadyExists, filePath);
                return false;
            }

            notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_CreatingFile, filePath);
            XFilesystem.CreateFile(filePath);

            notifier.Info(XDialogLib.Ctx_RegenSolutionFiles);
            XProcessResult ubtResult = await _engineUbt.GenerateVSProjectFilesAsync(_uprjFileAbsPath);
            if (ubtResult.IsError)
            {
                notifier.Erro(XDialogLib.Ctx_RegenSolutionFiles, ubtResult.StdOut);
                return false;
            }
            return true;
        }

    }

    public class FUnrealTemplates
    {
        public static FUnrealTemplates Load(string templateDescriptorPath)
        {
            string templatePath = Path.GetDirectoryName(templateDescriptorPath);

            XmlDocument xml = new XmlDocument();
            try { 
                xml.Load(templateDescriptorPath);
            } catch (Exception e)
            {
                Debug.Print(e.Message);
                return new FUnrealTemplates();
            }

            FUnrealTemplates result = new FUnrealTemplates();

            XmlNodeList templateNodes =  xml.GetElementsByTagName("template");
            foreach(XmlNode tplNode in templateNodes)
            {
                string ctx     = tplNode.Attributes["context"]?.Value;
                string ueCsv   = tplNode.Attributes["ue"]?.Value;
                string name    = tplNode.Attributes["name"]?.Value;
                string relPath = tplNode.Attributes["path"]?.Value;
                if (ctx == null || name == null || ueCsv == null || relPath == null) continue;

                XmlNode uiNode = tplNode.SelectSingleNode("ui");
                string uiName = uiNode?.Attributes["label"]?.Value;
                string uiDesc = uiNode?.Attributes["desc"]?.Value;
                if (uiName == null || uiDesc == null) continue;

                string absPath = XFilesystem.PathCombine(templatePath, relPath);
                FUnrealTemplate tpl = new FUnrealTemplate(name, absPath, uiName, uiDesc);
                XmlNodeList placeHolderNodes = tplNode.SelectNodes("placeholder");
                foreach(XmlElement plhNode in placeHolderNodes)
                {
                    string role  = plhNode.Attributes["role"]?.Value;
                    string value = plhNode.Attributes["value"]?.Value;
                    if (role == null || value == null) continue;

                    tpl.SetPlaceHolder(role, value);
                }

                XmlNode metaNode = tplNode.SelectSingleNode("meta");
                if (metaNode != null)
                {
                    foreach (XmlAttribute attr in metaNode?.Attributes)
                    {
                        string metaName  = attr.Name;
                        string metaValue = attr.Value;
                        tpl.SetMeta(metaName, metaValue);
                    }
                }

                string[] ueArray = ueCsv.Split(',');
                foreach(string ue in ueArray)
                {
                    result.SetTemplate(ctx, ue, name, tpl);
                }
            }
            return result;
        }


        Dictionary<string, FUnrealTemplate> templatesByKey;
        private Dictionary<string, List<FUnrealTemplate>> templatesByContext;

        public FUnrealTemplates()
        {
            templatesByKey = new Dictionary<string, FUnrealTemplate>();
            templatesByContext = new Dictionary<string, List<FUnrealTemplate>>();
        }

        public int Count { get { return templatesByKey.Count; } }

        public void SetTemplate(string context, string ue, string name, FUnrealTemplate tpl)
        {
            string key = context + "_" + ue + "_" + name;
            templatesByKey[key] = tpl;

            string ctxKey = context + "_" + ue;
            if (!templatesByContext.TryGetValue(ctxKey, out var list))
            {
                list = new List<FUnrealTemplate>();
                templatesByContext[ctxKey] = list;
            };
            list.Add(tpl);
        }

        public FUnrealTemplate GetTemplate(string context, string ue, string name)
        {
            string key = context + "_" + ue + "_" + name;
            if (!templatesByKey.TryGetValue(key, out FUnrealTemplate tpl)) return null;
            return tpl;
        }

        public List<FUnrealTemplate> GetTemplates(string context, string ue)
        {
            string ctxKey = context + "_" + ue;
            return templatesByContext[ctxKey];
        }
    }

    public class FUnrealTemplate
    {
        Dictionary<string, string> placeHolders;
        Dictionary<string, string> metas;

        public string Name { get; internal set; }
        public string BasePath { get; internal set; }
        public string Label { get; private set; }
        public string Description { get; private set; }

        public FUnrealTemplate(string name, string templatePath, string label, string desc)
        {
            Name = name;
            BasePath = templatePath;
            Label = label;
            Description = desc;
            placeHolders = new Dictionary<string, string>();
            metas = new Dictionary<string, string>();
        }

        public int PlaceHolderCount { get { return placeHolders.Count; } }

        public void SetPlaceHolder(string role, string name)
        {
            placeHolders[role] = name;
        }

        public string GetPlaceHolder(string role)
        {
            if (!placeHolders.TryGetValue(role, out string name)) return null;
            return name;
        }

        public bool HasPlaceHolder(string role)
        {
            return placeHolders.ContainsKey(role);
        }

        public void SetMeta(string name, string value)
        {
            metas[name] = value;
        }

        public string GetMeta(string name)
        {
            if (!metas.TryGetValue(name, out string value)) return null;
            return value;
        }
    } 
}
