using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using Community.VisualStudio.Toolkit;
using System.Diagnostics;
using System.Linq;

namespace FUnreal
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(FUnrealPackage.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class FUnrealPackage : ToolkitPackage
    {
        public const string PackageGuidString = "43b90373-5388-42b6-9074-100c2b543eec";

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initialization code that rely on services provided by VisualStudio.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
        /// <param name="progress">A provider for progress updates.</param>
        /// <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            // When initialized asynchronously, the current thread may be a background thread at this point.
            // Do any initialization that requires the UI thread after switching to the UI thread.
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            //System.Collections.Generic.IEnumerable<object> cmds = await this.RegisterCommandsAsync();
            //Debug.Print(">>>>>>>>> FUNREAL cmds: {0}", cmds.Count());
            await this.RegisterCommandsAsync();

            FUnrealService unrealService = FUnrealService.SetUp_OnUIThread();
            FUnrealVS unrealVS = new FUnrealVS();

            AddPluginCmd.Instance.Controller    = new AddPluginController(unrealService, unrealVS);
            DeletePluginCmd.Instance.Controller = new DeletePluginController(unrealService, unrealVS);
            RenamePluginCmd.Instance.Controller = new RenamePluginController(unrealService, unrealVS);
            AddModuleCmd.Instance.Controller    = new AddModuleController(unrealService, unrealVS);
            RenameModuleCmd.Instance.Controller = new RenameModuleController(unrealService, unrealVS);
            DeleteModuleCmd.Instance.Controller = new DeleteModuleController(unrealService, unrealVS);

            FolderMenu.Instance.Controller      = new FolderMenuController(unrealService, unrealVS);
            AddSourceClassCmd.Instance.Controller      = new AddSourceClassController(unrealService, unrealVS);
            DeleteSourceFolderCmd.Instance.Controller = new DeleteSourceController(unrealService, unrealVS);

            SourceFileMenu.Instance.Controller  = new SourceFileMenuController(unrealService, unrealVS);
            DeleteSourceFileCmd.Instance.Controller = new DeleteSourceController(unrealService, unrealVS);


            PluginModuleMenu.Instance.Controller = new PluginModuleMenuController(unrealService, unrealVS);

            Debug.Print(">>>>>>>>> FUNREAL LOADED!");
        }

        #endregion
    }
}
