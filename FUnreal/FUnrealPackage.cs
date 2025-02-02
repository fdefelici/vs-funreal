using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using Task = System.Threading.Tasks.Task;
using Community.VisualStudio.Toolkit;
using System.Management;

namespace FUnreal
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuidString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideOptionPage(typeof(FUnrealTemplateOptionsPage_Provider.OptionPage), "FUnreal", "Templates", 0, 0, true)] //Where Resource ID should be stored?
    [ProvideProfile(typeof(FUnrealTemplateOptionsPage_Provider.OptionPage), "FUnreal", "Templates", 0, 0, true)]
    public sealed class FUnrealPackage : ToolkitPackage
    {
        public const string PackageGuidString = "43b90373-5388-42b6-9074-100c2b543eec";

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
            //As a workaround, stop loading the code if it is not an Unreal Project.
            //NOTE: This workaround works only if VS IDE is launched from scratch. 
            //      Don't work if from a VS IDE solution, it is opened another solution using File -> Open -> Project/Solution (VSpackage is not reloaded)

            if (await FUnrealVS.IsUnrealSolutionAsync())
            {
                FUnrealVS unrealVS = await FUnrealVS.CreateAsync(this);
                printTitle(unrealVS.Output);

                FUnrealService unrealService = FUnrealService.Create(unrealVS);
                if (unrealService == null)
                {
                    unrealVS.Output.Erro($"{XDialogLib.Title_FUnreal} failed to load!");
                    unrealVS.Output.ForceFocus();
                    unrealVS.StatusBar.ShowMessage($"{XDialogLib.Title_FUnreal} fails. Please check {XDialogLib.Title_FUnreal} Output window!");
                    return;
                }

                var projectLoadHandler = new ProjectReloadHandler(unrealService, unrealVS);    //object instance kept alive by unrealVS
                var emptyFolderHandler = new DetectEmptyFolderHandler(unrealService, unrealVS);//object instance kept alive by unrealVS

                unrealVS.AddProjectLoadedHandler(projectLoadHandler.ExecuteAsync);
                unrealVS.AddProjectLoadedHandler(emptyFolderHandler.ExecuteAsync);
                
                // Registers all Commands 
                await this.RegisterCommandsAsync();
                // Configure ContextMenu Commands
                ContextMenuManager ctxMenuMgr = new ContextMenuManager(unrealService, unrealVS);
                // Configure ExtensionMenu Commands
                ExtensionMenuManager extMenuMgr = new ExtensionMenuManager(unrealService, unrealVS);
                //Configure Option Page Action
                OptionPageManager optPageMgr = new OptionPageManager(unrealService, unrealVS);

                unrealVS.Output.Info($"{XDialogLib.Title_FUnreal} setup completed.");
                unrealVS.StatusBar.ShowMessage($"{XDialogLib.Title_FUnreal} is ready ;-)");

                //Simulate Project Loaded event at startup to launch the discovery
                await unrealVS.ForceLoadProjectEventAsync(); //eventually even FireAndForget

                XDebug.Info("Loaded");
            }
            else
            {
                XDebug.Info("Not Loaded! No Unreal project detected!");
            }
        }

        private void printTitle(IFUnrealLogger output)
        {
            //output.ForceFocus();
            string str = @"                                                            
 ███████╗██╗   ██╗███╗   ██╗██████╗ ███████╗ █████╗ ██╗     
 ██╔════╝██║   ██║████╗  ██║██╔══██╗██╔════╝██╔══██╗██║     
 █████╗  ██║   ██║██╔██╗ ██║██████╔╝█████╗  ███████║██║     
 ██╔══╝  ██║   ██║██║╚██╗██║██╔══██╗██╔══╝  ██╔══██║██║     
 ██║     ╚██████╔╝██║ ╚████║██║  ██║███████╗██║  ██║███████╗
 ╚═╝      ╚═════╝ ╚═╝  ╚═══╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝╚══════╝
                                                           ";
            output.PlainText(str);
        }
    }
}
