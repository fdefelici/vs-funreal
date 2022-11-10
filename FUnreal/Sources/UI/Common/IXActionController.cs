using Community.VisualStudio.Toolkit;
using EnvDTE;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    public abstract class IXActionController
    {

        protected FUnrealService _unrealService;
        protected FUnrealVS _unrealVS;
        protected ContextMenuManager _ctxMenuMgr;

        public IXActionController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr)
        {
            _unrealService = unrealService;
            _unrealVS = unrealVS;
            _ctxMenuMgr = ctxMenuMgr;
        }

        public IXActionCmd Command { get; set; }
       
        public virtual Task DoActionAsync() 
        {
            return Task.CompletedTask;
        }  

        public virtual async Task<bool> ShouldBeVisibleAsync() 
        {
            return await _ctxMenuMgr.IsActiveAsync(Command.ID);
        } 
    }
}
