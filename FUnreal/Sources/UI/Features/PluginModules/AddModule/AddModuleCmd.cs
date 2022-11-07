using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FUnreal
{
    [Command(VSCTSymbols.AddModuleCmd)]
    public class AddModuleCmd : XActionCmd<AddModuleCmd>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            Debug.Print(">>>> Add Module Called");
            await Controller.DoActionAsync();
        }
    }
}
