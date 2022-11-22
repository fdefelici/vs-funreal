using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.VCProjectEngine;
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
                if (_controller != null)
                {   //TODO: To delete this link. No more required by Controller to know about the Command
                    _controller.Command = this;
                }
            } 
        }
        private IXActionController _controller;

        public int ID => Command.CommandID.ID;

        public bool Enabled { get { return Command.Enabled; } set { Command.Enabled = value; } }

        public string Label { get => Command.Text; set => Command.Text = value; }

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
            //Command.Visible = ThreadHelper.JoinableTaskFactory.Run(async () => await Controller.ShouldBeVisibleAsync());
            Command.Visible = ThreadHelper.JoinableTaskFactory.Run(async () => await ContextMenuManager.Instance.IsActiveAsync(this));
        }

        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return Controller.DoActionAsync();
        }
    }
}
