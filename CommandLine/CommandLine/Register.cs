using System;
using System.Collections;
using System.Collections.Generic;
namespace CommandLine
{
    public class Register<T> : ICollection<T> where T : class
    {
        public string Name { get; }
        public int Count { get { return _Ts.Count; } }
        public bool IsReadOnly { get { return false; } }
        private Dictionary<string, T> _Ts;
        public Register(string name)
        {
            _Ts = new Dictionary<string, T>();
            Name = name;
        }
        public Register(string name, IEnumerable<T> content) : this(name)
        {
            if (content == null) throw new ArgumentNullException("Content");
            foreach (T item in content)
                Add(item);
        }
        public T this[string index] { get { return Find(index); } }
        public T Find(string key)
        {
            T value;
            if (_Ts.TryGetValue(key, out value)) return value;
            return default(T);
        }
        public void Add(T item)
        {
            _Ts.Add(item.ToString(), item);
        }
        public IEnumerator<T> GetEnumerator()
        {
            return _Ts.Values.GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _Ts.Values.GetEnumerator();
        }
        public void Clear()
        {
            _Ts.Clear();
        }
        public bool Contains(T item)
        {
            return _Ts.ContainsKey(item.ToString());
        }
        public bool Contains(string name)
        {
            return _Ts.ContainsKey(name);
        }
        public void CopyTo(T[] array, int arrayIndex)
        {
            _Ts.Values.CopyTo(array, arrayIndex);
        }
        public bool Remove(T item)
        {
            return _Ts.Remove(item.ToString());
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
