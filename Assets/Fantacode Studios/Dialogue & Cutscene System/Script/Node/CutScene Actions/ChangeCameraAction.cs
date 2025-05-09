using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class ChangeCameraAction : CutSceneAction
    {
        public Camera newCam;
        public Camera oldCam;
        public float blendTime = 0;


        public ChangeCameraAction(ChangeCameraAction changeCamera = null)
        {
            
            actionType = ActionType.ChangeCamera;
#if UNITY_EDITOR
            if (changeCamera != null)
                EditorUtility.CopySerializedManagedFieldsOnly(changeCamera, this);
#endif
        }

        public override IEnumerator ExecuteAction()
        {
            GameObject.FindObjectOfType<CutsceneEditorManager>().currentCamera.gameObject.SetActive(false);
            if (!ActionValidationWhilePlaying(newCam))
                yield break;
            newCam.gameObject.SetActive(true);
            if (blendTime > 0)
            {
                var targetPos = newCam.transform.position;
                var targetRot = newCam.transform.rotation;
                var startPos = GameObject.FindObjectOfType<CutsceneEditorManager>().currentCamera.transform.position;
                var startRot = GameObject.FindObjectOfType<CutsceneEditorManager>().currentCamera.transform.rotation;

                newCam.transform.position = startPos;
                newCam.transform.rotation = startRot;

                float timer = 0f;
                while (timer <= blendTime)
                {
                    newCam.transform.position = Vector3.Lerp(startPos, targetPos, timer / blendTime);
                    newCam.transform.rotation = Quaternion.Lerp(startRot, targetRot, timer / blendTime);

                    yield return new WaitForFixedUpdate();

                    timer += Time.fixedDeltaTime;
                }

                newCam.transform.rotation = targetRot;
                newCam.transform.position = targetPos;
            }

            GameObject.FindObjectOfType<CutsceneEditorManager>().currentCamera = newCam.gameObject;
        }


#if UNITY_EDITOR
        public override void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {
            var node = cutSceneNode.currentAction as ChangeCameraAction;

            List<ChangeCameraAction> actions = new List<ChangeCameraAction>() { node };
            if (selectedNodes != null)
            {
                actions = selectedNodes.Select(n => n.currentAction as ChangeCameraAction).ToList();
                var firstNode = actions.First();
                node.newCam = (Camera)GetFieldValue(actions.All(n => n.newCam == firstNode.newCam), null, firstNode.newCam);
                node.blendTime = (float)GetFieldValue(actions.All(n => n.blendTime == firstNode.blendTime), null, firstNode.blendTime);
            }

            var newCam = (Camera)EditorGUILayout.ObjectField("New Camera", node.newCam, typeof(Camera), true);
            if (newCam != node.newCam)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.newCam = newCam;
            }
            ValidationWarning(node.newCam,"New Camera is not assigned",graph);
            GUILayout.Space(5);

            var blendTime = EditorGUILayout.FloatField("Blend Time", node.blendTime);
            if (blendTime != node.blendTime)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.blendTime = blendTime;
            }
            
        }
        public override bool Validation()
        {
            return (newCam == null);
        }
#endif
    }
}
