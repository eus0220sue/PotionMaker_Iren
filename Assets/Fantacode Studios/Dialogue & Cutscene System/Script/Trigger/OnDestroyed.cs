using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class OnDestroyed : TriggerBase
    {
        private void OnDestroy()
        {
            cutscene.StartCutscene();
        }
    }
}
