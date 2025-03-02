using System.Threading.Tasks;

namespace FUnreal
{
    public class FUnrealGenerateTool : IFUnrealBuildTool
    {
        private IXProcess _process;

        public string BinPath { get; private set; }
     
        public FUnrealGenerateTool(string binPath)
            : this(binPath, new XProcess())
        {
            BinPath = binPath;
        }

        public FUnrealGenerateTool(string ubtBinPath, IXProcess process)
        {
            _process = process;
            BinPath = ubtBinPath;
        }

        public async Task<XProcessResult> GenerateVSProjectFilesAsync(string uprojectFilePath)
        {
            XProcessResult result = await _process.RunAsync(BinPath, new string[]{});
            return result;
        }
    }
}