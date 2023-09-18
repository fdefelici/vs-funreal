using Community.VisualStudio.Toolkit;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace FUnreal
{
    internal partial class OptionsProvider
    {
        [ComVisible(true)]
        public class GeneralOptions : BaseOptionPage<MyOptions> {  }
    }

    public class MyOptions : BaseOptionModel<MyOptions>
    {

        [Category("My Option List")]
        [DisplayName("My Option")]
        [Description("My Beautyfull bool")]
        [DefaultValue(false)]
        public bool MyBool { get; set; }

        [Category("My Option List")]
        [DisplayName("My Path")]
        [Description("My Beautyfull Path")]
        [TypeConverter(typeof(CustomPathConverter))]
        [DefaultValue("")]
        public string MyPath { get; set; }
    }
}
