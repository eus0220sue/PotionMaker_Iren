#if cinemachine
#if UNITY_6000_0_OR_NEWER
using Unity.Cinemachine;
#else
using Cinemachine;
#endif
#endif

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.Events;
using System;
#endif
using System.Linq;

namespace FC_CutsceneSystem
{
    public class CutsceneEditorManager : MonoBehaviour
    {
#if cinemachine
        public CinemachineBrain mainCinemachineBrain;
#endif

        public GameObject cameraAngles;

        [HideInInspector]
        public GameObject currentCamera;


        public static CutsceneEditorManager instance;

        [HideInInspector]
        public List<NodeBase> selectedNodes = new List<NodeBase>();

        public List<NodeBase> SelectedNodesList(CutsceneGraph graph)
        {
            selectedNodes = new List<NodeBase>();
            selectedNodes = graph.nodes.Where(n => n.IsSelected && n.nodeType != NodeType.Start).ToList();
            return selectedNodes;
        }
        private void Awake()
        {
            instance = this;
        }
        public CutsceneEditorManager()
        {
            instance = this;
        }
        public CutsceneSystemDatabase DB
        {
            get { return (CutsceneSystemDatabase)Resources.Load("Database/FC_Database"); }
        }

#if UNITY_EDITOR

        public CutsceneGraph CreateNewGraph(string wantedName)
        {
            var graph = (GameObject)Resources.Load("Graph/Graph");
            GameObject obj = Instantiate(graph);
            
            obj.gameObject.transform.parent = FindObjectOfType<CutsceneEditorManager>().gameObject.transform;
            obj.gameObject.name = wantedName;
            Undo.RegisterCreatedObjectUndo(obj, "Create New Graph");
            CutsceneGraph curGraph = obj.GetComponent<CutsceneGraph>();
            CreateNode(curGraph, NodeType.Start, new Vector2(50, 50));
            curGraph.CutsceneEditorManager = FindObjectOfType<CutsceneEditorManager>();
            curGraph.graphID = Guid.NewGuid().ToString();
            return curGraph;
        }

        public NodeBase CreateNode(CutsceneGraph curGraph, NodeType nodeType, Vector2 mousePos, Port connectingPort = null)
        {
            if (curGraph != null)
            {
                Undo.RegisterCompleteObjectUndo(curGraph, "Created new node");
                NodeBase nodeToCreate = null;

                switch (nodeType)
                {
                    case NodeType.Dialog:
                        nodeToCreate = new DialogueNode();
                        break;
                    case NodeType.Choice:
                        nodeToCreate = new ChoiceNode();
                        break;
                    case NodeType.SetFact:
                        nodeToCreate = new SetFactNode();
                        break;
                    case NodeType.Cutscene:
                        nodeToCreate = new CutSceneNode();
                        break;
                    case NodeType.Start:
                        nodeToCreate = new StartNode();
                        break;
                    case NodeType.TriggerEvent:
                        nodeToCreate = new TriggerEventNode();
                        break;
                    case NodeType.Random:
                        nodeToCreate = new RandomNode();
                        break;
                    default:
                        break;
                }
                nodeToCreate.parentGraph = curGraph;
                nodeToCreate.InitNode();
                nodeToCreate.RectPos = mousePos;
                curGraph.nodes.Add(nodeToCreate);

                if (connectingPort != null && curGraph.CheckValidConnection(nodeToCreate,connectingPort))
                {
                    connectingPort.CreateConnection(nodeToCreate.input);
                    curGraph.connectingPort = null;
                }
                return nodeToCreate;
            }
            return null;
        }

        public NodeBase DeleteNode(NodeBase deleteNode, CutsceneGraph curGraph)
        {
            Undo.RecordObject(curGraph, "Deleted Node");
            if (curGraph != null && deleteNode != null && deleteNode.nodeType != NodeType.Start)
            {
                deleteNode.output.RemoveconnectedPorts();
                deleteNode.input.RemoveconnectedPorts();
                curGraph.nodes.Remove(deleteNode);
                curGraph.DebugMode(debugMode:DB.debugMode);
            }
            curGraph.drawSelectionBox = false;
            return deleteNode;
        }
        public void DrawGrid(Rect viewRect, float gridSpacing, float gridCapcity, Color gridColor, Vector2 canvasScrollPosition)
        {
            viewRect.width += canvasScrollPosition.x;
            viewRect.height += canvasScrollPosition.y;
            int widthDivs = Mathf.CeilToInt(viewRect.width / gridSpacing);
            int heightDivs = Mathf.CeilToInt(viewRect.height / gridSpacing);

            Handles.BeginGUI();
            Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridCapcity);

            for (int x = 0; x < widthDivs; x++)
            {
                Handles.DrawLine(new Vector3(gridSpacing * x, 0f, 0f), new Vector3(gridSpacing * x, viewRect.height, 0f));
            }

            for (int y = 0; y < heightDivs; y++)
            {
                Handles.DrawLine(new Vector3(0f, gridSpacing * y, 0f), new Vector3(viewRect.width, gridSpacing * y, 0f));
            }
            Handles.color = Color.black;
            Handles.EndGUI();
        }
        public GUISkin GetEditorSkinns()
        {
            return (GUISkin)Resources.Load("EditorSkin/NodeEditorSkin");
        }
        public static void SetPlaceHolder(string text, string placeHolder, Color color, int fontSize = 12)
        {
            if (string.IsNullOrEmpty(text))
            {
                Rect pos = new Rect(GUILayoutUtility.GetLastRect());
                GUIStyle style = new GUIStyle
                {
                    alignment = TextAnchor.UpperLeft,
                    padding = new RectOffset(2, 0, 2, 0),
                    fontStyle = FontStyle.Italic,
                    fontSize = fontSize,
                    normal =
                {
                    textColor = color
                }
                };
                EditorGUI.LabelField(pos, placeHolder, style);
            }
        }
#endif
    }
}

