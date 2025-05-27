using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

public class DialogueEditorWindow : EditorWindow
{
    private DialogueGraph m_graph;
    private List<DialogueNodeView> m_nodeViews = new List<DialogueNodeView>();
    private Vector2 m_drag;
    private DialogueNodeView m_startNodeView = null;
    private DialogueNodeView m_selectedNode = null;

    [MenuItem("Window/Dialogue/Node Editor")]
    public static void Open()
    {
        GetWindow<DialogueEditorWindow>("Dialogue Editor");
    }

    private void OnEnable()
    {
        if (m_graph != null)
        {
            LoadNodeViews();
        }
    }

    private void OnGUI()
    {
        DrawToolbar();

        if (m_graph != null)
        {
            DrawGrid();
            DrawConnections();
            ProcessEvents(Event.current);

            foreach (var view in m_nodeViews)
            {
                view.ProcessEvents(Event.current);
                view.Draw();
            }

            if (GUI.changed)
                Repaint();
        }
        else
        {
            GUILayout.Label("No graph loaded. Click 'Open Graph' to load one.", EditorStyles.boldLabel);
        }
    }

    private void DrawToolbar()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar);
        if (GUILayout.Button("Open Graph", EditorStyles.toolbarButton))
        {
            string path = EditorUtility.OpenFilePanel("Load DialogueGraph", "Assets", "asset");
            if (!string.IsNullOrEmpty(path))
            {
                path = FileUtil.GetProjectRelativePath(path);
                m_graph = AssetDatabase.LoadAssetAtPath<DialogueGraph>(path);
                Debug.Log("[Editor] Graph Loaded: " + m_graph);

                LoadNodeViews();
            }
        }

        if (GUILayout.Button("Add Node", EditorStyles.toolbarButton))
        {
            var newNode = CreateInstance<DialogueNode>();
            int nodeIndex = m_graph.m_nodes.Count;
            newNode.name = $"NewNode_{nodeIndex}";
            newNode.m_position = new Vector2(100 + 220 * (nodeIndex % 5), 100 + 120 * (nodeIndex / 5)); // ÁßÃ¸ ¹æÁö
            AssetDatabase.AddObjectToAsset(newNode, m_graph);
            AssetDatabase.SaveAssets();
            m_graph.m_nodes.Add(newNode);
            m_nodeViews.Add(new DialogueNodeView(newNode, newNode.m_position));
        }

        if (GUILayout.Button("Delete Selected", EditorStyles.toolbarButton))
        {
            if (m_selectedNode != null)
            {
                DeleteNode(m_selectedNode);
                m_selectedNode = null;
            }
        }

        GUILayout.EndHorizontal();
    }

    private void DrawGrid()
    {
        Handles.color = new Color(0.3f, 0.3f, 0.3f, 0.3f);
        for (float x = 0; x < position.width; x += 20)
            Handles.DrawLine(new Vector3(x, 0), new Vector3(x, position.height));
        for (float y = 0; y < position.height; y += 20)
            Handles.DrawLine(new Vector3(0, y), new Vector3(position.width, y));
        Handles.color = Color.white;
    }

    private void ProcessEvents(Event e)
    {
        if (e.type == EventType.MouseDown && e.button == 0)
        {
            bool clickedNode = false;
            foreach (var view in m_nodeViews)
            {
                if (view.m_rect.Contains(e.mousePosition))
                {
                    clickedNode = true;
                    SetSelectedNode(view);
                    break;
                }
            }

            if (!clickedNode)
                SetSelectedNode(null);
        }

        if (e.type == EventType.MouseDown && e.button == 1)
        {
            foreach (var view in m_nodeViews)
            {
                if (view.m_rect.Contains(e.mousePosition))
                {
                    SetSelectedNode(view);
                    break;
                }
            }
        }

        if (e.type == EventType.MouseDrag && e.button == 2)
        {
            m_drag = e.delta;
            foreach (var view in m_nodeViews)
                view.Drag(m_drag);
            e.Use();
        }
    }

    private void SetSelectedNode(DialogueNodeView selected)
    {
        m_selectedNode = selected;
        foreach (var view in m_nodeViews)
            view.m_isSelected = (view == selected);
    }

    private void DrawConnections()
    {
        foreach (var view in m_nodeViews)
        {
            if (view.m_node.m_nextNode != null)
            {
                var from = view.GetOutputPosition();
                var toView = m_nodeViews.Find(v => v.m_node == view.m_node.m_nextNode);
                if (toView != null)
                {
                    var to = toView.GetInputPosition();
                    Handles.DrawBezier(from, to, from + Vector2.right * 50, to + Vector2.left * 50, Color.white, null, 3f);
                }
            }
        }

        if (m_startNodeView != null)
        {
            var from = m_startNodeView.GetOutputPosition();
            var to = Event.current.mousePosition;
            Handles.DrawBezier(from, to, from + Vector2.right * 50, to + Vector2.left * 50, Color.gray, null, 2f);
            GUI.changed = true;
        }
    }

    private void LoadNodeViews()
    {
        m_nodeViews.Clear();
        if (m_graph == null)
        {
            return;
        }


        foreach (var node in m_graph.m_nodes)
        {
            if (node == null)
            {
                continue;
            }

            var view = new DialogueNodeView(node, node.m_position);
            m_nodeViews.Add(view);
        }

        GUI.changed = true;
    }

    public static void RequestNodeDelete(DialogueNodeView view)
    {
        var window = GetWindow<DialogueEditorWindow>();
        window.DeleteNode(view);
    }

    private void DeleteNode(DialogueNodeView view)
    {
        if (m_graph.m_nodes.Contains(view.m_node))
        {
            m_graph.m_nodes.Remove(view.m_node);
            DestroyImmediate(view.m_node, true);
            AssetDatabase.SaveAssets();
            LoadNodeViews();
        }
    }
}
