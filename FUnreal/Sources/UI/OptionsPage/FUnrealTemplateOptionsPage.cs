using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Community.VisualStudio.Toolkit;

namespace FUnreal
{
    public partial class FUnrealTemplateOptionsPage_Provider
    {
        [ComVisible(true)]
        public class OptionPage : BaseOptionPage<FUnrealTemplateOptionsPage> { }
    }

    public class FUnrealTemplateOptionsPage : BaseOptionModel<FUnrealTemplateOptionsPage>
    {
        [Category("Templates")]
        [DisplayName("Descriptor Path")]
        [Description("Path to templates descriptor file")]
        [DefaultValue("")]
        [TypeConverter(typeof(StringToPathConverter))]
        public string TemplateDescriptorPath { get; set; }

    }
}
