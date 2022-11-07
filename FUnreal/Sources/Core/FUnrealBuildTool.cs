using System.Threading.Tasks;

namespace FUnreal
{
    public interface IFUnrealBuildTool
    {
        Task<XProcessResult> GenerateVSProjectFilesAsync(string uprojectFilePath);
    }

    public class FUnrealBuildTool : IFUnrealBuildTool
    {
        private IXProcess _process;
        private string _ubtExecPath;

        public FUnrealBuildTool(string enginePath)
            : this(enginePath, new XProcess())
        {
        }

        public FUnrealBuildTool(string ubtBinPath, IXProcess process)
        {
            _process = process;
            _ubtExecPath = ubtBinPath;
        }

        public async Task<XProcessResult> GenerateVSProjectFilesAsync(string uprojectFilePath)
        {
            XProcessResult result = await _process.RunAsync(_ubtExecPath, new string[] { 
                "-projectFiles", 
               $"-project=\"{uprojectFilePath}\"",
                "-game",
                "-rocket"
            });
            return result;
        }
    }
}
