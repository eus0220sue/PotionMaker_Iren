using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace FC_CutsceneSystem
{
    [Serializable]
    public class TriggerEventNode : NodeBase
    {
        [HideInInspector]
        public UnityEvent Event = new UnityEvent();

        public TriggerEventNode(TriggerEventNode eventTriggerNode = null)
        {
            NodeName = "Trigger Event";
            nodeIndicator = "EventTriggerIndicator";
#if UNITY_EDITOR
            if (eventTriggerNode != null)
                EditorUtility.CopySerializedManagedFieldsOnly(eventTriggerNode.Event, Event);
#endif
        }
#if UNITY_EDITOR
        public override void InitNode()
        {
            base.InitNode();
            nodeType = NodeType.TriggerEvent;
            input.connectionType = ConnectionType.Multiple;
            output.connectionType = ConnectionType.Multiple;
            NodeRect = new Rect(0, 0, 180, 100);
        }

        public override void UpdateNodeGUI(Event e, Rect viewRect, GUISkin viewSkin)
        {
            nodeStyle = IsSelected ? "EventTriggerNodeSelected" : "EventTriggerNodeDefault";
            base.UpdateNodeGUI(e, viewRect, viewSkin);
        }
#endif
    }


}