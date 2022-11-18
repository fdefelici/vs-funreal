using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Xml;

namespace FUnreal
{

    public class FUnrealTemplates
    {
        public static FUnrealTemplates Load(string templateDescriptorPath)
        {
            string templatePath = XFilesystem.PathParent(templateDescriptorPath);

            XmlDocument xml = new XmlDocument();
            try
            {
                xml.Load(templateDescriptorPath);
            }
            catch (Exception e)
            {
                Debug.Print(e.Message);
                return new FUnrealTemplates();
            }

            FUnrealTemplates result = new FUnrealTemplates();

            XmlNodeList templateNodes = xml.GetElementsByTagName("template");
            foreach (XmlNode tplNode in templateNodes)
            {
                string ctx = tplNode.Attributes["context"]?.Value;
                string ueCsv = tplNode.Attributes["ue"]?.Value;
                string name = tplNode.Attributes["name"]?.Value;
                string relPath = tplNode.Attributes["path"]?.Value;
                if (ctx == null || name == null || ueCsv == null || relPath == null) continue;

                XmlNode uiNode = tplNode.SelectSingleNode("ui");
                string uiName = uiNode?.Attributes["label"]?.Value;
                string uiDesc = uiNode?.Attributes["desc"]?.Value;
                if (uiName == null || uiDesc == null) continue;

                string absPath = XFilesystem.PathCombine(templatePath, relPath);
                FUnrealTemplate tpl = new FUnrealTemplate(name, absPath, uiName, uiDesc);
                
                /*
                XmlNodeList placeHolderNodes = tplNode.SelectNodes("placeholder");
                foreach (XmlElement plhNode in placeHolderNodes)
                {
                    string role = plhNode.Attributes["role"]?.Value;
                    string value = plhNode.Attributes["value"]?.Value;
                    if (role == null || value == null) continue;

                    tpl.SetPlaceHolder(role, value);
                }
                */

                XmlNode metaNode = tplNode.SelectSingleNode("meta");
                if (metaNode != null)
                {
                    foreach (XmlAttribute attr in metaNode?.Attributes)
                    {
                        string metaName = attr.Name;
                        string metaValue = attr.Value;
                        tpl.SetMeta(metaName, metaValue);
                    }
                }

                string[] ueArray = ueCsv.Split(',');
                foreach (string ue in ueArray)
                {
                    result.SetTemplate(ctx, ue, name, tpl);
                }
            }
            return result;
        }


        Dictionary<string, FUnrealTemplate> templatesByKey;
        private Dictionary<string, List<FUnrealTemplate>> templatesByContext;

        public FUnrealTemplates()
        {
            templatesByKey = new Dictionary<string, FUnrealTemplate>();
            templatesByContext = new Dictionary<string, List<FUnrealTemplate>>();
        }

        public int Count { get { return templatesByKey.Count; } }

        public void SetTemplate(string context, string ue, string name, FUnrealTemplate tpl)
        {
            string key = context + "_" + ue + "_" + name;
            templatesByKey[key] = tpl;

            string ctxKey = context + "_" + ue;
            if (!templatesByContext.TryGetValue(ctxKey, out var list))
            {
                list = new List<FUnrealTemplate>();
                templatesByContext[ctxKey] = list;
            };
            list.Add(tpl);
        }

        public FUnrealTemplate GetTemplate(string context, string ue, string name)
        {
            string key = context + "_" + ue + "_" + name;
            if (!templatesByKey.TryGetValue(key, out FUnrealTemplate tpl)) return null;
            return tpl;
        }

        public List<FUnrealTemplate> GetTemplates(string context, string ue)
        {
            string ctxKey = context + "_" + ue;
            return templatesByContext[ctxKey];
        }
    }

    public class FUnrealTemplate
    {
        private Dictionary<string, string> metas;

        public string Name { get; internal set; }
        public string BasePath { get; internal set; }
        public string Label { get; private set; }
        public string Description { get; private set; }

        public FUnrealTemplate(string name, string templatePath, string label, string desc)
        {
            Name = name;
            BasePath = templatePath;
            Label = label;
            Description = desc;
            metas = new Dictionary<string, string>();
        }

        public void SetMeta(string name, string value)
        {
            metas[name] = value;
        }

        public string GetMeta(string name)
        {
            if (!metas.TryGetValue(name, out string value)) return null;
            return value;
        }
    }
}
