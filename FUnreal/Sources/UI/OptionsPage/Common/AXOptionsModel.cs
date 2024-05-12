using Community.VisualStudio.Toolkit;
using System;

namespace FUnreal
{
    public abstract class AXOptionModel<T> : BaseOptionModel<T> where T : BaseOptionModel<T>, new()
    {
        private int _currentVersion;
        private Action OnChangedHandlers;

        public AXOptionModel() 
        {
            _currentVersion = -1;
        }

        public int GetVersion()
        {
            int result = 0;

            Type t = GetType();
            var props = t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var prop in props)
            {
                var value = prop.GetValue(this);
                result += value.GetHashCode();
            }

            return result;
        }

        public void AddSavedHandler(Action handler)
        {
            Saved += (options) => handler();
        }

        public void AddChangedHandler(Action handler)
        {
            bool isFirstTime = _currentVersion == -1;
            if (isFirstTime) //start tracking for changes
            {
                _currentVersion = GetVersion();
                AddSavedHandler(DetectChangesOnSaveHandler);
            }
            OnChangedHandlers += handler;
        }

        private void DetectChangesOnSaveHandler()
        {
            int newVersion = GetVersion();
            if (_currentVersion == newVersion) return;
            _currentVersion = newVersion;
            OnChangedHandlers?.Invoke();
        }
    }
}
