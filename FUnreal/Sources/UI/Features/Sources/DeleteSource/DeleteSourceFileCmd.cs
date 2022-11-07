using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FUnreal
{
    [Command(VSCTSymbols.DeleteSourceFileCmd)]
    public class DeleteSourceFileCmd : XActionCmd<DeleteSourceFileCmd>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            Debug.Print(">>>> DeleteSourceFileCmd Called");
            await Controller.DoActionAsync();
        }
    }
}
