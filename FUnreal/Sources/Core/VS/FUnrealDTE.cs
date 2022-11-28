using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FUnreal
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread", Justification = "<Pending>")]
    public class FUnrealDTE
    {
        public static async Task<FUnrealDTE> CreateInstanceAsync()
        {
            var udte = new FUnrealDTE();
            await udte.InitializeAsync();
            return udte;
        }


        private Project _project;
        private DTE2 _DTE2;
        public bool IsValid { get; internal set; }
        public string UENatvisPath { get; internal set; }

        private FUnrealDTE() { }

        public async Task InitializeAsync()
        {
            await XThread.SwitchToUIThreadIfItIsNotAsync();

            IsValid = false;

            _DTE2 = Package.GetGlobalService(typeof(DTE)) as DTE2;
            if (_DTE2 == null)
            {
                XDebug.Erro("DTE2: Failed to retrieve DTE2 service!");
                return;
            }

            UENatvisPath = FindUENatvisPath();
            await ReloadProjectAsync();   

            if (UENatvisPath == null)
            {
                XDebug.Erro("DTE2: Failed to retrieve UE NatVis path!");
                return;
            }
            if (_project == null)
            {
                XDebug.Erro("DTE2: Failed to retrieve UE Project!");
                return;
            }

            IsValid = true;
        }

        public async Task ReloadProjectAsync()
        {
            await XThread.SwitchToUIThreadIfItIsNotAsync();
            _project = FindUEProject();
        }

        public async Task<bool> ExistsSubpathFromSelectedFolderAsync(string[] parts)
        {
            await XThread.SwitchToUIThreadIfItIsNotAsync();


            if (_DTE2.SelectedItems.Count != 1) return false;

            SelectedItem selected = _DTE2.SelectedItems.Item(1);
            ProjectItem nextItem = selected.ProjectItem;

            foreach (var part in parts)
            {
                bool found = TryFindProjectItemChild(nextItem, part, out ProjectItem outItem);
                if (!found) return false;

                nextItem = outItem;
            }
            return true;
        }

        public async Task<bool> ExistsFolderInSelectedFolderParentAsync(string name)
        {
            await XThread.SwitchToUIThreadIfItIsNotAsync();

            if (_DTE2.SelectedItems.Count != 1) return false;

            SelectedItem selected = _DTE2.SelectedItems.Item(1);
            ProjectItem prjItem = selected.ProjectItem;
            if (prjItem == null) return false;

            object parentObj = prjItem.Collection?.Parent;
            if (parentObj == null) return false;
            if (!(parentObj is ProjectItem)) return false;

            ProjectItem parentItem = (ProjectItem)parentObj;

            bool found = TryFindProjectItemChild(parentItem, name, out ProjectItem outItem);
            return found;
        }


        public async Task<bool> AddSubpathToSelectedFolderAsync(string[] parts)
        {
            await XThread.SwitchToUIThreadIfItIsNotAsync();

            if (_DTE2.SelectedItems.Count != 1) return false;

            SelectedItem selected = _DTE2.SelectedItems.Item(1);
            ProjectItem nextItem = selected.ProjectItem;

            foreach (var part in parts)
            {
                bool found = TryFindProjectItemChild(nextItem, part, out ProjectItem outItem);
                if (found)
                {
                    nextItem = outItem;
                    continue;
                }

                var newItem = nextItem.ProjectItems.AddFolder(part, VSConstants.ItemTypeGuid.VirtualFolder_string);
                nextItem = newItem;

                //Note: In this case is not needed to Expand the nodes (with ProjectItem.ExpandView() or UIHierarchyItem.Items.Expanded)
                //      because in the "Selection" all the path was expanded by the user
            }

            //Filters (virtual folders) need to be saved otherwise when closing the project
            // a popup asking to Save filters appear. And if you don't save, when reopen the project filters want be there
            //var project = selected.ProjectItem.ContainingProject;
            _project.Save();
            return true;
        }

        public async Task<bool> RemoveProjectItemByRelPathAsync(string relPathToProjectExcluded)
        {
            await XThread.SwitchToUIThreadIfItIsNotAsync();

            string[] parts = XFilesystem.PathSplit(relPathToProjectExcluded);

            bool found = TryFindProjectItemBySubPath(_project, parts, out ProjectItem item);

            if (!found) return false;

            item?.Remove();

            _project.Save();
            return true;
        }

        /*
        * Remove folder from VS just in case empty folders exists (in this case ubt regeneration does't update Virtual Folder / Filters)
        * By the fact, works on currently Selected Items, should check for related ProjectItem validity (not null), because
        * could have been already removed by other api such as RemoveProjectItemByRelPathAsync
        */
        public async Task<bool> RemoveFoldersFromSelectionAsync()
        {
            await XThread.SwitchToUIThreadIfItIsNotAsync();

            SelectedItems selectedItems = _DTE2.SelectedItems;
            if (selectedItems.Count == 0) return false;

            bool hasRemovedSomething = false;
            foreach (SelectedItem eachSelected in selectedItems)
            {
                ProjectItem eachItem = eachSelected.ProjectItem;
                if (eachItem == null) continue;  //Check if project item still exists from the selection
                if (eachItem.Kind == VSConstants.ItemTypeGuid.VirtualFolder_string)
                {
                    eachItem.Remove();
                    hasRemovedSomething = true;
                }
            }

            if (hasRemovedSomething)
            {
                _project?.Save();
            }
            return true;
        }

        private bool TryFindProjectItemBySubPath(object parentAsObj, string[] subPath, out ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            item = null;

            object NextItem = parentAsObj;
            for (int i = 0; i < subPath.Count(); ++i)
            {
                bool found = TryFindProjectItemChild(NextItem, subPath[i], out item);
                if (!found) return false;
                NextItem = item;
            }
            return true;
        }

        private bool TryFindProjectItemChild(object parentAsObj, string childName, out ProjectItem child)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            child = null;

            ProjectItems children = GetProjectItemsFrom(parentAsObj);
            if (children == null) return false;

            foreach (ProjectItem each in children)
            {
                if (each.Name == childName)
                {
                    child = each;
                    return true;
                }
            }
            return false;
        }

        private Project FindUEProject()
        {
            XDebug.Info("Looking for DTE Project...");

            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (Project project in _DTE2.Solution.Projects)
            {
                if (project.Name.Equals("Games", StringComparison.OrdinalIgnoreCase))
                {
                    if (project.ProjectItems.Count == 1)
                    {
                        ProjectItem item = project.ProjectItems.Item(1); //Collection 1-based (not starting from 0!!!)
                        Project SubPrj = item.SubProject;
                        if (SubPrj == null) return null;

                        XDebug.Info("Item: {0}", item.Name);
                        XDebug.Info("Full: {0}", SubPrj.FullName);
                        XDebug.Info("Name: {0}", SubPrj.Name);
                        XDebug.Info("File: {0}", SubPrj.FileName);
                        XDebug.Info("Kind: {0}", SubPrj.Kind);
                        return SubPrj;
                    }
                }
            }
            return null;
        }

        private string FindUENatvisPath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach (Project project in _DTE2.Solution.Projects)
            {
                /*
                Debug.Print("Project Full: {0}", project.FullName);
                Debug.Print("Project Name: {0}", project.Name);
                Debug.Print("Project File: {0}", project.FileName);
                Debug.Print("        Kind: {0}", project.Kind);
                Debug.Print("       Items: {0}", project.ProjectItems.Count);
                */

                //Microsoft.VisualStudio.CommonIDE.Solutions
                //DteMiscProject
                if (project.Name.Equals("Visualizers", StringComparison.OrdinalIgnoreCase))
                {
                    if (project.ProjectItems.Count != 1) return null;
                    ProjectItem item = project.ProjectItems.Item(1); //Collection 1-based (not starting from 0!!!)

                    if (item.FileCount != 1) return null;
                    string absFilePath = item.FileNames[1];    //Collection 1-based (not starting from 0!!!)
                    Debug.Print(" Natvis file path: {0}", absFilePath);

                    return absFilePath;
                }
            }

            return null;
        }


        //https://stackoverflow.com/questions/12993960/visual-studio-2012-dte-change-focus-to-a-new-solution-item
        //Succeded to expand folders to the item, but Select api don't work (Solution Explorer Scrollbar doesn't move to the item)
        //This was because calling this methond On Project Loaded, VS first select the Project (And so my selection was overriden)
        //Trick to make selection work, is to call this method with a Delay after reload.

        //Example of api, to select multiple elment (like shift+selection). Not useful now, but just to keep it.
        //SolutionExplorerNode.SelectDown(vsUISelectionType.vsUISelectionTypeSetCaret, 1);
        public async Task TryToSelectSolutionExplorerItemAsync(string[] relPathAfterProjectName) 
        {
            await XThread.SwitchToUIThreadIfItIsNotAsync();

            XDebug.Info($"Selecting Subpath to project {XFilesystem.ToPath(relPathAfterProjectName)} ...");

            //NOTE: GetItem works only if item is expanded on Solution Explorer View (otherwise throws exception)
            //UIHierarchyItem foundItem = SolutionExplorerNode.GetItem(@"UENLOpt\Games\UENLOpt\Source\UENLOpt\MyActor.h");
           
            //string[] parts = new string[] { "Source", "UENLOpt", "MyActor.h" };

            UIHierarchyItem lastItem = GetProjectUIHierarchItem();
            if (lastItem == null)
            {
                XDebug.Erro("Projct not found! Selection process skipped!");
                return;
            }

            bool found = false;
            foreach (var part in relPathAfterProjectName)
            {
                found = TryFindUIHierItemChild(lastItem, part, out UIHierarchyItem child);
                if (!found) break;
                
                child.UIHierarchyItems.Expanded = true;
                lastItem = child;
            }
            if (!found)
            {
                XDebug.Info($"Hierarchy item not found for: {XFilesystem.ToPath(relPathAfterProjectName)}");
                return;
            }

            lastItem.Select(vsUISelectionType.vsUISelectionTypeSelect); //vsUISelectionType.vsUISelectionTypeSetCaret: To just move the scrollbar to the item, without selection
            XDebug.Info($"Selected project subpath: {XFilesystem.ToPath(relPathAfterProjectName)}");
        }

        private UIHierarchyItem GetProjectUIHierarchItem()
        {
            /* SolutionExplorer
                - Solution
                   -  Engine
                   -  Games
                       - Project
           */

            UIHierarchy SolutionExplorerNode = _DTE2.ToolWindows.SolutionExplorer;
            if (!SolutionExplorerNode.UIHierarchyItems.Expanded) SolutionExplorerNode.UIHierarchyItems.Expanded = true;
            if (SolutionExplorerNode.UIHierarchyItems.Count == 0) return null;
  
            UIHierarchyItem solutionNode = SolutionExplorerNode.UIHierarchyItems.Item(1);
            if (!solutionNode.UIHierarchyItems.Expanded) solutionNode.UIHierarchyItems.Expanded = true;
            if (solutionNode.UIHierarchyItems.Count == 0) return null;

            UIHierarchyItem gamesNode = solutionNode.UIHierarchyItems.Item(2);
            if (!gamesNode.UIHierarchyItems.Expanded) gamesNode.UIHierarchyItems.Expanded = true;
            if (gamesNode.UIHierarchyItems.Count == 0) return null;

            UIHierarchyItem projectNode = gamesNode.UIHierarchyItems.Item(1);
            if (!projectNode.UIHierarchyItems.Expanded) projectNode.UIHierarchyItems.Expanded = true;
            if (projectNode.UIHierarchyItems.Count == 0) return null;

            return projectNode;
        }

        //NOTE: This method use a mix of UIHierarchyItem and ProjectItem api. 
        //      Makes everything with just ProjectItem was possible, (very important was to call ProjectItem.ExpandView(), before AddFolder, because otherwise even if the folder was properly added, when clicking on the node in VS Solution Explorer the UI freeze!!!)
        //      but we ended up with all the folder tree expanded in Solution Explorer. (and ProjectItem haven't api to check Expanded state and also to Collapse the item)
        //      The api needed to check Expanded stante and Collapse are only in UIHierarchyItem, so in the end:
        //      - UIHierarchyItem used to navigate the three (expanding nodes) and restore them in original expanded state state
        //      - ProjectItem to create the Folder
        public async Task AddSubpathToProjectAsync(string[] relPathAfterProjectName)
        {
            await XThread.SwitchToUIThreadIfItIsNotAsync();

            XDebug.Info($"Adding Subpath to project {XFilesystem.ToPath(relPathAfterProjectName)} ...");
            //string[] parts = new string[] { "Source", "UENLOpt", "MyActor.h" };

            UIHierarchyItem lastItem = GetProjectUIHierarchItem();
            if (lastItem == null)
            {
                XDebug.Erro($"Cannot find Project UIHierarchyItem!");
                return;
            }

            UIHierarchyItem firstNotExpanded = null;
            bool added = false;
            foreach (var part in relPathAfterProjectName)
            {
                bool originalExpanded = lastItem.UIHierarchyItems.Expanded;
                bool found = TryFindUIHierItemChild(lastItem, part, out UIHierarchyItem child);
                if (found)
                {
                    if (firstNotExpanded == null && !originalExpanded)
                    {
                        firstNotExpanded = lastItem;
                    }

                    lastItem = child;
                } else
                {
                    //Adding folder to the project item
                    var newPItem = GetProjectItemsFrom(lastItem.Object).AddFolder(part, VSConstants.ItemTypeGuid.VirtualFolder_string);
                    //newPItem.Save(); ProjectItem cannot be save. Raise un unsupported method exception fot the ProjectItem. Maybe depends on the fact that it is a Virtual Folder
                    added = true;

                    //Look for the related UIHierarchyItem to continue the search (something like projectItem.GetUIHierItem doesn't exist)
                    bool childFound = TryFindUIHierItemChild(lastItem, part, out UIHierarchyItem newChild);
                    if (childFound)
                    {
                        lastItem = newChild;
                    } else
                    {
                        //shold not happen
                        XDebug.Erro($"For the ProjectItem '{part}' just added, was not found the respective UIHierarchyItem!");
                        return;
                    }
                }
            } 
            
            if (firstNotExpanded != null)
            {
                //by the fact the only way to explor UIHierarchyItem is to expand them, 
                //just restoring the first unexpanded to its original value. (so the search doesn't have impact on the user)
                firstNotExpanded.UIHierarchyItems.Expanded = false;
            }

            if (added)
            {
                _project.Save();
                XDebug.Info($"Added Subpath to project: {XFilesystem.ToPath(relPathAfterProjectName)}");
            }
            else
            {
                XDebug.Info($"Subpath already exists: {XFilesystem.ToPath(relPathAfterProjectName)}");
            }
            return;
        }

        //Not Used, but could be useful in the future.
        private bool TryFindUIHierItems(string[] relPathAfterProject, out List<UIHierarchyItem> uiItems)
        {
            UIHierarchyItem lastItem = GetProjectUIHierarchItem();

            var originalExpandState = new List<bool>();
            var uiItemParents = new List<UIHierarchyItem>();

            bool found = false;
            foreach(var part in relPathAfterProject) 
            {
                found = false;
                bool originalExpanded = lastItem.UIHierarchyItems.Expanded;
                if (!lastItem.UIHierarchyItems.Expanded)
                {
                    lastItem.UIHierarchyItems.Expanded = true;
                }
                foreach (UIHierarchyItem each in lastItem.UIHierarchyItems)
                {
                    if (each.Name == part)
                    {
                        uiItemParents.Add(lastItem);
                        originalExpandState.Add(originalExpanded);
                        lastItem = each;
                        found = true;
                        break;
;                   }
                    lastItem.UIHierarchyItems.Expanded = false;
                }
            }
            
            //restore expansion
            for(int i = uiItemParents.Count-1; i >=0; i--)
            {
                uiItemParents[i].UIHierarchyItems.Expanded = originalExpandState[i];
            }

            if (!found)
            {
                uiItems = null;
            } else
            {
                uiItems = uiItemParents;
                uiItems.Add(lastItem);
            }
            return found;
        }

        //Note: exploring UIHierarchyItems works only if parent node is "expanded" (so children are visible on the ui)
        private bool TryFindUIHierItemChild(UIHierarchyItem parent, string childName, out UIHierarchyItem child)
        {
            if (!parent.UIHierarchyItems.Expanded)
            {
                parent.UIHierarchyItems.Expanded = true;
            }

            foreach (UIHierarchyItem each in parent.UIHierarchyItems)
            {
                if (each.Name == childName)
                {
                    child = each;
                    return true;
                }
            }
            child = null;
            return false;
        }

        
        //This method is needed just because Project and ProjectItem haven't a common interface, but object
        private static ProjectItems GetProjectItemsFrom(object aProjectOrProjectItem)
        {
            if (aProjectOrProjectItem is Project)
            {
                return ((Project)aProjectOrProjectItem).ProjectItems;
            }
            else if (aProjectOrProjectItem is ProjectItem)
            {
                return ((ProjectItem)aProjectOrProjectItem).ProjectItems;
            }
            return null;
        }
    }

}


/* SOME DTE TRIES

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

/*
 
  DTE2 dTE2 = Package.GetGlobalService(typeof(DTE)) as DTE2;

Solution sol = dTE2.Solution;
Debug.Print("SOLUTION NAME: {0}", sol.FullName);
Debug.Print("SOLUTION FILENAME: {0}", sol.FileName);
Debug.Print("Projects Count: {0}", sol.Projects.Count);

string solAbsPath = sol.FileName;
            
string uprjFilePath = XFilesystem.FileChangeExtension(solAbsPath, "uproject");
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
                
        //  Skip:
        // - "Engine" Folder project and related UEXX subproject
        //

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

                string visualStudioDebuggingPath = XFilesystem.PathParent(absFilePath);
                string extrasPath = XFilesystem.PathParent(visualStudioDebuggingPath);
                enginePath = XFilesystem.PathParent(extrasPath);

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
*/


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

VS.Events.ProjectItemsEvents.AfterAddProjectItems += (items) => 
{
    Debug.Print("++++++++++++++++++++++++ ITEM ADDED!");
    foreach (var item in items)
    {
        Debug.Print(item.FullPath);
    }
};
*/