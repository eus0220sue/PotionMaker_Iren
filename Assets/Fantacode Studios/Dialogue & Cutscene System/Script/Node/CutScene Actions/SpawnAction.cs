using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace FC_CutsceneSystem
{
    public class SpawnAction : CutSceneAction
    {
        public GameObject objectToSpawn;
        public GameObject parentObject;
        public Vector3 posToInstantiate;
        public Vector3 quatToInstantiate;

        public SpawnAction(SpawnAction instantiateObject = null)
        {
            actionType = ActionType.Spawn;
#if UNITY_EDITOR
            if (instantiateObject != null)
                EditorUtility.CopySerializedManagedFieldsOnly(instantiateObject, this);
#endif
        }

        public override IEnumerator ExecuteAction()
        {
            if (!ActionValidationWhilePlaying(objectToSpawn))
                yield break;
            var spawnedObject = GameObject.Instantiate(objectToSpawn);
            if (parentObject != null)
                spawnedObject.transform.parent = parentObject.transform;
            spawnedObject.transform.localPosition = posToInstantiate;
            spawnedObject.transform.localEulerAngles = quatToInstantiate;
            spawnedObject.name = objectToSpawn.name;
        }

#if UNITY_EDITOR
        public override void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {
            var node = cutSceneNode.currentAction as SpawnAction;

            List<SpawnAction> actions = new List<SpawnAction>() { node };
            if (selectedNodes != null)
            {
                actions = selectedNodes.Select(n => n.currentAction as SpawnAction).ToList();
                var firstNode = actions.First();
                node.objectToSpawn = (GameObject)GetFieldValue(actions.All(n => n.objectToSpawn == firstNode.objectToSpawn), null, firstNode.objectToSpawn);
                node.parentObject = (GameObject)GetFieldValue(actions.All(n => n.parentObject == firstNode.parentObject), null, firstNode.parentObject);
                node.posToInstantiate.x = (float)GetFieldValue(actions.All(n => n.posToInstantiate.x == firstNode.posToInstantiate.x), 0f, firstNode.posToInstantiate.x);
                node.posToInstantiate.y = (float)GetFieldValue(actions.All(n => n.posToInstantiate.y == firstNode.posToInstantiate.y), 0f, firstNode.posToInstantiate.y);
                node.posToInstantiate.z = (float)GetFieldValue(actions.All(n => n.posToInstantiate.z == firstNode.posToInstantiate.z), 0f, firstNode.posToInstantiate.z);
                node.quatToInstantiate.x = (float)GetFieldValue(actions.All(n => n.quatToInstantiate.x == firstNode.quatToInstantiate.x), 0f, firstNode.quatToInstantiate.x);
                node.quatToInstantiate.y = (float)GetFieldValue(actions.All(n => n.quatToInstantiate.y == firstNode.quatToInstantiate.y), 0f, firstNode.quatToInstantiate.y);
                node.quatToInstantiate.z = (float)GetFieldValue(actions.All(n => n.quatToInstantiate.z == firstNode.quatToInstantiate.z), 0f, firstNode.quatToInstantiate.z);
            }

            var objectToSpawn =(GameObject)EditorGUILayout.ObjectField("GameObject",node.objectToSpawn, typeof(GameObject), true);
            if (objectToSpawn != node.objectToSpawn)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.objectToSpawn = objectToSpawn;
            }
            ValidationWarning(node.objectToSpawn, "Object is not assigned", graph);
            GUILayout.Space(5);


            var posToInstantiate = EditorGUILayout.Vector3Field("Position", node.posToInstantiate);
            if (posToInstantiate != node.posToInstantiate)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.posToInstantiate = posToInstantiate;
            }
            GUILayout.Space(5);

            var quatToInstantiate = EditorGUILayout.Vector3Field("Rotation", node.quatToInstantiate);
            if (quatToInstantiate != node.quatToInstantiate)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.quatToInstantiate = quatToInstantiate;
            }
            GUILayout.Space(5);

            var parentObject = (GameObject)EditorGUILayout.ObjectField("Parent Object (optional)", node.parentObject, typeof(GameObject), true);
            if (parentObject != node.parentObject)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.parentObject = parentObject;
            }
        }
        public override bool Validation()
        {
            return (objectToSpawn == null);
        }
#endif
    }
}