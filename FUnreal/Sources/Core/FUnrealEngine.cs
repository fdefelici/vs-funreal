namespace FUnreal
{
    public class FUnrealEngine
    {
        public FUnrealEngine(XVersion version, string enginePath, IFUnrealBuildTool ubt)
        {
            Version = version;
            EnginePath = enginePath;
            UnrealBuildTool = ubt;
        }

        public XVersion Version { get; }
        public string EnginePath { get; }
        public IFUnrealBuildTool UnrealBuildTool { get; }
    }
}