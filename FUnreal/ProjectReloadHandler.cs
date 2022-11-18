using Community.VisualStudio.Toolkit;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FUnreal
{
    internal class ProjectReloadHandler
    {
        private FUnrealService unrealService;
        private FUnrealVS unrealVS;
        private FUnrealNotifier _notifier;
        private string _errorMsg;

        public ProjectReloadHandler(FUnrealService unrealService, FUnrealVS unrealVS)
        {
            this.unrealService = unrealService;
            this.unrealVS = unrealVS;
            _notifier = new FUnrealNotifier();
            _errorMsg = "";
            _notifier.OnSendMessage = (type, msg1, msg2) => 
            {
                if (type == FUnrealNotifier.MessageType.ERRO)
                {
                    _errorMsg = msg2;
                }
            };
        }

        public async Task ExecuteAsync()
        {
            _errorMsg = "";

            unrealVS.Output.Info($"Scanning {unrealService.ProjectName} project ...");
            bool success = await unrealService.UpdateProjectAsync(_notifier);
            if (!success)
            {
                unrealVS.Output.Erro($"{unrealService.ProjectName} scan failed! {_errorMsg}");
                unrealVS.Output.ForceFocus();

                string callToAct = $"Please fix the issue and regenerate VS Solution with UBT, before continuing to use {XDialogLib.Title_FUnrealToolbox}!";
                unrealVS.Output.Warn(callToAct);
                await VS.MessageBox.ShowErrorAsync($"Wrong UE project layout detected! {callToAct}", _errorMsg);
                return;
            }

            var project = unrealService.GetUProject();

            int plugCount = project.Plugins.Count;
            int moduCount = project.AllModules.Count;
            string message = $"{project.Name} project scan completed. Found {plugCount} plugins and {moduCount} modules.";

            unrealVS.Output.Info(message);
            XDebug.Info(message);
            return;
        }
    }
}