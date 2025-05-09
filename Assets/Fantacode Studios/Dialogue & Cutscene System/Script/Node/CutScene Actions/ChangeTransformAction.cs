using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace FC_CutsceneSystem
{
    public class ChangeTransformAction : CutSceneAction
    {
        public GameObject objectToChangePos;

        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale = Vector3.one;
        public Vector3 localPosition;
        public Vector3 localRotation;


        public Transform transform;

        public bool changePosition;
        public bool changeRotation;
        public bool changeScale;
        public bool changeLocalPosition;
        public bool changeLocalRotation;

        public TransformSource transformSource;
        public ObjectSource objectSource1;
        public ObjectSource objectSource2;

        public ChangeTransformAction(ChangeTransformAction changeTransformAction = null)
        {
            actionType = ActionType.ChangeTransform;
#if UNITY_EDITOR
            if (changeTransformAction != null)
                EditorUtility.CopySerializedManagedFieldsOnly(changeTransformAction, this);
#endif
        }

        public override IEnumerator ExecuteAction()
        {
            if (objectSource1 == ObjectSource.UsePlayer)
                objectToChangePos = GameObject.FindGameObjectWithTag("Player");

            if (!ActionValidationWhilePlaying(objectToChangePos))
                yield break;

            if (transformSource == TransformSource.SetValues)
            {
                if (changePosition) objectToChangePos.transform.position = position;
                if (changeRotation) objectToChangePos.transform.eulerAngles = rotation;
                if (changeScale) objectToChangePos.transform.localScale = scale;
                if (changeLocalPosition) objectToChangePos.transform.localPosition = localPosition;
                if (changeLocalRotation) objectToChangePos.transform.localEulerAngles = localRotation;
            }
            else
            {
                
                if (objectSource2 == ObjectSource.UsePlayer)
                    transform = GameObject.FindGameObjectWithTag("Player").transform;
                if (!ActionValidationWhilePlaying(transform))
                    yield break;
                objectToChangePos.transform.position = transform.position;
                objectToChangePos.transform.eulerAngles = transform.eulerAngles;
            }
            yield break;
        }


#if UNITY_EDITOR
        public override void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {
            var node = cutSceneNode.currentAction as ChangeTransformAction;

            List<ChangeTransformAction> actions = new List<ChangeTransformAction>() { node };
            if (selectedNodes != null)
            {
                actions = selectedNodes.Select(n => n.currentAction as ChangeTransformAction).ToList();
                var firstNode = actions.First();
                node.objectToChangePos = (GameObject)GetFieldValue(actions.All(n => n.objectToChangePos == firstNode.objectToChangePos), null, firstNode.objectToChangePos);
                node.objectSource1 = (ObjectSource)GetFieldValue(actions.All(n => n.objectSource1 == firstNode.objectSource1), ObjectSource.AssignObject, firstNode.objectSource1);
                node.transformSource = (TransformSource)GetFieldValue(actions.All(n => n.transformSource == firstNode.transformSource), TransformSource.SetValues, firstNode.transformSource);
                node.transform = (Transform)GetFieldValue(actions.All(n => n.transform == firstNode.transform), null, firstNode.transform);
                node.objectSource2 = (ObjectSource)GetFieldValue(actions.All(n => n.objectSource2 == firstNode.objectSource2), ObjectSource.AssignObject, firstNode.objectSource2);

                node.changePosition = (bool)GetFieldValue(actions.All(n => n.changePosition == firstNode.changePosition), false, firstNode.changePosition);
                node.position.x = (float)GetFieldValue(actions.All(n => n.position.x == firstNode.position.x),0f, firstNode.position.x);
                node.position.y = (float)GetFieldValue(actions.All(n => n.position.y == firstNode.position.y), 0f, firstNode.position.y);
                node.position.z = (float)GetFieldValue(actions.All(n => n.position.z == firstNode.position.z), 0f, firstNode.position.z);
                node.changeRotation = (bool)GetFieldValue(actions.All(n => n.changeRotation == firstNode.changeRotation), false, firstNode.changeRotation);
                node.rotation.x = (float)GetFieldValue(actions.All(n => n.rotation.x == firstNode.rotation.x), 0f, firstNode.rotation.x);
                node.rotation.y = (float)GetFieldValue(actions.All(n => n.rotation.y == firstNode.rotation.y), 0f, firstNode.rotation.y);
                node.rotation.z = (float)GetFieldValue(actions.All(n => n.rotation.z == firstNode.rotation.z), 0f, firstNode.rotation.z);
                node.changeScale = (bool)GetFieldValue(actions.All(n => n.changeScale == firstNode.changeScale), false, firstNode.changeScale);
                node.scale.x = (float)GetFieldValue(actions.All(n => n.scale.x == firstNode.scale.x), 0f, firstNode.scale.x);
                node.scale.y = (float)GetFieldValue(actions.All(n => n.scale.y == firstNode.scale.y), 0f, firstNode.scale.y);
                node.scale.z = (float)GetFieldValue(actions.All(n => n.scale.z == firstNode.scale.z), 0f, firstNode.scale.z);
                node.changeLocalPosition = (bool)GetFieldValue(actions.All(n => n.changeLocalPosition == firstNode.changeLocalPosition), false, firstNode.changeLocalPosition);
                node.localPosition.x = (float)GetFieldValue(actions.All(n => n.localPosition.x == firstNode.localPosition.x), 0f, firstNode.localPosition.x);
                node.localPosition.y = (float)GetFieldValue(actions.All(n => n.localPosition.y == firstNode.localPosition.y), 0f, firstNode.localPosition.y);
                node.localPosition.z = (float)GetFieldValue(actions.All(n => n.localPosition.z == firstNode.localPosition.z), 0f, firstNode.localPosition.z);
                node.changeLocalRotation = (bool)GetFieldValue(actions.All(n => n.changeLocalRotation == firstNode.changeLocalRotation), false, firstNode.changeLocalRotation);
                node.localRotation.x = (float)GetFieldValue(actions.All(n => n.localRotation.x == firstNode.localRotation.x), 0f, firstNode.localRotation.x);
                node.localRotation.y = (float)GetFieldValue(actions.All(n => n.localRotation.y == firstNode.localRotation.y), 0f, firstNode.localRotation.y);
                node.localRotation.z = (float)GetFieldValue(actions.All(n => n.localRotation.z == firstNode.localRotation.z), 0f, firstNode.localRotation.z);
            }


            GUILayout.BeginHorizontal();
            GameObject objectToChangePos = null;
            if (node.objectSource1 == ObjectSource.AssignObject)
                objectToChangePos = (GameObject)EditorGUILayout.ObjectField("Object To Change", node.objectToChangePos, typeof(GameObject), true);
            else
            {
                GUILayout.Label("Object To Change");
                GUILayout.Label(new GUIContent("Player", "Player-tagged object"));
            }
            var objectSource1 = (ObjectSource)EditorGUILayout.EnumPopup(node.objectSource1, GUILayout.Width(20));
            if (objectToChangePos != node.objectToChangePos || objectSource1 != node.objectSource1)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                {
                    n.objectToChangePos = objectToChangePos;
                    n.objectSource1 = objectSource1;
                }
            }
            GUILayout.EndHorizontal();
            if (node.objectSource1 == ObjectSource.AssignObject)
                ValidationWarning(node.objectToChangePos, "Object is not assigned", graph);
            GUILayout.Space(5);
            var transformSource = (TransformSource)EditorGUILayout.EnumPopup(node.transformSource, GUILayout.Width(Screen.width * 0.3f));
            if (transformSource != node.transformSource)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.transformSource = transformSource;
            }
               
            GUILayout.Space(5);

            if (node.transformSource == TransformSource.SetFromObject)
            {
                GUILayout.BeginHorizontal();
                Transform transform = null;
                if (node.objectSource2 == ObjectSource.AssignObject)
                    transform = (Transform)graph.SetField(node.transform, (Transform)EditorGUILayout.ObjectField("New Transform", node.transform, typeof(Transform), true));
                else
                {
                    GUILayout.Label("New Transform");
                    GUILayout.Label(new GUIContent("Player Transform", "player-tagged object's Tansform"));
                }
                var objectSource2 = (ObjectSource)EditorGUILayout.EnumPopup(node.objectSource2, GUILayout.Width(20));
                if (transform != node.transform || objectSource2 != node.objectSource2)
                {
                    UndoGraph(graph);
                    foreach (var n in actions)
                    {
                        n.transform = transform;
                        n.objectSource2 = objectSource2;
                    }
                }
                GUILayout.EndHorizontal();
                if (node.objectSource2 == ObjectSource.AssignObject)
                    ValidationWarning(node.transform, "Object is not assigned", graph);
                GUILayout.Space(5);
            }

            else if (node.transformSource == TransformSource.SetValues)
            {
                //position
                GUILayout.BeginHorizontal();
                var changePosition = EditorGUILayout.Toggle("Position", node.changePosition);
                if (changePosition != node.changePosition)
                {
                    UndoGraph(graph);
                    foreach (var n in actions)
                        n.changePosition = changePosition;
                }
                if (node.changePosition)
                {
                    var position = EditorGUILayout.Vector3Field("", node.position);
                    if (position != node.position)
                    {
                        UndoGraph(graph);
                        foreach (var n in actions)
                            n.position = position;
                    }
                }
                GUILayout.EndHorizontal();

                //rotation
                GUILayout.BeginHorizontal();
                var changeRotation = EditorGUILayout.Toggle("Rotation", node.changeRotation);
                if (changeRotation != node.changeRotation)
                {
                    UndoGraph(graph);
                    foreach (var n in actions)
                        n.changeRotation = changeRotation;
                }
                if (node.changeRotation)
                {
                    var rotation = EditorGUILayout.Vector3Field("", node.rotation);
                    if (rotation != node.rotation)
                    {
                        UndoGraph(graph);
                        foreach (var n in actions)
                            n.rotation = rotation;
                    }
                }
                GUILayout.EndHorizontal();

                //scale
                GUILayout.BeginHorizontal();
                var changeScale = EditorGUILayout.Toggle("Scale", node.changeScale);
                if (changeScale != node.changeScale)
                {
                    UndoGraph(graph);
                    foreach (var n in actions)
                        n.changeScale = changeScale;
                }
                if (node.changeScale)
                {
                    var scale = EditorGUILayout.Vector3Field("", node.scale);
                    if (scale != node.scale)
                    {
                        UndoGraph(graph);
                        foreach (var n in actions)
                            n.scale = scale;
                    }
                }
                GUILayout.EndHorizontal();

                //local position
                GUILayout.BeginHorizontal();
                var changeLocalPosition = EditorGUILayout.Toggle("Local Position", node.changeLocalPosition);
                if (changeLocalPosition != node.changeLocalPosition)
                {
                    UndoGraph(graph);
                    foreach (var n in actions)
                        n.changeLocalPosition = changeLocalPosition;
                }
                if (node.changeLocalPosition)
                {
                    var localPosition = EditorGUILayout.Vector3Field("", node.localPosition);
                    if (localPosition != node.localPosition)
                    {
                        UndoGraph(graph);
                        foreach (var n in actions)
                            n.localPosition = localPosition;
                    }
                }
                GUILayout.EndHorizontal();

                //local rotation
                GUILayout.BeginHorizontal();
                var changeLocalRotation = EditorGUILayout.Toggle("Local Rotation", node.changeLocalRotation);
                if (changeLocalRotation != node.changeLocalRotation)
                {
                    UndoGraph(graph);
                    foreach (var n in actions)
                        n.changeLocalRotation = changeLocalRotation;
                }
                if (node.changeLocalRotation)
                {
                    var localRotation = EditorGUILayout.Vector3Field("", node.localRotation);
                    if (localRotation != node.localRotation)
                    {
                        UndoGraph(graph);
                        foreach (var n in actions)
                            n.localRotation = localRotation;
                    }
                }
                GUILayout.EndHorizontal();
            }
        }

        public override bool Validation()
        {
            return (objectToChangePos == null && objectSource1 == ObjectSource.AssignObject)
                     || (transform == null && objectSource2 == ObjectSource.AssignObject && transformSource == TransformSource.SetFromObject);
        }
#endif
    }
}