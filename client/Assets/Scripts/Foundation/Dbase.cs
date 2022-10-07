// Dbase.cs
// Created by xiaojl Sep/24/2022
// 数据存储

using System.Collections;
using System.Collections.Generic;

public class Dbase : IDictionary<string, object>
{
    public ICollection<string> Keys   { get { return ((IDictionary<string, object>)_data).Keys; } }
    public ICollection<object> Values { get { return ((IDictionary<string, object>)_data).Values; } }
    public int                 Count  { get { return ((ICollection<KeyValuePair<string, object>>)_data).Count; } }

    public Dbase()
    {
        _data = new Dictionary<string, object>();
    }

    public void Clear()
    {
        _data.Clear();
    }

    public object Get(string key)
    {
        object value;
        _data.TryGetValue(key, out value);
        return value;
    }

    public T Get<T>(string key)
    {
        var value = Get(key);
        if (value == null)
            return default(T);

        return (T)value;
    }

    public void Set(string key, object value)
    {
        _data[key] = value;
    }

    public void Add(string key, object value)
    {
        Set(key, value);
    }

    public bool Remove(string key)
    {
        return _data.Remove(key);
    }

    public bool ContainsKey(string key)
    {
        return _data.ContainsKey(key);
    }

    public bool TryGetValue(string key, out object value)
    {
        return _data.TryGetValue(key, out value);
    }

    void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
    {
        Add(item.Key, item.Value);
    }

    bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
    {
        return ContainsKey(item.Key);
    }

    void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, object>>)_data).CopyTo(array, arrayIndex);
    }

    bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
    {
        return Remove(item.Key);
    }

    IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
    {
        return ((IEnumerable<KeyValuePair<string, object>>)_data).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)_data).GetEnumerator();
    }

    bool ICollection<KeyValuePair<string, object>>.IsReadOnly
    {
        get { return ((ICollection<KeyValuePair<string, object>>)_data).IsReadOnly; }
    }

    object IDictionary<string, object>.this[string key]
    {
        get { return Get(key); }
        set { Set(key, value); }
    }

    Dictionary<string, object> _data;
}