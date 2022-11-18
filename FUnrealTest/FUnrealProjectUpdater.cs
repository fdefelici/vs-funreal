using FUnreal;

namespace FUnrealTest
{
    internal class FUnrealProjectUpdater
    {
        private FUnrealProject project;
        private readonly object _lock = new object(); 

        public FUnrealProjectUpdater(FUnrealProject project)
        {
            this.project = project;
        }

        public void HandleCreatePluginFolder(string fullPath)
        {
            if (!XFilesystem.DirectoryExists(fullPath)) return;

            var plugFile = XFilesystem.FindFile(fullPath, false, "*.uplugin");
            if (plugFile == null) return;

            _CreatePluginWithLock(fullPath);
        }

        public void HandleCreatePluginFile(string fullPath)
        {
            if (!XFilesystem.FileExists(fullPath)) return;

            _CreatePluginWithLock(fullPath);
        }

        private void _CreatePluginWithLock(string fullPath)
        {
            string name = XFilesystem.GetFilenameNoExt(fullPath);
            if (project.Plugins.Exists(name)) return;

            //Checking Scenario where a second .uplugin file has been added 
            string path = XFilesystem.PathParent(fullPath);
            var files = XFilesystem.FindFiles(path, false, "*.uplugin");
            //if (files.Count > 1) return;


            lock (_lock)
            {
                if (project.Plugins.Exists(name)) return;

                var plug = new FUnrealPlugin(project, name, fullPath);
                project.Plugins.Add(plug);


                //Search Modules
            }
        }
        public void HandleDeletePluginFolder(string fullPath)
        {
            if (XFilesystem.DirectoryExists(fullPath)) return;

            lock (_lock) 
            { 
                var plug = project.Plugins.FindByPath(fullPath);
                if (plug == null) return;

                project.Plugins.Remove(plug);
            }
        }

        public void HandleRenamePluginFolder(string oldPath, string newPath)
        {
            if (!XFilesystem.DirectoryExists(newPath)) return;

            var plugFile = XFilesystem.FindFile(newPath, false, "*.uplugin");
            if (plugFile == null) return;

            string name = XFilesystem.GetFilenameNoExt(plugFile);
            var plug = project.Plugins[name];
            if (plug == null) return;

            if (plug.FullPath != oldPath) return;

            lock (_lock)
            {
                if (plug.FullPath != oldPath) return;
                plug.SetDescriptorFilePath(plugFile);
            }
        }
    }
}