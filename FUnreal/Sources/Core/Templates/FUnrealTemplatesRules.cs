using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    public enum FUnrealTemplateLoadRule
    {
        MustLoad,
        LoadIfAny,
        DontLoad
    }

    public class FUnrealTemplatesRules
    {
        public FUnrealTemplateLoadRule LoadPlugins { get; set; }
        public FUnrealTemplateLoadRule LoadPluginModules { get; set; }
        public FUnrealTemplateLoadRule LoadGameModules { get; set; }
        public FUnrealTemplateLoadRule LoadSources { get; set; }

        public string TemplatePrefix { get; set; }

        public FUnrealTemplatesRules() 
        {
            LoadPlugins = FUnrealTemplateLoadRule.DontLoad;
            LoadPluginModules = FUnrealTemplateLoadRule.DontLoad;
            LoadGameModules = FUnrealTemplateLoadRule.DontLoad;
            LoadSources = FUnrealTemplateLoadRule.DontLoad;
            TemplatePrefix = string.Empty;
        }

    }
}
