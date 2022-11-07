using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Documents;

namespace FUnreal.Sources.Core
{
    public class FUnrealUProjectFile : XJsonFile
    {
        public FUnrealUProjectPlugins Plugins { get; }
        public string EngineAssociation { get; internal set; }

        public FUnrealUProjectFile(string filePath) : base(filePath)
        {
            EngineAssociation = (string)_json["EngineAssociation"];
            Plugins = new FUnrealUProjectPlugins(_json["Plugins"]);
        }
    }

    public class FUnrealUProjectPlugins : XJsonArray<FUnrealUProjectPlugin>
    {
        public FUnrealUProjectPlugins(JToken jsonArray) 
            : base(jsonArray, "Name")
        { }
    }

    public class FUnrealUProjectPlugin : XJson
    {
        public FUnrealUProjectPlugin() { }

        public FUnrealUProjectPlugin(JToken json) : base(json) { 
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
}
