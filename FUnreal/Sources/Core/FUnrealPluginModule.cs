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
            TargetFilePath = XFilesystem.PathCombine(FullPath, $"{moduleName}.Build.cs");
        }

        public string Name { get; }
        public string FullPath { get; }
        public string TargetFilePath { get; }

        public bool Exists { get { return XFilesystem.FileExists(TargetFilePath); } }

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