using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System;
using System.Threading.Tasks;

namespace FUnreal
{
    public abstract class XContextMenuActionCmd<T> : XActionCmd<T>
        where T : class, new()
    {     
        protected override void BeforeQueryStatus(EventArgs e)
        {
            ThreadHelper.JoinableTaskFactory.Run(async () => await ContextMenuManager.Instance.ConfigureCommandAsync(this));
        }
    }
}
