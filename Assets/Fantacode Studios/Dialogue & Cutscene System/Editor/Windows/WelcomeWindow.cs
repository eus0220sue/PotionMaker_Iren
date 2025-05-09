using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace FC_CutsceneSystem
{
    [InitializeOnLoad]
    public class WelcomeWindow : EditorWindow
    {  
        public static WelcomeWindow welcomeWindow;
        public const string cinemachine = "cinemachine";
        public const string inputsystem = "inputsystem";

        public static string windowShowedKey = "notShowed"; 

        [InitializeOnLoadMethod]
        public static void ShowWindow() 
        {
            if (PlayerPrefs.GetString(windowShowedKey) != "showed")
            {
                InitEditorWindow();
                PlayerPrefs.SetString(windowShowedKey, "showed");
            }
        }
         
        public static void InitEditorWindow()   
        {
            welcomeWindow = (WelcomeWindow)EditorWindow.GetWindow<WelcomeWindow>();  
            GUIContent titleContent = new GUIContent("Welcome"); 
            welcomeWindow.titleContent = titleContent; 
            welcomeWindow.minSize = new Vector2(450, 245);
            welcomeWindow.maxSize = new Vector2(450, 245);
        }   
        private void OnGUI() 
        {
            if(welcomeWindow == null)
                welcomeWindow = (WelcomeWindow)EditorWindow.GetWindow<WelcomeWindow>();
            GUILayout.Space(10);
            GUI.Label(new Rect(5, 10, position.width - 10, 55), "You can create non-linear dialogues & cutscenes using this system. Please refer to the quick start guide to see how to get started and create a simple cutscene.", GetEditorSkinns().GetStyle("WelcomeLabel"));
            
            if (GUI.Button(new Rect(0, 80, 110, 35), "", GetEditorSkinns().GetStyle("QuickStartBTN")))
                Application.OpenURL("https://fantacode.gitbook.io/cutscene-system/documentation/quickstart");
            if (GUI.Button(new Rect(110, 80, 110, 35), "", GetEditorSkinns().GetStyle("DocumentationBTN")))
                Application.OpenURL("https://fantacode.gitbook.io/cutscene-system/");
            if (GUI.Button(new Rect(221, 80, 110, 35), "", GetEditorSkinns().GetStyle("TutorialBTN")))
                Application.OpenURL("https://youtube.com/playlist?list=PLnbdyws4rcAt_3RFIDzCJZpfyN51uMRRk"); 
            GUILayout.Space(110);
            AddOnModules();

            GUI.Box(new Rect(0, 185, position.width, 2), "", GetEditorSkinns().GetStyle("Line"));

            if (GUI.Button(new Rect(60, 200, 135, 35), "Cutscene Editor", GetEditorSkinns().GetStyle("Button1")))
                NodeEditorWindow.InitEditorWindow();

            if (GUI.Button(new Rect(255, 200, 135, 35), "Database Window", GetEditorSkinns().GetStyle("Button2")))
                DatabaseWindow.InitDatabaseWindow();


        }
        private void AddOnModules()
        {
            var _cinemachine = false;
            var _inputsystem = false;
            var sybmols = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup).Split(';');
            for (int i = 0; i < sybmols.Length; i++)
            {
                if (string.Equals(cinemachine, sybmols[i].Trim())) 
                    _cinemachine = true;
                if (string.Equals(inputsystem, sybmols[i].Trim()))
                    _inputsystem = true;
            }

            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField(new GUIContent("Addon Module :"), EditorStyles.boldLabel);
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            var _cine = EditorGUILayout.Toggle("",_cinemachine, GetEditorSkinns().GetStyle(_cinemachine ? "ToggleTrue" : "ToggleFalse"),GUILayout.Width(17),GUILayout.Height(17));
            EditorGUILayout.LabelField(new GUIContent("Cinemachine", "This option enables Cinemachine support, but keep in mind that you must have installed the Cinemachine package before enabling this option"), GUILayout.Width(80));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            var _input = EditorGUILayout.Toggle("", _inputsystem, GetEditorSkinns().GetStyle(_inputsystem ? "ToggleTrue" : "ToggleFalse"), GUILayout.Width(17), GUILayout.Height(17));
            EditorGUILayout.LabelField(new GUIContent("New Input System", "This option enables New Input System support, but keep in mind that you must have installed the new Input System package before enabling this option"), GUILayout.Width(110));
            GUILayout.EndHorizontal();

            var sybmolValueChanged = EditorGUI.EndChangeCheck();

            if (_cine != _cinemachine)
            {
                if (_cine)
                {
                    if (EditorUtility.DisplayDialog("Cinemachine", "This option enables Cinemachine support, but keep in mind that you must have installed the new Input System package before enabling this option", "OK", "Cancel"))
                        ScriptingDefineSymbolController.ToggleScriptingDefineSymbol(cinemachine, _cine);
                    else
                        sybmolValueChanged = false;
                }
                else
                    ScriptingDefineSymbolController.ToggleScriptingDefineSymbol(cinemachine, _cine);
            }

            if (_input != _inputsystem)
            {
                if (_input)
                {
                    if (EditorUtility.DisplayDialog("New Input System", "This option enables New Input System support, but keep in mind that you must have installed the new Input System package before enabling this option", "OK", "Cancel"))
                    {
                        ScriptingDefineSymbolController.ToggleScriptingDefineSymbol(inputsystem, _input);
                        CutsceneSystemDatabase db = (CutsceneSystemDatabase)Resources.Load("Database/FC_Database");
                        db.useNewInputSystem = true;
                    }
                    else
                        sybmolValueChanged = false;
                }
                else
                    ScriptingDefineSymbolController.ToggleScriptingDefineSymbol(inputsystem, _input);
            }

            if (sybmolValueChanged)
                ScriptingDefineSymbolController.ReimportScripts();

        }

        GUISkin GetEditorSkinns()
        {
            return (GUISkin)Resources.Load("EditorSkin/NodeEditorSkin");
        }
    }


    public static class ScriptingDefineSymbolController
    {
        public static void ToggleScriptingDefineSymbol(string symbol, bool value)
        {
            if (value == true)
                AddingDefineSymbols(symbol);
            else
                RemovingDefineSymbols(symbol);
        }

        public static void AddingDefineSymbols(string symbol)
        {
            foreach (var group in GetInstalledBuildTargetGroups())
            {
                try
                {
                    var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
                    if (!string.IsNullOrEmpty(defines))
                        defines += ";";
                    defines += symbol;
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            ReloadScript();
        }

        public static void RemovingDefineSymbols(string symbol)
        {
            foreach (var group in GetInstalledBuildTargetGroups())
            {
                try
                {
                    var symbols = new List<string>(PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';'));
                    symbols.Remove(symbol);
                    var defines = string.Join(";", symbols.ToArray());
                    PlayerSettings.SetScriptingDefineSymbolsForGroup(group, defines);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            ReloadScript();
        }

        public static HashSet<BuildTargetGroup> GetInstalledBuildTargetGroups()
        {
            var targetGroups = new HashSet<BuildTargetGroup>();
            foreach (BuildTarget target in (BuildTarget[])Enum.GetValues(typeof(BuildTarget)))
            {
                BuildTargetGroup group = BuildPipeline.GetBuildTargetGroup(target);
                if (BuildPipeline.IsBuildTargetSupported(group, target))
                {
                    targetGroups.Add(group);
                }
            }
            return targetGroups;
        }

        public static void ReimportScripts()
        {
            AssetDatabase.ImportAsset("Assets/Non-Linear Dialogue & Cutscene System/Scripts");
        }

        public static void ReloadScript()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.RequestScriptReload();
        }
    }
}