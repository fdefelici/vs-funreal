using System;

namespace FUnreal
{
    public class XVersion
    {
        public static XVersion FromSemVer(string semVerStr)
        {
            string[] parts = semVerStr.Split('.');
            if (parts.Length != 2) return null;

            if (!int.TryParse(parts[0], out int major))
            {
                return null;
            }

            if (!int.TryParse(parts[1], out int minor))
            {
                return null;
            }
            return new XVersion(major, minor);
        }

        public XVersion(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }

        public int Major { get; }
        public int Minor { get; }

        public string AsString()
        {
            return $"{Major}.{Minor}";
        }
    }
}