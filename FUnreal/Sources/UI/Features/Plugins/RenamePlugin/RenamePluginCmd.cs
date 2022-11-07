using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System;

namespace FUnreal
{
    [Command(VSCTSymbols.RenamePluginCmd)]
    public class RenamePluginCmd : XActionCmd<RenamePluginCmd>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            Debug.Print(">>>> Rename Plugin Called");
            await Controller.DoActionAsync();
        }
    }
}
