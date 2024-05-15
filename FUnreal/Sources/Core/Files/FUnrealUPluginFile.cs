using Newtonsoft.Json.Linq;
using System;

namespace FUnreal
{
    public class FUnrealUPluginJsonFile : XJsonFile
    {
        public FUnrealUPluginModulesJson Modules { get; }
        public FUnrealUPluginPluginsJson Plugins { get; }

        public FUnrealUPluginJsonFile(string filePath) : base(filePath)
        {
            Modules = new FUnrealUPluginModulesJson(_json["Modules"]);
            Plugins = new FUnrealUPluginPluginsJson(_json["Plugins"]);
        }
    }

    //Modules : [] 
    public class FUnrealUPluginModulesJson : XJsonArray<FUnrealUPluginModuleJson>
    {
        public FUnrealUPluginModulesJson(JToken jsonArray)
            : base(jsonArray, "Name")
        { }
    }

    //Each Module { }
    public class FUnrealUPluginModuleJson : XJson
    {
        public FUnrealUPluginModuleJson() : base(new JObject()) { }
        public FUnrealUPluginModuleJson(JToken json) : base(json) { }


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

    //Modules : [] 
    public class FUnrealUPluginPluginsJson : XJsonArray<FUnrealUPluginPluginJson>
    {
        public FUnrealUPluginPluginsJson(JToken jsonArray)
            : base(jsonArray, "Name")
        { }
    }


    //Each Plugin { }
    public class FUnrealUPluginPluginJson : XJson
    {
        public FUnrealUPluginPluginJson() : base(new JObject()) { }
        public FUnrealUPluginPluginJson(JToken json) : base(json) { }

        public string Name
        {
            get { return (string)base["Name"]; }
            set { base["Name"] = value; }
        }
    }
}