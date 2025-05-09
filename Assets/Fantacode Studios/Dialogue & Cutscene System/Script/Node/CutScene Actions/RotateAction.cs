using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class RotateAction : CutSceneAction
    {
        public Vector3 rotation;
        public GameObject objectToRotate;
        public GameObject rotateTowards;
        public RotationSource rotationSource;
        public float rotateSpeed = 50f;

        public ObjectSource objectSource1;
        public ObjectSource objectSource2;

        public RotateAction(RotateAction rotate = null)
        {
            actionType = ActionType.Rotate;
#if UNITY_EDITOR
            if (rotate != null)
                EditorUtility.CopySerializedManagedFieldsOnly(rotate, this);
#endif
        }

        public override IEnumerator ExecuteAction()
        {
            if (objectSource1 == ObjectSource.UsePlayer)
                objectToRotate = GameObject.FindGameObjectWithTag("Player");

            if (!ActionValidationWhilePlaying(objectToRotate))
                yield break;
            Quaternion rot = Quaternion.identity;

            if (rotationSource == RotationSource.RotateTowardsObject)
            {
                if (objectSource2 == ObjectSource.UsePlayer)
                    rotateTowards = GameObject.FindGameObjectWithTag("Player");
                if (!ActionValidationWhilePlaying(rotateTowards))
                    yield break;
                rot = Quaternion.LookRotation(Vector3.Scale((rotateTowards.transform.position - objectToRotate.transform.position), new Vector3(1, 0, 1)));
            }
            else if (rotationSource == RotationSource.RotationValue)
            {
                rot = Quaternion.Euler(rotation);
            }
            else if (rotationSource == RotationSource.AddRotation)
            {
                var rotEuler = objectToRotate.transform.eulerAngles + rotation;
                rot = Quaternion.Euler(rotEuler);
            }

            while (objectToRotate.transform.rotation != rot)
            {
                objectToRotate.transform.rotation = Quaternion.RotateTowards(objectToRotate.transform.rotation, rot, rotateSpeed * Time.deltaTime);
                yield return null;
            }
            //objectToRotate.transform.rotation = rot;
        }


#if UNITY_EDITOR
        public override void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {
            var node = cutSceneNode.currentAction as RotateAction;

            List<RotateAction> actions = new List<RotateAction>() { node };
            if (selectedNodes != null)
            {
                actions = selectedNodes.Select(n => n.currentAction as RotateAction).ToList();
                var firstNode = actions.First();
                node.objectToRotate = (GameObject)GetFieldValue(actions.All(n => n.objectToRotate == firstNode.objectToRotate), null, firstNode.objectToRotate);
                node.objectSource1 = (ObjectSource)GetFieldValue(actions.All(n => n.objectSource1 == firstNode.objectSource1), ObjectSource.AssignObject, firstNode.objectSource1);
                node.rotationSource = (RotationSource)GetFieldValue(actions.All(n => n.rotationSource == firstNode.rotationSource), RotationSource.AddRotation, firstNode.rotationSource);
                node.rotateTowards = (GameObject)GetFieldValue(actions.All(n => n.rotateTowards == firstNode.rotateTowards), null, firstNode.rotateTowards);
                node.objectSource2 = (ObjectSource)GetFieldValue(actions.All(n => n.objectSource2 == firstNode.objectSource2), ObjectSource.AssignObject, firstNode.objectSource2);
                node.rotateSpeed = (float)GetFieldValue(actions.All(n => n.rotateSpeed == firstNode.rotateSpeed), 0f, firstNode.rotateSpeed);
                node.rotation.x = (float)GetFieldValue(actions.All(n => n.rotation.x == firstNode.rotation.x), 0f, firstNode.rotation.x);
                node.rotation.y = (float)GetFieldValue(actions.All(n => n.rotation.y == firstNode.rotation.y), 0f, firstNode.rotation.y);
                node.rotation.z = (float)GetFieldValue(actions.All(n => n.rotation.z == firstNode.rotation.z), 0f, firstNode.rotation.z);
            }

            GUILayout.BeginHorizontal();
            GameObject objectToRotate = null;
            if (node.objectSource1 == ObjectSource.AssignObject)
                objectToRotate = (GameObject)EditorGUILayout.ObjectField("Object To Rotate", node.objectToRotate, typeof(GameObject), true);
            else
            {
                GUILayout.Label("Object To Rotate");
                GUILayout.Label(new GUIContent("Player", "Player-tagged object"));
            }
            var objectSource1 = (ObjectSource)EditorGUILayout.EnumPopup(node.objectSource1, GUILayout.Width(20));
            if (objectToRotate != node.objectToRotate || objectSource1 != node.objectSource1)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                {
                    n.objectToRotate = objectToRotate;
                    n.objectSource1 = objectSource1;
                }
            }
            GUILayout.EndHorizontal();
            if (node.objectSource1 == ObjectSource.AssignObject)
                ValidationWarning(node.objectToRotate, "Object is not assigned", graph);

            GUILayout.Space(5);
            GUILayout.BeginHorizontal();
            var rotationSource = (RotationSource)EditorGUILayout.EnumPopup(node.rotationSource);

            GameObject rotateTowards = null;
            ObjectSource objectSource2 = ObjectSource.AssignObject;
            Vector3 rotation = Vector3.zero;

            if (node.rotationSource == RotationSource.RotateTowardsObject)
            {
                GUILayout.BeginHorizontal();
                
                if (node.objectSource2 == ObjectSource.AssignObject)
                    rotateTowards = (GameObject)EditorGUILayout.ObjectField(node.rotateTowards, typeof(GameObject), true);
                else
                    GUILayout.Label(new GUIContent("Player Transform", "player-tagged object's Tranform"));
                objectSource2 = (ObjectSource)EditorGUILayout.EnumPopup(node.objectSource2, GUILayout.Width(20));
                GUILayout.EndHorizontal();
                GUILayout.EndHorizontal();
                if (node.objectSource2 == ObjectSource.AssignObject)
                    ValidationWarning(node.rotateTowards, "Object is not assigned", graph);
            }
            else
            {
                rotation = EditorGUILayout.Vector3Field("", node.rotation);
                GUILayout.EndHorizontal();
            }
            if (rotationSource != node.rotationSource || rotateTowards != node.rotateTowards || objectSource2 != node.objectSource2 || rotation != node.rotation)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                {
                    n.rotationSource = rotationSource;
                    n.rotateTowards = rotateTowards;
                    n.objectSource2 = objectSource2;
                    n.rotation = rotation;
                }
            }
            GUILayout.Space(5);

            var rotateSpeed = EditorGUILayout.FloatField("Rotate Speed", node.rotateSpeed);
            if (rotateSpeed != node.rotateSpeed)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.rotateSpeed = rotateSpeed;
            }
        }

        public override bool Validation()
        {
            return (objectToRotate == null && objectSource1 == ObjectSource.AssignObject)
                     || (rotateTowards == null && objectSource2 == ObjectSource.AssignObject && rotationSource == RotationSource.RotateTowardsObject);
        }
#endif
    }
}