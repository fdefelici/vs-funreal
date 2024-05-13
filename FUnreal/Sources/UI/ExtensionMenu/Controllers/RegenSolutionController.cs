using System.Threading.Tasks;

namespace FUnreal
{
    public class RegenSolutionController : AXActionController
    {
        private bool _IsRegenerationOnGoing;
        private FUnrealNotifier _notifier;

        public RegenSolutionController(FUnrealService unrealService, FUnrealVS unrealVS) 
            : base(unrealService, unrealVS)
        {
            _IsRegenerationOnGoing = false;
            _notifier = new FUnrealNotifier();
            _notifier.OnSendMessage += (type, context, message) =>
            {
                if (type == FUnrealNotifier.MessageType.INFO) _unrealVS.Output.Info(message);
                else if (type == FUnrealNotifier.MessageType.WARN) _unrealVS.Output.Warn(message);
                else _unrealVS.Output.Erro(message);
            };
        }
        //NOTE: If regeneration updated VS Solution, VS Reload Project Popup will appear.
        //      Then FUnrealVS reload project listner will be triggered and project scanning begin (looking for plugins and modules)
        public override async Task DoActionAsync()
        {
            _unrealVS.Output.ForceFocus();
            if (_IsRegenerationOnGoing) return;

            _IsRegenerationOnGoing = true;
            _ = await _unrealService.RegenSolutionFilesAsync(_notifier);
            _IsRegenerationOnGoing = false;
        }
    }
}
