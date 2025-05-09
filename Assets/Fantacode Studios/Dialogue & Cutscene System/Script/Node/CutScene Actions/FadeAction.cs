using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;



namespace FC_CutsceneSystem
{
    public enum FadeType { FadeInAndOut, FadeIn, FadeOut}

    public class FadeAction : CutSceneAction
    {
        public FadeType fadeType;
        public float fadeInTime = 1;
        public float fadeOutTime = 1;
        public float fadeWaitTime = 1;
        public Sprite faderSprite;

        public FadeAction(FadeAction fade = null)
        {
            actionType = ActionType.Fade;
#if UNITY_EDITOR
            if (fade != null)
                EditorUtility.CopySerializedManagedFieldsOnly(fade, this);
#endif
        }

        public override IEnumerator ExecuteAction()
        {
            fadeInTime = fadeInTime == 0 ? 0.1f : fadeInTime;
            fadeOutTime = fadeOutTime == 0 ? 0.1f : fadeOutTime;
            fadeWaitTime = fadeWaitTime == 0 ? 0.1f : fadeWaitTime;

            if (fadeType == FadeType.FadeInAndOut || fadeType == FadeType.FadeIn)
                 yield return FadeIn(fadeInTime);
            if(fadeType == FadeType.FadeInAndOut)
            {
                Color col = CutsceneManager.instance.faderImage.color;
                col.a = 1;
                CutsceneManager.instance.faderImage.color = col;
                yield return new WaitForSecondsRealtime(fadeWaitTime);
            }
            if (fadeType == FadeType.FadeInAndOut || fadeType == FadeType.FadeOut)
                yield return FadeOut(fadeOutTime);
        }
        IEnumerator FadeOut(float fadeOutTime)
        {
            for (float i = 1; i >= 0; i -= 1 / fadeOutTime * Time.deltaTime)
            {
                Color color = CutsceneManager.instance.faderImage.color;
                color.a = i;
                CutsceneManager.instance.faderImage.color = color;
                yield return new WaitForSecondsRealtime(fadeOutTime * Time.deltaTime * (1 / fadeOutTime) * Time.deltaTime);
            }
            Color c = CutsceneManager.instance.faderImage.color;
            c.a = 0;
            CutsceneManager.instance.faderImage.color = c;
        }
        IEnumerator FadeIn(float fadeInTime)
        {
            for (float i = 0; i <= 1; i += 1 / fadeInTime * Time.deltaTime)
            {
                Color color = CutsceneManager.instance.faderImage.color;
                color.a = i;
                CutsceneManager.instance.faderImage.color = color;
                yield return new WaitForSecondsRealtime(fadeInTime * Time.deltaTime * (1 / fadeInTime) * Time.deltaTime);
            }
            Color c = CutsceneManager.instance.faderImage.color;
            c.a = 1;
            CutsceneManager.instance.faderImage.color = c;
        }

#if UNITY_EDITOR
        public override void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {
            var node = cutSceneNode.currentAction as FadeAction;
            List<FadeAction> actions = new List<FadeAction>() { node };

            if (selectedNodes != null)
            {
                actions = selectedNodes.Select(n => n.currentAction as FadeAction).ToList();
                var firstNode = actions.First();

                node.fadeType = (FadeType)GetFieldValue(actions.All(n => n.fadeType == firstNode.fadeType), FadeType.FadeInAndOut, firstNode.fadeType);
                node.fadeInTime = (float)GetFieldValue(actions.All(n => n.fadeInTime == firstNode.fadeInTime), 0f, firstNode.fadeInTime);
                node.fadeOutTime = (float)GetFieldValue(actions.All(n => n.fadeOutTime == firstNode.fadeOutTime), 0f, firstNode.fadeOutTime);
                node.fadeWaitTime = (float)GetFieldValue(actions.All(n => n.fadeWaitTime == firstNode.fadeWaitTime), 0f, firstNode.fadeWaitTime);
                node.faderSprite = (Sprite)GetFieldValue(actions.All(n => n.faderSprite == firstNode.faderSprite), null, firstNode.faderSprite);
            }


            using (new EditorGUILayout.VerticalScope("box"))
            {
                //GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel) { normal = { textColor = Color.cyan } };
                //EditorGUILayout.LabelField("Fade Action Settings", headerStyle);

                using (new EditorGUILayout.VerticalScope("box"))
                {

                    var fadeType = (FadeType)EditorGUILayout.EnumPopup("Fade Type", node.fadeType);
                    if (fadeType != node.fadeType)
                    {
                        UndoGraph(graph);
                        foreach (var n in actions)
                            n.fadeType = fadeType;
                    }
                }

                if (node.fadeType == FadeType.FadeInAndOut || node.fadeType == FadeType.FadeIn)
                {
                    using (new EditorGUILayout.VerticalScope("box"))
                    {

                        var fadeInTime = EditorGUILayout.FloatField("Fade In Time", node.fadeInTime);
                        if (fadeInTime != node.fadeInTime)
                        {
                            UndoGraph(graph);
                            foreach (var n in actions)
                                n.fadeInTime = fadeInTime;
                        }
                    }
                }

                if (node.fadeType == FadeType.FadeInAndOut)
                {
                    using (new EditorGUILayout.VerticalScope("box"))
                    {

                        var fadeWaitTime = EditorGUILayout.FloatField("Fade Wait Time", node.fadeWaitTime);
                        if (fadeWaitTime != node.fadeWaitTime)
                        {
                            UndoGraph(graph);
                            foreach (var n in actions)
                                n.fadeWaitTime = fadeWaitTime;
                        }
                    }
                }

                if (node.fadeType == FadeType.FadeInAndOut || node.fadeType == FadeType.FadeOut)
                {
                    using (new EditorGUILayout.VerticalScope("box"))
                    {

                        var fadeOutTime = EditorGUILayout.FloatField("Fade Out Time", node.fadeOutTime);
                        if (fadeOutTime != node.fadeOutTime)
                        {
                            UndoGraph(graph);
                            foreach (var n in actions)
                                n.fadeOutTime = fadeOutTime;
                        }
                    }
                }

                using (new EditorGUILayout.VerticalScope("box"))
                {
                    EditorGUILayout.LabelField("Additional Settings", EditorStyles.boldLabel);

                    var faderSprite = (Sprite)EditorGUILayout.ObjectField("Fader Sprite (optional)", node.faderSprite, typeof(Sprite), true);
                    if (faderSprite != node.faderSprite)
                    {
                        UndoGraph(graph);
                        foreach (var n in actions)
                            n.faderSprite = faderSprite;
                    }
                }
            }
        }


#endif
    }
}
