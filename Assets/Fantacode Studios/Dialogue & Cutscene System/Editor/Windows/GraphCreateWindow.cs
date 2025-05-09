using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace FC_CutsceneSystem
{
    public class GraphCreateWindow : EditorWindow
    {
        #region variables 
        static GraphCreateWindow curPopUp;
        string wantedName;
        GUISkin viewSkin;
        Rect PrecentageRect = new Rect(0, 0, 1, 1);
        #endregion

        #region main method
        public static void InitPopUp()
        {
            curPopUp = (GraphCreateWindow)EditorWindow.GetWindow<GraphCreateWindow>();
            GUIContent titleContent = new GUIContent("Create new graph");
            curPopUp.titleContent = titleContent;
            curPopUp.maxSize = new Vector2(275, 120);
            curPopUp.minSize = new Vector2(275, 120);
        }
        private void OnEnable()
        {
            viewSkin = (GUISkin)Resources.Load("EditorSkin/NodeEditorSkin");
        }
        void OnGUI()
        {
            if(curPopUp == null)
                curPopUp = (GraphCreateWindow)EditorWindow.GetWindow<GraphCreateWindow>();
            GUI.skin = viewSkin;
            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);

            GUILayout.BeginVertical();

            EditorGUILayout.LabelField("Enter Name", viewSkin.GetStyle("Label"));
            wantedName = EditorGUILayout.TextField(wantedName, viewSkin.GetStyle("TextField"), GUILayout.Height(35));

            GUILayout.Space(15);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Cancel", viewSkin.GetStyle("CancelButton"), GUILayout.Height(35)))
                curPopUp.Close();

            GUILayout.Space(10);


            //CREATE A NEW GRAPH
            if (GUILayout.Button("Create graph", viewSkin.GetStyle("CreateButton"), GUILayout.Height(35)) || Event.current.keyCode == KeyCode.Return)
            {
                if (!string.IsNullOrEmpty(wantedName))
                {
                    CutsceneGraph curGraph = CutsceneEditorManager.instance.CreateNewGraph(wantedName);
                    if (curGraph != null)
                    {
                        curGraph.graphName = wantedName;
                        NodeEditorWindow curWindow = (NodeEditorWindow)EditorWindow.GetWindow<NodeEditorWindow>();
                        if (curWindow != null)
                        {
                            curWindow.curGraph = curGraph;
                        }
                    }
                    curPopUp.Close();
                }
                else
                    EditorUtility.DisplayDialog("Node message:", "Please enter a valid name!", "OK");
            }

            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
        #endregion
    }
}