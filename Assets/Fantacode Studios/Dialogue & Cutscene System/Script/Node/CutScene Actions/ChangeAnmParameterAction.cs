using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace FC_CutsceneSystem
{
    public class ChangeAnmParameterAction : CutSceneAction
    {
        public Animator animator;
        public string parameter;
        public AnimatorParameterType parameterType;
        public ObjectSource objectSource;


        public bool boolTriggerValue;
        public float floatValue;
        public int intValue;

        public ChangeAnmParameterAction(ChangeAnmParameterAction changeAnmParameter = null)
        {
            actionType = ActionType.ChangeAnmParameter;
#if UNITY_EDITOR
            if (changeAnmParameter != null)
                EditorUtility.CopySerializedManagedFieldsOnly(changeAnmParameter, this);
#endif
        }
        public override IEnumerator ExecuteAction()
        {
            if (objectSource == ObjectSource.UsePlayer)
                animator = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
            if (!ActionValidationWhilePlaying(animator))
                yield break;
            switch (parameterType)
            {
                case AnimatorParameterType.Bool:
                    animator.SetBool(parameter, boolTriggerValue);
                    break;
                case AnimatorParameterType.Int:
                    animator.SetInteger(parameter, intValue);
                    break;
                case AnimatorParameterType.Float:
                    animator.SetFloat(parameter, floatValue);
                    break;
                case AnimatorParameterType.Trigger:
                    animator.SetTrigger(parameter);
                    break;
                default:
                    break;
            }
            yield break;
        }
#if UNITY_EDITOR
        public override void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {
            var node = cutSceneNode.currentAction as ChangeAnmParameterAction;

            List<ChangeAnmParameterAction> actions = new List<ChangeAnmParameterAction>() { node };
            if (selectedNodes != null)
            {
                actions = selectedNodes.Select(n => n.currentAction as ChangeAnmParameterAction).ToList();
                var firstNode = actions.First();
                node.animator = (Animator)GetFieldValue(actions.All(n => n.animator == firstNode.animator), null, firstNode.animator);
                node.objectSource = (ObjectSource)GetFieldValue(actions.All(n => n.objectSource == firstNode.objectSource), ObjectSource.AssignObject, firstNode.objectSource);
                node.parameterType = (AnimatorParameterType)GetFieldValue(actions.All(n => n.parameterType == firstNode.parameterType), AnimatorParameterType.Bool, firstNode.parameterType);
                node.parameter = (string)GetFieldValue(actions.All(n => n.parameter == firstNode.parameter), "--", firstNode.parameter);
                node.boolTriggerValue = (bool)GetFieldValue(actions.All(n => n.boolTriggerValue == firstNode.boolTriggerValue), false, firstNode.boolTriggerValue);
                node.intValue = (int)GetFieldValue(actions.All(n => n.intValue == firstNode.intValue), 0, firstNode.intValue);
                node.floatValue = (float)GetFieldValue(actions.All(n => n.floatValue == firstNode.floatValue), 0, firstNode.floatValue);
            }

            GUILayout.BeginHorizontal();
            Animator animator = null;
            if (node.objectSource == ObjectSource.AssignObject)
                animator = (Animator)EditorGUILayout.ObjectField("Animator", node.animator, typeof(Animator), true);
            else
            {
                GUILayout.Label("Animator");
                GUILayout.Label(new GUIContent("Player Animator", "Player-tagged object's Animator"));
            }
            var objectSource = (ObjectSource)EditorGUILayout.EnumPopup(node.objectSource, GUILayout.Width(20));
            if(animator != node.animator || objectSource != node.objectSource)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                {
                    n.animator = animator;
                    n.objectSource = objectSource;
                }
            }
            GUILayout.EndHorizontal();
            if(node.objectSource == ObjectSource.AssignObject)
                ValidationWarning(node.animator,"Animator is null",graph);
            GUILayout.Space(5);

            var parameterType = (AnimatorParameterType)EditorGUILayout.EnumPopup("Parameter Type", node.parameterType);
            if(parameterType != node.parameterType)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.parameterType = parameterType;
            }
            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            var parameter = EditorGUILayout.TextField("Parameter", node.parameter);
            if (parameter != node.parameter)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.parameter = parameter;
            }
            switch (node.parameterType)
            {
                case AnimatorParameterType.Bool:
                    var boolTriggerValue = EditorGUILayout.Toggle(node.boolTriggerValue, GUILayout.Width(25));
                    if(boolTriggerValue != node.boolTriggerValue)
                    {
                        UndoGraph(graph);
                        foreach (var n in actions)
                            n.boolTriggerValue = boolTriggerValue;
                    }
                    break;
                case AnimatorParameterType.Int:
                    var intValue = EditorGUILayout.IntField(node.intValue, GUILayout.Width(25));
                    if (intValue != node.intValue)
                    {
                        UndoGraph(graph);
                        foreach (var n in actions)
                            n.intValue = intValue;
                    }
                    break;
                case AnimatorParameterType.Float:
                    var floatValue = EditorGUILayout.FloatField(node.floatValue, GUILayout.Width(25));
                    if (floatValue != node.floatValue)
                    {
                        UndoGraph(graph);
                        foreach (var n in actions)
                            n.floatValue = floatValue;
                    }
                    break;
                case AnimatorParameterType.Trigger:
                    break;
                default:
                    break;
            }
            GUILayout.EndHorizontal();
            
        }
        public override bool Validation()
        {
            return (animator == null && objectSource == ObjectSource.AssignObject);
        }
#endif
    }
}