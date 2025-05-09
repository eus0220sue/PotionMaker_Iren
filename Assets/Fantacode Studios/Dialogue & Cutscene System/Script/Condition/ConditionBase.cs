using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace FC_CutsceneSystem
{

    [Serializable]
    public class ConditionBase
    {
        public virtual bool CheckSatisfied()
        {
            return false;
        }
    }
}