using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System;

namespace FUnreal
{
    [Command(VSCTSymbols.RenameModuleCmd)]
    public class RenameModuleCmd : XActionCmd<RenameModuleCmd>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            Debug.Print(">>>> Rename Module Called");
            await Controller.DoActionAsync();
        }
    }
}
