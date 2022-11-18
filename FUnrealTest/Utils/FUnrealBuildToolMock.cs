using FUnreal;

namespace FUnrealTest
{
    public class FUnrealBuildToolMock : IFUnrealBuildTool
    {
        public FUnrealBuildToolMock()
        {
            Called = false;
            UProjectFilePath = "";
            BinPath = "";
        }

        public string BinPath { get; }


        public async Task<XProcessResult> GenerateVSProjectFilesAsync(string uprojectFilePath)
        {
            await Task.Run(() => { 
                Called = true;
                UProjectFilePath = uprojectFilePath;
            });
            XProcessResult result = new XProcessResult(0, "", true);
            return result;
        }

        public bool Called { get; internal set; }
        public string UProjectFilePath { get; internal set; }
    }
}