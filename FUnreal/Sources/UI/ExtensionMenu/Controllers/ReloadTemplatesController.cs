using System.Threading.Tasks;

namespace FUnreal
{
    public class ReloadTemplatesController : AXActionController
    {
        public ReloadTemplatesController(FUnrealService unrealService, FUnrealVS unrealVS) 
            : base(unrealService, unrealVS)
        { }

        public override Task DoActionAsync()
        {
            FUnrealService.UpdateTemplates(_unrealVS, _unrealService);
            return Task.CompletedTask;
        }
    }
}
