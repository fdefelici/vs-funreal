namespace FUnreal
{
    public class XTPL_DescriptorModel 
    {
        [XNotNullAttrValidator]
        public string version = null;

        [XNotNullAttrValidator]
        public XTPL_TemplatesModel templates = null;
    }

    public class XTPL_TemplatesModel
    {
        public XTPL_PluginModel[] plugins = new XTPL_PluginModel[0];
        public XTPL_PluginModuleModel[] plugin_modules = new XTPL_PluginModuleModel[0];
        public XTPL_GameModuleModel[] game_modules = new XTPL_GameModuleModel[0];
        public XTPL_ClassModel[] classes = new XTPL_ClassModel[0];
    }
  
    public class XTPL_PluginModel
    {
        public class XTPL_PluginMetaModel
        {
            public bool has_module = false;
        }

        [XNotNullAttrValidator]
        public string path = null;
        [XNotNullAttrValidator]
        public string[] ue = null;
        [XNotNullAttrValidator]
        public string label = null;
        [XNotNullAttrValidator]
        public string desc = null;
        [XNotNullAttrValidator]
        public XTPL_PluginMetaModel meta = null;
    }

    public class XTPL_PluginModuleModel
    {
        public class XTPL_ModulePluginMetaModel
        {
            [XNotNullAttrValidator]
            public string type = null;
            [XNotNullAttrValidator]
            public string phase = null;
        }

        [XNotNullAttrValidator]
        public string path = null;
        [XNotNullAttrValidator]
        public string[] ue = null;
        [XNotNullAttrValidator]
        public string label = null;
        [XNotNullAttrValidator]
        public string desc = null;
        [XNotNullAttrValidator]
        public XTPL_ModulePluginMetaModel meta = null;
    }

    public class XTPL_GameModuleModel
    {
        public class XTPL_GameModuleMetaModel
        {
            [XNotNullAttrValidator]
            public string type = null;
            [XNotNullAttrValidator]
            public string phase = null;
            [XStringContrainedValueAttrValidator("Game", "Client", "Server", "Editor", "Program")]
            public string target = null;
        }

        [XNotNullAttrValidator]
        public string path = null;
        [XNotNullAttrValidator]
        public string[] ue = null;
        [XNotNullAttrValidator]
        public string label = null;
        [XNotNullAttrValidator]
        public string desc = null;
        [XNotNullAttrValidator]
        public XTPL_GameModuleMetaModel meta = null;
    }

    public class XTPL_ClassModel
    {
        public class XTPL_ClassMetaModel
        {
            [XNotNullAttrValidator]
            public string header = null;
            [XNotNullAttrValidator]
            public string source = null;
        }

        [XNotNullAttrValidator]
        public string path = null;
        [XNotNullAttrValidator]
        public string[] ue = null;
        [XNotNullAttrValidator]
        public string label = null;
        [XNotNullAttrValidator]
        public string desc = null;
        [XNotNullAttrValidator]
        public XTPL_ClassMetaModel meta = null;
    }
}
