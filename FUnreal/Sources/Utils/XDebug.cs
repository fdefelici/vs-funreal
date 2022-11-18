using System;
using System.Diagnostics;

namespace FUnreal
{
    public static class XDebug
    {
        private const string INFO = "INFO";
        private const string WARN = "WARN";
        private const string ERRO = "ERRO";

        [Conditional("DEBUG")]
        public static void Info(string format, params string[] args)
        {
            WriteMessage(INFO, format, args);
        }

        [Conditional("DEBUG")]
        public static void Warn(string format, params string[] args)
        {
            WriteMessage(WARN, format, args);
        }

        [Conditional("DEBUG")]
        public static void Erro(string format, params string[] args)
        {
            WriteMessage(ERRO, format, args);
        }

        [Conditional("DEBUG")]
        private static void WriteMessage(string type, string format, params string[] args)
        {
            string timestamp = DateTime.Now.ToString(@"yyyy-MM-dd hh:mm:ss");
            string content = XString.Format(format, args);

            string message = $"[{timestamp}][{XDialogLib.Title_FUnrealToolbox}][{type}] {content}";
            Debug.Print(message);
        }

    }
}
