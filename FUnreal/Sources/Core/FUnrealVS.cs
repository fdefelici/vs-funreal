using Community.VisualStudio.Toolkit;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;

namespace FUnreal
{
    public class FUnrealVS
    {
        public static Action OnSolutionLoad {           
            set
            {
                VS.Events.SolutionEvents.OnAfterBackgroundSolutionLoadComplete += value;
            }
        }

        public FUnrealVS()
        {

            /*
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

            VS.Events.SolutionEvents.OnAfterBackgroundSolutionLoadComplete += () =>
            {
                Debug.Print("-------------------------- NEW SOLUTION LOADED!!!!");
            };
            */
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

        private async Task<bool> AreAllSelectedItemsOfSameTypeAsync(SolutionItemType type)
        {
            var items = await VS.Solutions.GetActiveItemsAsync();

            foreach (var item in items)
            {
                if (item.Type != type) return false;
            }
            return true;
        }

        public async Task<bool> IsSelectCtxProjectNodeAsync()
        {
            return await AreAllSelectedItemsOfSameTypeAsync(SolutionItemType.Project);
        }

        public async Task<bool> IsSelectCtxItemNodeAsync()
        {
            return await AreAllSelectedItemsOfSameTypeAsync(SolutionItemType.PhysicalFile);
        }

        public async Task<bool> IsSelectCtxFolderNodeAsync()
        {
            return await AreAllSelectedItemsOfSameTypeAsync(SolutionItemType.VirtualFolder);
        }

        public async Task<bool> IsSelectCtxMiscNodeAsync()
        {
            var items = await VS.Solutions.GetActiveItemsAsync();
            if (items.Count() < 1) return false;

            bool atLeastOneFolder = false;
            bool atLeastOneFile = false;
            bool otherItemType = false;
            foreach (var item in items)
            {
                if (item.Type == SolutionItemType.PhysicalFile)
                {
                    atLeastOneFile = true;
                } 
                else if (item.Type == SolutionItemType.VirtualFolder)
                {
                    atLeastOneFolder = true;
                }
                else
                {
                    otherItemType = true;
                }
            }
            return atLeastOneFile && atLeastOneFolder && !otherItemType;
        }

        public async Task<bool> IsMultiSelectionAsync()
        {
            var items = await VS.Solutions.GetActiveItemsAsync();
            return items.Count() > 1;
        }

        public static bool IsUnrealSolution()
        {
            return ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                // Switch to main thread
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                // Do your work on the main thread here.

                var solution = await VS.Solutions.GetCurrentSolutionAsync();
                string solutionPath = solution.FullPath;
                string uprojectPath = XFilesystem.FileChangeExtension(solutionPath, "uproject");
                return XFilesystem.FileExists(uprojectPath);
            });

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

/*
     protected override void BeforeQueryStatus(EventArgs e)
     {
         ThreadHelper.ThrowIfNotOnUIThread();

         this.Command.Visible = false;

         DTE2 dTE2 = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE)) as DTE2;
         Debug.Print("Selected Count: {0}", dTE2.SelectedItems.Count);
         if (dTE2.SelectedItems.Count != 1) return;

         SelectedItem item = dTE2.SelectedItems.Item(1);
         if (item.Project != null) return;

         string fileName = item.Name;
         Debug.Print("Selected: {0}", fileName);

         if (!XFilesystem.HasExtension(fileName, ".Build.cs")) return;

         this.Command.Visible = true;
     }
     */


/*
       protected override void BeforeQueryStatus(EventArgs e)
       {
           ThreadHelper.ThrowIfNotOnUIThread();


           var guidSet  = VsMenus.guidCciSet;

           var guidProj = VsMenus.IDM_VS_CTXT_PROJNODE;


           Debug.WriteLine(e);

           this.Command.Visible = false;

           DTE2 dTE2 = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE)) as DTE2;

           var attrs = dTE2.ContextAttributes;


           Debug.Print("Selected Count: {0}", dTE2.SelectedItems.Count);
           if (dTE2.SelectedItems.Count != 1) return;

           SelectedItem item = dTE2.SelectedItems.Item(1);

           Debug.Print("Selected: {0}", item.Name);

           this.Command.Visible = true;

       }
       */