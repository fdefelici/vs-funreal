using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    internal static class VSCTSymbols
    {
        public const int ProjectMenu        = 0x0001;
        public const int AddPluginCmd       = 0x0003;

        public const int PluginMenu         = 0x0004;
        public const int DeletePluginCmd    = 0x0007;
        public const int RenamePluginCmd    = 0x0008;
        public const int AddModuleCmd       = 0x0009;

        public const int PluginModuleMenu   = 0x0011;
        public const int RenameModuleCmd    = 0x0014;
        public const int DeleteModuleCmd    = 0x0015;

        public const int FolderMenu         = 0x0017;
        public const int AddSourceClassCmd         = 0x0019;
        public const int DeleteSourceFolderCmd      = 0x0020;

        public const int SourceFileMenu              = 0x0022;
        public const int DeleteSourceFileCmd         = 0x0024;

        public const int MixedSourceMenu = 0x0026;
        public const int MixedSourceCmd = 0x0028;
    }
}
