using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class DBSaving : MonoBehaviour
    {
        FactDB factDB;
        private void OnEnable()
        {
            factDB = this.gameObject.GetComponent<CutsceneManager>().factDB;
            for (int i = 0; i < factDB.Key.Count; i++)
            {
                if (!factDB.conditions.ContainsKey(factDB.Key[i]))
                    factDB.conditions.Add(factDB.Key[i], factDB.Value[i]);
            }
        }
    }
}
