using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace FUnreal
{
    public class FUnrealUProjectFile : XJsonFile
    {
        public FUnrealUProjectPluginsJson Plugins { get; }
        public FUnrealUProjectModulesJson Modules { get; }
        public string EngineAssociation { get; internal set; }

        public FUnrealUProjectFile(string filePath) : base(filePath)
        {
            EngineAssociation = (string)_json["EngineAssociation"];
            Plugins = new FUnrealUProjectPluginsJson(_json["Plugins"]);
            Modules = new FUnrealUProjectModulesJson(_json["Modules"]);
        }
    }

    public class FUnrealUProjectPluginsJson : XJsonArray<FUnrealUProjectPluginJson>
    {
        public FUnrealUProjectPluginsJson(JToken jsonArray) 
            : base(jsonArray, "Name")
        { }
    }

    public class FUnrealUProjectPluginJson : XJson
    {
        public FUnrealUProjectPluginJson() { }

        public FUnrealUProjectPluginJson(JToken json) : base(json) { 
        }
        public string Name { 
            get
            {
                return (string)base["Name"];
            } 
            set {
                base["Name"] = value;
            } 
        }
    }

    public class FUnrealUProjectModulesJson : XJsonArray<FUnrealUProjectModuleJson>
    {
        public FUnrealUProjectModulesJson(JToken jsonArray)
            : base(jsonArray, "Name")
        { }
    }

    public class FUnrealUProjectModuleJson : XJson
    {
        public FUnrealUProjectModuleJson() : base(new JObject()) { }
        public FUnrealUProjectModuleJson(JToken json) : base(json) { }


        public string Name
        {
            get { return (string)base["Name"]; }
            set { base["Name"] = value; }
        }
        public string Type
        {
            get { return (string)base["Type"]; }
            set { base["Type"] = value; }
        }
        public string LoadingPhase
        {
            get { return (string)base["LoadingPhase"]; }
            set { base["LoadingPhase"] = value; }
        }
    }
}
