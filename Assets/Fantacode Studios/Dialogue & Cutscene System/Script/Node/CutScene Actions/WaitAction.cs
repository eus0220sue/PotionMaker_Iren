using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class WaitAction : CutSceneAction
    {
        public float waitTime = 0;

        public WaitAction(WaitAction wait = null)
        {
            actionType = ActionType.Wait;
#if UNITY_EDITOR
            if (wait != null)
                EditorUtility.CopySerializedManagedFieldsOnly(wait, this);
#endif
        }

        public override IEnumerator ExecuteAction()
        {
            yield return new WaitForSecondsRealtime(waitTime);
        }

#if UNITY_EDITOR
        public override void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {
            var node = cutSceneNode.currentAction as WaitAction;
            List<WaitAction> actions = new List<WaitAction>() { node };
            if (selectedNodes != null)
            {
                actions = selectedNodes.Select(n => n.currentAction as WaitAction).ToList();
                var firstNode = actions.First();
                node.waitTime = (float)GetFieldValue(actions.All(n => n.waitTime == firstNode.waitTime), 0f, firstNode.waitTime);
            }
            var waitTime = EditorGUILayout.FloatField("Wait Time (seconds)", node.waitTime);
            if (waitTime != node.waitTime)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.waitTime = waitTime;
            }
        }
#endif
    }
}