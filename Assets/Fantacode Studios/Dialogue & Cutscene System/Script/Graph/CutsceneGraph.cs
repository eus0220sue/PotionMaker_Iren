using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace FC_CutsceneSystem
{
    public class CutsceneGraph : MonoBehaviour
    {
        [Tooltip("The cutscene will start from this camera. If no camera is specified, the cutscene will set the current main camera (the MainCamera tagged Camera) to cutscene starting camera")]
        public Camera startingCamera;

        [HideInInspector] 
        public string graphID;

        [HideInInspector]
        [SerializeReference] 
        public List<NodeBase> nodes;

        [HideInInspector]
        public bool IsExecuting = false;

        //[Tooltip("If you enable Cutscene Priority, the cutscene will only run if nothing else is running. Otherwise this cutscene will play when triggered. However, keep in mind that playing two cutscenes with dialogue at the same time will result in a conflict.")]
        //public bool cutscenePriority = false;


        #region public variables
        [HideInInspector]
        public string graphName = "New Graph";


        [HideInInspector]
        public CutsceneEditorManager cutsceneEditorManager;
        [HideInInspector]
        [SerializeReference] public List<Group> groups = new List<Group>();

        [HideInInspector]
        [SerializeReference] public NodeBase selectedNode; 
        [HideInInspector]
        public Rect windowRect;

        [HideInInspector]
        public bool drawSelectionBox = false;
        [HideInInspector]
        public bool multipleNodeSelection = false;    //selecting nodes using selection box
        [HideInInspector]
        public bool multiSelection = false;             //selecting nodes using keyboard input(left control key)
        [HideInInspector]
        public bool isCreatingConnection = false;
        [HideInInspector]
        public Vector2 windowOrigin = Vector2.zero;
        [HideInInspector]
        public bool Grid = true;
        [HideInInspector]
        public float zoom = 1.0f;
        [HideInInspector]
        public bool validation;

        object activeObject = null;


        [HideInInspector]
        public Port selectedConnectionPort;

        [HideInInspector]
        public List<NodeBase> multipleInputsNodes = new List<NodeBase>();

        Vector2 selectionBoxStartPos;     //selection box
        [HideInInspector]
        public Port connectingPort;

        [HideInInspector]
        public bool isQuickConnect;

        public CutsceneEditorManager CutsceneEditorManager
        {
            get
            {
                return cutsceneEditorManager == null ? FindObjectOfType<CutsceneEditorManager>() : cutsceneEditorManager;
            }
            set { cutsceneEditorManager = value; }
        }

        public object ActiveObject
        {
            get { return activeObject; }
            set
            {
                activeObject = value;
            }
        }

        #endregion
#if UNITY_EDITOR
        private void OnEnable()
        {
            multiSelection = false;
            if (nodes == null)
                nodes = new List<NodeBase>();
        }

        #region main methods
        public void UpdateGraphGUI(Event e, Rect viewRect, GUISkin viewSkin)  
        {
            if (nodes == null)
                nodes = new List<NodeBase>();
            graphName = graphName != gameObject.name? gameObject.name:graphName;
            if (isCreatingConnection)
                DrawConnectionModeToMouse(e.mousePosition);
            if (nodes.Count > 0)
            { 
                ProcessEvents(e, viewRect);
                nodes.ForEach(n => n.DrawLines());
                nodes.ForEach(n => n.UpdateNodeGUI(e, viewRect, viewSkin));


                //nodes.ForEach(n =>  n.UpdateNodeGUI(e, viewRect, viewSkin));
                //if (selectedNode != null && selectedNode.IsSelected)
                //    selectedNode.UpdateNodeGUI(null, viewRect, viewSkin);
            }
            else
                ActiveObject = null;

            var activeNode = nodes.FirstOrDefault(n => n.IsSelected);
            if (activeNode != null)
                ActiveObject = activeNode;
            else if (ActiveObject != null && ActiveObject.GetType() != typeof(PortConnection))
                ActiveObject = null;
        }

        void ProcessEvents(Event e, Rect viewRect) 
        {
            if (e.button == 0 && e.isMouse)
            {
                //MOUSE DOWN OPERATION
                if (e.type == EventType.MouseDown)
                    MouseDown(e);

                //MOUSE UP OPERATION
                else if (e.type == EventType.MouseUp)
                    MouseUp();
            }
            var rect = new Rect(viewRect.x + 100, viewRect.y + 100, viewRect.width - 200, viewRect.height - 200);
            if (!viewRect.Contains(e.mousePosition))
            {

                //windowOrigin += new Vector2(40 * Time.deltaTime, 0);
                //if(e.type == EventType.MouseUp)
                    drawSelectionBox = false;
                isCreatingConnection = false;
            }

        }
        #endregion

        #region mouse events
        void MouseDown(Event e)
        {
            selectionBoxStartPos = e.mousePosition;   //for selectBox Creation
            selectedNode = null;
#if UNITY_EDITOR
            Selection.activeObject = this;
#endif
            //check the clickednode is already selected or not
            bool clickedOnSelectedNode = nodes.Any(h => h.IsSelected && h.NodeRect.Contains(e.mousePosition));

            if (!clickedOnSelectedNode)
                multipleNodeSelection = false;
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                var node = nodes[i];
                //CLICKED ON OUTPUT PORT
                if (node.output.Rect.Contains(e.mousePosition))
                {
                    StartConnection(node);
                    break;
                }
                //CLICKED ON NODE
                else if (node.NodeRect.Contains(e.mousePosition))
                {
                    SetSelectedNode(node);
                    break;
                }
            }

            if (selectedNode == null && !isCreatingConnection && e.button != 2)
            {
                nodes.ForEach(n => n.SetIsSelected(false));
                drawSelectionBox = true;
            }
            for (int i = groups.Count - 1; i >= 0; i--)
            {
                var group = groups[i];

                //CLICKED ON GROUP
                if (group.groupRect.Contains(e.mousePosition) && !HasSelectedNodes())
                {
                    foreach (var g in groups)
                        g.isDrag = false;
                    group.isDrag = true;
                    drawSelectionBox = false;
                    break;
                }
                else
                {
                    group.isDrag = false;
                }
            }
        }

        public void MouseUp()
        {
            if (nodes.All(n => !n.inputFieldRect.Contains(Event.current.mousePosition)))
                GUI.FocusControl(null);
            if (!HasSelectedNodes())
            {
                ActiveObject = null;
#if UNITY_EDITOR
                Selection.activeObject = this;
#endif
            }
            drawSelectionBox = false;
            if (isCreatingConnection)
                ConnectNodes();
        }
        #endregion



        public void DeselectNodes(NodeBase exceptNode = null)
        {
            foreach (var node in nodes)
            {
                if (exceptNode == null)
                    node.SetIsSelected(false);
                else if (node.nodeID != exceptNode.nodeID)
                    node.SetIsSelected(false);
            }
        }

        void SetIsSelected(NodeBase node)
        {

            if (multiSelection)
            {
                if (node.IsSelected)
                    node.SetIsSelected(false);
                else
                    node.SetIsSelected(true);
            }
            else
                node.SetIsSelected(true);
        }

        void StartConnection(NodeBase node)
        {
            isCreatingConnection = true;
            connectingPort = node.output;
            DeselectNodes();
        }
        void ConnectNodes()
        {
            NodeBase nodeToConnnect = null;
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                var node = nodes[i];
                if (node.nodeRect.Contains(Event.current.mousePosition))
                {
                    if(node.nodeType != NodeType.Start && CheckValidConnection(node,connectingPort))
                    {
                        nodeToConnnect = node;
                        break;
                    }
                }
            }
            isCreatingConnection = false;

            if (nodeToConnnect != null)
            {
                nodeToConnnect.input.CreateConnection(connectingPort);
                if (CutsceneEditorManager.DB.debugMode)
                {
                    DebugMode();
                }

                if (CutsceneEditorManager.DB.warningPopup && nodeToConnnect.duplicateConnectionWarning && connectingPort.Node.duplicateConnectionWarning)
                {
                    WarningPopup.InitEditorWindow(this);
                }
                connectingPort = null;
            }
            else
            {
                isQuickConnect = true;
            }
            multipleNodeSelection = false;
        }


        public bool CheckValidConnection(NodeBase node,Port connectingPort)
        {
            return (node.output != connectingPort && !node.input.connections.Any(c => c.Port.ID == connectingPort.ID) && //avoid reconnection
                    (connectingPort.Node.nodeType == NodeType.Dialog || node.nodeType != NodeType.Choice)  &&// only connect dialogue to choice
                    
                    /*(node.isMainBranch == connectingPort.Node.isMainBranch || node.input.connections.Count == 0)*/  //only connect to same branch 
                    /*(node.nodeType != NodeType.Dialog || connectingPort.Node.isMainBranch) */  //connection only from mainbranch
                    (!connectingPort.connections.Any(p => p.Node.nodeType == NodeType.Choice) || node.nodeType == NodeType.Choice) && //avoid connection when already a choice connection
                    (!connectingPort.connections.Any(p => p.Node.nodeType != NodeType.Choice) || node.nodeType != NodeType.Choice)  //avoid choice connection when there is already another(except choice) connection
                    /*(!connectingPort.connections.Any(p => p.Node.nodeType == NodeType.Dialog) || node.nodeType != NodeType.Dialog)*/ //avoid 2 dialogue node connection from same parent
                    );
        }



        void SetSelectedNode(NodeBase node)
        {
            if (!node.IsSelected && !multiSelection && !multipleNodeSelection)
                DeselectNodes(node);
            SetIsSelected(node);
            selectedNode = node;
            ActiveObject = selectedNode;
        }
        


        #region utility methods
        void DrawConnectionModeToMouse(Vector2 mousePosition)
        {
            Rect rect = connectingPort.Rect;
            Handles.BeginGUI();
            Vector3 startPos = new Vector3(rect.x + rect.width - 9, rect.y + (rect.height * 0.42f), 0);
            Vector3 endPos = new Vector3(mousePosition.x, mousePosition.y, 0);
            Vector3 startTan = startPos + new Vector3(65, 0, 0);
            Vector3 endTan = endPos - new Vector3(65, 0, 0);
            Handles.DrawBezier(startPos, endPos, startTan, endTan, Color.white, null, 5f);
            Handles.EndGUI();
        }

        public void DrawSelectionBox(Event e, GUISkin viewSkin)
        {
            if (drawSelectionBox && e.button != 1 && e.modifiers != EventModifiers.Alt)
            {
                Vector2 cubeScale = new Vector2(e.mousePosition.x - selectionBoxStartPos.x, e.mousePosition.y - selectionBoxStartPos.y);

                float signX = (Mathf.Sign((float)cubeScale.x) - 1) / 2;
                float signY = (Mathf.Sign((float)cubeScale.y) - 1) / 2;
                float absX = Mathf.Abs(cubeScale.x);
                float absY = Mathf.Abs(cubeScale.y);

                Rect rect = new Rect(selectionBoxStartPos.x + (absX * signX), selectionBoxStartPos.y + (absY * signY), absX, absY);
                GUI.Box(rect, "", viewSkin.GetStyle("SelectionBox"));

                //node selection
                nodes.ForEach((n =>
                {
                    bool isSelected = rect.OverlapRect(n.NodeRect) ? multipleNodeSelection = true : false;
                    n.SetIsSelected(isSelected);
                }));
            }
        }

        public bool HasSelectedNodes()
        {
            if (nodes.Any(n => n.IsSelected))
                return true;
            return false;
        }

        public object SetField(object oldValue, object newValue)
        {
            if (oldValue != null && newValue != null && oldValue.ToString() != newValue.ToString())
            {
                Undo.RegisterCompleteObjectUndo(this, "Update Field");
            }
            return newValue;
        }
        #endregion


        #region debug mode
        public void DebugMode(bool debugMode = true)
        {
            nodes.ForEach(n => { if (n.nodeType != NodeType.Start) n.parentNodes = new List<string>(); n.duplicateConnectionWarning = false; });

            multipleInputsNodes = new List<NodeBase>();

            if (debugMode)
            {
                var startNode = nodes.Where(n => n.nodeType == NodeType.Start).ToList();
                
                SetParentNodes(startNode);
                nodes.ForEach(n => n.check = false);
                SetWarningNodes(multipleInputsNodes);
            }
        }

        public void SetParentNodes(List<NodeBase> nodes)
        {
            List<NodeBase> childNodes = new List<NodeBase>();
            foreach (var node in nodes)
            {
                foreach (var port in node.output.connections)
                {
                    if (!port.Node.check)
                    {
                        if (port.Node.input.connections.Count > 1 && !multipleInputsNodes.Contains(port.Node))
                            multipleInputsNodes.Add(port.Node);
                        childNodes.Add(port.Node);
                        port.Node.check = true;
                    }
                    if (!port.Node.parentNodes.Contains(port.nodeId) && port.Node.nodeType != NodeType.Random)
                        port.Node.parentNodes.Add(port.nodeId);

                    if (port.Node.nodeType == NodeType.Choice || port.Node.nodeType == NodeType.Random || port.conditions.Count > 0)
                        continue;

                    foreach (var id in node.parentNodes)
                    {
                        if (!port.Node.parentNodes.Contains(id))
                            port.Node.parentNodes.Add(id);
                    }
                }
            }

            if (childNodes.Count > 0)
                SetParentNodes(childNodes);
        }

        public void SetWarningNodes(List<NodeBase> nodes)
        {
            foreach (var node in nodes)
            {
                Dictionary<string, NodeBase> allParentNodes = new Dictionary<string, NodeBase>();

                // Loop through all input connections
                for (int i = 0; i < node.input.connections.Count; i++)
                {
                    
                    foreach (var parentNodeId in node.input.connections[i].Node.parentNodes)
                    {
                        allParentNodes.TryGetValue(parentNodeId, out NodeBase existingNode);



                        if (existingNode == null)
                        {
                            allParentNodes.Add(parentNodeId, node.input.connections[i].Node);
                        }
                        else
                        {
                            var connection1 = existingNode.output.connections.FirstOrDefault(c => c.nodeId == node.nodeID);
                            if(connection1 != null && connection1.conditions.Count == 0)  
                                existingNode.duplicateConnectionWarning = true;
                            var connection2 = node.input.connections[i].Node.output.connections.FirstOrDefault(c => c.nodeId == node.nodeID);
                            if (connection2 != null && connection2.conditions.Count == 0)
                                node.input.connections[i].Node.duplicateConnectionWarning = true;
                            node.duplicateConnectionWarning = true;
                        }
                    }

                }
            }
        }
        #endregion
#endif
    }
}