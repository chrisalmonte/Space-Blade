using UnityEditor;

[CustomEditor(typeof(PrefabIDComponent))]
public class PrefabIDComponentDrawer : Editor
{
    public override void OnInspectorGUI()
    {
        PrefabIDComponent prefab = (PrefabIDComponent)target;

        EditorGUILayout.LabelField("Prefab ID: ", prefab.ObjectID == string.Empty 
            ? "<UNASSIGNED>" : prefab.ObjectID);
    }
}
