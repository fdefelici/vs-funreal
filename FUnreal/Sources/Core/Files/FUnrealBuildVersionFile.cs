namespace FUnreal.Sources.Core
{
    public class FUnrealBuildVersionFile : XJsonFile
    {
        public int MajorVersion { get; private set; }
        public int MinorVersion { get; private set; }
        public int PatchVersion { get; private set; }

        public FUnrealBuildVersionFile(string filePath) : base(filePath)
        {
            MajorVersion = (int)_json["MajorVersion"];
            MinorVersion = (int)_json["MinorVersion"];
            PatchVersion = (int)_json["PatchVersion"];
        }
    }
}
