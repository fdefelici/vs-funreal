using System;
using System.Collections.Generic;
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
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows.Shapes;

namespace FUnreal
{
    public enum FUnrealSourceType { INVALID = -1, PUBLIC, PRIVATE, CUSTOM }

    public class FUnrealService
    {
        public static FUnrealService Create(FUnrealVS unrealVS)
        {
            string uprjFilePath = unrealVS.GetUProjectFilePath();
            if (!XFilesystem.FileExists(uprjFilePath))
            {
                unrealVS.Output.Erro("UProject file not found at the expected path: {0}", uprjFilePath);
                return null;
            }

            unrealVS.Output.Info("UE Project descriptor found at {0}", uprjFilePath);

            // Detect Engine Instance
            string enginePath = unrealVS.GetUnrealEnginePath(); //Try detecting UE base path from solution configuration
            if (enginePath == null || !XFilesystem.DirectoryExists(enginePath))
            {
                unrealVS.Output.Erro("Cannot detect a valid UE path: ", uprjFilePath);
                return null;
            }

            var uprojectFile = new FUnrealUProjectFile(uprjFilePath);
            string versionStr = uprojectFile.EngineAssociation;
            var version = XVersion.FromSemVer(versionStr);
            if (version == null)
            {
                unrealVS.Output.Erro("Cannot detect UE version from .uproject file!");
                return null;
            }

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

            if (!XFilesystem.FileExists(ubtBin))
            {
                unrealVS.Output.Erro("Cannot detect UBT at path {0}", ubtBin);
                return null;
            }

            FUnrealEngine engine = new FUnrealEngine(version, enginePath, new FUnrealBuildTool(ubtBin));

            unrealVS.Output.Info("UE Version: {0}", engine.Version.AsString());
            unrealVS.Output.Info("UE Path: {0}", engine.EnginePath);
            unrealVS.Output.Info("UBT Path: {0}", engine.UnrealBuildTool.BinPath);


            // Load Templates
            string vsixDllPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            string vsixBasePath = XFilesystem.PathParent(vsixDllPath);
            string templatePath = XFilesystem.PathCombine(vsixBasePath, "Templates");
            string templateDescPath = XFilesystem.PathCombine(templatePath, "descriptor.xml");
            XDebug.Info("VSIX Dll Path: {0}", vsixDllPath);
            XDebug.Info("VSIX Base Path: {0}", vsixBasePath);
            XDebug.Info("Template Descriptor Path: {0}", templateDescPath);

            if (!XFilesystem.FileExists(templateDescPath))
            {
                unrealVS.Output.Erro("Cannot locate templates at path: {0}", templateDescPath);
                return null;
            }

            FUnrealTemplates templates = FUnrealTemplates.Load(templateDescPath);

            return new FUnrealService(engine, uprjFilePath, templates);
        }

        private FUnrealEngine _engine;
        private IFUnrealBuildTool _engineUbt;
        private string _engineMajorVer;

        private string _uprjFileAbsPath;
        
        private string _prjPath;
        private string _pluginsPath;
        private string _sourcePath;
        private FUnrealTemplates _templates;
        private FUnrealProject _projectModel;

        public FUnrealService(FUnrealEngine engine, string uprjAbsPath, FUnrealTemplates templates)
        {
            _engine = engine;
            _engineUbt = engine.UnrealBuildTool;
            _engineMajorVer = engine.Version.Major.ToString();

            _uprjFileAbsPath = uprjAbsPath;
            ProjectName = XFilesystem.GetFilenameNoExt(uprjAbsPath);
            _templates = templates;
        
            _prjPath = XFilesystem.PathParent(uprjAbsPath);
            _pluginsPath = XFilesystem.PathCombine(_prjPath, "Plugins");
            _sourcePath = XFilesystem.PathCombine(_prjPath, "Source");

            _projectModel = new FUnrealProject(ProjectName, uprjAbsPath); //just to avoid NullPointerException in case project model is never loaded
        }

        public FUnrealService(FUnrealEngine engine, FUnrealProject project, FUnrealTemplates templates)
             : this(engine, project.DescriptorFilePath, templates)
        {
            _projectModel = project;
        }

        public FUnrealProject GetUProject()
        {
            return _projectModel;
        }

        public async Task<bool> UpdateProjectAsync(FUnrealNotifier notifier)
        {
            FUnrealProjectFactory factory = new FUnrealProjectFactory();

            var upro = await factory.CreateV4Async(_uprjFileAbsPath, notifier);

            if (upro == null) return false;

            _projectModel = upro;
            return true;
        }


        public string ProjectName { get;  }

        public bool IsSourceCodePath(string fullPath, bool afterModuleDir=false)
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

        public void ComputeSourceCodePaths(string currentPath, string className, FUnrealSourceType classType,
            out string headerPath, out string sourcePath, out string sourceRelPath)
        {
            bool isPublicPath = false;
            bool isPrivatePath = false;
            //bool isFreePath = false;

            string modulePath = ModulePathFromSourceCodePath(currentPath);

            string relPathAfterModuleDir = XFilesystem.PathSubtract(currentPath, modulePath);
            string firstModuleSubfolder = XFilesystem.PathSplit(relPathAfterModuleDir)[0];

            if (firstModuleSubfolder == "Public")
            {
                sourceRelPath = XFilesystem.PathSubtract(relPathAfterModuleDir, "Public");
                isPublicPath = true;
            }
            else if (firstModuleSubfolder == "Private")
            {
                sourceRelPath = XFilesystem.PathSubtract(relPathAfterModuleDir, "Private");
                isPrivatePath = true;
            }
            else
            {
                sourceRelPath = relPathAfterModuleDir;
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
                }
                else
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

        private FUnrealPlugin PluginByName(string name)
        {
            return GetUProject().Plugins[name];
        }

        private FUnrealModule ModuleByName(string name)
        {
            return GetUProject().AllModules[name];
        }

        //TODO: Usato solo da ProjectRelativePluginPath
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
        public string ProjectRelativePath(string fullPath)
        {
            string prjPath = AbsProjectPath();
            string absPath = fullPath;
            string relPath = XFilesystem.PathSubtract(absPath, prjPath, true);
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
            var found = GetUProject().AllModules.FindByBelongingPath(path);
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
            return XFilesystem.DirectoryExists(sourcePath);
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
            if (moduleNameOrNull != null && ExistsModule(moduleNameOrNull))
            {
                var prevModule = ModuleByName(moduleNameOrNull);
                var relPath = ProjectRelativePath(prevModule.FullPath);
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_ModuleAlreadyExists, relPath);
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
            if (!ExistsPlugin(pluginName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginNotFound, pluginName);
                return false;
            }

            var plugin = PluginByName(pluginName);
            string plugPath = plugin.FullPath;
            notifier.Erro(XDialogLib.Ctx_DeletingFiles, XDialogLib.Info_DeletingFolder, plugin.FullPath);
            if (!XFilesystem.DeleteDir(plugPath))
            {
                notifier.Erro(XDialogLib.Ctx_DeletingFiles, XDialogLib.Error_Delete);
                return false;
            }

            if (XFilesystem.IsEmptyDir(_pluginsPath))
            {
                notifier.Info(XDialogLib.Ctx_DeletingFiles, XDialogLib.Info_DeletingFolder, _pluginsPath);
                if (!XFilesystem.DeleteDir(_pluginsPath))
                {
                    notifier.Erro(XDialogLib.Ctx_DeletingFiles, XDialogLib.Error_Delete);
                }
            }


            //Update uproject file
            var uprojectJson = new FUnrealUProjectFile(_uprjFileAbsPath);
            if (uprojectJson.Plugins) {
                var pluginJson = uprojectJson.Plugins[pluginName];
                if (pluginJson) {
                    notifier.Info(XDialogLib.Ctx_UpdatingProject, XDialogLib.Info_UpdatingProjectDescriptorFile, _uprjFileAbsPath);
                    pluginJson.Remove();
                    uprojectJson.Plugins.RemoveIfEmpty();
                    uprojectJson.Save();
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

            var plugin = PluginByName(pluginName);

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

            var plugin = PluginByName(pluginName);

            notifier.Info(XDialogLib.Ctx_ConfiguringTemplate, XDialogLib.Info_TemplateCopyingFiles, plugin.SourcePath);
            PlaceHolderReplaceStrategy strategy = new PlaceHolderReplaceStrategy();
            strategy.AddFileExtension(".cpp", ".h", ".cs", ".uplugin");
            strategy.AddPlaceholder(moduleNamePH, moduleName);

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

            var plugin = PluginByName(pluginName);
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
            if (!ExistsPlugin(pluginName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginNotFound, pluginName);
                return false;
            }

            var plugin = PluginByName(pluginName);
            FUnrealModule module = plugin.Modules[moduleName];
            if (module == null) {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_PluginModuleNotFound, pluginName, moduleName);
                return false;
            }

            var project = GetUProject();

            //1. Remove dependency from other modules .Build.cs
            //notifier.Info(XDialogLib.Ctx_UpdatingModuleDependency);
            string moduleDepend = $"\"{moduleName}\"";
            //string regexDepend = @"(?<!,\s*)\s*""SEARCH""\s*,|,{0,1}\s*""SEARCH""\s*";
            string regexDepend = @"(?<!,\s*)\s*""SEARCH""\s*,|,{0,1}\s*""SEARCH""";
            regexDepend = regexDepend.Replace("SEARCH", moduleName);
            foreach(var other in project.AllModules) //plugin.Modules
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

            //string modulePath = ModulePathFromSourceCodePath(absBasePath);
            //string moduleName = XFilesystem.GetLastPathToken(modulePath);

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
            string moduleApi = classType == FUnrealSourceType.PUBLIC ? $"{module.ApiMacro} " : ""; //Final space to separate from Class Name
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

        public async Task<bool> DeleteSourcesAsync(List<string> sourcePaths, FUnrealNotifier notifier)
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

            if (ExistsModule(moduleName))
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

                    //Capture Group1 for all module names such as: ("Mod1", "Mod2") and replacing with ("Mod1", "Mod2", "ModuleName")
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
            if (!ExistsModule(moduleName))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_GameModuleNotFound, moduleName);
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
            //TODO: Should stop in case moduleName is Primary Module. (by now just prevented by UI button disabled)


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

            //4. Remove module in all project [TARGET].Target.cs 
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
            //Eventually check if is a valid module path

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

        public async Task<bool> RenameFileAsync(string filePath, string newFileNameWithExt, FUnrealNotifier notifier)
        {
            if (!ExistsSourceFile(filePath))
            {
                notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Error_FileNotFound, filePath);
                return false;
            }

            string newFilePath = XFilesystem.FileChangeNameWithExt(filePath, newFileNameWithExt);
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

            bool isHeaderFileScenario = XFilesystem.HasExtension(filePath, ".h") && XFilesystem.HasExtension(newFileNameWithExt, ".h");
            if (isHeaderFileScenario)
            {
                var project = GetUProject();
                var allModules = project.AllModules;

                var fileModule = allModules.FindByBelongingPath(filePath);
                if (fileModule == null)
                {
                    notifier.Erro(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.ErrorMsg_ModuleNotExists);
                    return false;
                }
                var fileModuleName = fileModule.Name;

                // Select all Modules dependent to fileModule
                notifier.Info(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Info_CheckingModuleDependency, fileModuleName);
                var dependentModules = new FUnrealCollection<IFUnrealModule>();
                dependentModules.Add(fileModule);
                foreach (var other in allModules)
                {
                    if (other == fileModule) continue;

                    string buildText = XFilesystem.ReadFile(other.BuildFilePath);

                    string dependency = $"\"{fileModuleName}\"";
                    if (buildText.Contains(dependency))
                    {
                        notifier.Info(XDialogLib.Ctx_CheckProjectPlayout, XDialogLib.Info_DependentModule, other.Name);
                        dependentModules.Add(other);
                    }
                }


                notifier.Info(XDialogLib.Ctx_UpdatingFiles);

                //Check file for #include "FILENAME.generated.h"
                string fileNameNoExt = XFilesystem.GetFilenameNoExt(filePath);
                string newFileNameNoExt = XFilesystem.GetFilenameNoExt(newFileNameWithExt);
                string incGenRegex = $@"(?<=#include\s+""){fileNameNoExt}(?=\.generated\.h"")";
                string fileContent = XFilesystem.ReadFile(filePath);
                if (Regex.IsMatch(fileContent, incGenRegex))
                {
                    notifier.Info(XDialogLib.Ctx_UpdatingFiles, XDialogLib.info_UpdatingFile, filePath);
                    fileContent = Regex.Replace(fileContent, incGenRegex, newFileNameNoExt);
                    XFilesystem.WriteFile(filePath, fileContent);
                }

                string incRegex = $@"(?<=#include\s+(?:""|<)(?:\w+/)*){fileNameNoExt}(?=\.h(?:""|>))";
                Action<string> replaceAction = (path) =>
                {
                    string text = XFilesystem.ReadFile(path);

                    if (Regex.IsMatch(text, incRegex))
                    {
                        notifier.Info(XDialogLib.Ctx_UpdatingFiles, XDialogLib.info_UpdatingFile, path);
                        text = Regex.Replace(text, incRegex, newFileNameNoExt);
                        XFilesystem.WriteFile(path, text);
                    }
                };


                //Configure Parellel Max Degree 

                await Task.Run(async () =>
                {
                    foreach (var module in dependentModules)
                    {
                        string modulePath = module.FullPath;
                        List<string> headerPaths = await XFilesystem.DirectoryFilesAsync(modulePath, "*.h", true);
                        Parallel.ForEach(headerPaths, replaceAction);

                        List<string> sourcePaths = await XFilesystem.DirectoryFilesAsync(modulePath, "*.cpp", true);
                        Parallel.ForEach(sourcePaths, replaceAction);
                    }
                });
            }

            //For all kind of file (.h included) rename the file
            {
                notifier.Info(XDialogLib.Ctx_RenamingFiles, XDialogLib.Info_RenamingFileToNewName, filePath, newFileNameWithExt);
                string fileRenamed = XFilesystem.RenameFileNameWithExt(filePath, newFileNameWithExt);
                if (fileRenamed == null)
                {
                    notifier.Erro(XDialogLib.Ctx_RenamingFiles, XDialogLib.Error_FileRenameFailed);
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

        
    } 
}
