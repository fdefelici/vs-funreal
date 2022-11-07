using EnvDTE90;
using Microsoft.VisualStudio.Debugger.Interop;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Documents;

namespace FUnreal
{
    public class FUnrealPluginModules : IEnumerable<FUnrealPluginModule>
    {
        private List<FUnrealPluginModule> _modules = new List<FUnrealPluginModule>();
        private Dictionary<string, FUnrealPluginModule> _byName = new Dictionary<string, FUnrealPluginModule>();    

        public void Add(FUnrealPluginModule module)
        {
            _modules.Add(module);
            _byName[module.Name] = module;
        }

        public IEnumerator<FUnrealPluginModule> GetEnumerator()
        {
            return _modules.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public FUnrealPluginModule this[string name]
        {
            get
            {
                if (_byName.TryGetValue(name, out var module))
                {
                    return module;
                }
                return null;
            }
        }

        public int Count { get { return _modules.Count; } }
    }
}