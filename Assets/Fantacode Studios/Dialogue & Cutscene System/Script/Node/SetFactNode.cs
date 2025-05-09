using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;


namespace FC_CutsceneSystem
{

    [Serializable]
    public class SetFactNode : NodeBase
    {
        [HideInInspector]
        public string factKey;
        [HideInInspector]
        public bool factState = false;

        public SetFactNode(SetFactNode node = null)
        {
            NodeName = "SetFact";
            nodeIndicator = "SetfactIndicator";
            if (node != null)
            {
                factKey = node.factKey;
                factState = node.factState;
            }
        }

#if UNITY_EDITOR
        public override void InitNode()
        {
            base.InitNode();
            nodeType = NodeType.SetFact;
            output.connectionType = ConnectionType.Multiple;
            input.connectionType = ConnectionType.Multiple;
        }

        public override void UpdateNodeGUI(Event e, Rect viewRect, GUISkin viewSkin)
        {
            nodeStyle = IsSelected ? "FactNodeSelected" : "FactNodeDefault";
            toggleStyle = factState ? "ToggleTrue" : "ToggleFalse";
            base.UpdateNodeGUI(e, viewRect, viewSkin);
            GUILayout.BeginArea(new Rect(NodeRect.x + 25, NodeRect.y + (NodeRect.height * 0.46f), NodeRect.width * .8f, NodeRect.height * .8f));
            EditorGUILayout.LabelField("Enter Fact", viewSkin.GetStyle("Label"));
            GUILayout.EndArea();
            inputFieldRect = new Rect(NodeRect.x + 25, NodeRect.y + (NodeRect.height * 0.6f), NodeRect.width * .8f, NodeRect.height * .19f);
            GUILayout.BeginArea(inputFieldRect);
            GUILayout.BeginHorizontal();
            factKey = (string)parentGraph.SetField(factKey, EditorGUILayout.TextField(factKey, viewSkin.GetStyle("TextField"), GUILayout.Height(NodeRect.height * .19f), GUILayout.Width(169)));
            factState = (bool)parentGraph.SetField(factState, EditorGUILayout.Toggle("", factState, viewSkin.GetStyle(toggleStyle), GUILayout.Height(NodeRect.height * .19f), GUILayout.Width(NodeRect.height * .19f)));
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
#endif
    }
}