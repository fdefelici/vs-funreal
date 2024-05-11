using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    public class FUnrealTemplateLoader
    {
        public static bool TryComputeTemplates(IFUnrealVS unrealVS, FUnrealEngine engine, out FUnrealTemplates outTemplates)
        {
            outTemplates = new FUnrealTemplates();

            var options = unrealVS.GetOptions();
            FixOptions(options);

            // Load Built-In Templates  (handle both cases Built-In and Custom
            string vsixDllPath = unrealVS.GetVSixDllPath();
            string vsixBasePath = XFilesystem.PathParent(vsixDllPath);
            string templatePath = XFilesystem.PathCombine(vsixBasePath, "Templates");
            string templateDescPath = XFilesystem.PathCombine(templatePath, "descriptor.json");
            XDebug.Info("VSIX Dll Path: {0}", vsixDllPath);
            XDebug.Info("VSIX Base Path: {0}", vsixBasePath);
            XDebug.Info("Template Descriptor Path: {0}", templateDescPath);

            if (!XFilesystem.FileExists(templateDescPath))
            {
                unrealVS.Output.Erro("Cannot locate built-in templates at path: {0}", templateDescPath);
                return false;
            }

            FUnrealTemplatesRules rules = new FUnrealTemplatesRules()
            {
                LoadPlugins = options.LoadBuiltInPlugins ? FUnrealTemplateLoadRule.MustLoad : FUnrealTemplateLoadRule.DontLoad,
                LoadPluginModules = options.LoadBuiltInPluginModules ? FUnrealTemplateLoadRule.MustLoad : FUnrealTemplateLoadRule.DontLoad,
                LoadGameModules = options.LoadBuiltInPluginModules ? FUnrealTemplateLoadRule.MustLoad : FUnrealTemplateLoadRule.DontLoad,
                LoadSources = options.LoadBuiltInPluginModules ? FUnrealTemplateLoadRule.MustLoad : FUnrealTemplateLoadRule.DontLoad,
                TemplatePrefix = "builtin"
            };

            var loadResult = FUnrealTemplates.TryLoad_V1_0(templateDescPath, rules, out FUnrealTemplates builtInTemplates);
            if (loadResult.IsFailure)
            {
                unrealVS.Output.Erro("Failed to load templates from path: {0}", templateDescPath);
                unrealVS.Output.Erro("Cause:");

                string reason = string.Empty;
                for (int i = 0; i < loadResult.Messages.Count; i++)
                {
                    var msg = loadResult.Messages[i];
                    unrealVS.Output.Erro("- {0}", msg);
                }
                return false;
            }

            outTemplates.MergeWith(builtInTemplates);


            //User-Defined Templates
            bool useCustomTemplates = options.TemplatesMode == TemplateMode.Custom;
            if (useCustomTemplates)
            {
                var userTemplatePath = options.CustomTemplateDescriptorPath;
                if (!string.IsNullOrEmpty(userTemplatePath))
                {
                    FUnrealTemplatesRules userRules = new FUnrealTemplatesRules()
                    {
                        LoadPlugins = options.LoadBuiltInPlugins ? FUnrealTemplateLoadRule.LoadIfAny : FUnrealTemplateLoadRule.MustLoad,
                        LoadPluginModules = options.LoadBuiltInPluginModules ? FUnrealTemplateLoadRule.LoadIfAny : FUnrealTemplateLoadRule.MustLoad,
                        LoadGameModules = options.LoadBuiltInPluginModules ? FUnrealTemplateLoadRule.LoadIfAny : FUnrealTemplateLoadRule.MustLoad,
                        LoadSources = options.LoadBuiltInPluginModules ? FUnrealTemplateLoadRule.LoadIfAny : FUnrealTemplateLoadRule.MustLoad,
                        TemplatePrefix = "custom"
                    };

                    var userTemplatesResult = FUnrealTemplates.TryLoad_V1_0(userTemplatePath, userRules, out FUnrealTemplates userTemplates);
                    if (userTemplatesResult.IsFailure)
                    {
                        unrealVS.Output.Erro("Failed to load custom templates from path: {0}", userTemplatePath);
                        unrealVS.Output.Erro("Cause:");

                        string reason = string.Empty;
                        for (int i = 0; i < loadResult.Messages.Count; i++)
                        {
                            var msg = loadResult.Messages[i];
                            unrealVS.Output.Erro("- {0}", msg);
                        }
                        return false;
                    }

                    outTemplates.MergeWith(userTemplates);
                }
                else
                {
                    unrealVS.Output.Erro("Selected template mode {0}, but custom template descriptor path is missing. Please check FUnreal options!", options.TemplatesMode.ToString());
                    return false;
                }
            }

            //Template Validation
            string ueMajorVer = engine.Version.Major.ToString();
            if (outTemplates.GetPlugins(ueMajorVer).Count == 0)
            {
                unrealVS.Output.Erro("Missing plugin templates. Please check FUnreal options or custom template descriptor if used!");
                return false;
            }
            if (outTemplates.GetPluginModules(ueMajorVer).Count == 0)
            {
                unrealVS.Output.Erro("Missing plugin module templates. Please check FUnreal options or custom template descriptor if used!");
                return false;
            }
            if (outTemplates.GetGameModules(ueMajorVer).Count == 0)
            {
                unrealVS.Output.Erro("Missing game module templates. Please check FUnreal options or custom template descriptor if used!");
                return false;
            }
            if (outTemplates.GetSources(ueMajorVer).Count == 0)
            {
                unrealVS.Output.Erro("Missing source templates. Please check FUnreal options or custom template descriptor if used!");
                return false;
            }

            return true;
        }

        public static void UpdateTemplates(FUnrealVS _unrealVS, FUnrealService _unrealService)
        {
            _unrealVS.Output.Info("Templates update in progress ...");
            bool success = TryComputeTemplates(_unrealVS, _unrealService.Engine, out FUnrealTemplates templates);
            if (success)
            {
                _unrealService.SetTemplates(templates);
                _unrealVS.Output.Info("Templates updated successfully!");
            }
            else
            {
                _unrealVS.Output.Erro("Failed to update templates!");
            }
        }

        private static void FixOptions(FUnrealTemplateOptionsPage options)
        {
            if (options.TemplatesMode == TemplateMode.BuiltIn)
            {
                options.LoadBuiltInPlugins = true;
                options.LoadBuiltInPluginModules = true;
                options.LoadBuiltInGameModule = true;
                options.LoadBuiltInSource = true;
            }
            else if (options.TemplatesMode == TemplateMode.Custom)
            {
                //keep options as it is
            }
        }
    }
}
