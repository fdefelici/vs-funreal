using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;

namespace FUnreal
{
    public enum FUnrealSourceType { INVALID = -1, PUBLIC, PRIVATE, CUSTOM }

    public struct FUnrealTargets
    {
        public const string GAME = "";
        public const string EDITOR = "Editor";
        public const string CLIENT = "Client";
        public const string SERVER = "Server";
        public const string PROGRAM = "Program";

        public static readonly string[] ALL = new string[] { GAME, EDITOR, CLIENT, SERVER, PROGRAM };
    }

    public class FUnrealService
    {
        public static FUnrealService Create(IFUnrealVS unrealVS)
        {
            string uprjFilePath = unrealVS.GetUProjectFilePath();
            if (!XFilesystem.FileExists(uprjFilePath))
            {
                unrealVS.Output.Erro("UProject file not found at the expected path: {0}", uprjFilePath);
                return null;
            }

            unrealVS.Output.Info("UE Project descriptor found at {0}", uprjFilePath);

            // Detect Engine Instance
            string enginePath = unrealVS.GetUnrealEnginePath(); //Try detecting UE "Engine" folder abs path from solution configuration  (<UE_ROOT>/Engine)
            if (enginePath == null || !XFilesystem.DirExists(enginePath))
            {
                unrealVS.Output.Erro("Cannot detect a valid UE path for: {0}", uprjFilePath);
                return null;
            }

            /* Version lookup on .uproject file abandoned in favor of <UE_ROOT>/Engine/Build/Build.version
            var uprojectFile = new FUnrealUProjectFile(uprjFilePath);
            string versionStr = uprojectFile.EngineAssociation;
            var version = XVersion.FromSemVer(versionStr);
            if (version == null)
            {
                unrealVS.Output.Erro("Cannot detect UE version from .uproject file!");
                return null;
            }
            */
            var buildVersionFilePath = XFilesystem.PathCombine(enginePath, "Build/Build.version");
            if (!XFilesystem.FileExists(buildVersionFilePath))
            {
                unrealVS.Output.Erro("Cannot detect UE version from Build.version file [file not found]: ", buildVersionFilePath);
                return null;
            }
            var buildVersionFile = new FUnrealBuildVersionFile(buildVersionFilePath);
            var version = new XVersion(buildVersionFile.MajorVersion, buildVersionFile.MinorVersion, buildVersionFile.PatchVersion);
            if (version.Major < 4)
            {
                unrealVS.Output.Erro("Cannot detect UE 4 or 5 version from Build.version. Current version detected is: ", version.AsString());
                return null;
            }

            string ubtBin = string.Empty;
            //NOTE: Eventually I could get rid off version check and find UBT executable by a filesystem scan instead.
            if (version.Major == 4)
            {
                //Example UE4: C:\Program Files\Epic Games\UE_4.27\Engine\Binaries\DotNET\UnrealBuildTool.exe
                ubtBin = XFilesystem.PathCombine(enginePath, "Binaries/DotNET/UnrealBuildTool.exe");
            }
            else if (version.Major >= 5) //5+
            {
                //Example UE5: C:\Program Files\Epic Games\UE_5.0\Engine\Binaries\DotNET\UnrealBuildTool\UnrealBuildTool.exe
                ubtBin = XFilesystem.PathCombine(enginePath, "Binaries/DotNET/UnrealBuildTool/UnrealBuildTool.exe");
            }

            if (!XFilesystem.FileExists(ubtBin))
            {
                unrealVS.Output.Erro("Cannot detect UBT at path {0}", ubtBin);
                return null;
            }

            FUnrealEngine engine = new FUnrealEngine(version, enginePath, new FUnrealBuildTool(ubtBin));

            unrealVS.Output.Info("UE Version: {0}", engine.Version.AsString());
            unrealVS.Output.Info("UE Path: {0}", engine.EnginePath);
            unrealVS.Output.Info("UBT Path: {0}", engine.UnrealBuildTool.BinPath);

            bool success = FUnrealTemplateLoader.TryComputeTemplates(unrealVS, engine, out FUnrealTemplates templates);
            if (!success)
            {
                unrealVS.Output.Erro("Some issue while loading templates. Eventually try to check Options or try reloading templates!");
                //return null;
            }

            return new FUnrealService(engine, uprjFilePath, templates);
        }

       

        public FUnrealEngine Engine { get; private set; }
        private IFUnrealBuildTool _engineUbt;
        private string _engineMajorVer;

        private string _uprjFileAbsPath;

        private string _prjPath;
        private string _pluginsPath;
        private string _sourcePath;
        private FUnrealTemplates _templates;
        private FUnrealProject _projectModel;
        FUnrealProjectFactory _projectModuleFactory;

        public FUnrealService(FUnrealEngine engine, string uprjAbsPath, FUnrealTemplates templates)
        {
            Engine = engine;
            _engineUbt = engine.UnrealBuildTool;
            _engineMajorVer = engine.Version.Major.ToString();

            _uprjFileAbsPath = uprjAbsPath;
            ProjectName = XFilesystem.GetFilenameNoExt(uprjAbsPath);
            _templates = templates;

            _prjPath = XFilesystem.PathParent(uprjAbsPath);
            _pluginsPath = XFilesystem.PathCombine(_prjPath, "Plugins");
            _sourcePath = XFilesystem.PathCombine(_prjPath, "Source");

            _projectModuleFactory = new FUnrealProjectFactory();
            _projectModel = new FUnrealProject(uprjAbsPath); //just to avoid NullPointerException in case project model is never loaded
        }

        public FUnrealService(FUnrealEngine engine, FUnrealProject project, FUnrealTemplates templates)
             : this(engine, project.DescriptorFilePath, templates)
        {
            _projectModel = project;
        }

        public void SetTemplates(FUnrealTemplates templates)
        {
            _templates = templates;
        }

        public List<string> KnownEmptyFolderPaths()
        {
            return _projectModuleFactory.EmptyFolderPaths;
        }

        public FUnrealProject GetUProject()
        {
            return _projectModel;
        }

        public async Task<bool> UpdateProjectAsync(FUnrealNotifier notifier)
        {
            var upro = await _projectModuleFactory.CreateAsync(_uprjFileAbsPath, notifier);

            if (upro == null) return false;

            _projectModel = upro;
            return true;
        }

        public async Task<bool> UpdateEmptyFoldersAsync()
        {
            await _projectModuleFactory.ScanEmptyFoldersAsync(_projectModel);
            return true;
        }


        public string ProjectName { get; }

        public bool IsSourceCodePath(string fullPath, bool afterModuleDir = false)
        {
            var module = GetUProject().AllModules.FindByBelongingPath(fullPath);
            if (module == null) return false;

            if (fullPath == module.BuildFilePath) return false;

            if (afterModuleDir && fullPath == module.FullPath) return false;

            return true;

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

            /*
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
            */

            /*
            string modulePath = ModulePathFromSourceCodePath(fullPath);
            if (modulePath == null) return false;

            if (fullPath.EndsWith(".Build.cs")) return false;
            */
        }

        public FUnrealSourceType TypeForSourcePath(string path)
        {
            if (!IsSourceCodePath(path)) return FUnrealSourceType.INVALID;

            string modulePath = ModulePathFromSourceCodePath(path);

            string afterModulePath = XFilesystem.PathSubtract(path, modulePath);

            string firstModuleSubfolder = XFilesystem.PathSplit(afterModulePath)[0];

            if (firstModuleSubfolder == "Public")
            {
                return FUnrealSourceType.PUBLIC;
            }

            if (firstModuleSubfolder == "Private")
            {
                return FUnrealSourceType.PRIVATE;
            }

            return FUnrealSourceType.CUSTOM;
        }

        public List<FUnrealPluginTemplate> PluginTemplates()
        {
            return _templates.GetPlugins(_engineMajorVer);
        }

        public List<FUnrealPluginModuleTemplate> PluginModuleTemplates()
        {
            return _templates.GetPluginModules(_engineMajorVer);
        }

        public List<FUnrealGameModuleTemplate> GameModuleTemplates()
        {
            return _templates.GetGameModules(_engineMajorVer);
        }
        
        public List<FUnrealClassTemplate> SourceTemplates()
        {
            return _templates.GetClasses(_engineMajorVer);
        }


        private FUnrealPlugin PluginByName(string name)
        {
            return GetUProject().Plugins[name];
        }

        private FUnrealModule ModuleByName(string name)
        {
            return GetUProject().AllModules[name];
        }

        //NOTE: used only by ProjectRelativePluginPath
        public string AbsPluginPath_RealOrTheorical(string plugName)
        {
            var plug = PluginByName(plugName);
            if (plug == null)
            {
                //default path
                return XFilesystem.PathCombine(_pluginsPath, plugName);
            }
            return plug.FullPath;
        }

        public string AbsProjectSourcePath()
        {
            return GetUProject().SourcePath;
        }

        public string AbsProjectPluginsPath()
        {
            return GetUProject().PluginsPath;
        }

        public string AbsModulePath(string modName)
        {
            var module = ModuleByName(modName);
            if (module == null) return null;
            return module.FullPath;
        }

        private string AbsProjectPath()
        {
            return _prjPath;
        }

        private string AbsProjectSourceFolderPath()
        {
            return _sourcePath;
        }

        // UI Only
        public string ProjectRelativePathForPlugin(string pluginName)
        {
            string prjPath = AbsProjectPath();
            string absPath = AbsPluginPath_RealOrTheorical(pluginName);
            string relPath = XFilesystem.PathSubtract(absPath, prjPath, true);
            return relPath;
        }

        // UI Only
        public string ProjectRelativePathForPluginModuleDefault(string plugName, string modName)
        {
            string prjPath = AbsProjectPath();

            //default path
            string plugPath = AbsPluginPath_RealOrTheorical(plugName);
            string absModPath = XFilesystem.PathCombine(plugPath, "Source", modName);

            string relPath = XFilesystem.PathSubtract(absModPath, prjPath, true);
            return relPath;
        }

        // UI Only
        public string ProjectRelativePathForGameModuleDefault(string moduleName)
        {
            string prjPath = AbsProjectPath();
            string absPath = XFilesystem.PathCombine(prjPath, "Source", moduleName);
            string relPath = XFilesystem.PathSubtract(absPath, prjPath, true);
            return relPath;
        }

        // UI Only
        public string ProjectRelativePath(string fullPath, bool keepProjectNameAsFirstItemOfThePath = true)
        {
            string prjPath = AbsProjectPath();
            string absPath = fullPath;
            string relPath = XFilesystem.PathSubtract(absPath, prjPath, keepProjectNameAsFirstItemOfThePath);
            return relPath;
        }

        // UI Only
        public string ModuleRelativePath(string fullPath)
        {
            string modPath = ModulePathFromSourceCodePath(fullPath);
            string relPath = XFilesystem.PathSubtract(fullPath, modPath, true);
            return relPath;
        }

        // UI Only
        public string ProjectRelativePathForModule(string modName)
        {
            var mod = ModuleByName(modName);
            if (mod == null) return null;
            return XFilesystem.PathSubtract(mod.FullPath, AbsProjectPath(), true);
        }

        public bool ExistsPlugin(string pluginName)
        {
            return GetUProject().Plugins.Exists(pluginName);
        }

        public bool ExistsModule(string moduleName)
        {
            /*
            string modulePath = AbsPluginModulePath(pluginName, moduleName);
            return Directory.Exists(modulePath);
            */
            var module = ModuleNamed(moduleName);
            return module != null;
        }

        private FUnrealModule ModuleNamed(string moduleName)
        {
            var project = GetUProject();
            return project.AllModules[moduleName];
        }

        public string ModuleNameFromSourceCodePath(string path)
        {
            var found = GetUProject().AllModules.FindByBelongingPath(path);
            if (found == null) return null;
            return found.Name;
            /*
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
            */
        }

        public string ModulePathFromSourceCodePath(string path)
        {
            var found = ModuleFromSourceCodePath(path);
            if (found == null) return null;
            return found.FullPath;
            /*
            string moduleGroupRegex = @"PRJ_PATH\\(?:Plugins\\[^\\]+?\\){0,1}Source\\[^\\]+";
            string prjExcape = Regex.Escape(_prjPath);
            moduleGroupRegex = moduleGroupRegex.Replace("PRJ_PATH", prjExcape);

            var match = Regex.Match(path, moduleGroupRegex);
            if (match.Success)
            {
                return match.Value;
            }
            return null;
            */
        }

        public FUnrealModule ModuleFromSourceCodePath(string path)
        {
            var found = GetUProject().AllModules.FindByBelongingPath(path);
            if (found == null) return null;
            return found;
        }

        public string PluginNameFromSourceCodePath(string path)
        {
            var found = GetUProject().Plugins.FindByBelongingPath(path);
            if (found == null) return null;
            return found.Name;
            /*
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
            */
        }

        public bool ExistsSourceDirectory(string sourcePath)
        {
            if (!IsSourceCodePath(sourcePath)) return false;
            return XFilesystem.DirExists(sourcePath);
        }

        public bool ExistsSourceFile(string sourcePath)
        {
            if (!IsSourceCodePath(sourcePath)) return false;
            return XFilesystem.FileExists(sourcePath);
        }

        public bool IsPluginFolder(string fullPath)
        {
            /*
            string pluginName = PluginNameFromSourceCodePath(fullPath);
            if (pluginName == null) return false;

            string expectedPath = AbsPluginPath_RealOrTheorical(pluginName);
            return expectedPath == fullPath;
            */

            var found = GetUProject().Plugins.FindByPath(fullPath);
            if (found == null) return false;
            return true;
        }

        public bool IsPluginDescriptorFile(string fullPath)
        {
            var found = GetUProject().Plugins.FindByBelongingPath(fullPath);
            if (found == null) return false;
            return fullPath == found.DescriptorFilePath;
        }

        public bool IsModuleBuildFile(string fullPath)
        {
            var found = GetUProject().AllModules.FindByBelongingPath(fullPath);
            if (found == null) return false;
            return fullPath == found.BuildFilePath;
        }

        public bool IsProjectDescriptorFile(string fullPath)
        {
            return _uprjFileAbsPath == fullPath;
        }

        public bool IsPluginModulePath(string fullPath)
        {
            string pluginName = PluginNameFromSourceCodePath(fullPath);
            string moduleName = ModuleNameFromSourceCodePath(fullPath);

            bool isTrue = pluginName != null && moduleName != null;
            return isTrue;
        }

        public bool IsGameModulePath(string fullPath)
        {
            string pluginName = PluginNameFromSourceCodePath(fullPath);
            string moduleName = ModuleNameFromSourceCodePath(fullPath);

            bool isTrue = pluginName == null && moduleName != null;
            return isTrue;
        }

        public bool IsPrimaryGameModulePath(string fullPath)
        {
            var module = GetUProject().GameModules.FindByBelongingPath(fullPath);
            if (module == null) return false;
            return module.IsPrimaryGameModule;
        }


        public async Task<FUnrealServicePluginResult> AddPluginAsync(string templeName, string pluginName, string moduleNameOrNull, FUnrealNotifier notifier)
        {
            string context = "plugins";
            string engine = _engineMajorVer;
            string name = templeName;

            if (ExistsPlugin(pluginName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.ErrorMsg_PluginNotExists, pluginName);
                return false;
            }
            if (moduleNameOrNull != null && ExistsModule(moduleNameOrNull))
            {
                var prevModule = ModuleByName(moduleNameOrNull);
                var relPath = ProjectRelativePath(prevModule.FullPath);
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_ModuleAlreadyExists, relPath);
                return false;
            }

            var tpl = _templates.GetPlugin(engine, name);
            if (tpl == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateNotFound, context, engine, name);
                return false;
            }

            string pluginNamePH = "@{TPL_PLUG_NAME}";
            string moduleNamePH = "@{TPL_MODU_NAME}";
            string moduleFilePH = "@{TPL_MODU_CLASS}";


            notifier.Info(XDialogLib.Ctx_ConfiguringTemplate, XDialogLib.Info_TemplateCopyingFiles, _pluginsPath);
            PlaceHolderReplaceVisitor strategy = new PlaceHolderReplaceVisitor();
            strategy.AddFileExtension(".cpp", ".h", ".cs", ".uplugin", "vcxproj"); //vcxproj from thirdpartylibrary
            strategy.AddPlaceholder(pluginNamePH, pluginName);

            if (moduleNameOrNull != null)
            {
                string fileName = moduleNameOrNull;
                if (!fileName.EndsWith("Module"))
                {
                    fileName = $"{fileName}Module";
                }
                strategy.AddPlaceholder(moduleNamePH, moduleNameOrNull);
                strategy.AddPlaceholder(moduleFilePH, fileName);
            }
            await XFilesystem.DirDeepCopyAsync(tpl.BasePath, _pluginsPath, strategy);

            //X. Regen VS Project
            bool taskSuccess = await FUnrealServiceTasks.Project_RegenSolutionFilesAsync(GetUProject(), _engineUbt, notifier);
            if (!taskSuccess) return false;

            //X. Update Project Model
            string pluginPath = XFilesystem.PathCombine(_pluginsPath, pluginName);
            var pluginAdded = _projectModuleFactory.AddPlugin(GetUProject(), pluginPath);
            //TODO: In case template is malvormed (example, missing .uplugin, pluginAdded can be null)
            FUnrealServicePluginResult success = true;
            success.DescrFilePath = pluginAdded.DescriptorFilePath;
            return success;
        }

        public async Task<FUnrealServiceSimpleResult> DeletePluginAsync(string pluginName, FUnrealNotifier notifier)
        {
            if (!ExistsPlugin(pluginName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginNotFound, pluginName);
                return false;
            }

            var plugin = PluginByName(pluginName);
            string plugPath = plugin.FullPath;

            bool taskSuccess;
            taskSuccess = FUnrealServiceTasks.Plugin_CheckIfNotLockedByOtherProcess(plugin, notifier);
            if (!taskSuccess) return false;

            notifier.Info(XDialogLib.Ctx_DeletingFiles, XDialogLib.Info_DeletingFolder, plugin.FullPath);
            if (!XFilesystem.DirDelete(plugPath))
            {
                notifier.Erro(XDialogLib.Ctx_DeletingFiles, XDialogLib.Error_Delete);
                return false;
            }

            if (XFilesystem.DirIsEmpty(_pluginsPath))
            {
                notifier.Info(XDialogLib.Ctx_DeletingFiles, XDialogLib.Info_DeletingFolder, _pluginsPath);
                if (!XFilesystem.DirDelete(_pluginsPath))
                {
                    notifier.Erro(XDialogLib.Ctx_DeletingFiles, XDialogLib.Error_Delete);
                }
            }

            //Update uproject file
            var uprojectJson = new FUnrealUProjectFile(_uprjFileAbsPath);
            if (uprojectJson.Plugins)
            {
                var pluginJson = uprojectJson.Plugins[pluginName];
                if (pluginJson)
                {
                    notifier.Info(XDialogLib.Ctx_UpdatingProject, XDialogLib.Info_UpdatingProjectDescriptorFile, _uprjFileAbsPath);
                    pluginJson.Remove();
                    uprojectJson.Plugins.RemoveIfEmpty();
                    uprojectJson.Save();
                }
            }

            // Remove plugin modules in dependent .Build.cs (if configured there)
            taskSuccess = await FUnrealServiceTasks.Plugin_DeleteModuleDependencyAsync(plugin, GetUProject().AllModules, notifier);

            // Remove plugin in dependent .uplugin (if configured there)
            taskSuccess = await FUnrealServiceTasks.Plugin_DeleteDependencyAsync(plugin, GetUProject().Plugins, notifier);
            if (!taskSuccess) return false;


            //X. Regen VS Project
            taskSuccess = await FUnrealServiceTasks.Project_RegenSolutionFilesAsync(GetUProject(), _engineUbt, notifier);
            if (!taskSuccess) return false;

            //X. Update Project Model
            _projectModuleFactory.RemovePlugin(GetUProject(), plugin);

            return true;
        }

        public async Task<FUnrealServicePluginResult> RenamePluginAsync(string pluginName, string pluginNewName, FUnrealNotifier notifier)
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

            var plugin = PluginByName(pluginName);

            bool taskSuccess;
            taskSuccess = FUnrealServiceTasks.Plugin_CheckIfNotLockedByOtherProcess(plugin, notifier);
            if (!taskSuccess) return false;

            //1. Rename .uplugin file and replace "FriendlyName" (only if is the same as pluginName) //TODO: Could override the name in anycase..
            string upluginFilePath = plugin.DescriptorFilePath;
            notifier.Info(XDialogLib.Ctx_UpdatingPlugin, XDialogLib.Info_UpdatingPluginDescriptorFile, upluginFilePath);

            JObject upluginJson = XFilesystem.JsonFileRead(upluginFilePath);
            string upluginName = (string)upluginJson["FriendlyName"];
            if (pluginName.Equals(upluginName))
            {
                upluginJson["FriendlyName"] = pluginNewName;
                XFilesystem.JsonFileWrite(upluginFilePath, upluginJson);
            }
            notifier.Info(XDialogLib.Ctx_UpdatingPlugin, XDialogLib.Info_RenamingPluginDescriptorFile, upluginFilePath, pluginNewName);
            XFilesystem.FileRename(upluginFilePath, pluginNewName);

            //2. Rename Plugin Folder
            notifier.Info(XDialogLib.Ctx_UpdatingPlugin, XDialogLib.Info_RenamingFolder, plugin.FullPath, pluginNewName);
            await XFilesystem.RenameDirAsync(plugin.FullPath, pluginNewName);

            //3. Rename plugin in .uproject (if plugin is configured there)
            FUnrealUProjectFile uprojectJson = new FUnrealUProjectFile(_uprjFileAbsPath);
            var plugingJson = uprojectJson.Plugins[pluginName];
            if (plugingJson)
            {
                notifier.Info(XDialogLib.Ctx_UpdatingProject, XDialogLib.Info_UpdatingProjectDescriptorFile, _uprjFileAbsPath);
                plugingJson.Name = pluginNewName;
                uprojectJson.Save();
            }

            //4. Rename plugin in other dependents .uplugin (if plugin is configured there)
            taskSuccess = await FUnrealServiceTasks.Plugin_RenameDependencyAsync(plugin, GetUProject().Plugins, pluginNewName, notifier);
            if (!taskSuccess) return false;

            //5. Regen VS Solution
            taskSuccess = await FUnrealServiceTasks.Project_RegenSolutionFilesAsync(GetUProject(), _engineUbt, notifier);
            if (!taskSuccess) return false;

            //project module update
            var plugRenamed = _projectModuleFactory.RenamePlugin(GetUProject(), plugin, pluginNewName);

            FUnrealServicePluginResult success = true;
            success.DescrFilePath = plugRenamed.DescriptorFilePath;
            return success;
        }

        public async Task<FUnrealServiceModuleResult> AddPluginModuleAsync(string templeName, string pluginName, string moduleName, FUnrealNotifier notifier)
        {
            string context = "modules";
            string engine = _engineMajorVer;
            string name = templeName;

            if (!ExistsPlugin(pluginName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.ErrorMsg_PluginNotExists, pluginName);
                return false;
            }

            if (ExistsModule(moduleName))
            {
                var prevModule = ModuleByName(moduleName);
                var relPath = ProjectRelativePath(prevModule.FullPath);
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_ModuleAlreadyExists, relPath);
                return false;
            }

            var tpl = _templates.GetPluginModule(engine, name);
            if (tpl == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateNotFound, context, engine, name);
                return false;
            }

            string tplPath = tpl.BasePath;
            if (!XFilesystem.DirExists(tplPath))
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateWrongConfig, context, engine, name);
                return false;
            }

            string metaType = tpl.Type;
            string metaPhase = tpl.Phase;
            if (metaType == null || metaPhase == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateWrongConfig, context, engine, name);
                return false;
            }

            var plugin = PluginByName(pluginName);

            notifier.Info(XDialogLib.Ctx_ConfiguringTemplate, XDialogLib.Info_TemplateCopyingFiles, plugin.SourcePath);
            PlaceHolderReplaceVisitor strategy = new PlaceHolderReplaceVisitor();
            strategy.AddFileExtension(".cpp", ".h", ".cs", ".uplugin", "vcxproj"); //vcxproj from thirdpartylibrary

            string fileName = moduleName;
            if (!fileName.EndsWith("Module"))
            {
                fileName = $"{fileName}Module";
            }

            string moduleNamePH = "@{TPL_MODU_NAME}";
            string moduleFilePH = "@{TPL_MODU_CLASS}";
            strategy.AddPlaceholder(moduleNamePH, moduleName);
            strategy.AddPlaceholder(moduleFilePH, fileName);

            string sourcePath = plugin.SourcePath;

            //TODO: Check if BasePath exists (just in case of wrong path to template file)

            await XFilesystem.DirDeepCopyAsync(tpl.BasePath, sourcePath, strategy);

            //Update .uplugin
            string upluginFilePath = plugin.DescriptorFilePath;
            notifier.Info(XDialogLib.Ctx_UpdatingPlugin, XDialogLib.Info_UpdatingPluginDescriptorFile, upluginFilePath);
            var upluginFile = new FUnrealUPluginJsonFile(upluginFilePath);
            upluginFile.Modules.Add(new FUnrealUPluginModuleJson()
            {
                Name = moduleName,
                Type = metaType,
                LoadingPhase = metaPhase
            });
            upluginFile.Save(); //todo: SaveAsync

            //X. Regen VS Project
            bool taskSuccess = await FUnrealServiceTasks.Project_RegenSolutionFilesAsync(GetUProject(), _engineUbt, notifier);
            if (!taskSuccess) return false;

            //X. Update Project Model
            var moduleAdded = _projectModuleFactory.AddPluginModule(GetUProject(), plugin, moduleName);

            FUnrealServiceModuleResult success = true;
            success.BuildFilePath = moduleAdded.BuildFilePath;
            return success;
        }

        public async Task<FUnrealServiceModuleResult> RenamePluginModuleAsync(string pluginName, string moduleName, string newModuleName, bool renameSourceFiles, FUnrealNotifier notifier)
        {
            if (!ExistsPlugin(pluginName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginNotFound, pluginName);
                return false;
            }
            if (!ExistsModule(moduleName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginModuleNotFound, pluginName, moduleName);
                return false;
            }
            if (ExistsModule(newModuleName))
            {
                var prevModule = ModuleByName(moduleName);
                var relPath = ProjectRelativePath(prevModule.FullPath);
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_ModuleAlreadyExists, relPath);
                return false;
            }

            var project = GetUProject();
            var plugin = project.Plugins[pluginName];
            var module = plugin.Modules[moduleName];

            bool taskSuccess;
            //1. Rename .Build.cs file and replace "class ModuleName" and constructor (only if is the same as moduleName)
            taskSuccess = FUnrealServiceTasks.Module_UpdateAndRenameBuildCs(module, newModuleName, notifier);
            if (!taskSuccess) return false;

            //2. Rename module .cpp and update #include directive in dependendent modules sources
            taskSuccess = await FUnrealServiceTasks.Module_UpdateAndRenameSourcesAsync(module, newModuleName, renameSourceFiles, project.AllModules, notifier);
            if (!taskSuccess) return false;

            //3. Update MODULENAME_API macro in all .h files under Public/**
            taskSuccess = FUnrealServiceTasks.Module_UpdateApiMacro(module, newModuleName, notifier);
            if (!taskSuccess) return false;

            //4. Rename Module Folder
            taskSuccess = await FUnrealServiceTasks.Module_RenameFolderAsync(module, newModuleName, notifier);
            if (!taskSuccess) return false;

            //5. Update module dependency in other module .Build.cs
            taskSuccess = FUnrealServiceTasks.Module_UpdateDependencyInOtherModules(module, newModuleName, project.AllModules, notifier);
            if (!taskSuccess) return false;

            //6. Rename module in .uplugin
            taskSuccess = FUnrealServiceTasks.Plugin_RenameModuleInDescriptor(plugin, module, newModuleName, notifier);
            if (!taskSuccess) return false;

            //7. Regen VS Solution
            taskSuccess = await FUnrealServiceTasks.Project_RegenSolutionFilesAsync(project, _engineUbt, notifier);
            if (!taskSuccess) return false;


            var moduleRenamed = _projectModuleFactory.RenamePluginModule(project, plugin, module, newModuleName);

            FUnrealServiceModuleResult success = true;
            success.BuildFilePath = moduleRenamed.BuildFilePath;
            return true;
        }

        public async Task<FUnrealServiceSimpleResult> DeletePluginModuleAsync(string pluginName, string moduleName, FUnrealNotifier notifier)
        {
            if (!ExistsPlugin(pluginName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginNotFound, pluginName);
                return false;
            }

            var plugin = PluginByName(pluginName);
            FUnrealModule module = plugin.Modules[moduleName];
            if (module == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginModuleNotFound, pluginName, moduleName);
                return false;
            }

            var project = GetUProject();

            bool taskSuccess;
            //1. Remove dependency from other modules .Build.cs
            taskSuccess = FUnrealServiceTasks.Module_DeleteDependencyInOtherModules(module, project.AllModules, notifier);
            if (!taskSuccess) return false;

            //2. Delete module path
            notifier.Info(XDialogLib.Ctx_DeletingModule, XDialogLib.Info_DeletingModuleFolder, module.FullPath);
            XFilesystem.DirDelete(module.FullPath);

            //3. Update .uplugin removing the module
            notifier.Info(XDialogLib.Ctx_UpdatingPlugin, XDialogLib.Info_UpdatingPluginDescriptorFile, plugin.DescriptorFilePath);
            FUnrealUPluginJsonFile upluginFile = new FUnrealUPluginJsonFile(plugin.DescriptorFilePath);
            var moduleJson = upluginFile.Modules[moduleName];
            if (moduleJson)
            {
                moduleJson.Remove();
                upluginFile.Save();
            }


            //X. Regen VS Project
            taskSuccess = await FUnrealServiceTasks.Project_RegenSolutionFilesAsync(GetUProject(), _engineUbt, notifier);
            if (!taskSuccess) return false;

            //X. Update Project Model
            _projectModuleFactory.DeletePluginModule(project, plugin, module);

            return true;
        }

        public async Task<FUnrealServiceSourceClassResult> AddSourceClassAsync(string templeName, string absBasePath, string className, FUnrealSourceType classType, FUnrealNotifier notifier)
        {
            string context = "classes";
            string engine = _engineMajorVer;
            string name = templeName;

            var module = GetUProject().AllModules.FindByBelongingPath(absBasePath);
            if (module == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.ErrorMsg_ModuleNotExists);
                return false;
            }

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
            var tpl = _templates.GetClass(engine, name);
            if (tpl == null || !XFilesystem.DirExists(tpl.BasePath))
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateNotFound, context, engine, name);
                return false;
            }

            string headerFileME = tpl.Header;
            string sourceFileME = tpl.Source;

            string tplHeaderPath = XFilesystem.PathCombine(tpl.BasePath, headerFileME);
            string tplSourcePath = XFilesystem.PathCombine(tpl.BasePath, sourceFileME);
            if (!XFilesystem.FileExists(tplHeaderPath) && !XFilesystem.FileExists(tplSourcePath))
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateNotFound, context, engine, name);
                return false;
            }


            string moduleApiPH = "@{TPL_MODU_API}";
            string incluPathPH = "@{TPL_SOUR_INCL}";
            string classNamePH = "@{TPL_SOUR_CLASS}";
            string moduleApi = classType == FUnrealSourceType.PUBLIC ? module.ApiMacro : string.Empty;

            string incluPath = XFilesystem.PathToUnixStyle(sourceRelPath);
            if (incluPath != "") incluPath += "/";             //Final Path separator to separate from Class Name

            notifier.Info(XDialogLib.Ctx_ConfiguringTemplate, XDialogLib.Info_TemplateCopyingFiles, headerPath);
            XFilesystem.FileCopy(tplHeaderPath, headerPath);

            notifier.Info(XDialogLib.Ctx_ConfiguringTemplate, XDialogLib.Info_TemplateCopyingFiles, sourcePath);
            XFilesystem.FileCopy(tplSourcePath, sourcePath);

            PlaceHolderReplaceVisitor strategy = new PlaceHolderReplaceVisitor();
            strategy.AddFileExtension(".h", ".cpp");
            strategy.AddPlaceholder(moduleApiPH, moduleApi);
            strategy.AddPlaceholder(incluPathPH, incluPath);
            strategy.AddPlaceholder(classNamePH, className);
            if (classType != FUnrealSourceType.PUBLIC)
            {
                //Get rid off the possible extra space due to TPL_MODU_API becoming Empty in case of Private or Custom classes
                strategy.AddPlaceholder("class  ", "class "); //replace "class double space" with "class space"
            }
            strategy.HandleFileContent(headerPath);
            strategy.HandleFileContent(sourcePath);

            notifier.Info(XDialogLib.Ctx_RegenSolutionFiles);
            XProcessResult ubtResult = await _engineUbt.GenerateVSProjectFilesAsync(_uprjFileAbsPath);
            if (ubtResult.IsError)
            {
                notifier.Erro(XDialogLib.Ctx_RegenSolutionFiles, ubtResult.StdOut);
                return false;
            }

            FUnrealServiceSourceClassResult success = true;
            success.HeaderPath = headerPath;
            success.SourcePath = sourcePath;
            return success;
        }

        public async Task<FUnrealServiceFilesResult> DeleteSourcesAsync(List<string> sourcePaths, FUnrealNotifier notifier)
        {
            notifier.Info(XDialogLib.Ctx_CheckProjectPlayout);
            if (sourcePaths.Count == 0)
            {
                notifier.Info(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.NothingToDelete);
                return true;
            }

            List<string> dirs = new List<string>();
            List<string> files = new List<string>();

            List<string> parentPaths = new List<string>();
            foreach (string sourcePath in sourcePaths)
            {
                if (ExistsSourceFile(sourcePath))
                {
                    parentPaths.Add(XFilesystem.PathParent(sourcePath));
                    files.Add(sourcePath);
                }
                else if (ExistsSourceDirectory(sourcePath))
                {
                    parentPaths.Add(XFilesystem.PathParent(sourcePath));
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

                bool deleted = XFilesystem.FileDelete(filePath);
                if (!deleted)
                {
                    notifier.Erro(XDialogLib.Ctx_DeletingFiles, XDialogLib.Error_Delete);
                    return false;
                }
            }

            foreach (string dirPath in dirs)
            {
                notifier.Info(XDialogLib.Ctx_DeletingDirectories, XDialogLib.Info_DeletingFolder, dirPath);

                bool deleted = XFilesystem.DirDelete(dirPath);
                if (!deleted)
                {
                    notifier.Erro(XDialogLib.Ctx_DeletingDirectories, XDialogLib.Error_Delete);
                    return false;
                }
            }

            //X. Regen VS Project
            bool taskSuccess = await FUnrealServiceTasks.Project_RegenSolutionFilesAsync(GetUProject(), _engineUbt, notifier);
            if (!taskSuccess) return false;

            //var emptyFolders = XFilesystem.SelectFolderPathsNotContainingAnyFile(parentPaths);

            FUnrealServiceFilesResult success = true;
            success.AllPaths = sourcePaths;
            success.FilePaths = files;
            success.DirPaths = dirs;
            success.AllParentPaths = parentPaths;
            return success;
        }

        public async Task<FUnrealServiceModuleResult> AddGameModuleAsync(string templeName, string moduleName, FUnrealNotifier notifier)
        {
            string context = "game_modules";
            string engine = _engineMajorVer;
            string name = templeName;

            if (ExistsModule(moduleName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_GameModuleAlreadyExists, moduleName);
                return false;
            }

            var tpl = _templates.GetGameModule(engine, name);
            if (tpl == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateNotFound, context, engine, name);
                return false;
            }

            string metaType = tpl.Type;
            string metaPhase = tpl.Phase;
            string metaTarget = tpl.Target;
            if (metaType == null || metaPhase == null || metaTarget == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckTemplate, XDialogLib.Error_TemplateWrongConfig, context, engine, name);
                return false;
            }

            string sourcePath = AbsProjectSourceFolderPath();

            notifier.Info(XDialogLib.Ctx_ConfiguringTemplate, XDialogLib.Info_TemplateCopyingFiles, sourcePath);
            PlaceHolderReplaceVisitor strategy = new PlaceHolderReplaceVisitor();
            strategy.AddFileExtension(".cpp", ".h", ".cs", "vcxproj"); //vcxproj from thirdpartylibrary

            string fileName = moduleName;
            if (!fileName.EndsWith("Module"))
            {
                fileName = $"{fileName}Module";
            }

            string moduleNamePH = "@{TPL_MODU_NAME}";
            string moduleFilePH = "@{TPL_MODU_CLASS}";
            strategy.AddPlaceholder(moduleNamePH, moduleName);
            strategy.AddPlaceholder(moduleFilePH, fileName);

            await XFilesystem.DirDeepCopyAsync(tpl.BasePath, sourcePath, strategy);

            //Update Project [TARGET].Target.cs file
            string targetName;
            if (metaTarget == "Game")
            {
                targetName = FUnrealTargets.GAME;
            }
            else
            {
                targetName = metaTarget; //Should check if its valid
            }
            var project = GetUProject();

            bool taskSuccess;

            //let's say no problem if this tasks don't end successfully (for instance the Target file is missing)
            taskSuccess = FUnrealServiceTasks.Project_AddModuleToTarget(project, targetName, moduleName, notifier);
            if (targetName != FUnrealTargets.EDITOR)
            {
                //By default game module needs to be added to editor target otherwise when launching from VS, 
                //following error appear 'The following modules are missing or built with a different engine version'
                taskSuccess = FUnrealServiceTasks.Project_AddModuleToTarget(project, FUnrealTargets.EDITOR, moduleName, notifier);
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

            //X. Regen VS Project
            taskSuccess = await FUnrealServiceTasks.Project_RegenSolutionFilesAsync(GetUProject(), _engineUbt, notifier);
            if (!taskSuccess) return false;

            //X. Update Project Model
            string modulePath = XFilesystem.PathCombine(sourcePath, moduleName);
            var moduleAdded = _projectModuleFactory.AddGameModule(_projectModel, modulePath);

            FUnrealServiceModuleResult success = true;
            success.BuildFilePath = moduleAdded.BuildFilePath;
            return success;
        }

        public async Task<FUnrealServiceModuleResult> RenameGameModuleAsync(string moduleName, string newModuleName, bool renameSourceFiles, FUnrealNotifier notifier)
        {
            if (!ExistsModule(moduleName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_ModuleNotFound, moduleName);
                return false;
            }
            if (ExistsModule(newModuleName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_GameModuleAlreadyExists, moduleName);
                return false;
            }

            var project = GetUProject();
            var module = project.GameModules[moduleName];
            bool IsPrimaryGameModule = module.IsPrimaryGameModule;


            //string modulePath = AbsGameModulePath(moduleName);
            //string moduleFilePath = XFilesystem.PathCombine(modulePath, $"{moduleName}.Build.cs");
            string modulePath = module.FullPath;
            string moduleFilePath = module.BuildFilePath;

            bool taskSuccess;
            //1. Rename .Build.cs file and replace "class ModuleName" and constructor (only if is the same as moduleName)
            taskSuccess = FUnrealServiceTasks.Module_UpdateAndRenameBuildCs(module, newModuleName, notifier);
            if (!taskSuccess) return false;

            //2. Rename module .h/.cpp and update #include directive in dependendent modules sources (look only to other game modules)
            taskSuccess = await FUnrealServiceTasks.Module_UpdateAndRenameSourcesAsync(module, newModuleName, renameSourceFiles, project.GameModules, notifier, IsPrimaryGameModule);
            if (!taskSuccess) return false;

            //3. Update MODULENAME_API macro in all .h files under Public/**
            taskSuccess = FUnrealServiceTasks.Module_UpdateApiMacro(module, newModuleName, notifier);
            if (!taskSuccess) return false;

            //4. Rename Module Folder
            taskSuccess = await FUnrealServiceTasks.Module_RenameFolderAsync(module, newModuleName, notifier);
            if (!taskSuccess) return false;

            //5. Update module dependency in other module .Build.cs
            taskSuccess = FUnrealServiceTasks.Module_UpdateDependencyInOtherModules(module, newModuleName, project.GameModules, notifier);
            if (!taskSuccess) return false;

            //6. Rename module in all project [TARGET].Target.cs 
            taskSuccess = FUnrealServiceTasks.Project_RenameModuleInTargets(project, module, newModuleName, notifier);
            if (!taskSuccess) return false;

            //7. Rename module in .uproject
            taskSuccess = FUnrealServiceTasks.Project_RenameModuleInDescriptor(project, module, newModuleName, notifier);
            if (!taskSuccess) return false;

            //8. Regen VS Project
            taskSuccess = await FUnrealServiceTasks.Project_RegenSolutionFilesAsync(project, _engineUbt, notifier);
            if (!taskSuccess) return false;

            //X. Update Project Model
            var moduleRenamed = _projectModuleFactory.RenameGameModule(_projectModel, module, newModuleName);

            FUnrealServiceModuleResult success = true;
            success.BuildFilePath = moduleRenamed.BuildFilePath;
            return success;
        }

        public async Task<FUnrealServiceSimpleResult> DeleteGameModuleAsync(string moduleName, FUnrealNotifier notifier)
        {
            var project = GetUProject();
            var module = project.GameModules[moduleName];
            if (module == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_ModuleNotFound, moduleName);
                return false;
            }
            //TODO: Should stop in case moduleName is Primary Module. (by now just prevented by UI button disabled)

            bool taskSuccess;

            //1. Remove dependency from other game modules .Build.cs
            taskSuccess = FUnrealServiceTasks.Module_DeleteDependencyInOtherModules(module, project.GameModules, notifier);
            if (!taskSuccess) return false;

            //2. Delete module path
            {
                notifier.Info(XDialogLib.Ctx_DeletingModule, XDialogLib.Info_DeletingModuleFolder, module.FullPath);
                XFilesystem.DirDelete(module.FullPath);
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

            //4. Remove module in all project [TARGET].Target.cs 
            {
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
                        csFile.RemoveExtraModule(moduleName);
                        csFile.Save();
                    }
                }
            }

            //5. Regen VS Project
            taskSuccess = await FUnrealServiceTasks.Project_RegenSolutionFilesAsync(project, _engineUbt, notifier);
            if (!taskSuccess) return false;

            _projectModuleFactory.DeleteGameModule(project, module);

            return true;
        }

        public async Task<FUnrealServiceFileResult> AddSourceFileAsync(string absBasePath, string fileName, FUnrealNotifier notifier)
        {
            //Eventually check if is a valid module path

            string filePath = XFilesystem.PathCombine(absBasePath, fileName);

            if (XFilesystem.FileExists(filePath))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_FileAlreadyExists, filePath);
                return false;
            }

            notifier.Info(XDialogLib.Ctx_UpdatingModule, XDialogLib.Info_CreatingFile, filePath);
            XFilesystem.FileCreate(filePath);

            //X. Regen VS Project
            bool taskSuccess = await FUnrealServiceTasks.Project_RegenSolutionFilesAsync(GetUProject(), _engineUbt, notifier);
            if (!taskSuccess) return false;

            FUnrealServiceFileResult success = true;
            success.FilePath = filePath;
            return success;
        }

        public async Task<FUnrealServiceFileResult> RenameFileAsync(string filePath, string newFileNameWithExt, FUnrealNotifier notifier)
        {
            if (!ExistsSourceFile(filePath))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_FileNotFound, filePath);
                return false;
            }

            string newFilePath = XFilesystem.FilePathChangeNameWithExt(filePath, newFileNameWithExt);
            if (ExistsSourceFile(newFilePath))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_FileAlreadyExists, newFilePath);
                return false;
            }

            //NOTA: Ridurre i file da scansionare verificando quali moduli esterni (oltre a quello cui appartiene il file)
            //      dipendono da quello cui appartiene il file.
            //      Quindi poi va lanciata ricerca di sostituzione sui file dei moduli trovati + il corrente.
            //      Ulteriormente il replace va fatto solo se:
            //      - il nuovo filename finisce per .h   (in tutti gli altri casi no)
            //      - il vecchio filename finisce per .h

            var project = GetUProject();

            bool isHeaderFileScenario = XFilesystem.HasExtension(filePath, ".h") && XFilesystem.HasExtension(newFileNameWithExt, ".h");
            if (isHeaderFileScenario)
            {
                var allModules = project.AllModules;

                var fileModule = allModules.FindByBelongingPath(filePath);
                if (fileModule == null)
                {
                    notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.ErrorMsg_ModuleNotExists);
                    return false;
                }
                // Select all Modules dependent from fileModule
                var dependentModules = FUnrealServiceTasks.Module_DependentModules(fileModule, allModules, notifier);
                dependentModules.Add(fileModule); //add also current module to be scanned for updating include directive


                notifier.Info(XDialogLib.Ctx_UpdatingFiles);

                //Check file for #include "FILENAME.generated.h"
                string fileNameNoExt = XFilesystem.GetFilenameNoExt(filePath);
                string newFileNameNoExt = XFilesystem.GetFilenameNoExt(newFileNameWithExt);
                string incGenRegex = $@"(?<=#include\s+""){fileNameNoExt}(?=\.generated\.h"")";
                string fileContent = XFilesystem.FileRead(filePath);
                if (Regex.IsMatch(fileContent, incGenRegex))
                {
                    notifier.Info(XDialogLib.Ctx_UpdatingFiles, XDialogLib.info_UpdatingFile, filePath);
                    fileContent = Regex.Replace(fileContent, incGenRegex, newFileNameNoExt);
                    XFilesystem.FileWrite(filePath, fileContent);
                }

                string oldIncludePath = FUnrealServiceTasks.Module_ComputeHeaderIncludePath(fileModule, filePath);
                string newIncludePath = FUnrealServiceTasks.Module_ComputeHeaderIncludePath(fileModule, newFilePath);
                await FUnrealServiceTasks.Modules_FixIncludeDirectiveFullPathAsync(dependentModules, oldIncludePath, newIncludePath, notifier);
            }

            //For any kind of file (.h included) rename the file
            notifier.Info(XDialogLib.Ctx_RenamingFiles, XDialogLib.Info_RenamingFileToNewName, filePath, newFileNameWithExt);
            string filePathRenamed = XFilesystem.FileRenameWithExt(filePath, newFileNameWithExt);
            if (filePathRenamed == null)
            {
                notifier.Erro(XDialogLib.Ctx_RenamingFiles, XDialogLib.Error_FileRenameFailed);
                return false;
            }

            bool taskSuccess;
            taskSuccess = await FUnrealServiceTasks.Project_RegenSolutionFilesAsync(project, _engineUbt, notifier);
            if (!taskSuccess) return false;

            FUnrealServiceFileResult success = true;
            success.FilePath = filePathRenamed;
            return success;
        }

        public async Task<FUnrealServiceFileResult> RenameFolderAsync(string folderPath, string newFolderName, FUnrealNotifier notifier)
        {
            if (!ExistsSourceDirectory(folderPath))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_SourceDirectoryNotFound, folderPath);
                return false;
            }

            string newFolderPath = XFilesystem.ChangeDirName(folderPath, newFolderName);
            if (ExistsSourceDirectory(newFolderPath))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_DirectoryAlreadyExists, newFolderPath);
                return false;
            }

            var project = GetUProject();
            var module = project.AllModules.FindByBelongingPath(folderPath);
            if (module == null)
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_ModuleNotFound, $"for path {folderPath}");
                return false;
            }

            //Examples:
            //#include "utils/core/alpha/Pippo.h"
            //#include "utils/core/Mario.h"
            //#include "altro/core/deriv/Other.h"
            //Rename utils/core to utils/core2

            //1. Detect if .h are involved and prepare substitution list for #include directive
            bool needIncludeUpdate = false;
            bool hasPublicHeaderInvolved = false;
            //handle only Public or Private specific case. In case of Custom folder is threated always as private.... (eventually could check in .Build.cs)
            if (folderPath != module.PublicPath && folderPath != module.PrivatePath)
            {
                needIncludeUpdate = true;
                hasPublicHeaderInvolved = XFilesystem.FileExists(folderPath, true, "*.h", filePath =>
                {
                    return XFilesystem.IsChildPath(filePath, module.PublicPath);
                });
            }

            if (needIncludeUpdate)
            {
                string incOldPath = FUnrealServiceTasks.Module_ComputeHeaderIncludePath(module, folderPath);
                string incNewPath = FUnrealServiceTasks.Module_ComputeHeaderIncludePath(module, newFolderPath);

                //2. Replace #include directive in current module Public + Private
                //NOTE: can be optimized in case header is Private. So only files under Privated need to be processed
                await FUnrealServiceTasks.Module_FixIncludeDirectiveBasePathAsync(module, incOldPath, incNewPath, notifier);

                //3. Replace #include directive in all dependent module for Public
                if (hasPublicHeaderInvolved)
                {
                    var otherModules = FUnrealServiceTasks.Module_DependentModules(module, project.AllModules, notifier);
                    await FUnrealServiceTasks.Modules_FixIncludeDirectiveBasePathAsync(otherModules, incOldPath, incNewPath, notifier);
                }
            }

            bool taskSuccess;
            //4. Renaming source folder
            taskSuccess = await FUnrealServiceTasks.Source_RenameFolderAsync(folderPath, newFolderName, notifier);
            if (!taskSuccess) return false;

            //5. Regen VS Solution
            taskSuccess = await FUnrealServiceTasks.Project_RegenSolutionFilesAsync(project, _engineUbt, notifier);
            if (!taskSuccess) return false;

            FUnrealServiceFileResult success = true;
            success.FilePath = newFolderPath;
            return success;
        }

        public void ComputeSourceCodePaths(string absPathSelected, string className, FUnrealSourceType sourceType, out string headerPath, out string sourcePath, out string sourceRelPath)
        {
            var module = ModuleFromSourceCodePath(absPathSelected);
            FUnrealServiceTasks.Module_ComputeSourceCodePaths(module, absPathSelected, className, sourceType, out headerPath, out sourcePath, out sourceRelPath);
        }
    }
}
