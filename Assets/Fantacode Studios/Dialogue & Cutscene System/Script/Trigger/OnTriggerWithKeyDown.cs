using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class OnTriggerWithKeyDown : TriggerBase
    {
        public GameObject objectToHit;
        public ObjectSource objectSource;
        List<KeyCode> keycode = new List<KeyCode>();

        bool triggering;

        public OnTriggerWithKeyDown()
        {
            objectSource = ObjectSource.UsePlayer;
        }

        private void OnEnable()
        {
            if (db.useDefaultInputSystem)
            {
                foreach (var key in db.interact)
                    keycode.Add(key);
            }
        }
        private void OnTriggerStay(Collider other)
        {
            if (objectSource == ObjectSource.UsePlayer)
                objectToHit = GameObject.FindGameObjectWithTag("Player");
            if (other.gameObject == objectToHit)
            {
                triggering = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject == objectToHit)
            {
                triggering = false;
            }
        }

        private void LateUpdate()
        {
            if (triggering)
            {
                if (InputManager.instance.InteractInput() || InputManager.instance.CheckInputDown(db.interact))
                {
                    startCutscene = true;
                    triggering = false;
                }
            }
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
            EditorGUILayout.LabelField("Trigger Only Once");
            triggerOnlyOnce = (bool)SetField(triggerOnlyOnce, EditorGUILayout.Toggle(triggerOnlyOnce));
            GUILayout.EndHorizontal();
        }
#endif
    }
}