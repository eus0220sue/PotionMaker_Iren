using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace FC_CutsceneSystem
{
    public static class LanguageManager
    {
        public static Action<string> languageChangeAction;

        public static void ChangeLanguage(string newLanguage)
        {
            CutsceneSystemDatabase db = (CutsceneSystemDatabase)Resources.Load("Database/FC_Database");
            if (db.languages.Contains(newLanguage) || newLanguage == "default" || String.IsNullOrEmpty(newLanguage))
            {
                PlayerPrefs.SetString("FC_CutsceneSystem_Language", newLanguage);
                languageChangeAction?.Invoke(newLanguage);
            }
            else
                Debug.LogWarning("\""+newLanguage+"\"" + " language is not available in your language database");
        }
    }
}