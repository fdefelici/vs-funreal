using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System;

namespace FUnreal
{
    [Command(VSCTSymbols.AddSourceClassCmd)]
    public class AddSourceClassCmd : XActionCmd<AddSourceClassCmd>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            Debug.Print(">>>> Add Item Called");

            await Controller.DoActionAsync();
        }
    }
}
