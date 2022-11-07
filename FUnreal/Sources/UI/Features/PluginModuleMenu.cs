using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using EnvDTE;
using EnvDTE80;

namespace FUnreal
{
    [Command(VSCTSymbols.PluginModuleMenu)]
    public class PluginModuleMenu : XMenuCmd<PluginModuleMenu>
    {
        /*
        protected override void BeforeQueryStatus(EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.Command.Visible = false;

            DTE2 dTE2 = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE)) as DTE2;
            Debug.Print("Selected Count: {0}", dTE2.SelectedItems.Count);
            if (dTE2.SelectedItems.Count != 1) return;

            SelectedItem item = dTE2.SelectedItems.Item(1);
            if (item.Project != null) return;

            string fileName = item.Name;
            Debug.Print("Selected: {0}", fileName);

            if (!XFilesystem.HasExtension(fileName, ".Build.cs")) return;

            this.Command.Visible = true;
        }
        */

    }
}
