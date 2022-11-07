using Community.VisualStudio.Toolkit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    public abstract class XActionCmd<T> : BaseCommand<T>
        where T : class, new()
    {
        public static T Instance { get; internal set; }
        public IXActionController Controller { get; set; }

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
}
