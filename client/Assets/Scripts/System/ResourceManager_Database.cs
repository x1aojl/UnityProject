using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public partial class ResourceManager
{
    private static LoadingRequest LoadFromDatabaseSync(string path)
    {
#if UNITY_EDITOR
        var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (asset == null)
            Diagnostics.RaiseException("Asset({0}) was not found in asset database", path);

        return new LoadingRequest(path, asset);
#else
        Diagnostics.RaiseException("Can not call ResourceManager.LoadFromDatabaseSync outside unity editor.");
        return null;
#endif
    }

    private static LoadingRequest LoadFromDatabaseAsync(string path)
    {
#if UNITY_EDITOR
        var asset = AssetDatabase.LoadAssetAtPath<Object>(path);    
        if (asset == null)
            Diagnostics.RaiseException("Asset({0}) was not found in asset database.", path);

        return new LoadingRequest(path, asset);
#else
        Diagnostics.RaiseException("Can not call ResourceManager.LoadFromDatabaseAsync outside unity editor.");
        return null;
#endif
    }
}