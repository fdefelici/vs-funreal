using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal
{
    public class XFilesystemEnumerable : IEnumerable<string>
    {
        public static XFilesystemEnumerable AdaptToNormal(IEnumerable<string> adapted)
        {
            Func<string, string> behav = str => XFilesystem.ToNormalPath(str);
            return new XFilesystemEnumerable(adapted, behav);
        }

        public static XFilesystemEnumerable AdaptToLong(IEnumerable<string> adapted)
        {
            Func<string, string> behav = str => XFilesystem.ToLongPath(str);
            return new XFilesystemEnumerable(adapted, behav);
        }

        private IEnumerable<string> _adapter;
        private Func<string, string> _behav;

        public XFilesystemEnumerable(IEnumerable<string> adapted, Func<string, string> transform)
        {
            _adapter = adapted;
            _behav = transform;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return new XFilesystemEnumerator(_adapter.GetEnumerator(), _behav);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class XFilesystemEnumerator : IEnumerator<string>
    {
        private IEnumerator<string> _adapted;
        private Func<string, string> _behav;

        public XFilesystemEnumerator(IEnumerator<string> adapted, Func<string, string> behav) { 
            _adapted = adapted;
            _behav = behav;
        }

        public string Current => _behav(_adapted.Current);

        object IEnumerator.Current => throw new NotImplementedException();

        public void Dispose()
        {
            _adapted.Dispose();
        }

        public bool MoveNext()
        {
            return _adapted.MoveNext();
        }

        public void Reset()
        {
            _adapted.Reset();
        }
    }
}
