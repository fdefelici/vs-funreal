using System.Threading.Tasks;

namespace FUnreal
{
    public interface IFUnrealBuildTool
    {
        string BinPath { get; }
        Task<XProcessResult> GenerateVSProjectFilesAsync(string uprojectFilePath);
    }

    public class FUnrealBuildTool : IFUnrealBuildTool
    {
        private IXProcess _process;

        public FUnrealBuildTool(string enginePath)
            : this(enginePath, new XProcess())
        {
        }

        public string BinPath { get; private set; }

        public FUnrealBuildTool(string ubtBinPath, IXProcess process)
        {
            _process = process;
            BinPath = ubtBinPath;
        }

        public async Task<XProcessResult> GenerateVSProjectFilesAsync(string uprojectFilePath)
        {
            XProcessResult result = await _process.RunAsync(BinPath, new string[] { 
                "-projectFiles", 
               $"-project=\"{uprojectFilePath}\"",
                "-game",
                "-rocket"
            });
            return result;
        }
    }
}
