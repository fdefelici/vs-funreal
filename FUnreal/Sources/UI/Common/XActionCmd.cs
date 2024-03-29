﻿using Community.VisualStudio.Toolkit;
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
        public AXActionController Controller { get; set; }
       
        public int ID => Command.CommandID.ID;
        public bool Enabled { get { return Command.Enabled; } set { Command.Enabled = value; } }
        public bool Visible { get { return Command.Visible; } set { Command.Visible = value; } }
        public string Label { get => Command.Text; set => Command.Text = value; }

        protected override Task InitializeCompletedAsync()
        {
            Instance = this as T;
            return Task.CompletedTask;
        }

        protected override void BeforeQueryStatus(EventArgs e)
        {   
            ThreadHelper.JoinableTaskFactory.Run(async () => await ContextMenuManager.Instance.ConfigureCommandAsync(this));
        }

        protected override Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            return Controller.DoActionAsync();
        }
    }
}
