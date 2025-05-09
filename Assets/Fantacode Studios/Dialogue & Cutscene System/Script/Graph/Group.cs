using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;


namespace FC_CutsceneSystem
{

    [System.Serializable]
    public class Group
    {
        [SerializeReference] public List<NodeBase> nodeList = new List<NodeBase>();
        public Rect groupRect;
        public bool isDrag = false;

        [SerializeField] string groupName = "";

        public void DrawGroupBox(Event e, CutsceneGraph curGraph)
        {
            AddNodesToGroup(curGraph);
#if UNITY_EDITOR
            GUI.Box(groupRect, "", CutsceneEditorManager.instance.GetEditorSkinns().GetStyle("GroupBG"));
            GUILayout.BeginArea(groupRect);
            groupName = (string)curGraph.SetField(groupName, EditorGUILayout.TextArea(groupName, CutsceneEditorManager.instance.GetEditorSkinns().GetStyle("Null"), GUILayout.Width(groupRect.width - 40), GUILayout.Height(100)));
            CutsceneEditorManager.SetPlaceHolder(groupName, "Enter Comment", Color.black, 25);
            GUILayout.EndArea();
            GUI.Box(new Rect(groupRect.x, groupRect.y, groupRect.width, 100), "", CutsceneEditorManager.instance.GetEditorSkinns().GetStyle("GroupTitleTextField"));


            if (GUI.Button(new Rect(groupRect.x + groupRect.width - 40, groupRect.y, 40, 40), "", CutsceneEditorManager.instance.GetEditorSkinns().GetStyle("CloseButton")))
            {
                Undo.RecordObject(curGraph, "Remove Group");
                curGraph.groups.Remove(this);
            }

#endif
            if (isDrag && e.type == EventType.MouseDrag && e.button == 0 && e.isMouse && !curGraph.isCreatingConnection)
            {

#if UNITY_EDITOR
                Undo.RecordObject(curGraph, "Update Group Rect");
#endif
                var prevRect = groupRect;
                groupRect.x += e.delta.x;
                groupRect.y += e.delta.y;

                groupRect.x = Mathf.Clamp(groupRect.x, 0, Mathf.Infinity);
                groupRect.y = Mathf.Clamp(groupRect.y, 0, Mathf.Infinity);

                var difX = groupRect.x - prevRect.x;
                var difY = groupRect.y - prevRect.y;

                foreach (var node in nodeList)
                {
                    node.nodeRect.x += difX;
                    node.nodeRect.y += difY;
                }
            }
        }

        void AddNodesToGroup(CutsceneGraph graph)
        {
            for (int i = 0; i < graph.nodes.Count; i++)
            {
                if (graph.nodes[i].IsSelected && groupRect.OverlapRect(graph.nodes[i].nodeRect) && !nodeList.Contains(graph.nodes[i]))
                    nodeList.Add(graph.nodes[i]);
                if (!groupRect.OverlapRect(graph.nodes[i].nodeRect) && nodeList.Contains(graph.nodes[i]))
                    nodeList.Remove(graph.nodes[i]);
            }
        }
        public void DrawCloseButton(CutsceneGraph curGraph)
        {

        }
    }
}