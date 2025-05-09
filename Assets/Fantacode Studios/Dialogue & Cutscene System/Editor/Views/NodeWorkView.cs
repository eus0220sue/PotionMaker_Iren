using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class NodeWorkView : ViewBase
    {
        #region Protected Variables
        Vector2 mousePos;

        /// <summary>
        /// Node selected by right click
        /// </summary>
        NodeBase selectedNode = null;
        List<NodeBase> usedNodes = new List<NodeBase>();
        PortConnection portConnection;
        #endregion

        #region constuctor
        public NodeWorkView() : base("work view") { }
        #endregion

        #region Main method
        public override void UpdateView(Event e, CutsceneGraph curGraph)
        {

            base.UpdateView(e, curGraph);
            if (curGraph != null)
            {
                for (int i = 0; i < curGraph.groups.Count; i++)
                    curGraph.groups[i].DrawGroupBox(e, curGraph);

                curGraph.UpdateGraphGUI(e, viewRect, viewSkin);
            }
            ProcessEvents(e);
        }
       
        public override void ProcessEvents(Event e)
        {
            


            if (curGraph != null)
            {
                //DrawSelectionBox
                curGraph.DrawSelectionBox(e, viewSkin);
                KeyBoardInputs(e);
            }

            base.ProcessEvents(e);

           

            if (e.button == 1 && e.type == EventType.MouseUp)
            {
                mousePos = e.mousePosition;
                selectedNode = null;

                //getting selectedNode
                if (curGraph != null)
                {
                    selectedNode = curGraph.nodes.FirstOrDefault(n => n.NodeRect.Contains(mousePos));
                    if (selectedNode != null && !selectedNode.IsSelected)
                    {
                        selectedNode.IsSelected = true;
                        curGraph.DeselectNodes(selectedNode);
                    }
                }

                

                if (selectedNode == null)
                    ProcessContextMenu(e, 0);
                else if (selectedNode.nodeType != NodeType.Start)
                {
                    if (selectedNode.nodeType == NodeType.Dialog)
                        ProcessContextMenu(e, 1);
                    else
                        ProcessContextMenu(e, 2);
                }
                if(curGraph != null)
                    curGraph.isQuickConnect = false;
            }
            else if (curGraph != null && curGraph.isQuickConnect)
            {
                mousePos = e.mousePosition;
                ProcessContextMenu(e, 3);
                curGraph.isQuickConnect = false;
            }
        }
        #endregion

        #region utility methods
        void ProcessContextMenu(Event e, int contextID)
        {
            //contextID == 0 :- clicking outside the node
            //contextID == 1 :- clicking inside the dialognode
            //contextID == 2 :- clicking inside the choicenode

            GenericMenu menu = new GenericMenu();

            if (curGraph != null)
            {
                EditorZoom.End();

                if (contextID == 3)
                {
                    menu.AddItem(new GUIContent("Dialogue Node"), false, ContextCallBack, ("Node", NodeType.Dialog, curGraph.connectingPort));
                    if (curGraph.connectingPort.Node.nodeType == NodeType.Dialog)
                        menu.AddItem(new GUIContent("Add Choice"), false, ContextCallBack, ("Node", NodeType.Choice, curGraph.connectingPort));
                    menu.AddItem(new GUIContent("Cutscene Node"), false, ContextCallBack, ("Node", NodeType.Cutscene, curGraph.connectingPort));
                    menu.AddItem(new GUIContent("Fact Node"), false, ContextCallBack, ("Node", NodeType.SetFact, curGraph.connectingPort));
                    menu.AddItem(new GUIContent("TriggerEvent Node"), false, ContextCallBack, ("Node", NodeType.TriggerEvent, curGraph.connectingPort));
                    menu.AddItem(new GUIContent("Random Node"), false, ContextCallBack, ("Node", NodeType.Random, curGraph.connectingPort));
                }
                else
                {
                   

                    if (curGraph.HasSelectedNodes() || selectedNode != null)
                    {
                        menu.AddItem(new GUIContent("Cut"), false, ContextCallBack, "16");
                        menu.AddItem(new GUIContent("Copy"), false, ContextCallBack, "10");
                        menu.AddItem(new GUIContent("Duplicate"), false, ContextCallBack, "17");
                    }
                    else
                    {
                        menu.AddDisabledItem(new GUIContent("Cut"));
                        menu.AddDisabledItem(new GUIContent("Copy"));
                        menu.AddDisabledItem(new GUIContent("Duplicate"));
                    }
                    if (curGraph.CutsceneEditorManager.selectedNodes.Count > 0)
                        menu.AddItem(new GUIContent("Paste"), false, ContextCallBack, "11");
                    else
                        menu.AddDisabledItem(new GUIContent("Paste"));
                    menu.AddSeparator("");
                }
                if (contextID == 0)
                {
                    menu.AddItem(new GUIContent("Create New Node/Fact Node"), false, ContextCallBack, "8");
                    menu.AddItem(new GUIContent("Create New Node/Dialogue Node"), false, ContextCallBack, "3");
                    menu.AddItem(new GUIContent("Create New Node/Cutscene Node"), false, ContextCallBack, "9");
                    menu.AddItem(new GUIContent("Create New Node/TriggerEvent Node"), false, ContextCallBack, "21");
                    menu.AddItem(new GUIContent("Create New Node/Random Node"), false, ContextCallBack, "23");
                }
                if (curGraph.HasSelectedNodes())
                {

                    menu.AddItem(new GUIContent("Create Group"), false, ContextCallBack, "15");
                }
            }
            if (contextID == 0)
            {
                menu.AddItem(new GUIContent("Create Cutscene Graph"), false, ContextCallBack, "2");

                if (curGraph != null)
                {
                    menu.AddSeparator("");
                    menu.AddItem(new GUIContent("Home"), false, ContextCallBack, "12");
                    menu.AddItem(new GUIContent("Select/All"), false, ContextCallBack, "19");
                    menu.AddItem(new GUIContent("Select/UsedNodes"), false, ContextCallBack, "20");
                    menu.AddItem(new GUIContent("Select/Unused Nodes"), false, ContextCallBack, "18");
                    menu.AddItem(new GUIContent("Arrange Nodes/Diagonally"), false, ContextCallBack, "13");
                    menu.AddItem(new GUIContent("Arrange Nodes/Horizontally"), false, ContextCallBack, "14");
                }
            }

            else if (contextID == 1 || contextID == 2)
            {
                if (curGraph != null)
                {
                    if (contextID == 1)
                    {
                        menu.AddSeparator("");
                        menu.AddItem(new GUIContent("Add Choice/Add 1 Choice"), false, ContextCallBack, "5");
                        menu.AddItem(new GUIContent("Add Choice/Add 2 Choice"), false, ContextCallBack, "6");
                        menu.AddItem(new GUIContent("Add Choice/Add 3 Choice"), false, ContextCallBack, "7");
                    }
                }
            }
            if (curGraph != null && curGraph.ActiveObject is PortConnection && curGraph.selectedConnectionPort != null && !curGraph.isQuickConnect)
            {
                menu.AddItem(new GUIContent("Delete Connection"), false, ContextCallBack, "22");
                portConnection = curGraph.ActiveObject as PortConnection;
            }
            if (curGraph != null && curGraph.nodes.Any(n => n.IsSelected && n.nodeType != NodeType.Start))
            {
                menu.AddItem(new GUIContent("Delete Node"), false, ContextCallBack, "4");
            }

            menu.ShowAsContext();
            e.Use();
            if (curGraph != null)
            {
                EditorZoom.Begin(curGraph.zoom, curGraph.windowRect);
                curGraph.MouseUp();
            }
        }
        void ContextCallBack(object obj)
        {
            if (obj is System.Runtime.CompilerServices.ITuple)
            {
                var tuple = (System.Runtime.CompilerServices.ITuple)obj;
                CutsceneEditorManager.instance.CreateNode(curGraph, (NodeType)tuple[1], mousePos, (Port)tuple[2]);

            }
            else
                switch (obj.ToString())
                {
                    case "2":
                        GraphCreateWindow.InitPopUp();
                        break;
                    case "3":
                        CutsceneEditorManager.instance.CreateNode(curGraph, NodeType.Dialog, mousePos);
                        break;
                    case "4":
                        DeleteNode();
                        break;
                    case "5":
                        CreateChoiceNode(1);
                        break;
                    case "6":
                        CreateChoiceNode(2);
                        break;
                    case "7":
                        CreateChoiceNode(3);
                        break;
                    case "8":
                        CutsceneEditorManager.instance.CreateNode(curGraph, NodeType.SetFact, mousePos);
                        break;
                    case "9":
                        CutsceneEditorManager.instance.CreateNode(curGraph, NodeType.Cutscene, mousePos);
                        break;
                    case "10":
                        CopyNodes();
                        break;
                    case "11":
                        PasteNodes(curGraph.CutsceneEditorManager.selectedNodes);
                        break;
                    case "12":
                        ChangeToHomePosition();
                        break;
                    case "13":
                        ArrangeNodesORSelectUnusedNodes(true);
                        break;
                    case "14":
                        ArrangeNodesORSelectUnusedNodes(false);
                        break;
                    case "15":
                        CreateGroup();
                        break;
                    case "16":
                        CutNode();
                        break;
                    case "17":
                        DuplicateNodes();
                        break;
                    case "18":
                        SelectNodes();
                        break;
                    case "19":
                        SelectAll();
                        break;
                    case "20":
                        SelectNodes(false);
                        break;
                    case "21":
                        CutsceneEditorManager.instance.CreateNode(curGraph, NodeType.TriggerEvent, mousePos);
                        break;
                    case "22":
                        portConnection.Node.input.RemoveConnection(curGraph.selectedConnectionPort);
                        break;
                    case "23":
                        CutsceneEditorManager.instance.CreateNode(curGraph, NodeType.Random, mousePos);
                        break;
                    default:

                        break;
                }
            if (curGraph != null)
                curGraph.multipleNodeSelection = false;
        }

        void CreateGroup()
        {
#if UNITY_EDITOR
            Undo.RecordObject(curGraph, "Create Group");
#endif
            var selectedNodes = curGraph.CutsceneEditorManager.SelectedNodesList(curGraph);
            float minX = selectedNodes.Min(x => x.RectPos.x);
            float minY = selectedNodes.Min(y => y.RectPos.y);
            float maxX = selectedNodes.Max(x => x.RectPos.x);
            float maxY = selectedNodes.Max(y => y.RectPos.y);
            Rect rect = new Rect(minX - 50, minY - 100, maxX - minX + 350, maxY - minY + 275);

            Group group = new Group();
            foreach (var node in selectedNodes)
                group.nodeList.Add(node);
            group.groupRect = rect;
            curGraph.groups.Add(group);
        }

        void CreateChoiceNode(int choiceCount)
        {
            var selectedNodes = curGraph.nodes.Where(n => n.IsSelected && n.nodeType == NodeType.Dialog).ToList();
            foreach (var node in curGraph.nodes)
                node.IsSelected = false;

            for (int j = 0; j < selectedNodes.Count; j++)
            {
                var pos = selectedNodes[j].output.connections.Count > 0 ? selectedNodes[j].output.connections.Last().Node.RectPos + new Vector2(0,150) : selectedNodes[j].NodeRect.position + new Vector2(300, 0);
                for (int i = 0; i < choiceCount; i++)
                {
                    var addedNode = CutsceneEditorManager.instance.CreateNode(curGraph, NodeType.Choice, pos + new Vector2(0, i * 150));
                    if (!selectedNodes[j].output.connections.Any(p => p.Node.nodeType != NodeType.Choice))
                        selectedNodes[j].output.CreateConnection(addedNode.input);
                    addedNode.IsSelected = true;
                }
            }

        }

        void SelectAll()
        {
            foreach (var node in curGraph.nodes)
                node.SetIsSelected(true);
        }

        void CopyNodes()
        {
            if (!(curGraph.HasSelectedNodes() || selectedNode != null))
                return;
            if (curGraph.CutsceneEditorManager.SelectedNodesList(curGraph).Count > 0)
                return;
            curGraph.CutsceneEditorManager.selectedNodes.Add(selectedNode);

            //if (curGraph.CutsceneEditorManager.SelectedNodesList(curGraph).Count > 0)
            //    return;
            //curGraph.CutsceneEditorManager.selectedNodes.Add(selectedNode);
        }

        void PasteNodes(List<NodeBase> duplicateNodes, bool useKey = false)
        {
            Undo.RegisterCompleteObjectUndo(curGraph, "Pasted new node");
            List<NodeBase> newNodes = new List<NodeBase>();
            for (int i = 0; i < duplicateNodes.Count; i++)
            {
                float x = duplicateNodes.Min(n => n.RectPos.x);
                float y = duplicateNodes.Min(n => n.RectPos.y);
                mousePos = mousePos == Vector2.zero ? curGraph.windowOrigin + new Vector2(50, 50) : mousePos;
                var pos = useKey ? new Vector2(100, 100) : mousePos - new Vector2(x, y);
                NodeBase nodeToCreate = null;
                switch (duplicateNodes[i].nodeType)
                {
                    case NodeType.Dialog:
                        DialogueNode dialogNode = duplicateNodes[i] as DialogueNode;
                        nodeToCreate = new DialogueNode(dialogNode);
                        break;
                    case NodeType.Choice:
                        ChoiceNode choiceNode = duplicateNodes[i] as ChoiceNode;
                        nodeToCreate = new ChoiceNode(choiceNode);
                        break;
                    case NodeType.SetFact:
                        SetFactNode setFactNode = duplicateNodes[i] as SetFactNode;
                        nodeToCreate = new SetFactNode(setFactNode);                       
                        break;
                    case NodeType.Cutscene:
                        CutSceneNode cutSceneNode = duplicateNodes[i] as CutSceneNode;
                        nodeToCreate = new CutSceneNode(cutSceneNode);
                        break;
                    case NodeType.TriggerEvent:
                        TriggerEventNode eventTriggerNode = duplicateNodes[i] as TriggerEventNode;
                        nodeToCreate = new TriggerEventNode(eventTriggerNode);
                        break;
                    case NodeType.Random:
                        nodeToCreate = new RandomNode();
                        break;
                    default:
                        break;
                }
                nodeToCreate.parentGraph = curGraph;
                nodeToCreate.InitNode();
                nodeToCreate.RectPos = duplicateNodes[i].parentGraph != curGraph && useKey ? curGraph.windowOrigin + duplicateNodes[i].RectPos - new Vector2(x - 100, y - 100) : pos + duplicateNodes[i].RectPos;
                curGraph.nodes.Add(nodeToCreate);
                newNodes.Add(nodeToCreate);
                nodeToCreate.IsSelected = true;
                duplicateNodes[i].IsSelected = false;
            }

            for (int i = 0; i < duplicateNodes.Count; i++)
            {
                for (int j = 0; j < duplicateNodes[i].output.connections.Count; j++)
                {
                    NodeBase outputNode = duplicateNodes[i].output.connections[j].Node;

                    var index = duplicateNodes.FindIndex(n => n == outputNode);
                    if (index >= 0)
                    {
                        newNodes[i].output.CreateConnection(newNodes[index].input);
                        

                        foreach (var n in duplicateNodes[i].output.connections[j].conditions)
                        {
                            if(n is FactCondition)
                            {
                                FactCondition newFact = new FactCondition();
                                EditorUtility.CopySerializedManagedFieldsOnly(n, newFact);
                                newNodes[i].output.connections.Last().conditions.Add(newFact);
                            }
                        }
                    }
                }

            }
            curGraph.nodes.FirstOrDefault(n => n.nodeType == NodeType.Start).IsSelected = false;
        }

        void DuplicateNodes()
        {
            var duplicateNodes = curGraph.nodes.Where(n => n.IsSelected && n.nodeType != NodeType.Start).ToList();
            PasteNodes(duplicateNodes, true);
        }

        void DeleteNode()
        {
            var selectedNodes = curGraph.nodes.Where(n => n.IsSelected).ToList();
            foreach (var node in selectedNodes)
                CutsceneEditorManager.instance.DeleteNode(node, curGraph);
        }

        void ChangeToHomePosition()
        {
            curGraph.windowOrigin = Vector2.zero;
        }

        void SelectNodes(bool usedNode = true)
        {
            usedNodes = new List<NodeBase>();
            ArrangeNodesORSelectUnusedNodes(arrangeNode: false);
            curGraph.nodes.ForEach(n => n.IsSelected = usedNodes.Contains(n) || n.nodeType == NodeType.Start ? (usedNode ? false : true) : (usedNode ? true : false));
        }

        void ArrangeNodesORSelectUnusedNodes(bool diagonally = true, bool arrangeNode = true)
        {
            Undo.RegisterCompleteObjectUndo(curGraph, "ArrangeNodes");
            var startNode = curGraph.nodes.Where(n => n.nodeType == NodeType.Start).ToList();
            ArrangeChildNodes(startNode, diagonally, arrangeNode);
            foreach (var node in curGraph.nodes)
                node.check = false;
        }

        void ArrangeChildNodes(List<NodeBase> Nodes, bool diagonally, bool arrangeNode = true)
        {
            List<NodeBase> childNodes = new List<NodeBase>();
            foreach (var node in Nodes)
            {
                foreach (var port in node.output.connections)
                {
                    if (!port.Node.check)
                    {
                        childNodes.Add(port.Node);
                        usedNodes.Add(port.Node);
                        port.Node.check = true;
                    }
                }
            }

            if (arrangeNode)
            {
                for (int i = 0; i < childNodes.Count; i++)
                {
                    var rect = diagonally ? childNodes[i].input.connections.First().Node.RectPos : Nodes.First().RectPos;
                    int index = childNodes[i].input.connections.First().Node.output.connections.FindIndex(c => c.Node == childNodes[i]);
                    while (childNodes.Any(n => n.RectPos == rect + new Vector2(400, index * 200) && n.nodeID != childNodes[i].nodeID) && childNodes.FindIndex(n => n.RectPos == rect + new Vector2(400, index * 200)) < i)
                    {
                        index++;
                    }
                    var pos = rect + new Vector2(400, index * 200);
                    childNodes[i].RectPos = pos;
                }
            }
            if (childNodes.Count > 0)
                ArrangeChildNodes(childNodes, diagonally, arrangeNode);
        }

        void CutNode()
        {
            Undo.RegisterCompleteObjectUndo(curGraph, "Cut node");
            CopyNodes();
            foreach (var node in curGraph.CutsceneEditorManager.SelectedNodesList(curGraph))
            {
                CutsceneEditorManager.instance.DeleteNode(node, curGraph);
            }
        }

        void KeyBoardInputs(Event e)
        {
            //KEYBOARD INPUTS

            //delete nodes
            if (e.keyCode == KeyCode.Delete)
            {
                selectedNode = curGraph.nodes.FirstOrDefault(n => n.IsSelected);
                curGraph.selectedNode = null;
                DeleteNode();
            }

            //go to home position
            if (e.keyCode == KeyCode.Home)
                curGraph.windowOrigin = Vector2.zero;

            //multiple node selection 
            if (e.control || e.shift)
                curGraph.multiSelection = true;
            else
                curGraph.multiSelection = false;

            //cut
            if (e.Equals(Event.KeyboardEvent("^x")))
                CutNode();

            //copy
            if (e.Equals(Event.KeyboardEvent("^c")))
                CopyNodes();

            //paste
            if (e.Equals(Event.KeyboardEvent("^v")))
                PasteNodes(curGraph.CutsceneEditorManager.selectedNodes, true);

            //duplicate
            if (e.Equals(Event.KeyboardEvent("^d")) || e.Equals(Event.KeyboardEvent("#d")))
                DuplicateNodes();

            //select all
            if (e.Equals(Event.KeyboardEvent("^a")))
                SelectAll();

            //focus node
            if (e.Equals(Event.KeyboardEvent("^f")))
            {
                if (curGraph.nodes.Any(n => n.IsSelected))
                {
                    var node = curGraph.nodes.FirstOrDefault(n => n.IsSelected);
                    node.parentGraph.windowOrigin = node.RectPos - new Vector2(node.parentGraph.windowRect.width * .5f, node.parentGraph.windowRect.height * .5f);
                    node.parentGraph.zoom = 0.8f;

                }
            }
        }
        #endregion
    }
}