using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using EnvDTE;
using EnvDTE80;
using System.Drawing.Printing;

namespace FUnreal
{
    [Command(VSCTSymbols.ToolboxMenu)]
    public class ToolboxMenu : XActionCmd<ToolboxMenu>
    {
        
    }
}
