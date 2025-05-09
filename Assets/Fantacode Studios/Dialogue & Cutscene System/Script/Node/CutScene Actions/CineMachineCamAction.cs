#if cinemachine
#if UNITY_6000_0_OR_NEWER
using Unity.Cinemachine;
#else
using Cinemachine;
#endif
#endif

using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace FC_CutsceneSystem
{
    public class CineMachineCamAction : CutSceneAction
    {
#if cinemachine
        CinemachineBrain mainCamera;
#if UNITY_6000_0_OR_NEWER
        public CinemachineCamera newCinemachineCam;
#else
        public CinemachineVirtualCameraBase newCinemachineCam;
#endif
#endif
        public float blendTime = 2f;
        public bool foldout;
        public CineMachineCamAction(CineMachineCamAction cineMachineCam = null)
        {
            actionType = ActionType.Cinemachine;
#if cinemachine
            if (Object.FindFirstObjectByType<CutsceneEditorManager>().mainCinemachineBrain != null)
                mainCamera = Object.FindFirstObjectByType<CutsceneEditorManager>().mainCinemachineBrain;
#if UNITY_EDITOR
            if (cineMachineCam != null)
                EditorUtility.CopySerializedManagedFieldsOnly(cineMachineCam, this);
#endif
#endif
        }

#if cinemachine
        public override IEnumerator ExecuteAction()
        {
            if (mainCamera == null && Object.FindFirstObjectByType<CutsceneEditorManager>().mainCinemachineBrain != null)
                mainCamera = Object.FindFirstObjectByType<CutsceneEditorManager>().mainCinemachineBrain;
            if (mainCamera == null)
                mainCamera = Object.FindFirstObjectByType<CinemachineBrain>();
            if (!ActionValidationWhilePlaying(mainCamera) || !ActionValidationWhilePlaying(newCinemachineCam))
                yield break;

#if UNITY_6000_0_OR_NEWER
            CinemachineBrain brain = Camera.main.GetComponent<CinemachineBrain>();
            ICinemachineCamera activeCamera = brain.ActiveVirtualCamera;
            if (activeCamera != null)
            {
                MonoBehaviour vcamComponent = activeCamera as MonoBehaviour;
                if (vcamComponent != null)
                    vcamComponent.gameObject.SetActive(false);
            }
            mainCamera.DefaultBlend.Time = blendTime;
#else

            mainCamera.ActiveVirtualCamera?.VirtualCameraGameObject.SetActive(false);
            mainCamera.m_DefaultBlend.m_Time = blendTime;
#endif
            newCinemachineCam.gameObject.SetActive(true);
            yield return new WaitForSeconds(blendTime);
        }
#endif

#if UNITY_EDITOR

        public override void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {
#if cinemachine
            var node = cutSceneNode.currentAction as CineMachineCamAction;
            List<CineMachineCamAction> actions = new List<CineMachineCamAction>() { node };
            if (selectedNodes != null)
            {
                actions = selectedNodes.Select(n => n.currentAction as CineMachineCamAction).ToList();
                var firstNode = actions.First();

#if UNITY_6000_0_OR_NEWER
                node.newCinemachineCam = (CinemachineCamera)GetFieldValue(actions.All(n => n.newCinemachineCam == firstNode.newCinemachineCam), null, firstNode.newCinemachineCam);

#else
                node.newCinemachineCam = (CinemachineVirtualCameraBase)GetFieldValue(actions.All(n => n.newCinemachineCam == firstNode.newCinemachineCam), null, firstNode.newCinemachineCam);

#endif



                node.foldout = (bool)GetFieldValue(actions.All(n => n.foldout == firstNode.foldout), false, firstNode.foldout);
                node.blendTime = (float)GetFieldValue(actions.All(n => n.blendTime == firstNode.blendTime), 0f, firstNode.blendTime);
            }

#if UNITY_6000_0_OR_NEWER
            var newCinemachineCam = (CinemachineCamera)EditorGUILayout.ObjectField("New CineCam", node.newCinemachineCam, typeof(CinemachineCamera), true);

#else
            var newCinemachineCam = (CinemachineVirtualCameraBase)EditorGUILayout.ObjectField("New CineCam", node.newCinemachineCam, typeof(CinemachineVirtualCameraBase), true);
#endif


            if (newCinemachineCam != node.newCinemachineCam)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.newCinemachineCam = newCinemachineCam;
            }
            ValidationWarning(node.newCinemachineCam, "New CineCam not assigned", graph);
            GUILayout.Space(5);

            var foldout = EditorGUILayout.Foldout(node.foldout, "Additional");
            if (foldout != node.foldout)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.foldout = foldout;
            }
            if (node.foldout)
            {
                GUILayout.Space(5);
                EditorGUI.indentLevel++;
                var blendTime = EditorGUILayout.FloatField("Blend Time", node.blendTime);
                if (blendTime != node.blendTime)
                {
                    UndoGraph(graph);
                    foreach (var n in actions)
                        n.blendTime = blendTime;
                }
                EditorGUI.indentLevel--;
            }
#else
            EditorGUILayout.HelpBox("Please enable cinemachine from the welcome window", MessageType.Error);

#endif

        }
#if cinemachine
        public override bool Validation()
        {
            return (newCinemachineCam == null);
        }
#endif
#endif
    }
}
