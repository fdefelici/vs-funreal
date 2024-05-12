using System.Threading.Tasks;

namespace FUnreal
{
    public abstract class AXActionController
    {
        protected FUnrealService _unrealService;
        protected FUnrealVS _unrealVS;

        public AXActionController(FUnrealService unrealService, FUnrealVS unrealVS)
        {
            _unrealService = unrealService;
            _unrealVS = unrealVS;
        }

        public virtual Task DoActionAsync() 
        {
            return Task.CompletedTask;
        }  
    }
}
