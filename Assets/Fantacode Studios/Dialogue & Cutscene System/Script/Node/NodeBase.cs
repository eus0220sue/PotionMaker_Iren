using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FC_CutsceneSystem
{

    [Serializable]
    public class NodeBase
    {
        #region  variables
        [HideInInspector]
        [SerializeField] string nodeName;
        [HideInInspector]
        public string nodeIndicator;
        [HideInInspector]
        public bool check = false;

        [HideInInspector]
        public Port output;
        [HideInInspector]
        public Port input;
        [HideInInspector]
        public string nodeID;

        [HideInInspector]
        [SerializeField] bool isSelected = false;
        [HideInInspector]
        public Rect nodeRect;
        [HideInInspector]
        public Rect inputFieldRect = new Rect();

        [HideInInspector]
        public Vector3 connectionMidPoint;
        [HideInInspector]
        public Vector3 connectionLockPoint;
        [HideInInspector]
        public NodeType nodeType;
        [HideInInspector]
        public CutsceneGraph parentGraph;

        protected string nodeStyle;
        protected string toggleStyle;

        [HideInInspector]
        public List<string> parentNodes = new List<string>();


        [HideInInspector]
        public bool duplicateConnectionWarning;

        [HideInInspector]
        public bool working;

        [HideInInspector]
        public bool warning;



        #endregion
        #region Properties

        public string NodeName
        {
            get { return nodeName; }
            set { nodeName = value; }
        }
        public bool IsSelected
        {
            get { return isSelected; }
            set { isSelected = value; }
        }
        public void SetIsSelected(bool select)
        {
            if (isSelected != select)
            {
#if UNITY_EDITOR
                Undo.RecordObject(parentGraph, "Update Selection");
#endif
                isSelected = select;
            }
        }
        public Rect NodeRect
        {
            get { return nodeRect; }
            set { nodeRect = value; }
        }
        public Vector2 RectPos
        {
            get { return nodeRect.position; }
            set { nodeRect.position = value; }
        }
        public Vector2 RectSize
        {
            get { return nodeRect.size; }
            set { nodeRect.size = value; }
        }
        #endregion


        #region main methods
#if UNITY_EDITOR
        public virtual void InitNode()
        {
            nodeID = Guid.NewGuid().ToString();
            NodeRect = new Rect(10f, 10f, 250f, 150f);
            input = new Port(this);
            output = new Port(this);
        }

        public virtual void UpdateNodeGUI(Event e, Rect viewRect, GUISkin viewSkin)
        {
            DragNode(e);
            if (working)
                nodeStyle = nodeIndicator;
            GUI.Box(NodeRect, NodeName, viewSkin.GetStyle(nodeStyle));

            var yPos = nodeType != NodeType.TriggerEvent && nodeType != NodeType.Random ? 0.3f : 0.4f;
            input.Rect = new Rect(NodeRect.x, NodeRect.y + (NodeRect.height * yPos), 24, 24);
            output.Rect = new Rect(NodeRect.x + NodeRect.width - 17, NodeRect.y + (NodeRect.height * yPos), 24, 24);

            string outputLabel = nodeType != NodeType.Start ? "output" : "";
            GUI.Label(new Rect(output.Rect.x - 50, output.Rect.y, 50, 24), outputLabel, viewSkin.GetStyle("Label"));

            //output button
            GUI.Button(output.Rect, "", viewSkin.GetStyle("Button"));

            //input button
            if (nodeType != NodeType.Start)
                GUI.Button(input.Rect, "\tInput", viewSkin.GetStyle("Button"));

            if (parentGraph.validation && warning)
                GUI.Box(new Rect(nodeRect.x + nodeRect.width - 40, nodeRect.y + 15, 15, 15), "", viewSkin.GetStyle("WarningIcon"));
        }

        public virtual void Validation()
        {

        }
        public void ValidationWarning(UnityEngine.Object obj, string message, CutsceneGraph graph)
        {
            if (graph.validation && obj == null)
                EditorGUILayout.HelpBox(message, MessageType.Warning);
        }
        public void DrawLines()
        {
            for (int i = 0; i < input.connections.Count; i++)
            {
                NodeBase outputNode = input.connections[i].Node;
                if (outputNode != null)
                {
                    Rect outputRect = outputNode.output.Rect;
                    PortConnection connection = outputNode.output.connections.FirstOrDefault(c => c.Node == this);

                    Vector3 startPos = new Vector3(outputRect.x + outputRect.width - 10, outputRect.y + (outputRect.height * 0.42f), 0);
                    Vector3 endPos = new Vector3(input.Rect.x + 5, input.Rect.y + input.Rect.height / 2 - 1.5f, 0);
                    Vector3 startTan = startPos + new Vector3(25, 0, 0);
                    Vector3 endTan = endPos - new Vector3(25, 0, 0);
                    connectionMidPoint = ((startPos + endPos) / 2);
                    //connectionLockPoint = CalculateBezierCurvePoint(.8f, startPos, startTan, endTan, endPos);

                    if (ClickedOnAConnectionLine(Event.current.mousePosition, startTan, endTan, 8) ||
                        ClickedOnAConnectionLine(Event.current.mousePosition,startPos , startTan, 8) ||
                        ClickedOnAConnectionLine(Event.current.mousePosition, endTan, endPos, 8))
                    {
                        parentGraph.ActiveObject = connection;
                        parentGraph.selectedConnectionPort = outputNode.output;
                    }

                    if (nodeType == NodeType.Choice && working)
                        Handles.color = parentGraph.ActiveObject == connection ? Color.cyan : new Color(1.0f, 0.64f, 0.0f);
                    else
                        Handles.color = parentGraph.ActiveObject == connection ? Color.cyan : duplicateConnectionWarning && outputNode.duplicateConnectionWarning && CutsceneEditorManager.instance.DB.debugMode ? Color.red : Color.white;


                    var list = new Vector3[] { startPos, startTan, endTan, endPos };
                    

                    if (connection.conditions.Count > 0)
                        GUI.Label(new Rect(connectionMidPoint.x - 13, connectionMidPoint.y - 13, 26, 26), "", CutsceneEditorManager.instance.GetEditorSkinns().GetStyle("ConnectionLock"));

                    float connectionThickness = 4f;
                    if (ClickedOnAConnectionLine(Event.current.mousePosition, startTan, endTan, 8,true) ||
                        ClickedOnAConnectionLine(Event.current.mousePosition, startPos, startTan, 8,true) ||
                        ClickedOnAConnectionLine(Event.current.mousePosition, endTan, endPos, 8,true))
                    {
                        connectionThickness = 6.5f;
                        if(Handles.color != Color.red)
                            Handles.color = parentGraph.ActiveObject == connection ? Color.cyan : Color.white;
                        if (duplicateConnectionWarning && outputNode.duplicateConnectionWarning)
                            EditorGUI.LabelField(new Rect(Event.current.mousePosition.x+5, Event.current.mousePosition.y+5, 200, 45), " There are multiple connections to the same node. This will result in the node executing multiple times.", EditorStyles.helpBox);
                    }
                    Handles.DrawAAPolyLine(connectionThickness, list);
                }
            }
            if (Event.current.keyCode == KeyCode.Delete)
                RemoveConnetion();
        }
        void RemoveConnetion()
        {
            if (parentGraph.ActiveObject is PortConnection && parentGraph.selectedConnectionPort != null)
            {
                var inp = parentGraph.ActiveObject as PortConnection;
                inp.Node.input.RemoveConnection(parentGraph.selectedConnectionPort);
                if (parentGraph.CutsceneEditorManager.DB.debugMode)
                {
                    parentGraph.DebugMode();
                }
                parentGraph.selectedConnectionPort = null;
                parentGraph.ActiveObject = null;
                
            }
        }

        

        public bool ClickedOnAConnectionLine(Vector2 clickPoint, Vector2 startPos, Vector2 endPos, double tolerance , bool hover = false)
        {
            //Reference  https://stackoverflow.com/questions/17692922/check-is-a-point-x-y-is-between-two-points-drawn-on-a-straight-line
            if (Event.current.type == EventType.MouseUp || hover)
            {
                double minX = Math.Min(endPos.x, startPos.x) - tolerance;
                double maxX = Math.Max(endPos.x, startPos.x) + tolerance;
                double minY = Math.Min(endPos.y, startPos.y) - tolerance;
                double maxY = Math.Max(endPos.y, startPos.y) + tolerance;

                if (clickPoint.x >= maxX || clickPoint.x <= minX || clickPoint.y <= minY || clickPoint.y >= maxY)
                    return false;

                if (endPos.x == startPos.x)
                {
                    if (Math.Abs(endPos.x - clickPoint.x) >= tolerance)
                        return false;
                    return true;
                }

                if (endPos.y == startPos.y)
                {
                    if (Math.Abs(endPos.y - clickPoint.y) >= tolerance)
                        return false;
                    return true;
                }
                double distFromLine = Math.Abs(((startPos.x - endPos.x) * (endPos.y - clickPoint.y)) - ((endPos.x - clickPoint.x) * (startPos.y - endPos.y))) /
                                      Math.Sqrt((startPos.x - endPos.x) * (startPos.x - endPos.x) + (startPos.y - endPos.y) * (startPos.y - endPos.y));

                if (distFromLine >= tolerance)
                    return false;
                else
                    return true;
            }
            return false;
        }
#endif
        Vector3 CalculateBezierCurvePoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            //B(t) = (1 - t)3P0 + 3(1 - t)2tP1 + 3(1 - t)t2P2 + t3P3 , 0 < t < 1
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3 * uu * t * p1;
            p += 3 * u * tt * p2;
            p += ttt * p3;

            return p;
        }

        public Port GetPort(string id)
        {
            if (input.ID == id)
                return input;
            if (output.ID == id)
                return output;
            return null;
        }

        void DragNode(Event e)
        {
            if (isSelected)
            {
                if (e.type == EventType.MouseDrag && !parentGraph.drawSelectionBox && e.button == 0 && e.isMouse && !inputFieldRect.Contains(e.mousePosition))
                {
#if UNITY_EDITOR
                    Undo.RecordObject(this.parentGraph, "Update Node Rect");
#endif
                    var prevRect = RectPos;
                    nodeRect.x += e.delta.x;
                    nodeRect.y += e.delta.y;
                    if (parentGraph.nodes.Any(n => (n.RectPos.x == 0) && n.isSelected && e.delta.x < 0))
                        RectPos = new Vector2(prevRect.x, RectPos.y);
                    if (parentGraph.nodes.Any(n => (n.RectPos.y == 0) && n.isSelected && e.delta.y < 0))
                        RectPos = new Vector2(RectPos.x, prevRect.y);
                    nodeRect.x = Mathf.Clamp(nodeRect.x, 0, Mathf.Infinity);
                    nodeRect.y = Mathf.Clamp(nodeRect.y, 0, Mathf.Infinity);
                }
            }
        }
        #endregion
    }

    [Serializable]
    public class Localization
    {
        public string language;
        public string languageText;
        public AudioClip audioClip;
    }
}