using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEditor;

namespace FC_CutsceneSystem
{
    public class CutsceneSystemDatabase : ScriptableObject
    {
        //localization
        [HideInInspector]
        public List<string> _languages = new List<string>();
        public List<string> languages = new List<string>();
             
        //characters
        public List<Character> characters = new List<Character>();


        //global settings
        [Header("Dialogue Settings")]
        public int letterPerSecond = 75;
        public AudioClip defaultAudio;


        [Header("Choice Settings")]
        public Sprite choiceListBG;
        public Color choiceTextColor;
        public Color selectedChoiceColor;

        public bool useNewInputSystem;
        public bool useDefaultInputSystem;
        public List<KeyCode> choiceDown;
        public List<KeyCode> choiceUp;
        public List<KeyCode> selectChoice;
        public List<KeyCode> skipDialogue;
        public List<KeyCode> interact;

        [HideInInspector]
        public bool debugMode = true;
        [HideInInspector]
        public bool warningPopup = true;


        public object SetField(object oldValue, object newValue)
        {
            if (oldValue != null && newValue != null && oldValue.ToString() != newValue.ToString())
            {
#if UNITY_EDITOR
                Undo.RegisterCompleteObjectUndo(this, "Update Field");
                EditorUtility.SetDirty(this);
#endif
            }
            return newValue;
        }
    }

    [Serializable]
    public class Character
    {
        public string name;
        public Sprite sprite;
    }
}


