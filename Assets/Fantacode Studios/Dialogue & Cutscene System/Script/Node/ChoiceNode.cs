using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace FC_CutsceneSystem
{

    [Serializable]
    public class ChoiceNode : NodeBase
    {
        #region variables
        [HideInInspector]
        [SerializeField] string choiceText;

        [HideInInspector]
        public List<Localization> localization = new List<Localization>();
        [HideInInspector]
        public bool foldout1;

        #endregion

        public ChoiceNode(ChoiceNode node = null)
        {
            NodeName = "Choice";
            nodeIndicator = "ChoiceIndicator";

            if (node != null)
            {
                choiceText = node.choiceText;
                localization = new List<Localization>();

                foreach (var l in node.localization)
                {
                    Localization loc = new Localization();
                    loc.language = l.language;
                    loc.languageText = l.languageText;
                    localization.Add(loc);
                }
            }

            if (node == null)
            {
                CutsceneSystemDatabase db = (CutsceneSystemDatabase)Resources.Load("Database/FC_Database");

                foreach (var l in db.languages)
                {
                    Localization loc = new Localization();
                    loc.language = l;
                    localization.Add(loc);
                }
            }
        }

        public string ChoiceText
        {
            get { return choiceText; }
            set { choiceText = value; }
        }

#if UNITY_EDITOR
        public override void InitNode()
        {
            base.InitNode();
            nodeType = NodeType.Choice;
            output.connectionType = ConnectionType.Multiple;
            input.connectionType = ConnectionType.Override;
            
        }

        public override void UpdateNodeGUI(Event e, Rect viewRect, GUISkin viewSkin)
        {
            nodeStyle = IsSelected ? "ChoiceNodeSelected" : "ChoiceNodeDefault";
            base.UpdateNodeGUI(e, viewRect, viewSkin);
            GUILayout.BeginArea(new Rect(NodeRect.x + 25, NodeRect.y + (NodeRect.height * 0.46f), NodeRect.width * .8f, NodeRect.height * .8f));
            EditorGUILayout.LabelField("Enter Choice", viewSkin.GetStyle("Label"));
            GUILayout.EndArea();
            inputFieldRect = new Rect(NodeRect.x + 25, NodeRect.y + (NodeRect.height * 0.6f), NodeRect.width * .8f, NodeRect.height * .19f);
            GUILayout.BeginArea(inputFieldRect);
            choiceText = (string)parentGraph.SetField(choiceText, EditorGUILayout.TextArea(choiceText, viewSkin.GetStyle("TextField"), GUILayout.Height(NodeRect.height * .2f)));
            GUILayout.EndArea();
        }
        public void UpdateLocalization(List<string> languages)
        {
            if (languages.Count == 0)
            {
                localization = new List<Localization>();
                return;
            }
            for (int i = 0; i < languages.Count; i++)
            {
                if (localization.Count == 0 || !localization.Any(l => l.language == languages[i]))
                {
                    Localization l = new Localization();
                    l.language = languages[i];
                    localization.Add(l);
                }
                else if (localization.Count > 0)
                {
                    for (int j = 0; j < localization.Count; j++)
                    {
                        if (!languages.Contains(localization[j].language))
                            localization.Remove(localization[j]);
                    }
                }
            }
        }
#endif
    }
}