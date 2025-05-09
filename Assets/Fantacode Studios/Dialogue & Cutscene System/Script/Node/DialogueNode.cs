using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using UnityEngine.UI;
using System.Linq;

namespace FC_CutsceneSystem
{

    [Serializable]
    public class DialogueNode : NodeBase
    {
        #region variables

        [HideInInspector]
        [SerializeField] string dialogText;
        [HideInInspector]
        [SerializeField] string mention;
        [HideInInspector]
        [SerializeField] AudioClip audio;
        [HideInInspector]
        [SerializeField] Sprite speakerImage;

        [HideInInspector]
        public List<Localization> localization = new List<Localization>();

        [HideInInspector]
        public List<Localization> localizationTemp = new List<Localization>();


        [HideInInspector]
        public bool wantToPlayAnm;
        [HideInInspector]
        public Animator animator;
        [HideInInspector]
        public string anmName;
        [HideInInspector]
        public int anmLayerIndex = 0;
        [HideInInspector]
        public float crossFadeTime = .3f;
        [HideInInspector]
        public ObjectSource objectSource;

        [HideInInspector]
        public bool useCharacter;
        [HideInInspector]
        public int speakerPopupIndex = 0;
        [HideInInspector]
        public bool foldout1;
        [HideInInspector]
        public bool foldout2;
        [HideInInspector]
        public bool foldout3;


        #endregion

        public DialogueNode(DialogueNode node = null)
        {
            NodeName = "Dialogue";
            nodeIndicator = "DialogueIndicator";
            if (node != null)
            {
                dialogText = node.dialogText;
                mention = node.mention;
                audio = node.audio;
                speakerImage = node.speakerImage;

                animator = node.animator;
                anmName = node.anmName;
                anmLayerIndex = node.anmLayerIndex;
                crossFadeTime = node.crossFadeTime;
                wantToPlayAnm = node.wantToPlayAnm;
                objectSource = node.objectSource;

                useCharacter = node.useCharacter;
                speakerPopupIndex = node.speakerPopupIndex;



                localization = new List<Localization>();

                foreach (var l in node.localization)
                {
                    Localization loc = new Localization();
                    loc.language = l.language;
                    loc.languageText = l.languageText;
                    loc.audioClip = l.audioClip;
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

        public string DialogText
        {
            get { return dialogText; }
            set { dialogText = value; }
        }

        public string speaker
        {
            get { return mention; }
            set { mention = value; }
        }
        public AudioClip Audio
        {
            get { return audio; }
            set { audio = value; }
        }
        public Sprite SpeakerImage
        {
            get { return speakerImage; }
            set { speakerImage = value; }
        }


#if UNITY_EDITOR
        public override void InitNode()
        {
            base.InitNode();
            nodeType = NodeType.Dialog;
            input.connectionType = ConnectionType.Multiple;
            output.connectionType = ConnectionType.Multiple;
        }

        public override void UpdateNodeGUI(Event e, Rect viewRect, GUISkin viewSkin)
        {
            nodeStyle = IsSelected ? "DialogNodeSelected" : "DialogNodeDefault";
            base.UpdateNodeGUI(e, viewRect, viewSkin);
            GUILayout.BeginArea(new Rect(NodeRect.x + 25, NodeRect.y + (NodeRect.height * 0.46f), NodeRect.width * .8f, NodeRect.height * .8f));
            EditorGUILayout.LabelField("Enter Dialogue", viewSkin.GetStyle("Label"));
            GUILayout.EndArea();
            inputFieldRect = new Rect(NodeRect.x + 25, NodeRect.y + (NodeRect.height * 0.6f), NodeRect.width * .8f, NodeRect.height * .19f);
            GUILayout.BeginArea(inputFieldRect);
            dialogText = (string)parentGraph.SetField(dialogText, EditorGUILayout.TextArea(dialogText, viewSkin.GetStyle("TextField"), GUILayout.Height(NodeRect.height * .2f)));
            GUILayout.EndArea();
        }

        public override void Validation()
        {
            warning = (wantToPlayAnm && animator == null && objectSource == ObjectSource.AssignObject);
        }

        public void UpdateLocalization(List<string> languages)
        {
            if(languages.Count == 0)
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
                    l.audioClip = null;
                    localization.Add(l);
                }
                else if(localization.Count > 0)
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
