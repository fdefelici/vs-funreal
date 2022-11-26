using System.Collections;
using System.Collections.Generic;

namespace FUnreal
{
    public interface IFunrealCollectionItem
    {
        string Name { get; }
        string FullPath { get; }
    }

    public class FUnrealCollection<T> : IEnumerable<T> where T : IFunrealCollectionItem
    {
        private List<T> list = new List<T>();
        private Dictionary<string, T> dict = new Dictionary<string, T>();

        public int Count { get { return list.Count; } }

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return list.GetEnumerator();
        }

        public void AddAll(FUnrealCollection<T> items)
        {
            foreach(var item in items)
            {
                Add(item);
            }
        }

        public void Add(T item)
        {
            this[item.Name] = item;
        }

        public void Clear()
        {
            list.Clear();
            dict.Clear();   
        }

        public T FindByPath(string fullPath)
        {
            foreach(var item in list)
            {
                if (item.FullPath == fullPath) return item;
            }
            return default(T);
        }

        public T FindByBelongingPath(string innerFullPath)
        {
            foreach (var item in list)
            {
                if (XFilesystem.IsChildPath(innerFullPath, item.FullPath, true)) return item;
            }
            return default(T);
        }

        public void Remove(T item)
        {
            list.Remove(item);
            dict.Remove(item.Name);
        }

        public void RemoveAll(FUnrealCollection<T> items)
        {
            foreach(var item in items)
            {
                list.Remove(item);
                dict.Remove(item.Name);
            }
        }

        public bool Exists(string name)
        {
            return this[name] != null;
        }

        public T this[string name]
        {
            get
            {
                if (dict.TryGetValue(name, out var found))
                {
                    return found;
                }
                return default(T);
            }

            set
            {
                int listPost = list.Count;
                if (dict.ContainsKey(name))
                {
                    var item = dict[name];
                    listPost = list.IndexOf(item);
                    list.RemoveAt(listPost);
                }

                dict[name] = value;
                list.Insert(listPost, value);
            }
        }
    }
}