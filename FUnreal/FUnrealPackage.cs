using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using Community.VisualStudio.Toolkit;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.VisualStudio.RpcContracts.Logging;

namespace FUnreal
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(VSCTSymbols.PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class FUnrealPackage : ToolkitPackage
    {
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


            //Autoloading VSPackage if a file (.uproject) is present, seems doesn't exist....(very strage)
            //https://learn.microsoft.com/en-us/visualstudio/extensibility/how-to-use-rule-based-ui-context-for-visual-studio-extensions?view=vs-2022
            //As a workaround, stop loading the code if it is not a Unreal Project.
            //NOTE: This workaround works only if VS IDE is launched from scratch. 
            //      Don't work if from a VS IDE solution, it is opened another solution using Open -> Project/Solution (VSpackage is not reloaded)

            if (FUnrealVS.IsUnrealSolution())
            {
                FUnrealVS unrealVS = await FUnrealVS.CreateAsync();
                FUnrealService unrealService = FUnrealService.Create(unrealVS);
                if (unrealService == null)
                {
                    unrealVS.Output.Erro($"{XDialogLib.Title_FUnrealToolbox} failed to load!");
                    unrealVS.Output.ForceFocus();
                    unrealVS.ShowStatusBarMessage($"{XDialogLib.Title_FUnrealToolbox} fails. Please check {XDialogLib.Title_FUnrealToolbox} Output window!");
                    return;
                }

                ContextMenuManager ctxMenuMgr = new ContextMenuManager(unrealService, unrealVS);

                var projectLoadHandler = new ProjectReloadHandler(unrealService, unrealVS); //object instance kept alive by unrealVS
                unrealVS.OnUProjectLoadedAsync = projectLoadHandler.ExecuteAsync;


                await this.RegisterCommandsAsync();

                /* Configure Menu Commands */
                ToolboxMenu.Instance.Controller = new ToolboxMenuController(unrealService, unrealVS, ctxMenuMgr);

                AddPluginCmd.Instance.Controller = new AddPluginController(unrealService, unrealVS, ctxMenuMgr);
                DeleteSourceCmd.Instance.Controller = new DeleteSourceController(unrealService, unrealVS, ctxMenuMgr);
                AddModuleCmd.Instance.Controller = new AddModuleController(unrealService, unrealVS, ctxMenuMgr);
                DeletePluginCmd.Instance.Controller = new DeletePluginController(unrealService, unrealVS, ctxMenuMgr);
                RenamePluginCmd.Instance.Controller = new RenamePluginController(unrealService, unrealVS, ctxMenuMgr);
                RenameModuleCmd.Instance.Controller = new RenameModuleController(unrealService, unrealVS, ctxMenuMgr);
                DeleteModuleCmd.Instance.Controller = new DeleteModuleController(unrealService, unrealVS, ctxMenuMgr);
                AddSourceClassCmd.Instance.Controller = new AddSourceClassController(unrealService, unrealVS, ctxMenuMgr);
                AddSourceFileCmd.Instance.Controller = new AddSourceFileController(unrealService, unrealVS, ctxMenuMgr);
                RenameSourceFileCmd.Instance.Controller = new RenameSourceFileController(unrealService, unrealVS, ctxMenuMgr);

                AddGameModuleCmd.Instance.Controller = new AddGameModuleController(unrealService, unrealVS, ctxMenuMgr);
                RenameGameModuleCmd.Instance.Controller = new RenameGameModuleController(unrealService, unrealVS, ctxMenuMgr);
                DeleteGameModuleCmd.Instance.Controller = new DeleteGameModuleController(unrealService, unrealVS, ctxMenuMgr);


                unrealVS.Output.Info($"{XDialogLib.Title_FUnrealToolbox} Loaded!");
                unrealVS.ShowStatusBarMessage($"{XDialogLib.Title_FUnrealToolbox} Ready ;-)");

                //Simulate Project Loaded event at startup
                projectLoadHandler.ExecuteAsync().FireAndForget();

                XDebug.Info("Loaded");
            }
            else
            {
                XDebug.Info("Not Loaded! No Unreal project detected!");
            }
        }

    }
}
