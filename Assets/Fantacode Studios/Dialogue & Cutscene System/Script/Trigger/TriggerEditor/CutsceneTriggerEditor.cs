using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace FC_CutsceneSystem
{
#if UNITY_EDITOR
    [CustomEditor(typeof(CutsceneTrigger))]
    public class CutsceneTriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var trigger = target as CutsceneTrigger;
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Choose Trigger");
            TriggerType newType = (TriggerType)EditorGUILayout.EnumPopup(trigger.triggerType);
            GUILayout.EndHorizontal();
            if (newType != trigger.triggerType)
            {
                Undo.RegisterFullObjectHierarchyUndo(trigger, "Add Component");
                trigger.triggerType = newType;
                trigger.ChangeTrigger();
            }
            GUILayout.Space(5);
        }
    }
#endif
}