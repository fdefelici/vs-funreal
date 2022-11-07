using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System;

namespace FUnreal
{
    [Command(VSCTSymbols.AddPluginCmd)]
    public class AddPluginCmd : XActionCmd<AddPluginCmd>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            Debug.Print(">>>> Add Plugin Called");

            await Controller.DoActionAsync();
        }
    }
}
