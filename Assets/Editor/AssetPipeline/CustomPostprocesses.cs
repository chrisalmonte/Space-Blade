using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CustomPostprocesses : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths, bool didDomainReload)
    {
        foreach (string assetPath in movedAssets)
        {
            CheckIfPrefabID(assetPath);
        }

        foreach (string assetPath in importedAssets)
        {
            CheckIfPrefabID(assetPath);
        }
    }

    static private void CheckIfPrefabID(string prefabPath)
    {
        if (AssetDatabase.GetMainAssetTypeAtPath(prefabPath).Equals(typeof(GameObject)))
        {
            AssetDatabase.LoadAssetAtPath<PrefabIDComponent>(prefabPath)?.UpdatePrefabID();
        }
    }
}
