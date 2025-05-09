using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using FC_CutsceneSystem;
using System.IO;
using System;
using System.Globalization;
using System.Text;
using System.Linq;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;

namespace FC_CutsceneSystem
{
#if UNITY_EDITOR
    public class DatabaseWindow : EditorWindow
    {
        public static DatabaseWindow curWindow;
        public CutsceneSystemDatabase db;
        SerializedObject _db;


        //CHARACTERS  c_
        bool displayError;
        public string newCharacterName;
        public Sprite newCharacterSprite;
        int columnCount;
        int rowCount;
        public Vector2 boxRect = new Vector2(130, 110);
        float boxOffset = 20;
        int offset = 5;
        private Rect c_characterTableViewRect;
        private Rect c_charcterAddingViewRect;
        static Vector2 c_windowOrigin = Vector2.zero;
        public Vector2 c_windowScrollView = new Vector2(100, 100);


        //LOCALIZATION  l_
        public List<CutsceneGraph> cutsceneGraphs;
        public List<NodeBase> totalNodes;

        private Rect l_languageEditorViewRect;

        static Vector2 l_windowOrigin = Vector2.zero;
        public Vector2 l_windowScrollView = new Vector2(100, 100);

        static Vector2 t_windowOrigin = Vector2.zero;
        public Vector2 t_windowScrollView = new Vector2(100, 30);


        //global settings _g
        static Vector2 g_windowOrigin = Vector2.zero;
        public Vector2 g_windowScrollView = new Vector2(100, 1100);


        //Facts  f_
        public FactDB factDB;
        static Vector2 f_windowOrigin = Vector2.zero;
        public Vector2 f_windowScrollView = new Vector2(100, 100);



        private Windows activeWindow;
        private string[] windows = { "Characters", "Localization", "Facts", "Settings" };

        public int currentLanguageToShowIndex;
        public string[] languagePopup = { "All" };

        public static void InitDatabaseWindow()
        {
            curWindow = (DatabaseWindow)EditorWindow.GetWindow<DatabaseWindow>();
            GUIContent titleContent = new GUIContent("Database");
            curWindow.titleContent = titleContent;
        }
        private void OnEnable()
        {
            Undo.undoRedoPerformed += Repaint;
            db = (CutsceneSystemDatabase)Resources.Load("Database/FC_Database");
            factDB = (FactDB)Resources.Load("Database/FactDB");

            ReloadTable();
        }
        private void OnDisable()
        {
            Undo.undoRedoPerformed -= Repaint;
        }

        private void OnGUI()
        {
            
            GUILayout.BeginArea(new Rect(5f, 4f, position.width, position.height));
            activeWindow = (Windows)GUILayout.Toolbar((int)activeWindow, windows, GUILayout.Width(position.width/1.5f));
            GUILayout.EndArea();

            if (curWindow == null)
                curWindow = (DatabaseWindow)EditorWindow.GetWindow<DatabaseWindow>();

            if (db == null)
                db = (CutsceneSystemDatabase)Resources.Load("Database/FC_Database");
           
            if (_db == null)
                _db = new SerializedObject(db);


            if (activeWindow == Windows.Characters)
                CharactersWindow();
            else if (activeWindow == Windows.Localization)
                LocalizationWindow();
            else if (activeWindow == Windows.Facts)
                FactsDatabase();
            else if (activeWindow == Windows.Settings)
                GlobalSettings();

            GUI.Box(new Rect(new Rect(0, 0, position.width + 1, position.height - 2)), "", GetEditorSkinns().GetStyle("Border"));
            GUI.Box(new Rect(new Rect(0, 24, position.width + 1, position.height - 26)), "", GetEditorSkinns().GetStyle("Border"));
        }

        

        #region Characters
        void CharactersWindow()
        {
            var charcterAddingRectWidth = position.width > 400 ? 200 : position.width / 4f;
            //character adding view
            c_charcterAddingViewRect = new Rect(new Rect(15, 35, charcterAddingRectWidth - 15, position.height - 20));
            GUILayout.BeginArea(new Rect(c_charcterAddingViewRect.x + 7, c_charcterAddingViewRect.y + 10, c_charcterAddingViewRect.width, c_charcterAddingViewRect.height));
            EditorGUILayout.LabelField("ADD CHARACTERS", GetEditorSkinns().GetStyle("Title"));
            EditorGUILayout.LabelField("Name", GUILayout.Width(c_charcterAddingViewRect.width * .3f), GUILayout.Height(25));
            newCharacterName = (string)SetField(newCharacterName, EditorGUILayout.TextField(newCharacterName));
            GUILayout.Space(5);
            EditorGUILayout.LabelField("Sprite", GUILayout.Width(c_charcterAddingViewRect.width * .3f), GUILayout.Height(25));
            newCharacterSprite = (Sprite)SetField(newCharacterSprite,EditorGUILayout.ObjectField(newCharacterSprite,typeof(Sprite),true));
            GUILayout.Space(10);
            if (GUILayout.Button("Create character", GUILayout.Height(25)))
            {
                if (newCharacterName == "")
                    displayError = true;
                else
                {
                    Character newCharacter = new Character();
                    newCharacter.name = newCharacterName;
                    newCharacter.sprite = newCharacterSprite;
                    db.characters.Add(newCharacter);
                    newCharacterName = "";
                    newCharacterSprite = null;
                    EditorUtility.SetDirty(db);
                    EditorUtility.DisplayDialog("Character", "New character created ", "Ok");
                    GUI.FocusControl(null);
                }
            }
            if(displayError)
                EditorGUILayout.HelpBox("Enter a valid name", MessageType.Error);
            GUILayout.EndArea();

            if (displayError && (Event.current.type == EventType.MouseDown || newCharacterName != ""))
                displayError = false;



            // showing characters
            c_characterTableViewRect = new Rect(charcterAddingRectWidth + 30, 24, position.width - charcterAddingRectWidth - 29, position.height - 26);
            columnCount = (int)((c_characterTableViewRect.width) / (boxRect.x + boxOffset));

            
            if (columnCount > 0)
            {
                //boxOffset = ((c_characterTableViewRect.width) % (boxRect.x)) / columnCount;
                var a = db.characters.Count % columnCount == 0 ? 0 : 1;
                rowCount = db.characters.Count / columnCount + a;


                c_windowScrollView = new Vector2(c_characterTableViewRect.width, (boxRect.y + boxOffset + 7) * rowCount);
                GUILayout.BeginArea(c_characterTableViewRect);
                c_windowOrigin = GUI.BeginScrollView(new Rect(boxOffset, boxOffset, position.width, position.height - boxOffset), c_windowOrigin, new Rect(0, 0, c_windowScrollView.x, c_windowScrollView.y), false, false);
                for (int i = 0; i * columnCount < db.characters.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    for (int j = 0; j < columnCount; j++)
                    {

                        if (db.characters.Count > i * columnCount + j) 
                        {
                            int index = i * columnCount + j;
                            GUILayout.Box("", GetEditorSkinns().GetStyle("CharacterBG"), GUILayout.Width(boxRect.x), GUILayout.Height(boxRect.y));
                            GUILayout.Space(boxOffset);

                            var x = boxRect.x * j + (j * boxOffset) + offset;
                            var y = boxRect.y * i + (i * boxOffset) + offset;
                            var rect = new Rect(x, y, boxRect.x - (2 * offset), boxRect.y);
                            CharcaterPack(index, rect);
                        }

                    }
                    GUILayout.EndHorizontal();
                    GUILayout.Space(boxOffset);
                }
                GUI.EndScrollView();
                GUILayout.EndArea();
            }
            GUI.Box(c_characterTableViewRect, "", GetEditorSkinns().GetStyle("Border"));
            Repaint(Event.current);
        }

        void CharcaterPack(int index,Rect rect)
        {
            GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal();
            db.characters[index].name = (string)db.SetField(db.characters[index].name, EditorGUILayout.TextField(db.characters[index].name, GetEditorSkinns().GetStyle("CharacterNameField"), GUILayout.Width(100)));
            GUILayout.Space(offset);
            if (GUILayout.Button("", GetEditorSkinns().GetStyle("CloseButton"), GUILayout.Height(15)))
            {
                Undo.RegisterCompleteObjectUndo(db, "Update Field");
                db.characters.RemoveAt(index);
                EditorUtility.SetDirty(db);
                GUI.FocusControl(null);
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(offset);
            if(db.characters.Count > index)
                db.characters[index].sprite = (Sprite)db.SetField(db.characters[index].sprite, EditorGUILayout.ObjectField(db.characters[index].sprite, typeof(Sprite), true, GUILayout.Height(boxRect.y - 38)));
            GUILayout.EndArea();
        }

        #endregion

        #region Localization
        void LocalizationWindow()
        {
            
            GUI.Box(new Rect(0, 80, position.width, 2), "", GetEditorSkinns().GetStyle("Line"));
            GUI.Label(new Rect(10, 40, position.width, 25),"LOCALIZATON TABLE", SetTextStyle(13));
            l_languageEditorViewRect = new Rect(10, 40, position.width-20, position.height - 20);


            GUILayout.BeginArea(l_languageEditorViewRect);

            GUILayout.BeginHorizontal();
            GUILayout.Label("");
            if (GUILayout.Button(new GUIContent("Export All", "Exporting all cutsecene graphs in the active scene"), GetEditorSkinns().GetStyle("BlackBTN"), GUILayout.Height(30), GUILayout.Width(100)))
                ExportToCSV();
            GUILayout.Space(15);
            if (GUILayout.Button(new GUIContent("Import All", "Export if any corrections have been made Before importing"), GetEditorSkinns().GetStyle("BlackBTN"), GUILayout.Height(30), GUILayout.Width(100)))
                ImportCSV();
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();

            var lang = languagePopup.Concat(db.languages.ToArray()).ToArray();
            currentLanguageToShowIndex = EditorGUILayout.Popup(currentLanguageToShowIndex, lang, GetEditorSkinns().GetStyle("BlackPopup"), GUILayout.Height(25), GUILayout.Width(position.width * .11f));
            GUILayout.Space(10);
            if (GUILayout.Button("Refresh", GetEditorSkinns().GetStyle("BlackBTN"), GUILayout.Height(25), GUILayout.Width(position.width * .11f)))
                ReloadTable();

            GUILayout.EndHorizontal();
            GUILayout.EndArea();

            //display table titles

            GUILayout.BeginArea(new Rect(10, 55, position.width - 20, position.height - 60));
            t_windowOrigin = GUI.BeginScrollView(new Rect(0, 70, position.width - 20, position.height - 60), new Vector2(l_windowOrigin.x, t_windowOrigin.y), new Rect(0, 0, t_windowScrollView.x, t_windowScrollView.y), false, false);
            t_windowOrigin = Handledrag(Event.current, t_windowOrigin);
            if (cutsceneGraphs.Count > 0 && totalNodes.Count > 0)
            {
                if (currentLanguageToShowIndex != 0 && db.languages.Count > 0)
                {
                    t_windowScrollView = new Vector2(position.width, 30);
                    currentLanguageToShowIndex = Mathf.Clamp(currentLanguageToShowIndex, 1, db.languages.Count);
                    DisplayTableTitle(db.languages[currentLanguageToShowIndex - 1]);
                }
                else
                {
                    currentLanguageToShowIndex = 0;
                    t_windowScrollView = new Vector2((Screen.width * .53f) + (db.languages.Count * Screen.width * .45f), 30);
                    DisplayTableTitle("");
                }
            }
            GUILayout.EndArea();
            GUI.EndScrollView();

            //display table contents
            GUILayout.BeginArea(new Rect(10, 155, position.width - 20, position.height - 160));
            l_windowOrigin = GUI.BeginScrollView(new Rect(0,0, position.width - 20 , position.height - 165), new Vector2(t_windowOrigin.x, l_windowOrigin.y), new Rect(0, 0, l_windowScrollView.x, l_windowScrollView.y), false, false);
            l_windowOrigin = Handledrag(Event.current, l_windowOrigin);
            if (cutsceneGraphs.Count > 0 && totalNodes.Count > 0)
            {
                if (currentLanguageToShowIndex != 0 && db.languages.Count > 0)
                {
                    l_windowScrollView = new Vector2(position.width, totalNodes.Count * 53);
                    currentLanguageToShowIndex = Mathf.Clamp(currentLanguageToShowIndex, 1, db.languages.Count);
                    DisplayTableContents(db.languages[currentLanguageToShowIndex - 1]);
                }
                else
                {
                    l_windowScrollView = new Vector2((Screen.width * .53f) + (db.languages.Count * Screen.width * .45f), totalNodes.Count * 53);
                    DisplayTableContents("");
                }
            }
            GUI.EndScrollView();
            GUILayout.EndArea();
            Repaint(Event.current);
        }

        void ExportToCSV()
        {
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            
            var filePath = EditorUtility.SaveFilePanel("Export CSV", "", SceneManager.GetActiveScene().name, "csv");
            if (filePath != "") 
            {
                db._languages = new List<string>();
                foreach (var l in db.languages)
                    db._languages.Add(l);
                string content = ConvertToCSVContent();
                try
                {
                   
                    var sw = File.CreateText(filePath);
                    using (var writer = sw)
                    {
                        writer.Write(content);
                    }
                    AssetDatabase.Refresh();
                    EditorUtility.DisplayDialog("Export","Export successfull", "OK");
                    ReloadTable();
                }
                catch
                {
                    EditorUtility.DisplayDialog("Error", "Error while saving the file. Make sure it's not opened in an another program.", "OK");
                }
            }
        }

        public string ConvertToCSVContent()
        {
            StringBuilder sb = new StringBuilder("Graph ID,Graph Name,Node ID,Node Type,Default Text,Default Audio FileName");
            
            foreach (var langauge in db.languages)
            {
                sb.Append(',').Append(langauge).Append(',').Append(langauge + " Audio FileName");
            }
            var cutsceneGraphs = FindObjectsOfType<CutsceneGraph>().ToList();
            foreach (var graph in cutsceneGraphs)
            {
                var nodes = graph.nodes.Where(n => n.nodeType == NodeType.Dialog || n.nodeType == NodeType.Choice);
                foreach (var node in nodes)
                {
                    if (node is DialogueNode)
                    {
                        var n = node as DialogueNode;

                        n.UpdateLocalization(db.languages);
                        var audio = n.Audio == null ? "" : n.Audio.name;
                        sb.Append('\n').Append(n.parentGraph.graphID).Append(',').Append(n.parentGraph.graphName).Append(',').Append(n.nodeID).Append(',').Append(n.nodeType).Append(',').Append(AddEscapeSymbols(n.DialogText)).Append(',').Append(audio);

                        for (int i = 0; i < db.languages.Count; i++)
                        {
                            audio = n.localization[i].audioClip == null ? "" : n.localization[i].audioClip.name;
                            sb.Append(',').Append(AddEscapeSymbols(n.localization[i].languageText)).Append(',').Append(audio);
                        }

                    }
                    else if (node is ChoiceNode)
                    {
                        var n = node as ChoiceNode;

                        n.UpdateLocalization(db.languages);
                        sb.Append('\n').Append(n.parentGraph.graphID).Append(',').Append(n.parentGraph.graphName).Append(',').Append(n.nodeID).Append(',').Append(n.nodeType).Append(',').Append(AddEscapeSymbols(n.ChoiceText)).Append(',').Append("");

                        for (int i = 0; i < db.languages.Count; i++)
                            sb.Append(',').Append(AddEscapeSymbols(n.localization[i].languageText)).Append(',').Append("");
                    }
                }
            }
            return sb.ToString();
        }

        void ImportCSV()
        {
            

            var filePath = EditorUtility.OpenFilePanel("Import CSV", "", "csv");
            if (filePath != "")
            {
                try
                {
                    StreamReader reader = new StreamReader(filePath, new UTF8Encoding(true));
                    db.languages = new List<string>();
                    foreach (var l in db._languages)
                        db.languages.Add(l);
                    UpdateLanguageTexts(filePath);
                    EditorUtility.DisplayDialog("Import", "Import successfull", "OK");
                    ReloadTable();
                }
                catch
                {
                    EditorUtility.DisplayDialog("Error", "Error while Importing the file. Make sure it's not opened in an another program.", "OK");
                }
            }
        }

        void UpdateLanguageTexts(string path)
        {
            var cutsceneGraphs = FindObjectsOfType<CutsceneGraph>().ToList();
            var data = GetContentsAsArray(path);
            foreach (var graph in cutsceneGraphs)
            {
                var nodes = graph.nodes.Where(n => n.nodeType == NodeType.Dialog || n.nodeType == NodeType.Choice);
                foreach (var node in totalNodes)
                {
                    
                    if (node is DialogueNode)
                    {
                        var n = node as DialogueNode;
                        n.localizationTemp = new List<Localization>();
                        UpdateFields(totalNodes.Count(), data, dialogueNode:n);

                        n.localization = n.localizationTemp;
                    }
                    if (node is ChoiceNode)
                    {
                        var n = node as ChoiceNode;
                        n.localization = new List<Localization>();
                        UpdateFields(totalNodes.Count(), data, choiceNode: n);
                    }
                }
            }
            return;
        }

        string[] GetContentsAsArray(string path)
        {
            var records = ReadCSVFile(path);
            List<string> datas = new List<string>();
            for (int j = 1; j < records.Count; j++)
            {
                foreach (var content in GetContent(records[j]))
                    datas.Add(content);
            }
            return datas.ToArray();
        }

        public void UpdateFields(int totalNodesCount,string[] data, DialogueNode dialogueNode = null,ChoiceNode choiceNode = null)
        {
            int totalColumns = 6 + (db.languages.Count * 2);
            if (dialogueNode != null)
            {
                int nodeIdIndex = 2;
                for (int i = 0; i < totalNodesCount; i++)
                {
                     if (data[nodeIdIndex] == dialogueNode.nodeID)
                    {
                        dialogueNode.DialogText = data[nodeIdIndex + 2];
                        if(dialogueNode.Audio == null || dialogueNode.Audio.name != data[nodeIdIndex+3])
                        {
                            if (String.IsNullOrEmpty(data[nodeIdIndex + 3]))
                            {
                                dialogueNode.Audio = null;
                            }
                            else
                            {
                                var clip = (AudioClip)Resources.Load("Audios/" + data[nodeIdIndex + 3]);
                                if (clip != null)
                                    dialogueNode.Audio = clip;
                            }
                        }
                        int textIndex = 4;
                        int nextTextIndex = 0;
                        for (int j = 0; j < db.languages.Count; j++)
                        {
                            Localization localization = new Localization();
                            localization.language = db.languages[j];
                            localization.languageText = data[nodeIdIndex + textIndex + nextTextIndex];

                            var audioName = data[nodeIdIndex + textIndex + nextTextIndex + 1];
                            if (j > dialogueNode.localization.Count)
                            {
                                if (dialogueNode.localization[j].audioClip == null || dialogueNode.localization[j].audioClip.name != audioName)
                                {
                                    if (String.IsNullOrEmpty(audioName))
                                    {
                                        localization.audioClip = null;
                                    }
                                    else
                                    {
                                        var clip = (AudioClip)Resources.Load("Audios/" + audioName);
                                        if (clip != null)
                                            localization.audioClip = clip;
                                        else
                                            localization.audioClip = dialogueNode.localization[j].audioClip;
                                    }
                                }
                                else
                                {
                                    localization.audioClip = dialogueNode.localization[j].audioClip;
                                }
                            }
                            else
                            {
                                localization.audioClip = (AudioClip)Resources.Load("Audios/" + audioName);
                            }

                            dialogueNode.localizationTemp.Add(localization);
                            nextTextIndex += 2;
                        }
                        break;
                    }
                    nodeIdIndex += totalColumns;
                }
            }
            else if (choiceNode != null)
            {
                int nodeIdIndex = 2;
                for (int i = 0; i < totalNodesCount; i++)
                {
                    if (data[nodeIdIndex] == choiceNode.nodeID)
                    {
                        choiceNode.ChoiceText = data[nodeIdIndex + 2];
                        int textIndex = 4;
                        int nextTextIndex = 0;
                        for (int j = 0; j < db.languages.Count; j++)
                        {
                            Localization localization = new Localization();
                            localization.language = db.languages[j];
                            localization.languageText = data[nodeIdIndex + textIndex + nextTextIndex];
                            choiceNode.localization.Add(localization);

                            nextTextIndex += 2;
                        }
                        break;
                    }
                    nodeIdIndex += totalColumns;
                }
            }
        }

        string AddEscapeSymbols(string languageText)
        {
            if (string.IsNullOrEmpty(languageText)) 
                return "";
            if (languageText.Contains("\n"))
                languageText = languageText.Replace("\n", "\\n");
            if (languageText.Contains(",") || languageText.Contains("\""))
                languageText = "\"" + languageText.Replace("\"", "\"\"") + "\"";
            return languageText;
        }

        string RemoveEscapeSymbols(string languageText)
        {
            string text = languageText.Replace("\\n", "\n");
            if (text.StartsWith("\"") && text.EndsWith("\""))
                text = text.Substring(1, text.Length - 2).Replace("\"\"", "\"");
            return text;
        }

        private List<string> GetContent(string line)
        {
            Regex csvRegex = new Regex("(^|,)(\"([^\"]+|\")*\"|[^,]*)");
            List<string> contents = new List<string>();
            foreach (Match match in csvRegex.Matches(line))
                contents.Add(RemoveEscapeSymbols(match.Value.TrimStart(',')));
            return contents;
        }

        private List<string> ReadCSVFile(string file)
        {
            var records = new List<string>();
            StreamReader reader = new StreamReader(file, new UTF8Encoding(true));
            string record;
            while ((record = reader.ReadLine()) != null)
                records.Add(record.TrimEnd());
            reader.Close();
            return records;
        }

        void DisplayTableTitle(string currentLanguageToShow)
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Graph", GetEditorSkinns().GetStyle("Label1"), GUILayout.Width(position.width * .1f), GUILayout.Height(25));
            EditorGUILayout.LabelField("Default Text", GetEditorSkinns().GetStyle("Label1"), GUILayout.Width(position.width * .33f), GUILayout.Height(25));
            EditorGUILayout.LabelField("Default Audio", GetEditorSkinns().GetStyle("Label1"), GUILayout.Width(position.width * .1f), GUILayout.Height(25));
            if (currentLanguageToShowIndex == 0)
            {
                foreach (var lang in db.languages)
                {
                    EditorGUILayout.LabelField(lang, GetEditorSkinns().GetStyle("Label1"), GUILayout.Width(position.width * .33f), GUILayout.Height(25));
                    EditorGUILayout.LabelField(lang + " Audio", GetEditorSkinns().GetStyle("Label1"), GUILayout.Width(position.width * .1f), GUILayout.Height(25));
                }
            }
            else if(db.languages.Count > 0)
            {
                EditorGUILayout.LabelField(currentLanguageToShow, GetEditorSkinns().GetStyle("Label1"), GUILayout.Width(position.width * .33f), GUILayout.Height(25));
                EditorGUILayout.LabelField(currentLanguageToShow + " Audio", GetEditorSkinns().GetStyle("Label1"), GUILayout.Width(position.width * .1f), GUILayout.Height(25));
            }
            GUILayout.EndHorizontal();
        }

        void DisplayTableContents(string currentLanguageToShow)
        {
            if (db == null)
                return;
            foreach (var n in totalNodes)
            {
                if (n is DialogueNode)
                {
                    var node = n as DialogueNode;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(node.parentGraph.graphName, GetEditorSkinns().GetStyle("Label1"), GUILayout.Width(position.width * .1f), GUILayout.Height(50));
                    node.DialogText = (string)node.parentGraph.SetField(node.DialogText, EditorGUILayout.TextArea(node.DialogText, GUILayout.Width(position.width * .33f), GUILayout.Height(50)));
                    node.Audio = (AudioClip)node.parentGraph.SetField(node.Audio, EditorGUILayout.ObjectField(node.Audio, typeof(AudioClip), true, GUILayout.Width(position.width * .1f), GUILayout.Height(50)));
                   
                    node.UpdateLocalization(db.languages);
                    if (currentLanguageToShowIndex == 0)
                    {
                        foreach (var lang in db.languages)
                        {

                            int index = node.localization.FindIndex(l => l.language == lang);
                            if (index > -1)
                            {
                                node.localization[index].languageText = (string)n.parentGraph.SetField(node.localization[index].languageText, EditorGUILayout.TextArea(node.localization[index].languageText, GUILayout.Width(position.width * .33f), GUILayout.Height(50)));
                                node.localization[index].audioClip = (AudioClip)n.parentGraph.SetField(node.localization[index].audioClip, EditorGUILayout.ObjectField(node.localization[index].audioClip, typeof(AudioClip), true, GUILayout.Width(position.width * .1f), GUILayout.Height(50)));
                            }
                        }
                    }
                    else if(db.languages.Count > 0)
                    {
                        int index = node.localization.FindIndex(l => l.language == currentLanguageToShow);
                        if(index > -1)
                        {
                            node.localization[index].languageText = (string)n.parentGraph.SetField(node.localization[index].languageText, EditorGUILayout.TextArea(node.localization[index].languageText, GUILayout.Width(position.width * .33f), GUILayout.Height(50)));
                            node.localization[index].audioClip = (AudioClip)n.parentGraph.SetField(node.localization[index].audioClip, EditorGUILayout.ObjectField(node.localization[index].audioClip, typeof(AudioClip), true, GUILayout.Width(position.width * .1f), GUILayout.Height(50)));
                        }
                    }
                    GUILayout.EndHorizontal();
                }
                if (n is ChoiceNode)
                {
                    var node = n as ChoiceNode;
                    GUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(node.parentGraph.graphName, GetEditorSkinns().GetStyle("Label1"), GUILayout.Width(position.width * .1f), GUILayout.Height(50));
                    node.ChoiceText = (string)node.parentGraph.SetField(node.ChoiceText, EditorGUILayout.TextArea(node.ChoiceText, GUILayout.Width(position.width * .33f), GUILayout.Height(50)));
                    EditorGUILayout.LabelField("", GUILayout.Width(position.width * .1f));

                    node.UpdateLocalization(db.languages);

                    if (currentLanguageToShowIndex == 0)
                    {
                        foreach (var lang in db.languages)
                        {

                            int index = node.localization.FindIndex(l => l.language == lang);
                            if (index > -1)
                            {
                                node.localization[index].languageText = (string)n.parentGraph.SetField(node.localization[index].languageText, EditorGUILayout.TextArea(node.localization[index].languageText, GUILayout.Width(position.width * .33f), GUILayout.Height(50)));
                                EditorGUILayout.LabelField("", GUILayout.Width(position.width * .1f));
                            }
                        }
                    }
                    else
                    {
                        int index = node.localization.FindIndex(l => l.language == currentLanguageToShow);
                        if (index > -1)
                        {
                            node.localization[index].languageText = (string)n.parentGraph.SetField(node.localization[index].languageText, EditorGUILayout.TextArea(node.localization[index].languageText, GUILayout.Width(position.width * .33f), GUILayout.Height(50)));
                            EditorGUILayout.LabelField("", GUILayout.Width(position.width * .1f));
                        }
                    }
                    GUILayout.EndHorizontal();
                }
            }
        }

        void ReloadTable()
        {
            totalNodes = new List<NodeBase>();
            cutsceneGraphs = FindObjectsOfType<CutsceneGraph>().ToList();
            foreach (var graph in cutsceneGraphs)
            {
                var nodes = graph.nodes.Where(n => n.nodeType == NodeType.Dialog || n.nodeType == NodeType.Choice).ToList();
                foreach (var node in nodes)
                    totalNodes.Add(node);
            }
        }
        #endregion

        #region Facts Database
        void FactsDatabase()
        {
            if(factDB == null)
                factDB = (FactDB)Resources.Load("Database/FactDB");
            f_windowScrollView = new Vector2(0, factDB.Key.Count * 30 + 20);
            var rect = new Rect(15, 15, position.width - 30, position.height - 20);
            GUILayout.BeginArea(rect);
            f_windowOrigin = GUI.BeginScrollView(new Rect(0, 25, position.width, position.height), f_windowOrigin, new Rect(0, 0, f_windowScrollView.x, f_windowScrollView.y), false, false);
            for (int i = 0; i < factDB.Key.Count; i++)
            {
                GUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(factDB.Key[i], GetEditorSkinns().GetStyle("TextField"), GUILayout.Width(rect.width * .8f),GUILayout.Height(25));
                EditorGUILayout.LabelField(factDB.Value[i] ? "True" : "False", GetEditorSkinns().GetStyle("TextField"), GUILayout.MinWidth(rect.width * .05f), GUILayout.Height(25));
                if (GUILayout.Button("Remove", GUILayout.Width(rect.width * .1f), GUILayout.Height(25)))
                {
                    Undo.RecordObject(factDB, "Fact removed");
                    factDB.conditions.Remove(factDB.Key[i]);
                    factDB.Value.RemoveAt(i);
                    factDB.Key.RemoveAt(i);
                    EditorUtility.SetDirty(factDB);
                }
                GUILayout.EndHorizontal();
            }
            GUI.EndScrollView();
            GUILayout.EndArea();

        }
        #endregion

        #region Settings
        void GlobalSettings()
        {
            if (Event.current.type == EventType.MouseDown)
                GUI.FocusControl(null);
            GUILayout.BeginArea(new Rect(15, 35, 400, position.height - 40));

            EditorGUI.BeginChangeCheck();

            g_windowOrigin = GUI.BeginScrollView(new Rect(0, 10, position.width, position.height), g_windowOrigin, new Rect(0, 0, g_windowScrollView.x, g_windowScrollView.y), false, false);
            //dialogue settings
            EditorGUILayout.LabelField("Dialogue settings",SetTextStyle(15),GUILayout.Height(25));
            db.letterPerSecond = EditorGUILayout.IntField("Letter Per Second", db.letterPerSecond);
            db.defaultAudio = (AudioClip)EditorGUILayout.ObjectField("Default Audio", db.defaultAudio, typeof(AudioClip), true);

            //Choice settings
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Choice settings", SetTextStyle(15), GUILayout.Height(25));
            db.choiceListBG = (Sprite)EditorGUILayout.ObjectField("ChoiceList BG", db.choiceListBG, typeof(Sprite), true);
            db.choiceTextColor = EditorGUILayout.ColorField("ChoiceText Color", db.choiceTextColor);
            db.selectedChoiceColor = EditorGUILayout.ColorField("Selected Choice Color", db.selectedChoiceColor);
            //Input manager
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Input Manager", SetTextStyle(15), GUILayout.Height(25));
            if (EditorGUI.EndChangeCheck())
                Undo.RecordObject(db, "Update Field");

#if inputsystem
            var temp1 = db.useDefaultInputSystem;
            var temp2 = db.useNewInputSystem;
            db.useNewInputSystem = (bool)db.SetField(db.useNewInputSystem, EditorGUILayout.Toggle("Use New InputSystem", db.useNewInputSystem, GUILayout.Width(170)));
            db.useDefaultInputSystem = (bool)db.SetField(db.useDefaultInputSystem,EditorGUILayout.Toggle("Use Default InputSystem", db.useDefaultInputSystem, GUILayout.Width(170)));

            if (!db.useNewInputSystem && !db.useDefaultInputSystem)
            {
                db.useNewInputSystem = temp1;
                db.useDefaultInputSystem = temp2;
            }
#else
            db.useDefaultInputSystem = true;
#endif

            _db.Update();
            if (db.useDefaultInputSystem)
            {
                SerializedProperty choiceDown = _db.FindProperty("choiceDown");
                EditorGUILayout.PropertyField(choiceDown);
                SerializedProperty choiceUp = _db.FindProperty("choiceUp");
                EditorGUILayout.PropertyField(choiceUp);
                SerializedProperty selectChoice = _db.FindProperty("selectChoice");
                EditorGUILayout.PropertyField(selectChoice);
                SerializedProperty select = _db.FindProperty("skipDialogue");
                EditorGUILayout.PropertyField(select);
                SerializedProperty interact = _db.FindProperty("interact");
                EditorGUILayout.PropertyField(interact);
            }

            //Language manager
            GUILayout.Space(10);
            EditorGUILayout.LabelField("Localization Languages", SetTextStyle(15), GUILayout.Height(25));
            var curLanguage = PlayerPrefs.GetString("FC_CutsceneSystem_Language") == "" ? "default" : PlayerPrefs.GetString("FC_CutsceneSystem_Language");
            EditorGUILayout.LabelField("Current Language : " + curLanguage, SetTextStyle(10), GUILayout.Height(25));
            SerializedProperty languageList = _db.FindProperty("languages");
            EditorGUILayout.PropertyField(languageList);
            _db.ApplyModifiedProperties();
            GUI.EndScrollView();
            GUILayout.EndArea();
        }
#endregion

            #region utility methods
        public void Repaint(Event e)
        {
            if ((e.type == EventType.MouseDrag || e.type == EventType.MouseMove || e.type == EventType.MouseDown || e.type == EventType.KeyDown) && e.type != EventType.Repaint)
                Repaint();
            if (e.type == EventType.MouseDown)
                GUI.FocusControl(null);
        }

        Vector2 Handledrag(Event e, Vector2 origin)
        {
            if (e.type == EventType.MouseDrag && (e.button == 0 && e.modifiers == EventModifiers.Alt) || e.button == 2)
            {
                origin.x -= e.delta.x/2;
            }
            origin.x = Mathf.Clamp(origin.x, 0, Mathf.Infinity);
            return origin;
        }

        public object SetField(object oldValue, object newValue)
        {
            if (oldValue != null && newValue != null && oldValue.ToString() != newValue.ToString())
            {
                Undo.RegisterCompleteObjectUndo(this, "Update Field");
                EditorUtility.SetDirty(db);
            }
            return newValue;
        }
 
        GUIStyle SetTextStyle(int fontsize)
        {
            var boldtext = new GUIStyle(GUI.skin.label);
            boldtext.fontStyle = FontStyle.Bold;
            boldtext.fontSize = fontsize;
            return boldtext;
        }

        GUISkin GetEditorSkinns()
        {
            return (GUISkin)Resources.Load("EditorSkin/NodeEditorSkin");
        }
            #endregion
    }
#endif
        }