using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FC_CutsceneSystem
{

    [Serializable]
    public class FactCondition : ConditionBase
    {
        [SerializeField] public string key;
        [SerializeField] public bool value;
        public Toggle factStatus;
        public override bool CheckSatisfied()
        {
            if (CutsceneManager.instance.factDB.conditions.ContainsKey(key))
            {
                if (CutsceneManager.instance.factDB.conditions[key] == value)
                {
                    return true;
                }
                return false;
            }
            return true;
        }
    }
}