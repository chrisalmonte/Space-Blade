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

    [SerializeField, HideInInspector] private string objectID;
    public string ObjectID => objectID == null ? string.Empty : objectID;

#if UNITY_EDITOR
    public void UpdatePrefabID()
    {
        if (!PrefabUtility.IsPartOfPrefabAsset(this))
        {
            Debug.LogWarning("Trying to edit prefab ID on an instance.", this);
            return; 
        }

        string path = AssetDatabase.GetAssetPath(gameObject)?.ToString();
        if (path == null || path.Equals("")) 
        {
            Debug.LogWarning("Invalid prefab path: " + path, this);
            return; 
        }

        Regex rgxPrefabName = new Regex(@"[^//]*(?=\.prefab)");
        string candidateID = rgxPrefabName.Match(path).Value;
        
        if(candidateID.Equals(objectID)) { return; }
        objectID = candidateID;
        Debug.Log("Assigned ID \"" + objectID + "\" to prefab " + path);

        PrefabIDComponent[] prefabIDs = Resources.FindObjectsOfTypeAll<PrefabIDComponent>();

        foreach (var prefab in prefabIDs)
        {
            if (prefab.objectID != null && prefab.objectID.Equals(objectID) && prefab != this)
            {
                objectID += "_" + AssetDatabase.GUIDFromAssetPath(path);

                Debug.LogWarning(prefab.objectID + " already exists. Please consider using a different name." +
                    "\nGUID has been appended to objectID to avoid duplication." +
                    "\nReassigned ID to " + objectID, this);

                break;
            }
        }
    }
#endif
}
