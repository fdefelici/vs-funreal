using Community.VisualStudio.Toolkit;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        //public IXActionCmd Command { get; set; }
       
        public virtual Task DoActionAsync() 
        {
            return Task.CompletedTask;
        }  
    }
}
