using Community.VisualStudio.Toolkit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.Internal.VisualStudio.PlatformUI;


namespace FUnreal
{

    public abstract class IFUnrealVS
    {
        public abstract string GetUnrealEnginePath();
        public abstract string GetUProjectFilePath();
        public abstract string GetVSixDllPath ();

        public IFUnrealLogger Output { get; protected set; }

        public abstract FUnrealTemplateOptionsPage GetOptions();
        public Action OnOptionsSaved;

    }

    public interface IFUnrealLogger
    {
        void Info(string format, params string[] args);
        void Warn(string format, params string[] args);
        void Erro(string format, params string[] args);

        void ForceFocus();
        void PlainText(string str);
    }


    public class FUnrealLogger : IFUnrealLogger
    {

        private const string INFO = "INFO";
        private const string WARN = "WARN";
        private const string ERRO = "ERRO";

        private OutputWindowPane _pane;
        public FUnrealLogger(OutputWindowPane pane)
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
            string content = XString.Format(format, args);   

            string message = $"[{timestamp}][{type}] {content}";
            _pane.WriteLine(message);
        }

        public void ForceFocus()
        {
            _pane.ActivateAsync().FireAndForget();
        }

        public void PlainText(string text)
        {
            _pane.WriteLine(text);
        }
    }

    public class FUnrealVS : IFUnrealVS
    {
        FUnrealTemplateOptionsPage _options = null;

        public override FUnrealTemplateOptionsPage GetOptions()
        {
            return _options;
        }

        public void ShowOptionPage()
        {
            _package.ShowOptionPage(typeof(FUnrealTemplateOptionsPage_Provider.OptionPage));
        }

        public static async Task<bool> IsUnrealSolutionAsync()
        {
            /*
            return ThreadHelper.JoinableTaskFactory.Run(async delegate
            {
                // Switch to main thread
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                // Do your work on the main thread here.

                var solution = await VS.Solutions.GetCurrentSolutionAsync();
                string solutionPath = solution.FullPath;
                string uprojectPath = XFilesystem.ChangeFilePathExtension(solutionPath, "uproject");
                return XFilesystem.FileExists(uprojectPath);
            });
            */

            await XThread.SwitchToUIThreadIfItIsNotAsync();
            
            var solution = await VS.Solutions.GetCurrentSolutionAsync();
            if (solution == null) return false;
            string solutionPath = solution.FullPath;
            string uprojectPath = XFilesystem.ChangeFilePathExtension(solutionPath, "uproject");
            return XFilesystem.FileExists(uprojectPath);
        }

        public static async Task<FUnrealVS> CreateAsync(FUnrealPackage package)
        {
            var uvs = new FUnrealVS(package);
            await uvs.InitializeAsync();
            return uvs;
        }

        public List<Func<Task>> OnUProjectLoadedAsyncList; //using list instead of delegate, to have control on execution flow of the tasks

        public void AddProjectLoadedHandler(Func<Task> task)
        {
            OnUProjectLoadedAsyncList.Add(task);
        }
        

        //public IFUnrealLogger Output { get; private set; }

        private FUnrealDTE _unrealDTE;
        private FUnrealPackage _package;

        public string WhenProjectReload_MarkItemForSelection { get; set; }
        public List<string> WhenProjectReload_MarkItemsForCreation { get; set; }

        private FUnrealVS(FUnrealPackage package) 
        { 
            _package = package;
        }

        public async Task InitializeAsync()
        {
            OnUProjectLoadedAsyncList = new List<Func<Task>>();

            VS.Events.SolutionEvents.OnAfterLoadProject += OnAfterLoadProjectHandler;

            OutputWindowPane pane = await VS.Windows.CreateOutputWindowPaneAsync(XDialogLib.Title_FUnreal);
            Output = new FUnrealLogger(pane);

            _unrealDTE = await FUnrealDTE.CreateInstanceAsync();
            if (!_unrealDTE.IsValid)
            {
                XDebug.Erro("Invalid FUnrealDTE instance!");
            }

            WhenProjectReload_MarkItemForSelection = null;
            WhenProjectReload_MarkItemsForCreation = null;


            _options = FUnrealTemplateOptionsPage.Instance;
            _options.AddChangedHandler(() => OnOptionsSaved?.Invoke());
        }

        public async Task ForceLoadProjectEventAsync()
        {
            var projectName = GetUProjectName();
            await OnAfterLoadProjectNameHandlerAsync(projectName);
        }

        private void OnAfterLoadProjectHandler(Project project)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () => await OnAfterLoadProjectNameHandlerAsync(project.Name));
            //Or FireAndForget....
        }

        private async Task OnAfterLoadProjectNameHandlerAsync(string projectName)
        {
            XDebug.Info($"Project Loaded Event detected: {projectName}");

            if (projectName != GetUProjectName()) return;

            foreach (var task in OnUProjectLoadedAsyncList)
            {
                await task.Invoke();
            }

            await _unrealDTE.ReloadProjectAsync();

            await TryToCreateSolutionExplorerItemsAsync();

            TryToSelectSolutionExplorerItemAsync().FireAndForget();
        }

        private async Task TryToCreateSolutionExplorerItemsAsync()
        {
            XDebug.Info("MarkItemsForCreation: {0}", WhenProjectReload_MarkItemsForCreation == null ? "NULL" : WhenProjectReload_MarkItemsForCreation.Count.ToString());

            if (WhenProjectReload_MarkItemsForCreation == null) return;

            await XThread.SwitchToUIThreadIfItIsNotAsync();

            var paths = WhenProjectReload_MarkItemsForCreation;
            WhenProjectReload_MarkItemsForCreation = null;

            foreach (var eachPath in paths)
            {
                string uprojectPath = GetUProjectPath();
                string relItemPath = XFilesystem.PathSubtract(eachPath, uprojectPath);
                string[] parts = XFilesystem.PathSplit(relItemPath);
                await _unrealDTE.AddSubpathToProjectAsync(parts);
            }

        }

        public string GetUProjectName()
        {
            var filePath = GetUProjectFilePath();
            var fileName = XFilesystem.GetFilenameNoExt(filePath);
            return fileName;
        }

        private async Task TryToSelectSolutionExplorerItemAsync()
        {
            if (string.IsNullOrEmpty(WhenProjectReload_MarkItemForSelection)) return;
            
            int delayMillis = 1000; //Task delayed, because after Project Reload, VS automatically select the active project (so selection will be overidden)

            string uprojectPath = GetUProjectPath();
            string relItemPath  = XFilesystem.PathSubtract(WhenProjectReload_MarkItemForSelection, uprojectPath);
            string[] parts = XFilesystem.PathSplit(relItemPath);

            WhenProjectReload_MarkItemForSelection = null;
            await Task.Delay(delayMillis).ContinueWith(async (t) => await _unrealDTE.TryToSelectSolutionExplorerItemAsync(parts), TaskScheduler.Default);
        }


        public string GetSolutionFilePath()
        {
            return VS.Solutions.GetCurrentSolution().FullPath;
        }

        public override string GetUProjectFilePath()
        {
            string solAbsPath = GetSolutionFilePath();
            string uprjFilePath = XFilesystem.ChangeFilePathExtension(solAbsPath, "uproject");
            return uprjFilePath;
        }

        public string GetUProjectPath()
        {
            string filePath = GetUProjectFilePath();
            string uprjPath = XFilesystem.PathParent(filePath);
            return uprjPath;
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

    

        public override string GetUnrealEnginePath()
        {
            string natvisFilePath = _unrealDTE.UENatvisPath;

            string visualStudioDebuggingPath = XFilesystem.PathParent(natvisFilePath);
            string extrasPath = XFilesystem.PathParent(visualStudioDebuggingPath);
            string enginePath = XFilesystem.PathParent(extrasPath);

            return enginePath;
        }

        public void ShowStatusBarMessage(string format, params string[] args)
        {
            string message = XString.Format(format, args);
            VS.StatusBar.ShowMessageAsync(message).FireAndForget();
        }

        public async Task<bool> ExistsFolderInSelectedFolderParentAsync(string name)
        {
            return await _unrealDTE.ExistsFolderInSelectedFolderParentAsync(name);
        }

        public async Task<bool> ExistsSubpathFromSelectedFolderAsync(params string[] parts)
        {
            return await _unrealDTE.ExistsSubpathFromSelectedFolderAsync(parts);
        }

        public async Task<bool> AddSubpathToSelectedFolderAsync(string[] parts)
        {
            return await _unrealDTE.AddSubpathToSelectedFolderAsync(parts);
        }


        public async Task<bool> RemoveProjectItemByRelPathAsync(string relPathToProjectExcluded)
        {
            return await _unrealDTE.RemoveProjectItemByRelPathAsync(relPathToProjectExcluded);
        }

        /*
         * Remove folder from VS just in case empty folders exists (in this case ubt regeneration does't update Virtual Folder / Filters)
         */
        public async Task<bool> RemoveFoldersIfAnyInCurrentSelectionAsync()
        {
            return await _unrealDTE.RemoveFoldersFromSelectionAsync();  
        }

        public override string GetVSixDllPath()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().Location;
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



