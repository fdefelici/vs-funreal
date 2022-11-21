using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    public abstract class XActionCmd<T> : BaseCommand<T>, IXActionCmd
        where T : class, new()
    {
        public static T Instance { get; internal set; }
        public IXActionController Controller { 
            get { return _controller; } 
            set 
            { 
                _controller = value;
                _controller.Command = this;
            } 
        }
        private IXActionController _controller;

        public int ID => Command.CommandID.ID;

        public bool Enabled { get { return Command.Enabled; } set { Command.Enabled = value; } }

        protected override Task InitializeCompletedAsync()
        {
            Instance = this as T;
            return Task.CompletedTask;
        }

        protected override void BeforeQueryStatus(EventArgs e)
        {
            //Better way to call async method from here, preserving call context?
            Command.Enabled = true;
            //Command.Visible = Controller.ShouldBeVisibleAsync().GetAwaiter().GetResult();  
            Command.Visible = ThreadHelper.JoinableTaskFactory.Run(async () => await Controller.ShouldBeVisibleAsync());
        }

        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return Controller.DoActionAsync();
        }
    }
}
