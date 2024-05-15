using System;
using System.Collections.Generic;
using System.Linq;

namespace FUnreal
{
    public class FUnrealTemplates
    {
        public static FUnrealGenericResult TryLoad_V1_0(string templateDescriptorPath, FUnrealTemplatesRules rules, out FUnrealTemplates templates)
        {
            if (!XFilesystem.FileExists(templateDescriptorPath))
            {
                templates = null;
                return FUnrealGenericResult.Failure("Templates file doesn't exists: " + templateDescriptorPath);
            }
            
            string jsonStr = XFilesystem.FileRead(templateDescriptorPath);
            if (jsonStr == null)
            {
                templates = null;
                return FUnrealGenericResult.Failure("Cannot read templates from: " + templateDescriptorPath);
            }

            bool parseSuccess = XJsonUtils.TryFromJsonString(jsonStr, out XTPL_DescriptorModel descriptor);
            if (!parseSuccess)
            {
                templates = null;
                return FUnrealGenericResult.Failure("Cannot parse json from templates descriptor: " + templateDescriptorPath);
            }

            if (descriptor.version != "1.0")
            {
                templates = null;
                return FUnrealGenericResult.Failure("Unsupported templates descriptor version: " + descriptor.version);
            }

            var validationResult = XObjectValidator.Validate(descriptor);
            if (validationResult.IsFailure)
            {
                templates = null;

                var res = FUnrealGenericResult.Failure("Validation failure for templates descriptor: " + templateDescriptorPath);
                res += validationResult;
                return res;
            }

            string templateBaseDir = XFilesystem.PathParent(templateDescriptorPath);
            templates = new FUnrealTemplates();

            FUnrealGenericResult addResult = FUnrealGenericResult.Success();
            addResult += TryAddPlugins(templates, templateBaseDir, descriptor, rules);
            addResult += TryAddPluginModules(templates, templateBaseDir, descriptor, rules);
            addResult += TryAddGameModules(templates, templateBaseDir, descriptor, rules);
            addResult += TryAddClasses(templates, templateBaseDir, descriptor, rules);

            return addResult;
        }

        private static FUnrealGenericResult TryAddClasses(FUnrealTemplates templates, string templateBaseDir, XTPL_DescriptorModel descriptor, FUnrealTemplatesRules rules)
        {
            XTPL_ClassModel[] classes = descriptor.templates.classes;

            if (rules.LoadClasses == FUnrealTemplateLoadRule.DontLoad) return FUnrealGenericResult.Success();

            if (rules.LoadClasses == FUnrealTemplateLoadRule.MustLoad && classes.Length == 0)
            {
                return FUnrealGenericResult.Failure("Templates descriptor must have source templates!");
            }

            for(int i=0; i < classes.Length; ++i)
            {
                var eachModel = classes[i];

                FUnrealClassTemplate template = new FUnrealClassTemplate();
                template.Name = $"{rules.TemplatePrefix}_class_{i}";
                template.BasePath = XFilesystem.PathCombine(templateBaseDir, eachModel.path);
                template.Label = eachModel.label;
                template.Description = eachModel.desc;

                template.Header = eachModel.meta.header;
                template.Source = eachModel.meta.source;

                string[] ueArray = eachModel.ue;
                foreach (string ue in ueArray)
                {
                    templates.SetClass(ue, template.Name, template);
                }
            }
            return FUnrealGenericResult.Success();
        }

        private static FUnrealGenericResult TryAddGameModules(FUnrealTemplates templates, string templateBaseDir, XTPL_DescriptorModel descriptor, FUnrealTemplatesRules rules)
        {
            XTPL_GameModuleModel[] game_modules = descriptor.templates.game_modules;

            if (rules.LoadGameModules == FUnrealTemplateLoadRule.DontLoad) return FUnrealGenericResult.Success();

            if (rules.LoadPluginModules == FUnrealTemplateLoadRule.MustLoad && game_modules.Length == 0)
            {
                return FUnrealGenericResult.Failure("Templates descriptor must have game module templates!");
            }

            for(int i=0; i < game_modules.Length; ++i)
            {
                var eachModel = game_modules[i];
                FUnrealGameModuleTemplate template = new FUnrealGameModuleTemplate();
                template.Name = $"{rules.TemplatePrefix}_gamemodule_{i}";
                template.BasePath = XFilesystem.PathCombine(templateBaseDir, eachModel.path);
                template.Label = eachModel.label;
                template.Description = eachModel.desc;

                template.Type = eachModel.meta.type;
                template.Phase = eachModel.meta.phase;
                template.Target = eachModel.meta.target;

                string[] ueArray = eachModel.ue;
                foreach (string ue in ueArray)
                {
                    templates.SetGameModule(ue, template.Name, template);
                }
            }
            return FUnrealGenericResult.Success();
        }

        private static FUnrealGenericResult TryAddPluginModules(FUnrealTemplates templates, string templateBaseDir, XTPL_DescriptorModel descriptor, FUnrealTemplatesRules rules)
        {
            XTPL_PluginModuleModel[] plugin_modules = descriptor.templates.plugin_modules;

            if (rules.LoadPluginModules == FUnrealTemplateLoadRule.DontLoad) return FUnrealGenericResult.Success();

            if (rules.LoadPluginModules == FUnrealTemplateLoadRule.MustLoad && plugin_modules.Length == 0)
            {
                return FUnrealGenericResult.Failure("Templates descriptor must have plugin module templates!");
            }

            for (int i=0; i < plugin_modules.Length; ++i)
            {
                var eachModel = plugin_modules[i];
                FUnrealPluginModuleTemplate template = new FUnrealPluginModuleTemplate();
                template.Name = $"{rules.TemplatePrefix}_pluginmodule_{i}";
                template.BasePath = XFilesystem.PathCombine(templateBaseDir, eachModel.path);
                template.Label = eachModel.label;
                template.Description = eachModel.desc;

                template.Type = eachModel.meta.type;
                template.Phase = eachModel.meta.phase;

                string[] ueArray = eachModel.ue;
                foreach (string ue in ueArray)
                {
                    templates.SetPluginModule(ue, template.Name, template);
                }
            }

            return FUnrealGenericResult.Success();
        }

        private static FUnrealGenericResult TryAddPlugins(FUnrealTemplates templates, string templateBaseDir, XTPL_DescriptorModel descriptor, FUnrealTemplatesRules rules)
        {
            XTPL_PluginModel[] plugins = descriptor.templates.plugins;

            if (rules.LoadPlugins == FUnrealTemplateLoadRule.DontLoad) return FUnrealGenericResult.Success();

            if (rules.LoadPlugins == FUnrealTemplateLoadRule.MustLoad && plugins.Length == 0)
            {
                return FUnrealGenericResult.Failure("Templates descriptor must have plugin templates!");
            }

            for(int i=0; i < plugins.Length; ++i)
            {
                var eachModel = plugins[i];
                FUnrealPluginTemplate template = new FUnrealPluginTemplate();
                template.Name = $"{rules.TemplatePrefix}_plugin_{i}";
                template.BasePath = XFilesystem.PathCombine(templateBaseDir, eachModel.path);
                template.Label = eachModel.label;
                template.Description = eachModel.desc;

                template.HasModule = eachModel.meta.has_module;

                string[] ueArray = eachModel.ue;
                foreach (string ue in ueArray)
                {
                    templates.SetPlugin(ue, template.Name, template);
                }
            }

            return FUnrealGenericResult.Success();
        }

        private Dictionary<string, AFUnrealTemplate> templatesByKey;
        private Dictionary<string, List<AFUnrealTemplate>> templatesByContext;

        public FUnrealTemplates()
        {
            templatesByKey = new Dictionary<string, AFUnrealTemplate>();
            templatesByContext = new Dictionary<string, List<AFUnrealTemplate>>();
        }

        public int Count { get => templatesByKey.Count; } 

        private void SetTemplate<T>(string ue, string name, T tpl) where T : AFUnrealTemplate
        {
            string context = typeof(T).Name;
            string key = context + "_" + ue + "_" + name;
            templatesByKey[key] = tpl;

            string ctxKey = context + "_" + ue;
            if (!templatesByContext.TryGetValue(ctxKey, out var list))
            {
                list = new List<AFUnrealTemplate>();
                templatesByContext[ctxKey] = list;
            };
            list.Add(tpl);
        }

        private T GetTemplate<T>(string ue, string name) where T : class
        {
            string context = typeof(T).Name;
            string key = context + "_" + ue + "_" + name;
            if (!templatesByKey.TryGetValue(key, out AFUnrealTemplate tpl)) return null;
            return tpl as T;
        }

        public List<T> GetTemplates<T>(string ue)
        {
            string context = typeof(T).Name;
            string ctxKey = context + "_" + ue;
            if (!templatesByContext.TryGetValue(ctxKey, out List<AFUnrealTemplate> tpls)) return new List<T>();
            return tpls.Cast<T>().ToList();
        }

        public void SetPlugin(string ue, string name, FUnrealPluginTemplate tpl)
        {
            SetTemplate(ue, name, tpl);
        }

        public FUnrealPluginTemplate GetPlugin(string ue, string name)
        {
            return GetTemplate<FUnrealPluginTemplate>(ue, name);
        }

        public void SetPluginModule(string ue, string name, FUnrealPluginModuleTemplate tpl)
        {
            SetTemplate(ue, name, tpl);
        }

        public FUnrealPluginModuleTemplate GetPluginModule(string ue, string name)
        {
            return GetTemplate<FUnrealPluginModuleTemplate>(ue, name);
        }

        private void SetGameModule(string ue, string name, FUnrealGameModuleTemplate tpl)
        {
            SetTemplate(ue, name, tpl);
        }

        public FUnrealGameModuleTemplate GetGameModule(string ue, string name)
        {
            return GetTemplate<FUnrealGameModuleTemplate>(ue, name);
        }

        private void SetClass(string ue, string name, FUnrealClassTemplate tpl)
        {
            SetTemplate(ue, name, tpl);
        }

        public FUnrealClassTemplate GetClass(string ue, string name)
        {
            return GetTemplate<FUnrealClassTemplate>(ue, name);
        }

        public List<FUnrealPluginTemplate> GetPlugins(string ueMajorVer)
        {
            return GetTemplates<FUnrealPluginTemplate>(ueMajorVer);
        }

        public List<FUnrealPluginModuleTemplate> GetPluginModules(string ueMajorVer)
        {
            return GetTemplates<FUnrealPluginModuleTemplate>(ueMajorVer);
        }

        public List<FUnrealGameModuleTemplate> GetGameModules(string ueMajorVer)
        {
            return GetTemplates<FUnrealGameModuleTemplate>(ueMajorVer);
        }

        public List<FUnrealClassTemplate> GetClasses(string ueMajorVer)
        {
            return GetTemplates<FUnrealClassTemplate>(ueMajorVer);
        }

        public void MergeWith(FUnrealTemplates others)
        {
            bool duplicatedKeys = false;
            foreach (var otherPair in others.templatesByKey)
            {
                if (templatesByKey.ContainsKey(otherPair.Key))
                {
                    duplicatedKeys = true;
                    break;
                }
            }

            if (duplicatedKeys) return;

            foreach (var otherPair in others.templatesByKey) 
            {
                templatesByKey.Add(otherPair.Key, otherPair.Value);
            }

            foreach (var otherPair in others.templatesByContext)
            {
                var ctxKey = otherPair.Key; 
                var ctxTpl = otherPair.Value;
                if (!templatesByContext.TryGetValue(ctxKey, out var list))
                {
                    list = new List<AFUnrealTemplate>();
                    templatesByContext[ctxKey] = list;
                };
                list.AddRange(ctxTpl);
            }
        }

        public void Sort()
        {
            var comparer = new LabelComparer();
            foreach (var pair in templatesByContext)
            {
                var list = pair.Value;
                list.Sort(comparer);
            }
        }

        private class LabelComparer : Comparer<AFUnrealTemplate>
        {
            public override int Compare(AFUnrealTemplate x, AFUnrealTemplate y)
            {
                return x.Label.CompareTo(y.Label);
            }
        }
    }

}
