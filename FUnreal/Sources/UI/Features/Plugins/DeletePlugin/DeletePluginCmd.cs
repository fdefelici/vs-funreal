using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows;
using System;

namespace FUnreal
{
    [Command(VSCTSymbols.DeletePluginCmd)]
    public class DeletePluginCmd : XActionCmd<DeletePluginCmd>
    {

    }
}
