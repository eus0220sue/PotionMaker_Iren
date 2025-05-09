using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace FC_CutsceneSystem
{
#if UNITY_EDITOR
    [CustomEditor(typeof(TriggerBase),true)]
    public class TriggerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var trigger = target as TriggerBase;
            trigger.CustomInspector();
        }
    }
#endif
}
