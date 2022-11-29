using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FUnreal 
{
    public class XJson
    {
        public JToken JTokenRaw { get; internal set; }


        public XJson()
        {
            JTokenRaw = null;
        }

        public XJson(JToken json)
        {
            JTokenRaw = json;
        }

        public static implicit operator bool(XJson j) { return j.Exists; }

        public bool Exists => JTokenRaw != null;

        public virtual JToken this[string name]
        {
            get
            {
                JToken token = JTokenRaw?[name];
                return token;
            }
            set
            {
                if (JTokenRaw == null) return;
                JTokenRaw[name] = value;
            }
        }
        public virtual void Remove()
        {
            JTokenRaw?.Remove();
        }
    }

    public class XJsonArray<T> : XJson
        where T : XJson
    {
        private string _searchField;
        public XJsonArray(JToken json, string searchField) : base(json)
        {
            _searchField = searchField;
        }

        public new T this[string name]
        {
            get
            {
                JToken token = JTokenRaw?.SelectToken($"$.[?(@.{_searchField} == '{name}')]");
                return (T)Activator.CreateInstance(typeof(T), new object[] { token });
            }
        }

        public void Add(T item)
        {
            ((JArray)JTokenRaw).Add(item.JTokenRaw);
        }

        public int Count
        {
            get
            {
                if (!this) return 0;
                return ((JArray)JTokenRaw).Count;
            }
        }

        public override void Remove()
        {
            JTokenRaw?.Parent.Remove();
        }

        public void RemoveIfEmpty()
        {
            if (Count == 0)
            {
                Remove();
            }
        }
    }

    public class XJsonFile
    {
        private string _filePath;
        protected XJson _json;

        public XJsonFile(string filePath)
        {
            _filePath = filePath;
            _json = new XJson(XFilesystem.JsonFileRead(filePath));
        }

        public void Save()
        {
            XFilesystem.JsonFileWrite(_filePath, _json.JTokenRaw);
        }
    }

}
