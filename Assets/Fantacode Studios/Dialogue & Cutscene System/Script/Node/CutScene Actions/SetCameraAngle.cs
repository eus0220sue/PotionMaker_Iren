using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class SetCameraAngle : CutSceneAction
    {
        public GameObject ObjectToFocus;
        public ObjectSource objectSource;

        public bool transition;
        public float blendTime = 0;
        public int startingAngleIndex; 
        public int targetAngleIndex; 

        public GameObject cameraAnglePrefab;


        public SetCameraAngle(SetCameraAngle setCameraAngle = null)
        {
            
            actionType = ActionType.SetCameraAngle;
#if UNITY_EDITOR
            if (setCameraAngle != null)
                EditorUtility.CopySerializedManagedFieldsOnly(setCameraAngle, this);
#endif
            cameraAnglePrefab = GameObject.FindObjectOfType<CutsceneEditorManager>().cameraAngles;
        }
        public override IEnumerator ExecuteAction() 
        {
            GameObject.FindObjectOfType<CutsceneEditorManager>().currentCamera.gameObject.SetActive(false);
            if (objectSource == ObjectSource.UsePlayer)
                ObjectToFocus = GameObject.FindGameObjectWithTag("Player");
            if (!ActionValidationWhilePlaying(ObjectToFocus))
                yield break;
            var newCam = CutsceneManager.instance.defaultCamera;

            if (cameraAnglePrefab == null)
                cameraAnglePrefab = GameObject.FindObjectOfType<CutsceneEditorManager>().cameraAngles;

            var angles = GameObject.Instantiate(cameraAnglePrefab,ObjectToFocus.transform,false);
            angles.name = "Camera Angles";

            List<Transform> cameraAngles = new List<Transform>();
            foreach (Transform angle in angles.transform)
                cameraAngles.Add(angle);



            newCam.SetActive(true);

            var targetPos = cameraAngles[targetAngleIndex].position;
            var targetRot = cameraAngles[targetAngleIndex].rotation;
            var startPos = cameraAngles[startingAngleIndex].position;
            var startRot = cameraAngles[startingAngleIndex].rotation;

            if (!transition)
            {
                targetPos = cameraAngles[startingAngleIndex].position;
                targetRot = cameraAngles[startingAngleIndex].rotation;
                startPos = GameObject.FindObjectOfType<CutsceneEditorManager>().currentCamera.transform.position;
                startRot = GameObject.FindObjectOfType<CutsceneEditorManager>().currentCamera.transform.rotation;
            }

            if (blendTime > 0)
            {
                if (transition)
                {
                    newCam.transform.position = cameraAngles[startingAngleIndex].position;
                    newCam.transform.eulerAngles = cameraAngles[startingAngleIndex].eulerAngles;
                    newCam.transform.rotation = startRot;
                }
                else
                {
                    newCam.transform.position = GameObject.FindObjectOfType<CutsceneEditorManager>().currentCamera.transform.position;
                    newCam.transform.eulerAngles = GameObject.FindObjectOfType<CutsceneEditorManager>().currentCamera.transform.eulerAngles;
                    newCam.transform.rotation = startRot;
                }

                float timer = 0f;
                while (timer <= blendTime)
                {
                    newCam.transform.position = Vector3.Lerp(startPos, targetPos, timer / blendTime);
                    newCam.transform.rotation = Quaternion.Lerp(startRot, targetRot, timer / blendTime);

                    yield return new WaitForFixedUpdate();

                    timer += Time.fixedDeltaTime;
                }
            }
            newCam.transform.rotation = targetRot;
            newCam.transform.position = targetPos;
            
            GameObject.FindObjectOfType<CutsceneEditorManager>().currentCamera = CutsceneManager.instance.defaultCamera.gameObject;
            yield break;
        }

#if UNITY_EDITOR
        public override void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {
            var node = cutSceneNode.currentAction as SetCameraAngle;

            List<SetCameraAngle> actions = new List<SetCameraAngle>() { node };
            if (selectedNodes != null)
            {
                actions = selectedNodes.Select(n => n.currentAction as SetCameraAngle).ToList();
                var firstNode = actions.First();
                node.objectSource = (ObjectSource)GetFieldValue(actions.All(n => n.objectSource == firstNode.objectSource), ObjectSource.AssignObject, firstNode.objectSource);
                node.ObjectToFocus = (GameObject)GetFieldValue(actions.All(n => n.ObjectToFocus == firstNode.ObjectToFocus), null, firstNode.ObjectToFocus);
                node.transition = (bool)GetFieldValue(actions.All(n => n.transition == firstNode.transition),false, firstNode.transition);
                node.startingAngleIndex = (int)GetFieldValue(actions.All(n => n.startingAngleIndex == firstNode.startingAngleIndex),0, firstNode.startingAngleIndex);
                node.targetAngleIndex = (int)GetFieldValue(actions.All(n => n.targetAngleIndex == firstNode.targetAngleIndex),0, firstNode.targetAngleIndex);
                node.blendTime = (float)GetFieldValue(actions.All(n => n.blendTime == firstNode.blendTime),0f, firstNode.blendTime);
            }

            cameraAnglePrefab = graph.CutsceneEditorManager.cameraAngles;
            if (cameraAnglePrefab == null)
            {
                EditorGUILayout.HelpBox("The Camera angle is null, please attatch the camera angle prefab to cutscene graph editor", MessageType.Warning);
                return;
            }

            GUILayout.BeginHorizontal();
            GameObject ObjectToFocus = null;
            if (node.objectSource == ObjectSource.AssignObject)
                ObjectToFocus = (GameObject)EditorGUILayout.ObjectField("Object To Focus", node.ObjectToFocus, typeof(GameObject), true);
            else
            {
                GUILayout.Label("Object To Focus");
                GUILayout.Label(new GUIContent("Player", "Player-tagged object"));
            }
            var objectSource = (ObjectSource)EditorGUILayout.EnumPopup(node.objectSource, GUILayout.Width(20));
            if (ObjectToFocus != node.ObjectToFocus || objectSource != node.objectSource)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                {
                    n.objectSource = objectSource;
                    n.ObjectToFocus = ObjectToFocus;
                }
            }
            GUILayout.EndHorizontal();
            if (node.objectSource == ObjectSource.AssignObject)
                ValidationWarning(node.ObjectToFocus, "Object is not assigned", graph);
            GUILayout.Space(5);


            var transition = EditorGUILayout.Toggle("Transition", node.transition);
            if (transition != node.transition)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.transition = transition;
            }
            GUILayout.Space(5);

            List<string> angleName = new List<string>();
            List<Transform> cameraAngles = new List<Transform>();
            foreach (Transform angle in cameraAnglePrefab.transform)
            {
                angleName.Add(angle.name);
                cameraAngles.Add(angle);
            }

            var startingAngleIndex = EditorGUILayout.Popup(node.transition ? "Starting Angle" : "Camera Angle", node.startingAngleIndex, angleName.ToArray());
            if (startingAngleIndex != node.startingAngleIndex)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.startingAngleIndex = startingAngleIndex;
            }
            if (node.transition)
            {
                GUILayout.Space(5);
                var targetAngleIndex = EditorGUILayout.Popup("Target Angle", node.targetAngleIndex, angleName.ToArray());
                if (targetAngleIndex != node.targetAngleIndex)
                {
                    UndoGraph(graph);
                    foreach (var n in actions)
                        n.targetAngleIndex = targetAngleIndex;
                }
            }
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
            return (ObjectToFocus == null && objectSource == ObjectSource.AssignObject);
        }
#endif
    }
}