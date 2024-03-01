using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using System.Text.RegularExpressions;
using UnityEditor;
#endif

[DisallowMultipleComponent]
public class PrefabIDComponent : MonoBehaviour
{
    //Creates and saves a unique ID for objects saved as a prefab, which is inherited by their instances.

    //[HideInInspector, SerializeField] private string objectID;
    [SerializeField] private string objectID;    

    public string ObjectID => objectID == null ? string.Empty : objectID;

#if UNITY_EDITOR
    private void OnValidate() => UpdatePrefabID();

    private void UpdatePrefabID()
    {
        if (!PrefabUtility.IsPartOfPrefabAsset(this)){ return; }

        string path = AssetDatabase.GetAssetPath(gameObject)?.ToString();
        if (path == null || path.Equals("")) { return; }

        Regex rgxPrefabName = new Regex(@"[^//]*(?=\.prefab)");
        string candidateID = rgxPrefabName.Match(path).Value;
        
        if(candidateID.Equals(objectID)) { return; }
        objectID = candidateID;

        PrefabIDComponent[] prefabIDs = Resources.FindObjectsOfTypeAll<PrefabIDComponent>();

        foreach (var prefab in prefabIDs)
        {
            if (prefab.objectID != null && prefab.objectID.Equals(objectID) && prefab != this)
            {
                objectID += "_" + AssetDatabase.GUIDFromAssetPath(path);
                Debug.LogWarning(prefab.objectID + " already exist. Please consider using a different name." +
                    "\nGUID has been appended to ID to avoid duplication.");

                break;
            }
        }

        Debug.Log("Assigned ID \"" + objectID + "\" to prefab " + path);
    }
#endif
}
