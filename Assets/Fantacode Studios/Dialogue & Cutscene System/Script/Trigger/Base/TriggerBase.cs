using UnityEngine;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FC_CutsceneSystem
{

    public class TriggerBase : MonoBehaviour
    {
        [HideInInspector]
        public bool startCutscene;
        [HideInInspector]
        public TriggerType triggerType;
        [HideInInspector]
        public CutscenePlayer cutscene;

        CutsceneGraph graph;

        public CutsceneSystemDatabase db;
        public bool triggerOnlyOnce;

        private void Awake()
        {
            db = (CutsceneSystemDatabase)Resources.Load("Database/FC_Database");
            if (cutscene != null)
                graph = cutscene.gameObject.GetComponent<CutsceneGraph>();
        }

        private void Update()
        {
            if (cutscene != null)
            {
                if (graph.IsExecuting)
                    startCutscene = false;
                else if (startCutscene)
                {
                    cutscene.StartCutscene();
                    if (triggerOnlyOnce)
                        this.enabled = false;
                }
                startCutscene = false;
            }
        }

#if UNITY_EDITOR
        public virtual void CustomInspector()
        {
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Cutscene");
            cutscene = (CutscenePlayer)SetField(cutscene, (CutscenePlayer)EditorGUILayout.ObjectField(cutscene, typeof(CutscenePlayer), true));
            GUILayout.EndHorizontal();
        }
#endif

        public object SetField(object oldValue, object newValue)
        {
            if (oldValue != null && newValue != null && oldValue.ToString() != newValue.ToString())
            {
#if UNITY_EDITOR
                Undo.RegisterCompleteObjectUndo(this, "Update Field");
#endif
            }
            return newValue;
        }
    }
}



