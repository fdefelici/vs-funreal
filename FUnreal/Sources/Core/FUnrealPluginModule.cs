namespace FUnreal
{
    public class FUnrealPluginModule : IFUnrealModule
    {
        private FUnrealPlugin plugin;
        private FUnrealPlugin plug;
        private string modName;
        private string modFile;

        /*
        public FUnrealPluginModule(FUnrealPlugin plugin, string moduleName)
        {
            this.plugin = plugin;
            Name = moduleName;
            FullPath = XFilesystem.PathCombine(plugin.SourcePath, moduleName);
            BuildFilePath = XFilesystem.PathCombine(FullPath, $"{moduleName}.Build.cs");
            PublicPath = XFilesystem.PathCombine(FullPath, "Public");
            ApiMacro = $"{moduleName.ToUpper()}_API";
        }
        */

        public FUnrealPluginModule(FUnrealPlugin plug, string modName, string modFile)
        {
            plugin = plug;
            Name = modName;
            BuildFilePath = modFile;
            FullPath = XFilesystem.PathParent(modFile);
            PublicPath = XFilesystem.PathCombine(FullPath, "Public");
            ApiMacro = $"{Name.ToUpper()}_API";
        }

        public string Name { get; }
        public string FullPath { get; }
        public string BuildFilePath { get; }

        public bool Exists { get { return XFilesystem.FileExists(BuildFilePath); } }

        public string PublicPath { get; internal set; }
        public string ApiMacro { get; internal set; }

        public override bool Equals(object obj)
        {
            if (!(obj is FUnrealPluginModule)) return false;
            FUnrealPluginModule mod = obj as FUnrealPluginModule;
            return Name == mod.Name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}