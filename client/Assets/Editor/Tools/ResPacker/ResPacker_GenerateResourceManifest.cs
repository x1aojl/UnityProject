using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public partial class ResPacker
{
    private static void GenerateReleaseResourceManifest(string output, string version, AssetBundleBuild[] bundles)
    {
        // 使用dbase保存client.manifest文件
        var dbase = new Dbase();

        // 使用dbase保存AssetBundle资源包信息
        var adb = new Dbase();

        // 使用dbase保存asset资源信息
        var asdb = new Dbase();

        // 生成AssetBundle清单
        // key:   bundle名
        // value: bundle FileSystem的mount路径
        foreach (var bundle in bundles)
        {
            var key   = bundle.assetBundleName;
            var value = PathUtil.Combine(MEDIA_BUNDLE_ROOT, key);

            adb.Add(key, value);

            // 添加AssetBundle资源名的资源到asset清单中
            // key:   asset资源名
            // value: bundle名
            foreach (var asset in bundle.assetNames)
                asdb.Add(asset, key);
        }

        // 将所需信息添加client.manifest文件中
        // dbase.Add("version", version);
        // dbase.Add("release_version", version);
        dbase.Add("bundles", adb);
        dbase.Add("assets", asdb);

        // 将client.manifest文件写入packet目录
        File.WriteAllText(output + "/client.manifest", JsonUtility.ToJson(dbase));
    }

    private static void GeneratePatchResourceManifest(string destIntermediatePath, string destVersion, Dbase sourceManifest, AssetBundleBuild[] bundles)
    {
        var bundleMap = sourceManifest.Get<Dictionary<string, object>>("bundles");
        var assetMap  = sourceManifest.Get<Dictionary<string, object>>("assets");

        var assetsPath = PathUtil.Combine(destIntermediatePath, "assets");

        // 添加新的bundle
        foreach (var bundle in bundles)
        {
            var name = bundle.assetBundleName;

            if (bundleMap.ContainsKey(name) == false)
                bundleMap.Add(name, MEDIA_BUNDLE_ROOT + "/" + name);

            // 更新资源
            foreach (var asset in bundle.assetNames)
            {
                if (assetMap.ContainsKey(asset))
                    assetMap[asset] = bundle.assetBundleName;
                else
                    assetMap.Add(asset, bundle.assetBundleName);
            }
        }

        // 存回去
        sourceManifest.Set("bundles", bundleMap);
        sourceManifest.Set("assets", assetMap);
        sourceManifest.Set("version", destVersion);

        // 写文件
        File.WriteAllText(assetsPath + "/client.manifest", JsonUtility.ToJson(sourceManifest));
    }
}