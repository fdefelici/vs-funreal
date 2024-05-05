using System.Threading.Tasks;

namespace FUnreal
{
    public class OpenOptionsController : AXActionController
    {
        public OpenOptionsController(FUnrealService unrealService, FUnrealVS unrealVS) 
            : base(unrealService, unrealVS)
        { }

        public override Task DoActionAsync()
        {
            _unrealVS.ShowOptionPage();

            return Task.CompletedTask;
        }
    }
}
