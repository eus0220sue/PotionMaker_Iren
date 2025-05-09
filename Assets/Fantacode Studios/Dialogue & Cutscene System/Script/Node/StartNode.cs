using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class StartNode : NodeBase
    {

        public StartNode()
        {
            NodeName = "Start";
           
        }

#if UNITY_EDITOR
        public override void InitNode()
        {
            base.InitNode();
            nodeType = NodeType.Start;
            output.connectionType = ConnectionType.Multiple;
            NodeRect = new Rect(0, 0, 175, 75);
            parentNodes = new List<string>() { nodeID };
        }

        public override void UpdateNodeGUI(Event e, Rect viewRect, GUISkin viewSkin)
        {
            nodeStyle = IsSelected ? "StartNodeSelected" : "StartNodeDefault";
            base.UpdateNodeGUI(e, viewRect, viewSkin);
        }
#endif
    }

}