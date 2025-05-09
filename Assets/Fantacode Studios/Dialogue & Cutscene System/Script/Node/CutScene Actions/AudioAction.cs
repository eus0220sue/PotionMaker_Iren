using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class AudioAction : CutSceneAction
    {
        public AudioClip audio;

        public AudioAction(AudioAction audio = null)
        {
            actionType = ActionType.Audio;
#if UNITY_EDITOR
            if (audio != null)
                EditorUtility.CopySerializedManagedFieldsOnly(audio, this);
#endif
        }
        public override IEnumerator ExecuteAction()
        {
            if (!ActionValidationWhilePlaying(audio))
                yield break;
            GameObject obj = new GameObject();
            obj.name = "Audio";
            AudioSource audioSource = obj.AddComponent<AudioSource>();
            audioSource.clip = audio;
            audioSource.playOnAwake = false;
            audioSource.Play();
            yield return new WaitUntil(() => !audioSource.isPlaying);
            Object.Destroy(obj);
        }

#if UNITY_EDITOR
        public override void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {
            var node = cutSceneNode.currentAction as AudioAction;

            List<AudioAction> actions = new List<AudioAction>() { node };
            if (selectedNodes != null)
            {
                actions = selectedNodes.Select(n => n.currentAction as AudioAction).ToList();
                var firstNode = actions.First();
                node.audio = (AudioClip)GetFieldValue(actions.All(n => n.audio == firstNode.audio), null, firstNode.audio);
            }

            var audio = (AudioClip)EditorGUILayout.ObjectField("Audio", node.audio, typeof(AudioClip), true);

            if (audio != node.audio)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.audio = audio;
            }
            ValidationWarning(node.audio,"Audio is not assigned",graph);
        }

        public override bool Validation()
        {
            return (audio == null);
        }
#endif
    }
}