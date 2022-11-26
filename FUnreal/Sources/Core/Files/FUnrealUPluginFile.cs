using FUnreal.Sources.Core;
using Newtonsoft.Json.Linq;
using System;

namespace FUnreal
{
    public class FUnrealUPluginJsonFile : XJsonFile
    {
        public FUnrealUPluginModulesJson Modules { get; }
        public FUnrealUPluginJsonFile(string filePath) : base(filePath)
        {
            Modules = new FUnrealUPluginModulesJson(_json["Modules"]);
        }
    }

    public class FUnrealUPluginModulesJson : XJsonArray<FUnrealUPluginModuleJson>
    {
        public FUnrealUPluginModulesJson(JToken jsonArray)
            : base(jsonArray, "Name")
        { }
    }

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
}