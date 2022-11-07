using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System;

namespace FUnreal
{
    [Command(VSCTSymbols.DeletePluginCmd)]
    public class DeletePluginCmd : XActionCmd<DeletePluginCmd>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Debug.Print(">>>> Remove Plugin Called");

            await Controller.DoActionAsync();
        }
    }
}
