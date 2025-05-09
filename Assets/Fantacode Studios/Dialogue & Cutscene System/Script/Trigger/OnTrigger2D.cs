using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class OnTrigger2D : TriggerBase
    {
        public GameObject objectToHit;
        public ObjectSource objectSource;


        public OnTrigger2D()
        {
            objectSource = ObjectSource.UsePlayer;
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (objectSource == ObjectSource.UsePlayer)
                objectToHit = GameObject.FindGameObjectWithTag("Player");
            if (collision.gameObject == objectToHit)
                startCutscene = true;
        }

#if UNITY_EDITOR
        public override void CustomInspector()
        {
            base.CustomInspector();
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Object To Hit");
            if (objectSource == ObjectSource.AssignObject)
                objectToHit = (GameObject)SetField(objectToHit, (GameObject)EditorGUILayout.ObjectField(objectToHit, typeof(GameObject), true));
            else
                GUILayout.Label("Using Player");
            objectSource = (ObjectSource)SetField(objectSource, (ObjectSource)EditorGUILayout.EnumPopup(objectSource, GUILayout.Width(20)));
            GUILayout.EndHorizontal();
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Trigger only once");
            triggerOnlyOnce = (bool)SetField(triggerOnlyOnce, (bool)EditorGUILayout.Toggle(triggerOnlyOnce));
            GUILayout.EndHorizontal();
        }
#endif
    }

}