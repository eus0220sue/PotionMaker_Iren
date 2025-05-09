
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FC_CutsceneSystem
{
    public enum ActionType { Animation, Move, Rotate, SetCameraAngle, ChangeCamera, Cinemachine, Audio, EnableOrDisable, Spawn, Fade, Wait, ChangeTransform, ChangeAnmParameter, ChangeSprite, Destroy }

    [System.Serializable]
    public class CutSceneAction
    {
        public ActionType actionType;

        static Dictionary<ActionType, string> ActionDisplayNames = new Dictionary<ActionType, string>()
        {
            { ActionType.EnableOrDisable, "Enable\\Disable Object"},
            { ActionType.Cinemachine, "Switch Cinemachine" },
            { ActionType.ChangeCamera, "Change Camera" },
            { ActionType.ChangeTransform, "Change Transform" },
            { ActionType.ChangeAnmParameter, "Set Animator Parameter" },
            { ActionType.ChangeSprite, "Change Sprite" },
            { ActionType.Animation, "Play Animation" },
            { ActionType.SetCameraAngle, "Set Camera Angle" }
        };

        public static string GetActionDisplayName(ActionType actionType)
        {
            return (ActionDisplayNames.ContainsKey(actionType)) ? CutSceneAction.ActionDisplayNames[actionType] : actionType.ToString();
        }

        public virtual IEnumerator ExecuteAction()
        {
            yield break;
        }  
        public static CutSceneAction CreateCutSceneAction(int actionIndex, CutSceneAction actionData = null)
        {
            var actionType = (ActionType)actionIndex;

            CutSceneAction cutSceneAction = null;

            switch (actionType)
            {
                case ActionType.Fade:
                    cutSceneAction = new FadeAction(actionData as FadeAction);
                    break;
                case ActionType.EnableOrDisable:
                    cutSceneAction = new SetActiveAction(actionData as SetActiveAction);
                    break;
                case ActionType.Move:
                    cutSceneAction = new MoveAction(actionData as MoveAction);
                    break;
                case ActionType.Cinemachine:
                    cutSceneAction = new CineMachineCamAction(actionData as CineMachineCamAction);
                    break;
                case ActionType.ChangeCamera:
                    cutSceneAction = new ChangeCameraAction(actionData as ChangeCameraAction);
                    break;
                case ActionType.ChangeTransform:
                    cutSceneAction = new ChangeTransformAction(actionData as ChangeTransformAction);
                    break;
                case ActionType.Rotate:
                    cutSceneAction = new RotateAction(actionData as RotateAction);
                    break;
                case ActionType.Animation:
                    cutSceneAction = new PlayAnimationAction(actionData as PlayAnimationAction);
                    break;
                case ActionType.Spawn:
                    cutSceneAction = new SpawnAction(actionData as SpawnAction);
                    break;
                case ActionType.Audio:
                    cutSceneAction = new AudioAction(actionData as AudioAction);
                    break;
                case ActionType.Wait:
                    cutSceneAction = new WaitAction(actionData as WaitAction);
                    break;
                case ActionType.Destroy:
                    cutSceneAction = new DestroyObjectAction(actionData as DestroyObjectAction);
                    break;
                case ActionType.ChangeAnmParameter:
                    cutSceneAction = new ChangeAnmParameterAction(actionData as ChangeAnmParameterAction);
                    break;
                case ActionType.ChangeSprite:
                    cutSceneAction = new ChangeSpriteAction(actionData as ChangeSpriteAction);
                    break;
                case ActionType.SetCameraAngle:
                    cutSceneAction = new SetCameraAngle(actionData as SetCameraAngle);
                    break;

                default:
                    break;
            }

            return cutSceneAction;
        }
#if UNITY_EDITOR
        public virtual void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {

        }
        public void UndoGraph(CutsceneGraph graph)
        {
            Undo.RegisterCompleteObjectUndo(graph, "Update Field");
        }
        public virtual bool Validation()
        {
            return false;
        }
#endif
        public object GetFieldValue(bool isSameValues, object defaultValue, object value)
        {
            if (!isSameValues || value == null)
                return defaultValue;
            else
                return value;
        }
        public bool ActionValidationWhilePlaying(Object obj)
        {
            if (obj == null)
                return false;
            return true;
        }
        public bool ValidationWarning(Object obj,string message,CutsceneGraph graph)
        {
            if (graph.validation && obj == null)
            {
#if UNITY_EDITOR
                EditorGUILayout.HelpBox(message, MessageType.Warning);
#endif
                return false;
            }
            return true;
        }
    }
}