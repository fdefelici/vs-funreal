using Community.VisualStudio.Toolkit;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using System.IO;
using Microsoft.VisualStudio.Threading;
using Microsoft.Internal.VisualStudio.PlatformUI;
using EnvDTE;
using EnvDTE80;
using FUnreal;

namespace FUnreal
{

    public class FUnrealLogger
    {

        private const string INFO = "INFO";
        private const string WARN = "WARN";
        private const string ERRO = "ERRO";

        private Community.VisualStudio.Toolkit.OutputWindowPane _pane;
        public FUnrealLogger(Community.VisualStudio.Toolkit.OutputWindowPane pane)
        {
            _pane = pane;
        }

        public void Info(string format, params string[] args)
        {
            WriteMessage(INFO, format, args);
        }

        public void Warn(string format, params string[] args)
        {
            WriteMessage(WARN, format, args);
        }

        public void Erro(string format, params string[] args)
        {
            WriteMessage(ERRO, format, args);
        }

        public void WriteMessage(string type, string format, params string[] args) 
        { 
            string timestamp = DateTime.Now.ToString(@"yyyy-MM-dd hh:mm:ss");
            string content = string.Format(format, args);   

            string message = $"[{timestamp}][{type}] {content}";
            _pane.WriteLine(message);
        }

        public void ForceFocus()
        {
            _pane.ActivateAsync().FireAndForget();
        }
    }

    public class FUnrealVS
    {
        public static async Task<FUnrealVS> CreateAsync()
        {
            var uvs = new FUnrealVS();
            await uvs.InitializeAsync();
            return uvs;
        }

        public Func<Task> OnUProjectLoadedAsync;

        public FUnrealLogger Output { get; private set; }

        private FUnrealVS() { }

        public async Task InitializeAsync()
        {
            VS.Events.SolutionEvents.OnAfterLoadProject += (project) =>
            {
                Debug.Print($"----------------- PROJECT LOADED: {project.Name}");

                string uprjFilePath = GetUProjectFilePath();
                string fileName = XFilesystem.GetFilenameNoExt(uprjFilePath);

                if (project.Name != fileName) return;

                OnUProjectLoadedAsync?.Invoke().FireAndForget();
            };


            Community.VisualStudio.Toolkit.OutputWindowPane pane = await VS.Windows.CreateOutputWindowPaneAsync(XDialogLib.Title_FUnrealToolbox);

            Output = new FUnrealLogger(pane);
        }




        public string GetSolutionFilePath()
        {
            return VS.Solutions.GetCurrentSolution().FullPath;
        }

        public string GetUProjectFilePath()
        {
            string solAbsPath = GetSolutionFilePath();
            string uprjFilePath = XFilesystem.FileChangeExtension(solAbsPath, "uproject");
            return uprjFilePath;
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

        public string GetUnrealEnginePath()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE2 dTE2 = Package.GetGlobalService(typeof(DTE)) as DTE2;

            foreach (EnvDTE.Project project in dTE2.Solution.Projects)
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

                    string visualStudioDebuggingPath = XFilesystem.PathParent(absFilePath);
                    string extrasPath = XFilesystem.PathParent(visualStudioDebuggingPath);
                    string enginePath = XFilesystem.PathParent(extrasPath);

                    Debug.Print(" Engine Path: {0}", enginePath);
                    return enginePath;
                }
            }

            return null;
        }

        public void ShowStatusBarMessage(string format, params string[] args)
        {
            string message = string.Format(format, args);
            VS.StatusBar.ShowMessageAsync(message).FireAndForget();
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
/*
watcher = new FileSystemWatcher();

watcher.Path = "C:\\_fdf\\workspace_unreal\\ws_unreal\\UENLOpt";
watcher.IncludeSubdirectories = true;
watcher.Filter = "*.Build.cs";
watcher.NotifyFilter = NotifyFilters.Attributes
                    | NotifyFilters.CreationTime
                    | NotifyFilters.DirectoryName
                    | NotifyFilters.FileName
                    | NotifyFilters.LastAccess
                    | NotifyFilters.LastWrite
                    | NotifyFilters.Security
                    | NotifyFilters.Size;
watcher.EnableRaisingEvents = true;
watcher.Created += OnCreated;
//watcher.Changed += OnChanged;
watcher.Renamed += OnRenamed;
watcher.Deleted += OnDeleted;
*/

/*
_pluginFilesWatcher = new XFSWatcher();
_pluginFilesWatcher.Path = prjPath;
_pluginFilesWatcher.Filter = "*.uplugin";
_pluginFilesWatcher.NotifyFilter = NotifyFilters.FileName; //| NotifyFilters.Security; //Security trigger OnChanged on CTRL+Z
_pluginFilesWatcher.IncludeSubdirectories = true;

_pluginFilesWatcher.OnCreated = (path) => { Debug.Print($"PLG CREATED: {path}"); };
_pluginFilesWatcher.OnDeleted = (path) => { Debug.Print($"PLG DELETED: {path}"); };
_pluginFilesWatcher.OnRenamed = (oldPath, newPath) => { Debug.Print($"PLG RENAMED: {oldPath} TO: {newPath}"); };

_pluginFilesWatcher.Start();
*/
/*
VS.Events.SolutionEvents.OnAfterBackgroundSolutionLoadComplete += () =>
{
    Debug.Print("----------------- SOLUTION LOADED!");
};
*/
