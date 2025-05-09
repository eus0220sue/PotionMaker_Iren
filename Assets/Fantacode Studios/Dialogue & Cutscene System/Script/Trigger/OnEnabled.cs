using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class OnEnabled : TriggerBase
    {
        private void OnEnable()
        {
            startCutscene = true;
        }
    }
}
