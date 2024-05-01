using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    public class StringToPathConverter : TypeConverter
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
            var pathAsString = value as string;
            //Allow to set field to Empty String, even if contains just blank spaces
            if (string.IsNullOrEmpty(pathAsString) || pathAsString.Trim().Length > 0)
            {
                return string.Empty;
            }

            pathAsString = pathAsString.Trim();
            if (!XFilesystem.FileExists(pathAsString))
            {
                //Show option error dialog
                throw new NotSupportedException($"Invalid path: {pathAsString}");
            }

            //NOTE: strangly base function fails with exception try converting a path to a string
            //return base.ConvertFrom(context, culture, value);
            return pathAsString;
        }
    }
}
