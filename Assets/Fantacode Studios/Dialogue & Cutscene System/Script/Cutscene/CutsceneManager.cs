using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace FC_CutsceneSystem 
{
    public class CutsceneManager : MonoBehaviour
    {
        public static CutsceneManager instance;

        public Action OnCutsceneStart;
        public Action OnCutsceneEnd;

        public Canvas canvas;
        public TextMeshProUGUI dialogText;
        public TextMeshProUGUI speakerText;
        public GameObject choiceListParent;
        public GameObject choiceListBGObject;

        public Image faderImage;
        public Image speakerImage;
        public Image dialogBG;
        public Image speakerNameBG;
        public AudioSource audioSource;
        public AudioSource defaultAudioSource;
        public FactDB factDB;
        public TextMeshProUGUI choicePrefab;
        public GameObject choiceScrollView;
        public GameObject defaultCamera;

        public void Awake()
        {
            instance = this;
        }
        public CutsceneManager()
        {
            instance = this;
        }


        public static void PlayCutscene(string graphName)
        {
            if (!String.IsNullOrEmpty(graphName))
            {
                var graphs = FindObjectsOfType<CutscenePlayer>().ToList();
                var hasGraph = graphs.Any(g => g.GetComponent<CutsceneGraph>().graphName == graphName);
                if(hasGraph)
                    graphs.FirstOrDefault(g => g.GetComponent<CutsceneGraph>().graphName == graphName).StartCutscene();
                else
                    Debug.LogWarning("We don't have a CutsceneGraph named " + "\"" + graphName + "\"");
            }
            else
                Debug.LogWarning("We don't have a CutsceneGraph named " + "\"\"");
        }


        public static bool GetFact(string fact)
        {
            if(CutsceneManager.instance.factDB.conditions.ContainsKey(fact))
                return CutsceneManager.instance.factDB.conditions[fact];
            return false;
        }


        public static void SetFact(string key, bool value)
        {
            if (CutsceneManager.instance.factDB.conditions.ContainsKey(key))
            {
                CutsceneManager.instance.factDB.conditions[key] = value;
                foreach (KeyValuePair<string, bool> pair in CutsceneManager.instance.factDB.conditions)
                {
                    for (int i = 0; i < CutsceneManager.instance.factDB.Key.Count; i++)
                    {
                        if (CutsceneManager.instance.factDB.Key[i] == pair.Key)
                            CutsceneManager.instance.factDB.Value[i] = value;
                    }
                }
            }
            else if (!CutsceneManager.instance.factDB.conditions.ContainsKey(key) && key != "")
            {
                CutsceneManager.instance.factDB.conditions.Add(key, value);
                CutsceneManager.instance.factDB.Key.Add(key);
                CutsceneManager.instance.factDB.Value.Add(value);
            }
        }
    }

}