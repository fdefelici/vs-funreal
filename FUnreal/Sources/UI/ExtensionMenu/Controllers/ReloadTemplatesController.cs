using System.Threading.Tasks;

namespace FUnreal
{
    public class ReloadTemplatesController : AXActionController
    {
        private bool _isOngoing;

        public ReloadTemplatesController(FUnrealService unrealService, FUnrealVS unrealVS) 
            : base(unrealService, unrealVS)
        {
            _isOngoing = false;
        }

        public override Task DoActionAsync()
        {
            if (_isOngoing) return Task.CompletedTask;

            _isOngoing = true;
            _unrealVS.StatusBar.ShowInfiniteProgress("FUnreal is updating templates ...");
            
            FUnrealTemplateLoader.UpdateTemplates(_unrealVS, _unrealService);
            
            _unrealVS.StatusBar.HideInfiniteProgress();
            _isOngoing = false;
            return Task.CompletedTask;
        }
    }
}
