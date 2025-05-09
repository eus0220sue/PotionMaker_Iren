using System.Linq;
using UnityEditor;
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class NodeEditorWindow : EditorWindow
    {
        #region Variables
        public static NodeEditorWindow curWindow;
        public NodeWorkView workView;
        public CutsceneGraph curGraph = null;
        public static CutsceneGraph currentGraph;
        public float viewPrecentage = 0.75f;
        public static bool isGraph;

        const float zoomMin = 0.1f;
        const float zoomMax = 1.0f;
        float zoom = 1.0f;
        Vector2 windowOrigin = Vector2.zero;
        private Rect viewRect;

        public Vector2 windowScrollView = new Vector2(100000, 100000); 


        bool playMode;
        #endregion

        private void OnEnable()
        {
            wantsMouseEnterLeaveWindow = true;  
            wantsMouseMove = true;
            Selection.selectionChanged += LoadGraph;
            isGraph = false; 

        } 

        private void OnDisable()
        {
            Selection.selectionChanged -= LoadGraph;
        }


        #region MainMethod
        public static void InitEditorWindow()
        {
            curWindow = (NodeEditorWindow)EditorWindow.GetWindow<NodeEditorWindow>();
            GUIContent titleContent = new GUIContent("Cutscene");
            curWindow.titleContent = titleContent;
            CreateViews();
        }
        

        public static void LoadGraph() 
        { 
            
            if (Selection.activeObject != null)
            {
                CutsceneGraph graph = null;
                if (Selection.activeObject is GameObject)
                {
                    GameObject obj = Selection.activeObject as GameObject; 
                    graph = obj.GetComponent<CutsceneGraph>();
                }  
                else if (Selection.activeObject is CutsceneGraph) 
                    graph = Selection.activeObject as CutsceneGraph; 
                if (graph != null)
                {
                    CreateViews();
                    if (curWindow != null)
                    { 
                        GUIContent titleContent = new GUIContent("Cutscene");
                        curWindow.titleContent = titleContent;
                        curWindow.curGraph = graph;
                        curWindow.windowOrigin = curWindow.curGraph.windowOrigin;
                        curWindow.zoom = curWindow.curGraph.zoom;
                        curWindow.curGraph.windowRect = curWindow.viewRect;
                        if (graph.CutsceneEditorManager.DB.debugMode)
                            curWindow.curGraph.DebugMode();
                        curWindow.Repaint();
                    }
                }
            }
        }
        private void OnGUI()
        {
            GUI.Box(new Rect(3, 3, position.width - 6, 35), "", GetEditorSkinns().GetStyle("WelcomeLabel"));
            Event e = Event.current;
            if (workView == null)
            {
                CreateViews();
                return;
            }
            if (playMode != EditorApplication.isPlaying || (curGraph == null && !isGraph))
            {
                LoadGraph();
                isGraph = true;
                playMode = EditorApplication.isPlaying;
            }

            viewRect = new Rect(new Rect(0, 35, position.width, position.height));
            GUI.Box(viewRect, "", GetEditorSkinns().GetStyle("ViewBG"));

            if (curGraph != null)
            {
                HandleZoomingAndPanning(e);
                windowOrigin = curGraph.windowOrigin;
                zoom = curGraph.zoom;
                curGraph.windowRect = viewRect;
            }
            else
            {
                zoom = 1;
                windowOrigin = Vector2.zero;
            }
            var scaledPosition = new Rect(position.x, position.y, (1 / zoom) * position.width, (1 / zoom) * position.height);
            workView.viewRect = new Rect(windowOrigin.x, windowOrigin.y, (position.width / zoom), (position.height / zoom));
            EditorZoom.Begin(zoom, viewRect);

            //GUI.skin.horizontalScrollbar = GUIStyle.none;
            //GUI.skin.verticalScrollbar = GUIStyle.none;

            windowOrigin = GUI.BeginScrollView(new Rect(0, 0, (1 / zoom) * position.width, (1 / zoom) * position.height), windowOrigin, new Rect(0, 0, windowScrollView.x, windowScrollView.y), false, false);

            if (curGraph != null)
            {
                if (curGraph.Grid)
                {
                    //draw grid  
                    CutsceneEditorManager.instance.DrawGrid(scaledPosition, 60, 0.3f, Color.black, windowOrigin);
                    CutsceneEditorManager.instance.DrawGrid(scaledPosition, 20, 0.1f, Color.black, windowOrigin);
                }
            }

            workView.UpdateView(e, curGraph);
            GUI.EndScrollView();
            EditorZoom.End();

            GUI.Box(new Rect(new Rect(0, 34, position.width, position.height)), "", GetEditorSkinns().GetStyle("Border"));
            GUI.Box(new Rect(new Rect(0, 0, position.width, position.height)), workView.viewTitle, GetEditorSkinns().GetStyle("Border"));

            if (curGraph != null)
            {
                SetGridOption();
                SetWarningAndErrorOption();
                GUIStyle style = curGraph.validation ? GetEditorSkinns().GetStyle("ValidateButtonOn") : GetEditorSkinns().GetStyle("ValidateButtonOff");
                if (GUI.Button(new Rect(position.width - 80, 10, 70, 18), new GUIContent("Validate", "Validate nodes"), style))
                    curGraph.validation = !curGraph.validation;
            }
            Repaint(e);
            
        }

        #endregion
        #region UtilityMethod

        void SetGridOption()
        {
            //Grid
            GUIStyle gridBTNSkin = curGraph.Grid ? GetEditorSkinns().GetStyle("GridEnable") : GetEditorSkinns().GetStyle("GridDisable");


            GUI.Box(new Rect(4, 3, 30, 30), "", GetEditorSkinns().GetStyle("WarningTitle"));
            if (GUI.Button(new Rect(11, 10, 17, 17), new GUIContent("", "Show grid"), gridBTNSkin))
                curGraph.Grid = !curGraph.Grid;
        }
        /// <summary>
        /// control connection warning and warning popup buttons
        /// </summary>
        void SetWarningAndErrorOption()
        {
            // warning popup

            GUI.Box(new Rect(36, 3, 30, 30), "", GetEditorSkinns().GetStyle("WarningTitle"));

            GUIStyle warningPopupBTNSkin = curGraph.CutsceneEditorManager.DB.warningPopup ? GetEditorSkinns().GetStyle("WarningOn") : GetEditorSkinns().GetStyle("WarningOff");
            if (GUI.Button(new Rect(41, 9, 20, 20), new GUIContent("", "Show connection warning popup"), warningPopupBTNSkin))
                curGraph.CutsceneEditorManager.DB.warningPopup = curGraph.CutsceneEditorManager.DB.warningPopup ? false : true;


            //warning error

            GUI.Box(new Rect(68, 3, 30, 30), "", GetEditorSkinns().GetStyle("WarningTitle"));

            GUIStyle warningBTNSkin = curGraph.CutsceneEditorManager.DB.debugMode ? GetEditorSkinns().GetStyle("ErrorOn") : GetEditorSkinns().GetStyle("ErrorOff");
            if (GUI.Button(new Rect(73, 9, 20, 20),new GUIContent("", "Show connections errors"), warningBTNSkin))
            {
                curGraph.CutsceneEditorManager.DB.debugMode = curGraph.CutsceneEditorManager.DB.debugMode ? false : true;
                curGraph.DebugMode(debugMode: curGraph.CutsceneEditorManager.DB.debugMode);
            }
        }
        static void CreateViews()
        {
            if (curWindow != null)
                curWindow.workView = new NodeWorkView();
            else
                curWindow = (NodeEditorWindow)EditorWindow.GetWindow<NodeEditorWindow>();
        }
#if UNITY_EDITOR
        public void Repaint(Event e)
        {
            if (e.type != EventType.Repaint && (e.type == EventType.MouseDrag || e.type == EventType.MouseDown || e.type == EventType.MouseUp || e.type == EventType.MouseMove || e.type == EventType.KeyDown))
            {
                Repaint();
                CutsceneGraphEditor.RepaintEditor();
            }
        }
#endif
        public void HandleZoomingAndPanning(Event e)
        {
            
            if (e.type == EventType.ScrollWheel)
            {
                Vector2 screenCoordsMousePos = Event.current.mousePosition;
                Vector2 delta = Event.current.delta;
                Vector2 zoomCoordsMousePos = (screenCoordsMousePos - viewRect.TopLeft()) / curGraph.zoom + curGraph.windowOrigin;
                float zoomDelta = -delta.y / 150.0f;
                float oldZoom = curGraph.zoom;
                curGraph.zoom += zoomDelta;
                curGraph.zoom = Mathf.Clamp(curGraph.zoom, zoomMin, zoomMax);
                curGraph.windowOrigin += (zoomCoordsMousePos - curGraph.windowOrigin) - (oldZoom / curGraph.zoom) * (zoomCoordsMousePos - curGraph.windowOrigin);
                e.Use();
            }
            if (e.type == EventType.MouseDrag && (e.button == 0 && e.modifiers == EventModifiers.Alt && curGraph.nodes.All(n => !n.IsSelected)) || e.button == 2)
            {
                curGraph.windowOrigin -= e.delta;
            }
            curGraph.windowOrigin.x = Mathf.Clamp(curGraph.windowOrigin.x, 0, Mathf.Infinity);
            curGraph.windowOrigin.y = Mathf.Clamp(curGraph.windowOrigin.y, 0, Mathf.Infinity);
        }

        public static void RepaintWindow()
        {
            if (curWindow != null)
                curWindow.Repaint();
        }

        GUISkin GetEditorSkinns()
        {
            return (GUISkin)Resources.Load("EditorSkin/NodeEditorSkin");
        }
        #endregion
    }
}