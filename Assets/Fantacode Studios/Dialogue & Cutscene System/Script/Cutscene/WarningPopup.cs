using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace FC_CutsceneSystem 
{
#if UNITY_EDITOR
    public class WarningPopup : EditorWindow
    {
        public static WarningPopup curWindow;
        public static bool dontShowAgain = false;
        public static CutsceneGraph curGraph;
        public static void InitEditorWindow(CutsceneGraph graph)
        {
            curGraph = graph;
            var icon = Resources.Load("Texture/Vector (1)") as Texture2D;
            curWindow = (WarningPopup)EditorWindow.GetWindow<WarningPopup>();
            curWindow.ShowUtility();
            GUIContent titleContent = new GUIContent(" Warning",icon);
            curWindow.titleContent = titleContent;
            curWindow.minSize = new Vector2(338, 133);
            curWindow.maxSize = new Vector2(338, 133);
            dontShowAgain = false;
        }

        private void OnGUI()
        {
            GUI.Box(new Rect(0, 0, 338, 179), "", CutsceneEditorManager.instance.GetEditorSkinns().GetStyle("WarningBG"));
            //GUI.Box(new Rect(0, 0, 338, 51), "Warning",CutsceneEditorManager.instance.GetEditorSkinns().GetStyle("WarningTitle"));
            //GUI.Box(new Rect(18, 16, 22, 20), "",CutsceneEditorManager.instance.GetEditorSkinns().GetStyle("WarningIcon"));
            GUI.Label(new Rect(13, 20, 300, 65), "You're creating multiple connections to the same node, \nwhich will result in the node executing multiple times \nunless you have conditions in the connections for \npreventing it.", CutsceneEditorManager.instance.GetEditorSkinns().GetStyle("WarningText"));

            string toggleStyle = dontShowAgain ? "ToggleOn" : "ToggleOff";

            dontShowAgain = GUI.Toggle(new Rect(13, 100, 15, 15), dontShowAgain, "", CutsceneEditorManager.instance.GetEditorSkinns().GetStyle(toggleStyle));

            GUI.Label(new Rect(33, 98, 120, 18), "Don't show again");

            if (GUI.Button(new Rect(246, 95, 78, 29), "Ok", CutsceneEditorManager.instance.GetEditorSkinns().GetStyle("OKButton")))
            {
                CutsceneSystemDatabase DB = (CutsceneSystemDatabase)Resources.Load("Database/FC_Database");
                DB.warningPopup = !dontShowAgain;
                if(curWindow == null)
                    curWindow = (WarningPopup)EditorWindow.GetWindow<WarningPopup>();
                curWindow.Close();
            }            
        }
    }
#endif
}
