using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace FUnreal
{
    public class XVersion
    {
        // Expected format: MAJOR.MINOR(.PATCH-LABEL)
        // where:
        // - MAJOR => int (mandatory)
        // - MINOR => int (mandatory)
        // - PATCH => int (optional)
        // - LABEL => string (optional) Separated by '-'
        public static XVersion FromSemVer(string semVerStr)
        {
            int labelSeparatorIndex = semVerStr.IndexOf('-');
            string semVerPart;
            string labelPart;

            if (labelSeparatorIndex == -1)
            {
                semVerPart = semVerStr;
                labelPart = string.Empty;
            } else
            {
                semVerPart = semVerStr.Substring(0, labelSeparatorIndex);
                labelPart = semVerStr.Substring(labelSeparatorIndex + 1);
            }

            string[] parts = semVerPart.Split('.');
            if (parts.Length < 2) return null;

            if (!int.TryParse(parts[0], out int major))
            {
                return null;
            }

            if (!int.TryParse(parts[1], out int minor))
            {
                return null;
            }

            int patch = -1;
            if (parts.Length >= 3)
            {
                if (!int.TryParse(parts[2], out int patchParse))
                {
                    return null;
                }
                patch = patchParse;
            }


            return new XVersion(major, minor, patch, labelPart);
        }

        public XVersion(int major, int minor)
            : this(major, minor, -1)
        { }

        public XVersion(int major, int minor, int patch)
           : this(major, minor, patch, string.Empty)
        { }

        public XVersion(int major, int minor, int patch, string label)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Label = label;
        }

        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }
        public string Label { get; }

        public string AsString()
        {
            StringBuilder result = new StringBuilder();
            result.Append(Major);
            result.Append(".");
            result.Append(Minor);
            if (Patch != -1)
            {
                result.Append(".");
                result.Append(Patch);
            }
            if (!string.IsNullOrEmpty(Label))
            {
                result.Append("-");
                result.Append(Label);
            }

            return result.ToString();
        }

        public override bool Equals(object obj)
        {
            if (!(obj is XVersion)) return false;

            XVersion other = obj as XVersion;

            if (other.Major != Major) return false;
            if (other.Minor != Minor) return false;
            if (other.Patch != Patch) return false;
            if (other.Label != Label) return false;
            return true;
        }

        public override int GetHashCode()
        {
            return Major + Minor + Patch + Label.GetHashCode();
        }
    }
}