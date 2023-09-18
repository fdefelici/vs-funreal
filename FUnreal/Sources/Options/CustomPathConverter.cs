using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    public class CustomPathConverter : TypeConverter
    {
       
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return true;
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            return true;
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var v = value as string;
            if (!XFilesystem.FileExists(v))
            {
                throw new FormatException($"Invalid path: {v}");
            }

            return base.ConvertFrom(context, culture, value);
        }


    }
}
