using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Diagnostics;

namespace FUnreal
{
    [Command(VSCTSymbols.DeleteSourceFolderCmd)]
    public class DeleteSourceFolderCmd : XActionCmd<DeleteSourceFolderCmd>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            Debug.Print(">>>> DeleteSourceFolderCmd Called");
            await Controller.DoActionAsync();
        }
    }
}
