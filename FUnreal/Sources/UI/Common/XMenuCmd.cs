using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    public abstract class XMenuCmd<T> : BaseCommand<T>
        where T : class, new()
    {
        public static T Instance { get; internal set; }
        public IXMenuController Controller { get; set; }

        protected override Task InitializeCompletedAsync()
        {
            Instance = this as T;
            return base.InitializeCompletedAsync();
        }

        protected override void BeforeQueryStatus(EventArgs e)
        {
            Command.Visible = Controller.ShouldBeVisible();
        }
    }

    public interface IXMenuController
    {
        bool ShouldBeVisible();
    }
}
