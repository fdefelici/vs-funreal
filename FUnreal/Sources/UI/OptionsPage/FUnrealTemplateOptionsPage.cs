using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Markup;
using Community.VisualStudio.Toolkit;

namespace FUnreal
{
    public partial class FUnrealTemplateOptionsPage_Provider
    {
        [ComVisible(true)]
        public class OptionPage : BaseOptionPage<FUnrealTemplateOptionsPage> { }
    }

    public enum TemplateMode 
    { 
        BuiltIn,
        Custom,
    }

    public enum SortMode
    {
        Alphabetical,
        Descriptor,
    }

    public class FUnrealTemplateOptionsPage : AXOptionModel<FUnrealTemplateOptionsPage>  //BaseOptionModel<FUnrealTemplateOptionsPage>
    {
        public const string Category_General = "0) General";
        public const string Category_CustomTemplates = "1) Custom Templates";

        private const string EvalutedOnlyMessage = "Evaluated only when 'Templates Mode' = 'Custom'";
       
        [Category(Category_General)]
        [DisplayName("01) Templates Mode")]
        [Description("Select templates mode to be used: Built-in (provided by FUnreal) or Custom (user-defined)")]
        //[DefaultValue(true)]  NOTE: Note working. Need to inline default in Property declaration
        [TypeConverter(typeof(EnumConverter))]
        public TemplateMode TemplatesMode { get; set; } = TemplateMode.BuiltIn;

        [Category(Category_General)]
        [DisplayName("02) Sort Mode")]
        [Description("Select templates sort mode to be used: Alphatical (by Label) or Descriptor (following order declaration in descriptor file)")]
        [TypeConverter(typeof(EnumConverter))]
        public SortMode SortMode { get; set; } = SortMode.Descriptor;

        [Category(Category_CustomTemplates)]
        [DisplayName("11) Descriptor Path")]
        [Description("Path to custom templates descriptor file. " + EvalutedOnlyMessage)]
        [TypeConverter(typeof(StringToPathConverter))]
        public string CustomTemplateDescriptorPath { get; set; } = string.Empty;

        [Category(Category_CustomTemplates)]
        [DisplayName("12) Use Built-In Plugins")]
        [Description("Load built-in Plugin templates. " + EvalutedOnlyMessage)]
        public bool LoadBuiltInPlugins { get; set; } = true;

        [Category(Category_CustomTemplates)]
        [DisplayName("13) Use Built-In Plugin Modules")]
        [Description("Load built-in Plugin Module templates. " + EvalutedOnlyMessage)]
        public bool LoadBuiltInPluginModules { get; set; } = true;

        [Category(Category_CustomTemplates)]
        [DisplayName("14) Use Built-In Game Modules")]
        [Description("Load built-in Game Module templates. " + EvalutedOnlyMessage)]
        public bool LoadBuiltInGameModules { get; set; } = true;

        [Category(Category_CustomTemplates)]
        [DisplayName("15) Use Built-In Classes")]
        [Description("Load built-in Class templates. " + EvalutedOnlyMessage)]
        public bool LoadBuiltInClasses { get; set; } = true;
    }

}
