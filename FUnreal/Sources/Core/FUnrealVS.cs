using Community.VisualStudio.Toolkit;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FUnreal
{
    public class FUnrealVS
    {
        public FUnrealVS()
        {
            VS.Events.ProjectItemsEvents.AfterAddProjectItems += (args) =>
            {
                foreach(var item in args)
                {
                    Debug.WriteLine(item.Name);
                }
            };

            VS.Events.ProjectItemsEvents.AfterRenameProjectItems += (args) =>
            {
                Debug.WriteLine("RENAMED: ");
                foreach (var item in args.ProjectItemRenames)
                {

                    Debug.WriteLine(item.OldName);
                    Debug.WriteLine(item.SolutionItem.Name);
                }
            };


            VS.Events.ProjectItemsEvents.AfterRemoveProjectItems += (args) =>
            {
                Debug.WriteLine("RENAMED: ");
                foreach (var item in args.ProjectItemRemoves)
                {

                    Debug.WriteLine(item.RemovedItemName);
                }
            };

        }

        public async Task<FUnrealVSItem> GetSelectedItemAsync()
        {
            SolutionItem moduleItem = await VS.Solutions.GetActiveItemAsync();
            return new FUnrealVSItem(moduleItem);
        }

        public async Task<bool> IsSingleSelectionAsync()
        {
            var items = await VS.Solutions.GetActiveItemsAsync();
            return items.Count() == 1;
        }

        /*
        public async Task<FUnrealVSPluginModule> GetSelectedPluginModuleAsync()
        {
            SolutionItem moduleItem = await VS.Solutions.GetActiveItemAsync();
            string moduleFilePath = moduleItem.FullPath; // module/path/ModuleName.Build.cs
            string modulePath = XFilesystem.PathParent(moduleFilePath);
            string moduleName = XFilesystem.GetFilenameNoExt(moduleFilePath, true);

            string pluginPath = XFilesystem.PathParent(modulePath, 2);
            string pluginName = XFilesystem.GetLastPathToken(pluginPath);

            FUnrealVSPluginModule result = new FUnrealVSPluginModule();
            result.PluginName = pluginName;
            result.ModuleName = moduleName;
            return result;
        }
        */

        public async Task<FUnrealVSPlugin> GetSelectedPluginAsync()
        {
            SolutionItem solutionItem = await VS.Solutions.GetActiveItemAsync();
            string pluginName = XFilesystem.GetFilenameNoExt(solutionItem.Text);

            FUnrealVSPlugin result = new FUnrealVSPlugin();
            result.PluginName = pluginName;
            return result;
        }

        public async Task<List<FUnrealVSItem>> GetSelectedItemsAsync()
        {
            var items = await VS.Solutions.GetActiveItemsAsync();

            List<FUnrealVSItem> result = new List<FUnrealVSItem>(); 
            foreach (var item in items)
            {
                result.Add(new FUnrealVSItem(item));
            }
            return result;
        }
    }

    /* 
       SolutionItem Properties for PhysicalFile
        - Extension         = ".uplugin"
        - Folder            = "C:\\_fdf\\workspace_unreal\\ws_unreal\\UENLOpt\\Plugins\\MyPlugin"
        - FullPath          = "C:\\_fdf\\workspace_unreal\\ws_unreal\\UENLOpt\\Plugins\\MyPlugin\\MyPlugin.uplugin"
        - Name              = "C:\\_fdf\\workspace_unreal\\ws_unreal\\UENLOpt\\Plugins\\MyPlugin\\MyPlugin.uplugin"
        - Parent            = "Plugins\\MyPlugin" (VirtualFolder)
        - Text              = "MyPlugin.uplugin"
        - Type              = PhysicalFile
     */

    /* 
       SolutionItem Properties for VirtualFolder attached to Project
        - Children          = IEnumerable<SolutionItem>
        - FullPath          = null
        - Name              = "Config"
        - Parent            = "UENLOpt" (Project) [SolutionItem]
        - Text              = "Config"
        - Type              = VirtualFolder
     */

    /* 
       SolutionItem Properties for VirtualFolder some levels under the project
        - Children          = IEnumerable<SolutionItem>
        - FullPath          = null
        - Name              = "Plugins\\NewPlugin2\\Source\\DajeMod"
        - Parent            = "Plugins\\NewPlugin2\\Source" (VirtualFolder) [SolutionItem]
        - Text              = "DajeMod"
        - Type              = VirtualFolder
     */

    public struct FUnrealVSPlugin
    {
        public string PluginName { get; internal set; }
    }

    public struct FUnrealVSPluginModule
    {
        public string PluginName { get; set; }
        public string ModuleName { get; set; }
    }

    public struct FUnrealVSItem
    {
        private SolutionItem _item;
        public FUnrealVSItem(SolutionItem item)
        {
            _item = item;
        }

        public string FullPath { 
            get 
            { 
                if (IsFile) return _item.FullPath;
                else if (IsVirtualFolder)
                {
                   SolutionItem project = _item.FindParent(SolutionItemType.Project);
                   string prjPath = project.FullPath; //"C:\\_fdf\\workspace_unreal\\ws_unreal\\UENLOpt\\Intermediate\\ProjectFiles\\UENLOpt.vcxproj"
                                                       //if (project.Parent.Type == SolutionItemType.SolutionFolder && project.Parent.Name == "Games") 
                   string cleanedPath = XFilesystem.PathParent(prjPath, 3);

                   string fullPath = XFilesystem.PathCombine(cleanedPath, _item.Name);
                    return fullPath;
                } 
                else return _item.FullPath; 
            } 
        }
        public bool IsVirtualFolder { get { return _item.Type == SolutionItemType.VirtualFolder; } }
        public bool IsFile { get { return _item.Type == SolutionItemType.PhysicalFile; } }
        public bool IsProject { get { return _item.Type == SolutionItemType.Project; } }

        public string ProjectName { 
            get 
            { 
                if (IsProject) return _item.Name;
                return _item.FindParent(SolutionItemType.Project).Name;
            } 
        }
    }
}