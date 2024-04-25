using Community.VisualStudio.Toolkit;
using EnvDTE;
using FUnreal.Sources.Core;
using Microsoft.VisualStudio.Shell;
using System.Security.Permissions;
using System.Threading.Tasks;

namespace FUnreal
{
	[Command(VSCTSymbols.CmdVsSln)]
	internal sealed class CommandVsSln : BaseCommand<CommandVsSln>
	{
		public static FUnrealService Service { get; set; }
		protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
		{
			await FUnrealServiceTasks.Project_RegenSolutionFilesAsync(Service.GetUProject(), Service.Engine.UnrealBuildTool, new FUnrealNotifier());
		}
	}
}
