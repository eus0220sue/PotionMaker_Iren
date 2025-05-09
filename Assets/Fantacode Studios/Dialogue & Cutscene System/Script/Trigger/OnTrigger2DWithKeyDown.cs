using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class OnTrigger2DWithKeyDown : TriggerBase
    {
        public GameObject objectToHit;
        public ObjectSource objectSource;
        List<KeyCode> keycode = new List<KeyCode>();

        bool triggering;

        public OnTrigger2DWithKeyDown()
        {
            objectSource = ObjectSource.UsePlayer;
        }

        private void OnEnable()
        {
            foreach (var key in db.interact)
                keycode.Add(key);
        }
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (objectSource == ObjectSource.UsePlayer)
                objectToHit = GameObject.FindGameObjectWithTag("Player");
            if (collision.gameObject == objectToHit)
            {
                triggering = true;
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (collision.gameObject == objectToHit)
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
            EditorGUILayout.LabelField("Trigger only once");
            triggerOnlyOnce = (bool)SetField(triggerOnlyOnce, EditorGUILayout.Toggle(triggerOnlyOnce));
            GUILayout.EndHorizontal();
        }
#endif
    }

}