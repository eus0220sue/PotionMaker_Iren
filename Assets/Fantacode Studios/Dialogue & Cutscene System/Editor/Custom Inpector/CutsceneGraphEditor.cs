#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
#endif
using UnityEngine;
using UnityEngine.Events;

namespace FC_CutsceneSystem
{
    [CustomEditor(typeof(CutsceneGraph)), CanEditMultipleObjects]
    public class CutsceneGraphEditor : Editor
    {
        CutsceneGraph graph;
        
        public static CutsceneGraphEditor editor;
        private CutsceneSystemDatabase db;

        GUIStyle SetTextStyle(int fontsize)
        {
            var boldtext = new GUIStyle(GUI.skin.label);
            boldtext.fontStyle = FontStyle.Bold;
            boldtext.fontSize = fontsize;
            return boldtext;
        }
        public override void OnInspectorGUI()
        {
            graph = target as CutsceneGraph;
            editor = this;
            GUILayout.Space(10);

            if(db == null)
                db = (CutsceneSystemDatabase)Resources.Load("Database/FC_Database");

            if (graph.ActiveObject is NodeBase)
            {
                serializedObject.Update();
                var activeObject = graph.ActiveObject as NodeBase;
                var selectedNodes = graph.nodes.Where(n => n.IsSelected).ToList();

                

                if (selectedNodes.Count > 1)
                {
                    if (selectedNodes.All(n => n.nodeType == selectedNodes.First().nodeType))
                    {
                        EditorGUILayout.LabelField(activeObject.NodeName, SetTextStyle(15), GUILayout.Height(25));
                        GUILayout.Space(5);
                    }
                    selectedNodes.ForEach(n => n.Validation());
                    SetFact(Nodes._setFactNode, selectedNodes);
                    Choice(Nodes._choiceNode, selectedNodes);
                    Dialogue(Nodes._dialogueNode, selectedNodes);
                    Cutscene(Nodes._cutSceneNode, selectedNodes);
                }

                else
                {
                    activeObject.Validation();
                    EditorGUILayout.LabelField(activeObject.NodeName, SetTextStyle(15), GUILayout.Height(25));
                    GUILayout.Space(5);
                    if (graph.ActiveObject is DialogueNode)
                    {
                        var node = graph.ActiveObject as DialogueNode;
                        Dialogue(node);
                    }

                    else if (graph.ActiveObject is ChoiceNode)
                    {
                        var node = graph.ActiveObject as ChoiceNode;
                        Choice(node);
                    }

                    else if (graph.ActiveObject is SetFactNode)
                    {
                        var node = graph.ActiveObject as SetFactNode;
                        SetFact(node);
                    }

                    else if (graph.ActiveObject is CutSceneNode)
                    {
                        var cutSceneNode = graph.ActiveObject as CutSceneNode;
                        Cutscene(cutSceneNode);
                    }

                    else if (graph.ActiveObject is TriggerEventNode)
                    {
                        var eventTriggerNode = graph.ActiveObject as TriggerEventNode;
                        int index = graph.nodes.FindIndex(n => n.nodeID == eventTriggerNode.nodeID);
                        SerializedProperty serializedProperty = serializedObject.FindProperty("nodes.Array.data[" + index + "].Event");
                        EditorGUILayout.PropertyField(serializedProperty);
                    }
                    serializedObject.ApplyModifiedProperties();
                }
            }
            else if (graph.ActiveObject is PortConnection)
            {
                serializedObject.Update();
                var pc = graph.ActiveObject as PortConnection;
                var conditions = pc.conditions;

                for (int i = 0; i < conditions.Count; i++)
                {
                    if (conditions[i] is FactCondition)
                    {
                        using (var scope = new EditorGUILayout.HorizontalScope())
                        {
                            var factCondition = conditions[i] as FactCondition;

                            EditorGUILayout.LabelField("Fact",GUILayout.Width(50));
                            factCondition.key = EditorGUILayout.TextField(factCondition.key);
                            GUILayout.Label("Should be", GUILayout.Width(70));
                            factCondition.value = (bool)graph.SetField(factCondition.value, GetBoolValue(factCondition));
                            
                            if (GUILayout.Button("-", GUILayout.Width(20)))
                            {
                                conditions.Remove(conditions[i]);
                                if (graph.CutsceneEditorManager.DB.debugMode)
                                {
                                    graph.DebugMode();
                                }
                            }
                        }
                    }
                }
                GUILayout.Space(10);
                if (GUILayout.Button("Add Fact Condition", GUILayout.Height(25), GUILayout.Width(170)))
                {
                    UndoGraph();
                    conditions.Add(new FactCondition());
                    if (graph.CutsceneEditorManager.DB.debugMode)
                    {
                        if (graph.CutsceneEditorManager.DB.debugMode)
                        {
                            graph.DebugMode();
                        }
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }
            else if (graph.ActiveObject == null)
            {
                base.OnInspectorGUI();
            }
            NodeEditorWindow.RepaintWindow();
        }
        public static void RepaintEditor()
        {
            if (editor != null)
                editor.Repaint();
        }

        private void OnEnable()
        {
            Selection.selectionChanged += NodeEditorWindow.LoadGraph;
            db = (CutsceneSystemDatabase)Resources.Load("Database/FC_Database");
        }
        private void OnDisable()
        {
            Selection.selectionChanged -= NodeEditorWindow.LoadGraph;
        }


        bool GetBoolValue(FactCondition fact)
        {
            fact.factStatus = (Toggle)EditorGUILayout.EnumPopup(fact.factStatus, GUILayout.Width(60));
            return fact.factStatus == Toggle.True ? true : false;
        }


        void SetFact(SetFactNode node, List<NodeBase> selectedNodes = null)
        {
            List<SetFactNode> nodes = new List<SetFactNode>() { node };

            if (selectedNodes != null)
            {
                if (selectedNodes.Any(n => n.nodeType != NodeType.SetFact))
                    return;
                nodes = selectedNodes.Select(n => n as SetFactNode).ToList();
                var firstNode = nodes.First();
                node.factKey = (string)GetFieldValue(nodes.All(n => n.factKey == firstNode.factKey), "--", firstNode.factKey);
                node.factState = (bool)GetFieldValue(nodes.All(n => n.factState == firstNode.factState), false, firstNode.factState);
            }
            
            GUILayout.BeginHorizontal();

            //Fact Key
            var factKey = EditorGUILayout.TextField("Fact", node.factKey);
            if (factKey != node.factKey)
            {
                UndoGraph();
                foreach (var n in nodes)
                    n.factKey = factKey;
            }

            //fact state
            var factState = EditorGUILayout.Toggle(node.factState, GUILayout.Width(25));
            if (factState != node.factState)
            {
                UndoGraph();
                foreach (var n in nodes)
                    n.factState = factState;
            }

            GUILayout.EndHorizontal();
        }
        void Choice(ChoiceNode node, List<NodeBase> selectedNodes = null)
        {
            List<ChoiceNode> nodes = new List<ChoiceNode>() { node };

            if (selectedNodes != null)
            {
                if (selectedNodes.Any(n => n.nodeType != NodeType.Choice))
                    return;
                nodes = selectedNodes.Select(n => n as ChoiceNode).ToList();
                var firstNode = nodes.First();
                node.ChoiceText = (string)GetFieldValue(nodes.All(n => n.ChoiceText == firstNode.ChoiceText), "--", firstNode.ChoiceText);
                node.foldout1 = (bool)GetFieldValue(nodes.All(n => n.foldout1 == firstNode.foldout1), false, firstNode.foldout1);
            }

            //Choice text
            EditorGUILayout.LabelField("Choice Text");
            var choiceText = EditorGUILayout.TextArea(node.ChoiceText);
            if (choiceText != node.ChoiceText)
            {
                UndoGraph();
                foreach (var n in nodes)
                    n.ChoiceText = choiceText;
            }
            GUILayout.Space(5);

            //localization
            if (selectedNodes == null)
            {
                var foldout1 = EditorGUILayout.Foldout(node.foldout1, "Localization");
                if (foldout1 != node.foldout1)
                {
                    UndoGraph();
                    foreach (var n in nodes)
                        n.foldout1 = foldout1;
                }
                if (node.foldout1 && db != null)
                {
                    EditorGUI.indentLevel++;

                    node.UpdateLocalization(db.languages);
                    for (int i = 0; i < node.localization.Count; i++)
                    {
                        GUILayout.Space(5);
                        EditorGUILayout.LabelField(node.localization[i].language);
                        var languageText = EditorGUILayout.TextArea(node.localization[i].languageText);
                        if (languageText != node.localization[i].languageText)
                        {
                            UndoGraph();
                            foreach (var n in nodes)
                                n.localization[i].languageText = languageText;
                        }
                    }

                    EditorGUI.indentLevel--;
                }
            }
        }
        void Dialogue(DialogueNode node, List<NodeBase> selectedNodes = null)
        {
            List<DialogueNode> nodes = new List<DialogueNode>() { node };
            if (selectedNodes != null && selectedNodes.Any(n => n.nodeType != NodeType.Dialog))
                return;
            if (selectedNodes != null)
            {
                nodes = selectedNodes.Select(n => n as DialogueNode).ToList();
                
                var firstNode = nodes.First();
                node.speakerPopupIndex = (int)GetFieldValue(nodes.All(n => n.speakerPopupIndex == firstNode.speakerPopupIndex), 0, firstNode.speakerPopupIndex);
                node.anmLayerIndex = (int)GetFieldValue(nodes.All(n => n.anmLayerIndex == firstNode.anmLayerIndex), 0, firstNode.anmLayerIndex);
                node.crossFadeTime = (float)GetFieldValue(nodes.All(n => n.crossFadeTime == firstNode.crossFadeTime), 0f, firstNode.crossFadeTime);
                node.wantToPlayAnm = (bool)GetFieldValue(nodes.All(n => n.wantToPlayAnm == firstNode.wantToPlayAnm), false, firstNode.wantToPlayAnm);
                node.useCharacter = (bool)GetFieldValue(nodes.All(n => n.useCharacter == firstNode.useCharacter), true, firstNode.useCharacter);
                node.foldout1 = (bool)GetFieldValue(nodes.All(n => n.foldout1 == firstNode.foldout1), false, firstNode.foldout1);
                node.foldout2 = (bool)GetFieldValue(nodes.All(n => n.foldout2 == firstNode.foldout2), false, firstNode.foldout2);
                node.foldout3 = (bool)GetFieldValue(nodes.All(n => n.foldout3 == firstNode.foldout3), false, firstNode.foldout3);
                node.speaker = (string)GetFieldValue(nodes.All(n => n.speaker == firstNode.speaker), "--", firstNode.speaker);
                node.anmName = (string)GetFieldValue(nodes.All(n => n.anmName == firstNode.anmName), "--", firstNode.anmName);
                node.animator = (Animator)GetFieldValue(nodes.All(n => n.animator == firstNode.animator), null, firstNode.animator);
                node.objectSource = (ObjectSource)GetFieldValue(nodes.All(n => n.objectSource == firstNode.objectSource), ObjectSource.AssignObject, firstNode.objectSource);
                node.SpeakerImage = (Sprite)GetFieldValue(nodes.All(n => n.SpeakerImage == firstNode.SpeakerImage), null, firstNode.SpeakerImage);
            }
            else
            { 
                //Dialogue Text
                var Audio = (AudioClip)EditorGUILayout.ObjectField("Dialogue Text", node.Audio, typeof(AudioClip), true);
                if (Audio != node.Audio)
                {
                    UndoGraph();
                    foreach (var n in nodes)
                        n.Audio = Audio;
                }

                //audio
                var DialogText = EditorGUILayout.TextArea(node.DialogText);
                if (DialogText != node.DialogText)
                {
                    UndoGraph();
                    foreach (var n in nodes)
                        n.DialogText = DialogText;
                }
                GUILayout.Space(5);

                //loclization
                var foldout2 = EditorGUILayout.Foldout(node.foldout2, "Localization");
                if (foldout2 != node.foldout2)
                {
                    UndoGraph();
                    foreach (var n in nodes)
                        n.foldout2 = foldout2;
                }
                if (node.foldout2 && db != null)
                {
                    EditorGUI.indentLevel++;
                    node.UpdateLocalization(db.languages);
                    for (int i = 0; i < node.localization.Count; i++)
                    {
                        GUILayout.Space(5);
                        var audioClip = (AudioClip)EditorGUILayout.ObjectField(node.localization[i].language, node.localization[i].audioClip, typeof(AudioClip), true);
                        if (audioClip != node.localization[i].audioClip)
                        {
                            UndoGraph();
                            foreach (var n in nodes)
                                n.localization[i].audioClip = audioClip;
                        }
                        var languageText = EditorGUILayout.TextArea(node.localization[i].languageText);
                        if (languageText != node.localization[i].languageText)
                        {
                            UndoGraph();
                            foreach (var n in nodes)
                                n.localization[i].languageText = languageText;
                        }
                    }
                    EditorGUI.indentLevel--;
                }
                GUILayout.Space(5);
            }

            //speaker
            var foldout3 = EditorGUILayout.Foldout(node.foldout3, "Speaker");
            if (foldout3 != node.foldout3)
            {
                UndoGraph();
                foreach (var n in nodes)
                    n.foldout3 = foldout3;
            }
            if (node.foldout3)
            {
                EditorGUI.indentLevel++;
                GUILayout.Space(5);
                var useCharacter = EditorGUILayout.Toggle("Use Character", node.useCharacter);
                if (useCharacter != node.useCharacter)
                {
                    UndoGraph();
                    foreach (var n in nodes)
                        n.useCharacter = useCharacter;
                }
                GUILayout.Space(5);

                if (node.useCharacter)
                {
                    if (db.characters.Count == 0)
                        EditorGUILayout.HelpBox("characters are empty, Please create characters from Database window", MessageType.Warning);
                    else if (node.speakerPopupIndex < db.characters.Count && db.characters.Count > 0)
                    {
                        List<string> allCharacters = new List<string>();
                        db.characters.ForEach(c => allCharacters.Add(c.name));
                        var speakerPopupIndex = EditorGUILayout.Popup("Speaker",node.speakerPopupIndex, allCharacters.ToArray());
                        node.speaker = db.characters[node.speakerPopupIndex].name;
                        node.SpeakerImage = db.characters[node.speakerPopupIndex].sprite;
                        if (speakerPopupIndex != node.speakerPopupIndex)
                        {
                            UndoGraph();
                            foreach (var n in nodes)
                            {
                                n.speakerPopupIndex = speakerPopupIndex;
                                n.useCharacter = useCharacter;
                            }
                        }
                    }
                    else
                        node.speakerPopupIndex = db.characters.Count - 1;
                }
                else
                {
                    var speaker = EditorGUILayout.TextField("Speaker", node.speaker);
                    if (speaker != node.speaker)
                    {
                        UndoGraph();
                        foreach (var n in nodes)
                            n.speaker = speaker;
                    }
                    GUILayout.Space(5);

                    var speakerImage = (Sprite)EditorGUILayout.ObjectField("Speaker Image", node.SpeakerImage, typeof(Sprite), true);
                    if (speakerImage != node.SpeakerImage)
                    {
                        UndoGraph();
                        foreach (var n in nodes)
                            n.SpeakerImage = speakerImage;
                    }
                }
                EditorGUI.indentLevel--;
            }
            GUILayout.Space(5);

            //Additional
            var foldout1 = EditorGUILayout.Foldout(node.foldout1, "Additional");
            if (foldout1 != node.foldout1)
            {
                UndoGraph();
                foreach (var n in nodes)
                    n.foldout1 = foldout1;
            }
            if (node.foldout1)
            {
                EditorGUI.indentLevel++;
                var wantToPlayAnm = EditorGUILayout.Toggle("Play Animation",node.wantToPlayAnm);
                if (wantToPlayAnm != node.wantToPlayAnm)
                {
                    UndoGraph();
                    foreach (var n in nodes)
                        n.wantToPlayAnm = wantToPlayAnm;
                }

                if (node.wantToPlayAnm)
                {
                    GUILayout.Space(5);
                    GUILayout.BeginHorizontal();
                    Animator animator = null;
                    if (node.objectSource == ObjectSource.AssignObject)
                        animator = (Animator)EditorGUILayout.ObjectField("Animator", node.animator, typeof(Animator), true);
                    else
                    {
                        EditorGUILayout.LabelField("Animator");
                        GUILayout.Label(new GUIContent("Player Animator", "Player-tagged object's Animator"));
                    }
                    EditorGUI.indentLevel--;
                    var objectSource = (ObjectSource)EditorGUILayout.EnumPopup(node.objectSource, GUILayout.Width(20));
                    EditorGUI.indentLevel++;
                    if (animator != node.animator || objectSource != node.objectSource)
                    {
                        UndoGraph();
                        foreach (var n in nodes)
                        {
                            n.animator = animator;
                            n.objectSource = objectSource;
                        }
                    }
                    GUILayout.EndHorizontal();
                    if(node.objectSource == ObjectSource.AssignObject)
                        node.ValidationWarning(node.animator, "Animator is null",graph);
                    GUILayout.Space(5);

                    var anmLayerIndex = EditorGUILayout.IntField("Animator Layer",node.anmLayerIndex);
                    if (anmLayerIndex != node.anmLayerIndex)
                    {
                        UndoGraph();
                        foreach (var n in nodes)
                            n.anmLayerIndex = anmLayerIndex;
                    }
                    GUILayout.Space(5);

                    var anmName = EditorGUILayout.TextField("Animation Name",node.anmName);
                    if (anmName != node.anmName)
                    {
                        UndoGraph();
                        foreach (var n in nodes)
                            n.anmName = anmName;
                    }
                    GUILayout.Space(5);

                    var crossFadeTime = EditorGUILayout.FloatField("Cross Fade Time", node.crossFadeTime);
                    if (crossFadeTime != node.crossFadeTime)
                    {
                        UndoGraph();
                        foreach (var n in nodes)
                            n.crossFadeTime = crossFadeTime;
                    }
                }
                EditorGUI.indentLevel--;
            }
        }
        void Cutscene(CutSceneNode node, List<NodeBase> selectedNodes = null)
        {
            if (selectedNodes != null)
            {
                if (selectedNodes.Any(n => n.nodeType != NodeType.Cutscene))
                    return;
                var nodes = selectedNodes.Select(n => n as CutSceneNode).ToList();
                if (nodes.Any(n => n.currentAction.actionType != nodes.First().currentAction.actionType))
                    return;

                node = new CutSceneNode(nodes.First());
                EditorGUILayout.LabelField(CutSceneAction.GetActionDisplayName(node.currentAction.actionType) + " Action", SetTextStyle(13));
                GUILayout.Space(10);
                node.currentAction.CustomEditor(node, graph, nodes);
            }
            else
            {
                EditorGUILayout.LabelField(CutSceneAction.GetActionDisplayName(node.currentAction.actionType) + " Action", SetTextStyle(13));
                GUILayout.Space(10);
                node.currentAction.CustomEditor(node, graph);
            }
        }



        object GetFieldValue(bool isSameValues, object defaultValue, object value)
        {
            if (!isSameValues || value == null)
                return defaultValue;
            else
                return value;
        }

        
        void UndoGraph()
        {
            Undo.RegisterCompleteObjectUndo(graph, "Update Field");
        }
    }

    public static class Nodes
    {
        public static SetFactNode _setFactNode = new SetFactNode();
        public static DialogueNode _dialogueNode = new DialogueNode();
        public static ChoiceNode _choiceNode = new ChoiceNode();
        public static CutSceneNode _cutSceneNode = new CutSceneNode();
        public static TriggerEventNode _triggerEventNode = new TriggerEventNode();
    }
}


