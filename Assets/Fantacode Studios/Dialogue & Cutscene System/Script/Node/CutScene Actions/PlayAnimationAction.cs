using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class PlayAnimationAction : CutSceneAction
    {
        public Animator animator;
        public string anmSource;
        public int anmLayerIndex = 0;
        public float crossFadeTime = .3f;
        public PlayAnimationUsing playUsing;
        public AnimatorParameterType parameterType;

        public ObjectSource objectSource;

        public bool boolTriggerValue;
        public int intValue;
        public float floatValue;

        public bool foldOut;


        public PlayAnimationAction(PlayAnimationAction playAnimation = null)
        {
            actionType = ActionType.Animation;
#if UNITY_EDITOR
            if (playAnimation != null)
                EditorUtility.CopySerializedManagedFieldsOnly(playAnimation, this);
#endif
        }

        public override IEnumerator ExecuteAction()
        {
            if (objectSource == ObjectSource.UsePlayer)
                animator = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();

            if (!ActionValidationWhilePlaying(animator))
                yield break;


            if (anmSource != "")
            {
                if (playUsing == PlayAnimationUsing.AnimationName)
                {
                    animator.CrossFadeInFixedTime(anmSource, crossFadeTime, anmLayerIndex);
                    yield return new WaitForSecondsRealtime(crossFadeTime + .2f);
                    yield return new WaitUntil(() => !animator.GetCurrentAnimatorStateInfo(anmLayerIndex).IsName(anmSource));
                }
                else if (playUsing == PlayAnimationUsing.AnimatorParameter)
                {
                    switch (parameterType)
                    {
                        case AnimatorParameterType.Bool:
                            animator.SetBool(anmSource, boolTriggerValue);
                            break;
                        case AnimatorParameterType.Int:
                            animator.SetInteger(anmSource, intValue);
                            break;
                        case AnimatorParameterType.Float:
                            animator.SetFloat(anmSource, floatValue);
                            break;
                        case AnimatorParameterType.Trigger:
                            animator.SetTrigger(anmSource);
                            break;
                    }
                }
            }
        }


#if UNITY_EDITOR
        public override void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {
            var node = cutSceneNode.currentAction as PlayAnimationAction;

            List<PlayAnimationAction> actions = new List<PlayAnimationAction>() { node };
            if (selectedNodes != null)
            {
                actions = selectedNodes.Select(n => n.currentAction as PlayAnimationAction).ToList();
                var firstNode = actions.First();
                node.animator = (Animator)GetFieldValue(actions.All(n => n.animator == firstNode.animator), null, firstNode.animator);
                node.objectSource = (ObjectSource)GetFieldValue(actions.All(n => n.objectSource == firstNode.objectSource), ObjectSource.AssignObject, firstNode.objectSource);
                node.playUsing = (PlayAnimationUsing)GetFieldValue(actions.All(n => n.playUsing == firstNode.playUsing), PlayAnimationUsing.AnimationName, firstNode.playUsing);
                node.parameterType = (AnimatorParameterType)GetFieldValue(actions.All(n => n.parameterType == firstNode.parameterType), AnimatorParameterType.Bool, firstNode.parameterType);
                node.anmSource = (string)GetFieldValue(actions.All(n => n.anmSource == firstNode.anmSource), "--", firstNode.anmSource);
                node.boolTriggerValue = (bool)GetFieldValue(actions.All(n => n.boolTriggerValue == firstNode.boolTriggerValue), false, firstNode.boolTriggerValue);
                node.intValue = (int)GetFieldValue(actions.All(n => n.intValue == firstNode.intValue), 0, firstNode.intValue);
                node.floatValue = (float)GetFieldValue(actions.All(n => n.floatValue == firstNode.floatValue), 0f, firstNode.floatValue);
                node.foldOut = (bool)GetFieldValue(actions.All(n => n.foldOut == firstNode.foldOut), false, firstNode.foldOut);
                node.anmLayerIndex = (int)GetFieldValue(actions.All(n => n.anmLayerIndex == firstNode.anmLayerIndex), 0, firstNode.anmLayerIndex);
                node.crossFadeTime = (float)GetFieldValue(actions.All(n => n.crossFadeTime == firstNode.crossFadeTime), 0f, firstNode.crossFadeTime);
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
            var objectSource = (ObjectSource)graph.SetField(node.objectSource, (ObjectSource)EditorGUILayout.EnumPopup(node.objectSource, GUILayout.Width(20)));
            if (animator != node.animator || objectSource != node.objectSource)
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
                ValidationWarning(node.animator,"animator is null",graph);
            GUILayout.Space(5);



            var playUsing = (PlayAnimationUsing)EditorGUILayout.EnumPopup("Play Using", node.playUsing);
            if (playUsing != node.playUsing)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.playUsing = playUsing;
            }

            if (node.playUsing == PlayAnimationUsing.AnimatorParameter)
            {
                GUILayout.Space(5);
                var parameterType = (AnimatorParameterType)EditorGUILayout.EnumPopup("Parameter Type", node.parameterType);
                if (parameterType != node.parameterType)
                {
                    UndoGraph(graph);
                    foreach (var n in actions)
                        n.parameterType = parameterType;
                }
            }
            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            var anmSource = EditorGUILayout.TextField(node.playUsing.ToString().CleanForUI(), node.anmSource);
            if (anmSource != node.anmSource)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.anmSource = anmSource;
            }
            if (node.playUsing == PlayAnimationUsing.AnimatorParameter)
            {
                switch (node.parameterType)
                {
                    case AnimatorParameterType.Bool:
                        var boolTriggerValue = EditorGUILayout.Toggle(node.boolTriggerValue, GUILayout.Width(25));
                        if (boolTriggerValue != node.boolTriggerValue)
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
            }
            GUILayout.EndHorizontal();
            if (node.playUsing == PlayAnimationUsing.AnimationName)
            {
                GUILayout.Space(5);
                var foldOut = EditorGUILayout.Foldout(node.foldOut, "Additional");
                if (foldOut != node.foldOut)
                {
                    UndoGraph(graph);
                    foreach (var n in actions)
                        n.foldOut = foldOut;
                }
                if (node.foldOut)
                {
                    EditorGUI.indentLevel++;

                    var anmLayerIndex = EditorGUILayout.IntField("Animator Layer", node.anmLayerIndex);
                    if (anmLayerIndex != node.anmLayerIndex)
                    {
                        UndoGraph(graph);
                        foreach (var n in actions)
                            n.anmLayerIndex = anmLayerIndex;
                    }
                    GUILayout.Space(5);
                    var crossFadeTime = EditorGUILayout.FloatField("Cross Fade Time", node.crossFadeTime);
                    if (crossFadeTime != node.crossFadeTime)
                    {
                        UndoGraph(graph);
                        foreach (var n in actions)
                            n.crossFadeTime = crossFadeTime;
                    }

                    EditorGUI.indentLevel--;
                }
            }

            
        }

        public override bool Validation()
        {
            return (animator == null && objectSource == ObjectSource.AssignObject);

        }
#endif
    }
}