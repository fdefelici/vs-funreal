using Microsoft.VisualStudio.VCProjectEngine;
using System;
using System.Collections.Generic;

namespace FUnreal
{
    public interface IDeepCopyVisitor
    {
        string HandlePath(string fsPath);
        void HandleFileContent(string targetFile);
    }

    public class NullDeepCopyVisitor : IDeepCopyVisitor
    {
        public void HandleFileContent(string targetFile) { }
        public string HandlePath(string fsPath) { return fsPath; }
    }

}