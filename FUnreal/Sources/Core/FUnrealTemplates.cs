using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Xml;

namespace FUnreal
{
    public class FUnrealTemplates
    {
        public static bool TryLoad_V1_0(string templateDescriptorPath, FUnrealTemplatesRules rules, out FUnrealTemplates templates)
        {
            if (!XFilesystem.FileExists(templateDescriptorPath))
            {
                XDebug.Erro("Templates file doesn't exists: " + templateDescriptorPath);
                templates = null;
                return false;
            }
            
            string jsonStr = XFilesystem.FileRead(templateDescriptorPath);
            if (jsonStr == null)
            {
                XDebug.Erro("Cannot read templates from: " + templateDescriptorPath);
                templates = null;
                return false;
            }

            bool parseSuccess = XJsonUtils.TryFromJsonString(jsonStr, out XTPL_DescriptorModel descriptor);
            if (!parseSuccess)
            {
                XDebug.Erro("Cannot parse json from templates descriptor: " + templateDescriptorPath);
                templates = null;
                return false;   
            }

            if (descriptor.version != "1.0")
            {
                XDebug.Erro("Unsupported templates descriptor version: " + descriptor.version);
                templates = null;
                return false;
            }

            bool validationSuccess = XObjectValidator.Validate(descriptor);
            if (!validationSuccess)
            {
                XDebug.Erro("Invalid templates descriptor: " + templateDescriptorPath);
                templates = null;
                return false;
            }

            string templateBaseDir = XFilesystem.PathParent(templateDescriptorPath);
            templates = new FUnrealTemplates();

            bool addResult = true;
            addResult &= TryAddPlugins(templates, templateBaseDir, descriptor.templates.plugins, rules.LoadPlugins);
            addResult &= TryAddPluginModules(templates, templateBaseDir, descriptor.templates.plugin_modules, rules.LoadPluginModules);
            addResult &= TryAddGameModules(templates, templateBaseDir, descriptor.templates.game_modules, rules.LoadGameModules);
            addResult &= TryAddSources(templates, templateBaseDir, descriptor.templates.sources, rules.LoadSources);


            if (!addResult) return false;
            return true;
        }

        private static bool TryAddSources(FUnrealTemplates templates, string templateBaseDir, XTPL_SourceModel[] sources, FUnrealTemplateLoadRule rule)
        {
            if (rule == FUnrealTemplateLoadRule.DontLoad) return true;

            if (rule == FUnrealTemplateLoadRule.MustLoad && sources.Length == 0)
            {
                XDebug.Erro("Templates descriptor must have source templates!");
                return false;
            }

            foreach (var eachModel in sources)
            {
                FUnrealSourceTemplate template = new FUnrealSourceTemplate();
                template.Name = eachModel.name;
                template.BasePath = XFilesystem.PathCombine(templateBaseDir, eachModel.path);
                template.Label = eachModel.ui.label;
                template.Description = eachModel.ui.desc;

                template.Header = eachModel.meta.header;
                template.Source = eachModel.meta.source;

                string[] ueArray = eachModel.ue.Split(',');
                foreach (string ue in ueArray)
                {
                    templates.SetSource(ue, template.Name, template);
                }
            }
            return true;
        }

        private static bool TryAddGameModules(FUnrealTemplates templates, string templateBaseDir, XTPL_GameModuleModel[] game_modules, FUnrealTemplateLoadRule rule)
        {
            if (rule == FUnrealTemplateLoadRule.DontLoad) return true;

            if (rule == FUnrealTemplateLoadRule.MustLoad && game_modules.Length == 0)
            {
                XDebug.Erro("Templates descriptor must have game module templates!");
                return false;
            }

            foreach (var eachModel in game_modules)
            {
                FUnrealGameModuleTemplate template = new FUnrealGameModuleTemplate();
                template.Name = eachModel.name;
                template.BasePath = XFilesystem.PathCombine(templateBaseDir, eachModel.path);
                template.Label = eachModel.ui.label;
                template.Description = eachModel.ui.desc;

                template.Type = eachModel.meta.type;
                template.Phase = eachModel.meta.phase;
                template.Target = eachModel.meta.target;

                string[] ueArray = eachModel.ue.Split(',');
                foreach (string ue in ueArray)
                {
                    templates.SetGameModule(ue, template.Name, template);
                }
            }
            return true;
        }

        private static bool TryAddPluginModules(FUnrealTemplates templates, string templateBaseDir, XTPL_PluginModuleModel[] plugin_modules, FUnrealTemplateLoadRule rule)
        {
            if (rule == FUnrealTemplateLoadRule.DontLoad) return true;

            if (rule == FUnrealTemplateLoadRule.MustLoad && plugin_modules.Length == 0)
            {
                XDebug.Erro("Templates descriptor must have plugin module templates!");
                return false;
            }

            foreach (var eachModel in plugin_modules)
            {
                FUnrealPluginModuleTemplate template = new FUnrealPluginModuleTemplate();
                template.Name = eachModel.name;
                template.BasePath = XFilesystem.PathCombine(templateBaseDir, eachModel.path);
                template.Label = eachModel.ui.label;
                template.Description = eachModel.ui.desc;

                template.Type = eachModel.meta.type;
                template.Phase = eachModel.meta.phase;

                string[] ueArray = eachModel.ue.Split(',');
                foreach (string ue in ueArray)
                {
                    templates.SetPluginModule(ue, template.Name, template);
                }
            }

            return true;
        }

        private static bool TryAddPlugins(FUnrealTemplates templates, string templateBaseDir, XTPL_PluginModel[] plugins, FUnrealTemplateLoadRule rule)
        {
            if (rule == FUnrealTemplateLoadRule.DontLoad) return true;

            if (rule == FUnrealTemplateLoadRule.MustLoad && plugins.Length == 0)
            {
                XDebug.Erro("Templates descriptor must have plugin templates!");
                return false;
            }

            foreach (var eachModel in plugins)
            {
                FUnrealPluginTemplate template = new FUnrealPluginTemplate();
                template.Name = eachModel.name;
                template.BasePath = XFilesystem.PathCombine(templateBaseDir, eachModel.path);
                template.Label = eachModel.ui.label;
                template.Description = eachModel.ui.desc;

                template.HasModule = eachModel.meta.has_module;

                string[] ueArray = eachModel.ue.Split(',');
                foreach (string ue in ueArray)
                {
                    templates.SetPlugin(ue, template.Name, template);
                }
            }

            return true; 
        }

        private Dictionary<string, object> templatesByKey;
        private Dictionary<string, List<object>> templatesByContext;

        public FUnrealTemplates()
        {
            templatesByKey = new Dictionary<string, object>();
            templatesByContext = new Dictionary<string, List<object>>();
        }

        public int Count { get => templatesByKey.Count; } 

        private void SetTemplate<T>(string ue, string name, T tpl) where T : class
        {
            string context = typeof(T).Name;
            string key = context + "_" + ue + "_" + name;
            templatesByKey[key] = tpl;

            string ctxKey = context + "_" + ue;
            if (!templatesByContext.TryGetValue(ctxKey, out var list))
            {
                list = new List<object>();
                templatesByContext[ctxKey] = list;
            };
            list.Add(tpl);
        }

        private T GetTemplate<T>(string ue, string name) where T : class
        {
            string context = typeof(T).Name;
            string key = context + "_" + ue + "_" + name;
            if (!templatesByKey.TryGetValue(key, out object tpl)) return null;
            return tpl as T;
        }

        public List<T> GetTemplates<T>(string ue)
        {
            string context = typeof(T).Name;
            string ctxKey = context + "_" + ue;
            if (!templatesByContext.TryGetValue(ctxKey, out List<object> tpls)) return new List<T>();
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

        private void SetSource(string ue, string name, FUnrealSourceTemplate tpl)
        {
            SetTemplate(ue, name, tpl);
        }

        public FUnrealSourceTemplate GetSource(string ue, string name)
        {
            return GetTemplate<FUnrealSourceTemplate>(ue, name);
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

        public List<FUnrealSourceTemplate> GetSources(string ueMajorVer)
        {
            return GetTemplates<FUnrealSourceTemplate>(ueMajorVer);
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
                    list = new List<object>();
                    templatesByContext[ctxKey] = list;
                };
                list.AddRange(ctxTpl);
            }


        }
    }

}
