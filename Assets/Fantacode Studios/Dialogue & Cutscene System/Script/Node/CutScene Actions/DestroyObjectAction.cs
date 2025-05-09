using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;


namespace FC_CutsceneSystem
{
    public class DestroyObjectAction : CutSceneAction
    {
        public GameObject objectToDestroy;

        public DestroyObjectAction(DestroyObjectAction destroyObject = null)
        {
            actionType = ActionType.Destroy;
#if UNITY_EDITOR
            if (destroyObject != null)
                EditorUtility.CopySerializedManagedFieldsOnly(destroyObject, this);
#endif
        }

        public override IEnumerator ExecuteAction()
        {
            if (!ActionValidationWhilePlaying(objectToDestroy))
                yield break;
            Object.Destroy(objectToDestroy);
        }


#if UNITY_EDITOR
        public override void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {
            var node = cutSceneNode.currentAction as DestroyObjectAction;

            List<DestroyObjectAction> actions = new List<DestroyObjectAction>() { node };
            if (selectedNodes != null)
            {
                actions = selectedNodes.Select(n => n.currentAction as DestroyObjectAction).ToList();
                var firstNode = actions.First();
                node.objectToDestroy = (GameObject)GetFieldValue(actions.All(n => n.objectToDestroy == firstNode.objectToDestroy), null, firstNode.objectToDestroy);
            }

            var objectToDestroy = (GameObject)EditorGUILayout.ObjectField("GameObject", node.objectToDestroy, typeof(GameObject), true);
            if (objectToDestroy != node.objectToDestroy)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.objectToDestroy = objectToDestroy;
            }
            ValidationWarning(node.objectToDestroy,"Object is not assigned", graph);
        }

        public override bool Validation()
        {
            return (objectToDestroy == null);
        }
#endif
    }
}