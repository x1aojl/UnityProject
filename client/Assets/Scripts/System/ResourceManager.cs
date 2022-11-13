using System;
using System.Collections.Generic;
using UnityEngine;

using Object = UnityEngine.Object;

public class LoadingRequest
{
    public LoadingRequest(string name, AssetBundleRequest request)
    {
        this._name = name;
        this._request = request;
    }

    public LoadingRequest(string name, Object data)
    {
        this._name = name;
        this._data = data;
    }

    public bool IsDone()
    {
        return Synchronize();
    }

    public bool IsAlive()
    {
        return this._data != null ||
               this._request != null;
    }

    public T GetData<T>()
    {
        if (_data != null)
            return (T)(object)_data;

        Synchronize();

        return (T)(object)_data;
    }

    public float Progress()
    {
        if (_data != null)
            return 1.0f;

        return _request.progress;
    }

    public Object GetRawObject()
    {
        return this.GetRawObject<Object>();
    }

    public Object GetInstantiateObject()
    {
        return this.GetInstantiateObject<Object>();
    }

    public Object GetInstantiateObject(Transform parent)
    {
        return this.GetInstantiateObject<Object>(parent);
    }

    public Object GetInstantiateObject(Vector3 position, Quaternion rotation)
    {
        return this.GetInstantiateObject<Object>(position, rotation);
    }

    public Object GetInstantiateObject(Transform parent, bool worldPositionStays)
    {
        return this.GetInstantiateObject<Object>(parent, worldPositionStays);
    }

    public Object GetInstantiateObject(Vector3 position, Quaternion rotation, Transform parent)
    {
        return this.GetInstantiateObject<Object>(position, rotation, parent);
    }

    public T GetRawObject<T>() where T : Object
    {
        if (_data != null)
            return (T)_data;

        Synchronize();

        return (T)_data;
    }

    public T GetInstantiateObject<T>() where T : Object
    {
        var rawOb = GetRawObject<T>();

        return rawOb != null ? Object.Instantiate<T>(rawOb) : null;
    }

    public T GetInstantiateObject<T>(Transform parent) where T : Object
    {
        var rawOb = GetRawObject<T>();

        return rawOb != null ? Object.Instantiate<T>(rawOb, parent) : null;
    }

    public T GetInstantiateObject<T>(Vector3 position, Quaternion rotation) where T : Object
    {
        var rawOb = GetRawObject<T>();

        return rawOb != null ? Object.Instantiate<T>(rawOb, position, rotation) : null;
    }

    public T GetInstantiateObject<T>(Transform parent, bool worldPositionStays) where T : Object
    {
        var rawOb = GetRawObject<T>();

        return rawOb != null ? Object.Instantiate<T>(rawOb, parent, worldPositionStays) : null;
    }

    public T GetInstantiateObject<T>(Vector3 position, Quaternion rotation, Transform parent) where T : Object
    {
        var rawOb = GetRawObject<T>();

        return rawOb != null ? Object.Instantiate<T>(rawOb, position, rotation, parent) : null;
    }

    private bool Synchronize()
    {
        if (_data != null)
            return true;

        if (_request.isDone)
            _data = _request.asset;

        return _data != null;
    }

    private string _name;
    private Object _data;
    private AssetBundleRequest _request;
}

public partial class ResourceManager
{
    public delegate void OnResourceLoaded(LoadingRequest request);

    internal static void Init()
    {
        _requests         = new Dictionary<string, WeakReference>();
        _loadingRequests = new Dictionary<LoadingRequest, OnResourceLoaded>();

        LoadAssetMap();
    }

    internal static void Shutdown()
    {
        UnloadAssetMap();

        _requests = null;
        _loadingRequests = null;
    }

    internal static void Update()
    {
        if (_loadingRequests.Count == 0)
            return;

        var loadedReqs = new List<LoadingRequest>();

        foreach (var kvp in _loadingRequests)
        {
            var req      = kvp.Key;
            var callback = kvp.Value;

            if (! req.IsDone())
                continue;

            callback(req);

            loadedReqs.Add(req);
        }

        foreach (var req in loadedReqs)
            _loadingRequests.Remove(req);
    }

    public static LoadingRequest LoadSync(string path)
    {
        if (Application.isEditor)
            return LoadFromDatabaseSync(path);

        WeakReference handle;

        if (_requests.TryGetValue(path, out handle))
        {
            if (handle.Target != null && ((LoadingRequest)handle.Target).IsAlive())
                return (LoadingRequest)handle.Target;

            _requests.Remove(path);
        }

        var ab = ResolveBundle(path);
        if (ab == null)
            Diagnostics.RaiseException("AssetBundle was not found when loading asset({0}).", path);

        var asset = ab.LoadAsset(path);
        if (asset == null)
            Diagnostics.RaiseException("Asset({0}) was not found in asset bundle({1}).", path, ab.name);

        var req = new LoadingRequest(path, asset);

        _requests.Add(path, new WeakReference(req));

        return req;
    }

    public static LoadingRequest LoadAsync(string path)
    {
        if (Application.isEditor)
            return LoadFromDatabaseAsync(path);

        WeakReference handle;

        if (_requests.TryGetValue(path, out handle))
        {
            if (handle.Target != null && ((LoadingRequest)handle.Target).IsAlive())
                return (LoadingRequest)handle.Target;

            _requests.Remove(path);
        }

        var ab = ResolveBundle(path);
        if (ab == null)
            Diagnostics.RaiseException("AssetBundle was not found when loading asset({0})", path);

        var asset = ab.LoadAssetAsync(path);
        if (asset == null)
            Diagnostics.RaiseException("Asset({0}) was not found in asset bundle({1}).", path, ab.name);

        var req = new LoadingRequest(path, asset);

        return req;
    }

    internal static LoadingRequest LoadSync(string path, OnResourceLoaded callback)
    {
        var req = LoadSync(path);

        AppendToQueue(req, callback);

        return req;
    }

    internal static LoadingRequest LoadAsync(string path, OnResourceLoaded callback)
    {
        var req = LoadAsync(path);

        AppendToQueue(req, callback);

        return req;
    }

    public static void UnloadUnusedAssets()
    {
        var keys = new List<string>();

        foreach (var kvp in _requests)
        {
            if (kvp.Value.Target == null)
                keys.Add(kvp.Key);
        }

        for (var i = 0; i < keys.Count; ++i)
            _requests.Remove(keys[i]);

        Resources.UnloadUnusedAssets();
    }

    private static void AppendToQueue(LoadingRequest req, OnResourceLoaded callback)
    {
        if (callback == null)
            return;

        if (req.IsDone())
        {
            callback(req);
            return;
        }

        OnResourceLoaded callbacks;

        if (_loadingRequests.TryGetValue(req, out callbacks))
            callbacks += callback;
        else
            _loadingRequests.Add(req, callback);
    }

    private static Dictionary<string, WeakReference>            _requests;
    private static Dictionary<LoadingRequest, OnResourceLoaded> _loadingRequests;
}