using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    public class FUnrealTemplatesRules
    {
        public bool MustHavePlugins { get; set; }
        public bool MustHavePluginModules { get; set; }
        public bool MustHaveGameModules { get; set; }
        public bool MustHaveSources { get; set; }

        public FUnrealTemplatesRules() 
        { 
            MustHavePlugins = false;
            MustHavePluginModules = false;
            MustHaveGameModules = false;
            MustHaveSources = false;
        }

    }
}
