#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FC_CutsceneSystem
{

    [CustomEditor(typeof(FactDB))]
    public class FactDBEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var factDb = target as FactDB;
            for (int i = 0; i < factDb.Key.Count; i++)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(factDb.Key[i], GUILayout.Width(Screen.width * .3f));
                EditorGUILayout.Toggle(factDb.Value[i], GUILayout.Width(Screen.width * .15f));
                if (GUILayout.Button("Remove Fact", GUILayout.Width(Screen.width * .4f)))
                {
                    Undo.RecordObject(factDb, "Fact removed");
                    factDb.conditions.Remove(factDb.Key[i]);
                    factDb.Value.RemoveAt(i);
                    factDb.Key.RemoveAt(i); 
                    EditorUtility.SetDirty(factDb);
                }
                GUILayout.EndHorizontal();
            }
        }
    }
}
