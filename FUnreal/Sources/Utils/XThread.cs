using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{

    public static class XThread
    {
        public static async Task SwitchToUIThreadIfItIsNotAsync()
        {
            bool isUIThread = ThreadHelper.CheckAccess();
            if (!isUIThread)
            {
                XDebug.Info("Context Switch Required!");
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            }
        }

    }

}