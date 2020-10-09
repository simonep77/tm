using System;
using System.Collections;
using System.Collections.Generic;
using TaskManagement.Interface;

namespace TaskManagement.Interface
{

    public class TaskRuntimeParametroLista : MarshalByRefObject, ITaskRuntimeParametroLista
    {
        private IDictionary<string, ITaskRuntimeParametro> mDiz = new Dictionary<string, ITaskRuntimeParametro>();
        ITaskRuntimeParametro IDictionary<string, ITaskRuntimeParametro>.this[string key] { get => throw new NotImplementedException(); set => this.mDiz[key] = value; }

        ICollection<string> IDictionary<string, ITaskRuntimeParametro>.Keys => this.mDiz.Keys;

        ICollection<ITaskRuntimeParametro> IDictionary<string, ITaskRuntimeParametro>.Values => this.mDiz.Values;

        int ICollection<KeyValuePair<string, ITaskRuntimeParametro>>.Count => this.mDiz.Count;

        bool ICollection<KeyValuePair<string, ITaskRuntimeParametro>>.IsReadOnly => this.mDiz.IsReadOnly;

        void IDictionary<string, ITaskRuntimeParametro>.Add(string key, ITaskRuntimeParametro value)
        {
            this.mDiz.Add(key, value);
        }

        void ICollection<KeyValuePair<string, ITaskRuntimeParametro>>.Add(KeyValuePair<string, ITaskRuntimeParametro> item)
        {
            this.mDiz.Add(item);
        }

        void ICollection<KeyValuePair<string, ITaskRuntimeParametro>>.Clear()
        {
            this.mDiz.Clear();
        }

        bool ICollection<KeyValuePair<string, ITaskRuntimeParametro>>.Contains(KeyValuePair<string, ITaskRuntimeParametro> item)
        {
            return this.mDiz.Contains(item);
        }

        bool IDictionary<string, ITaskRuntimeParametro>.ContainsKey(string key)
        {
            return this.mDiz.ContainsKey(key);
        }

        void ICollection<KeyValuePair<string, ITaskRuntimeParametro>>.CopyTo(KeyValuePair<string, ITaskRuntimeParametro>[] array, int arrayIndex)
        {
            this.mDiz.CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<string, ITaskRuntimeParametro>> IEnumerable<KeyValuePair<string, ITaskRuntimeParametro>>.GetEnumerator()
        {
            return this.mDiz.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this.mDiz.GetEnumerator();
        }

        bool IDictionary<string, ITaskRuntimeParametro>.Remove(string key)
        {
            return this.mDiz.Remove(key);
        }

        bool ICollection<KeyValuePair<string, ITaskRuntimeParametro>>.Remove(KeyValuePair<string, ITaskRuntimeParametro> item)
        {
            return this.mDiz.Remove(item);
        }

        bool IDictionary<string, ITaskRuntimeParametro>.TryGetValue(string key, out ITaskRuntimeParametro value)
        {
            return this.mDiz.TryGetValue(key, out value);
        }
    }
}