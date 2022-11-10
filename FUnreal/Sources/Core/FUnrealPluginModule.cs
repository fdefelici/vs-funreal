namespace FUnreal
{
    public class FUnrealPluginModule
    {
        private FUnrealPlugin plugin;
        
        public FUnrealPluginModule(FUnrealPlugin plugin, string moduleName)
        {
            this.plugin = plugin;
            Name = moduleName;
            FullPath = XFilesystem.PathCombine(plugin.SourcePath, moduleName);
            BuildFilePath = XFilesystem.PathCombine(FullPath, $"{moduleName}.Build.cs");
            PublicPath = XFilesystem.PathCombine(FullPath, "Public");
            ApiMacro = $"{moduleName.ToUpper()}_API";
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