using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Diagnostics;
using System;
using EnvDTE;
using EnvDTE80;

namespace FUnreal
{
    [Command(VSCTSymbols.ProjectMenu)]
    public class ProjectMenu : BaseCommand<ProjectMenu>
    {
        protected override void BeforeQueryStatus(EventArgs e)
        {
            //await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            ThreadHelper.ThrowIfNotOnUIThread();

            this.Command.Visible = false;

            DTE2 dTE2 = Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(DTE)) as DTE2;
            Debug.Print("Selected Count: {0}", dTE2.SelectedItems.Count);
            if (dTE2.SelectedItems.Count != 1) return;

            SelectedItem item = dTE2.SelectedItems.Item(1);

            Debug.Print("Selected: {0}", item.Name);

            this.Command.Visible = true;

        }

    }
}
