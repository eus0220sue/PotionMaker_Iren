using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FC_CutsceneSystem
{
    public enum ActivationState { Enable, Disable }

    public class SetActiveAction : CutSceneAction
    {
        public GameObject setActiveObject;
        public ActivationState objectState;
        public ObjectSource objectSource;

        public SetActiveAction(SetActiveAction setActive = null)
        {
            actionType = ActionType.EnableOrDisable;
#if UNITY_EDITOR
            if (setActive != null)
                EditorUtility.CopySerializedManagedFieldsOnly(setActive, this);
#endif
        }

        public override IEnumerator ExecuteAction()
        {
            if (objectSource == ObjectSource.UsePlayer)
                setActiveObject = Resources.FindObjectsOfTypeAll<Transform>().ToList().Find(o => o.tag == "Player").gameObject;
            if (!ActionValidationWhilePlaying(setActiveObject)) 
                yield break;
            setActiveObject.SetActive(objectState == ActivationState.Enable); 
        }
#if UNITY_EDITOR
        public override void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {
            var node = cutSceneNode.currentAction as SetActiveAction;

            List<SetActiveAction> actions = new List<SetActiveAction>() { node };
            if (selectedNodes != null)
            {
                actions = selectedNodes.Select(n => n.currentAction as SetActiveAction).ToList();
                var firstNode = actions.First();
                node.setActiveObject = (GameObject)GetFieldValue(actions.All(n => n.setActiveObject == firstNode.setActiveObject), null, firstNode.setActiveObject);
                node.objectState = (ActivationState)GetFieldValue(actions.All(n => n.objectState == firstNode.objectState), ActivationState.Disable, firstNode.objectState);
                node.objectSource = (ObjectSource)GetFieldValue(actions.All(n => n.objectSource == firstNode.objectSource), ObjectSource.AssignObject, firstNode.objectSource);
            }

            EditorGUILayout.BeginVertical("box");
            //GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.cyan } };
            GUIStyle warningStyle = new GUIStyle(EditorStyles.label) { normal = { textColor = Color.red } };

            //EditorGUILayout.LabelField("Set Active Object Settings", headerStyle);
            //EditorGUILayout.Space(5);

            GUILayout.BeginHorizontal();
            GameObject setActiveObject = null;
            if (node.objectSource == ObjectSource.AssignObject)
            {
                setActiveObject = (GameObject)EditorGUILayout.ObjectField(new GUIContent("GameObject", "Select the object to activate/deactivate"), node.setActiveObject, typeof(GameObject), true);
            }
            else
            {
                GUILayout.Label(new GUIContent("GameObject", "Player-tagged object"), EditorStyles.boldLabel);
            }
            var objectSource = (ObjectSource)EditorGUILayout.EnumPopup(node.objectSource, GUILayout.Width(100));
            if (setActiveObject != node.setActiveObject || objectSource != node.objectSource)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                {
                    n.setActiveObject = setActiveObject;
                    n.objectSource = objectSource;
                }
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space(5);
            var objectState = (ActivationState)EditorGUILayout.EnumPopup(new GUIContent("Object State", "Choose activation state"), node.objectState);
            if (objectState != node.objectState)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.objectState = objectState;
            }
            EditorGUILayout.Space(5);

            if (node.setActiveObject == null)
            {
                EditorGUILayout.LabelField("Warning: Object is not assigned", warningStyle);
            }

            EditorGUILayout.EndVertical();
        }


        public override bool Validation()
        {
            return (setActiveObject == null);
        }
#endif
    }
}
