using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FC_CutsceneSystem
{

    public class RandomNode : NodeBase
    {
        public RandomNode()
        {
            NodeName = "Random";
            nodeIndicator = "RandomIndicator";
        }
#if UNITY_EDITOR
        public override void InitNode()
        {
            base.InitNode();
            nodeType = NodeType.Random;
            input.connectionType = ConnectionType.Multiple;
            output.connectionType = ConnectionType.Multiple;
            NodeRect = new Rect(0, 0, 180, 100);
        }

        public override void UpdateNodeGUI(Event e, Rect viewRect, GUISkin viewSkin)
        {
            nodeStyle = IsSelected ? "RandomNodeSelected" : "RandomNodeDefault";
            base.UpdateNodeGUI(e, viewRect, viewSkin);
        }
#endif
    }
}
