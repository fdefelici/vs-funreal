using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    public static class XString
    {
        public static string Format(string format, params string[] args)
        {
            try
            {
                return string.Format(format, args);
            } catch (Exception) {
                return format;
            }
        }

        public static bool IsEqualToAny(string str, params string[] args)
        {
            return args.Contains(str);
        }
    }
}
