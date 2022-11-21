using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using System;
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
        private DTE2 dTE2;
        public bool IsValid { get; internal set; }
        public string UENatvisPath { get; internal set; }

        private FUnrealDTE() { }

        public async Task InitializeAsync()
        {
            await XThread.SwitchToUIThreadIfItIsNotAsync();

            IsValid = false;

            dTE2 = Package.GetGlobalService(typeof(DTE)) as DTE2;
            if (dTE2 == null)
            {
                XDebug.Erro("DTE2: Failed to retrieve DTE2 service!");
                return;
            }

            UENatvisPath = FindUENatvisPath();
            _project = FindUEProject();

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

        public async Task<bool> ExistsSubpathFromSelectedFolderAsync(string[] parts)
        {
            await XThread.SwitchToUIThreadIfItIsNotAsync();


            if (dTE2.SelectedItems.Count != 1) return false;

            SelectedItem selected = dTE2.SelectedItems.Item(1);
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

            if (dTE2.SelectedItems.Count != 1) return false;

            SelectedItem selected = dTE2.SelectedItems.Item(1);
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

            if (dTE2.SelectedItems.Count != 1) return false;

            SelectedItem selected = dTE2.SelectedItems.Item(1);
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

            bool found = TryFindProjectItem(_project, parts, out ProjectItem item);

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

            SelectedItems selectedItems = dTE2.SelectedItems;
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

        private bool TryFindProjectItem(Project parent, string[] parts, out ProjectItem item)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            item = null;

            ProjectItems children = parent.ProjectItems;
            if (children == null) return false;
            if (parts.Count() == 0) return false;

            string first = parts[0];
            ProjectItem firstChild = null;
            foreach (ProjectItem prjChild in children)
            {
                if (prjChild.Name == first)
                {
                    firstChild = prjChild;
                    break;
                }
            }

            if (firstChild == null) return false;

            ProjectItem NextItem = firstChild;
            for (int i = 1; i < parts.Count(); ++i)
            {
                bool found = TryFindProjectItemChild(NextItem, parts[i], out item);
                if (!found) return false;
                NextItem = item;
            }

            return true;
        }

        private bool TryFindProjectItemChild(ProjectItem parent, string childName, out ProjectItem child)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            child = null;

            var children = parent.ProjectItems;
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

            foreach (Project project in dTE2.Solution.Projects)
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

            foreach (Project project in dTE2.Solution.Projects)
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


        //https://flylib.com/books/en/3.50.1.58/1/
        //https://learn.microsoft.com/it-it/dotnet/api/envdte.uihierarchyitems.expanded?view=visualstudiosdk-2022
        //https://stackoverflow.com/questions/12993960/visual-studio-2012-dte-change-focus-to-a-new-solution-item
        //By now, succed to expand folders to the item, but Select api don't work (Solution Explorer Scrollbar doesn't move to the item)
        public void TryToSelectSolutionExplorerItem()
        {

            UIHierarchy SolutionExplorerNode = dTE2.ToolWindows.SolutionExplorer;

            //NOTE: GetItem works only if item is expanded on Solution Explorer View.
            //UIHierarchyItem foundItem = SolutionExplorerNode.GetItem(@"UENLOpt\Games\UENLOpt\Source\UENLOpt\MyActor.h");

            XDebug.Info($"Hierarchy Items Count: {SolutionExplorerNode.UIHierarchyItems.Count}");

            foreach(UIHierarchyItem each in SolutionExplorerNode.UIHierarchyItems)
            {
                XDebug.Info($"Hierarchy item: {SolutionExplorerNode.UIHierarchyItems.Item(1).Name}");
            }
            /*
                Solution
                    -  Engine
                    -  Games
                        - Project
            */
            UIHierarchyItem solutionNode = SolutionExplorerNode.UIHierarchyItems.Item(1);
            UIHierarchyItem gamesNode = solutionNode.UIHierarchyItems.Item(2);
            UIHierarchyItem projectNode = gamesNode.UIHierarchyItems.Item(1);

            string[] parts = new string[] { "Source", "UENLOpt", "MyActor.h" };
            //string[] parts = new string[] { "Source", "UENLOpt" };

            UIHierarchyItem lastItem = projectNode;
            bool found = false;
            foreach (var part in parts)
            {
                found = false;
                foreach(UIHierarchyItem each in lastItem.UIHierarchyItems)
                {

                    if (!(each.Object is ProjectItem)) continue;
                    ProjectItem pItem = (ProjectItem)each.Object;
                    XDebug.Info($"Project item: {pItem.Name}");

                    string simpleName = pItem.Name; //even each.Name is good. Return the label of item

                    if (simpleName == part)
                    {
                        lastItem = each;
                        //Note: exploring UIHierarchyItems works only if parent node is "expanded" (so children are visible on the ui)
                        //Note2: Eventually could test UIHierarchyItems.Expanded and set Expanded to true?!
                        pItem.ExpandView(); 
                        found = true;
                        break;
                    }
                }

                if (!found) break;
            }


            if (!found) 
            {
                XDebug.Info($"Hierarchy item not found!");
                return;
            }

            XDebug.Info($"Hierarchy item found: {lastItem.Name}");

            //This seems to produce some effects and select Games/Project node
            //SolutionExplorerNode.SelectDown(vsUISelectionType.vsUISelectionTypeSetCaret, 1);
  
            lastItem.Select(vsUISelectionType.vsUISelectionTypeSetCaret);

            
        }

    }


}
