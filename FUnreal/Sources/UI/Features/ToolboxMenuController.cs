using System;
using System.Threading.Tasks;

namespace FUnreal
{
    public class ToolboxMenuController : IXActionController
    {
        public ToolboxMenuController(FUnrealService unrealService, FUnrealVS unrealVS, ContextMenuManager ctxMenuMgr) 
            : base(unrealService, unrealVS, ctxMenuMgr) { }
    }
}