using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace FC_CutsceneSystem
{
    public class ChangeSpriteAction : CutSceneAction
    {
        public SpriteRenderer image;
        public Sprite newSprite;
        public ObjectSource objectSource;
        public ChangeSpriteAction(ChangeSpriteAction changeSprite = null)
        {
            actionType = ActionType.ChangeSprite;
#if UNITY_EDITOR
            if (changeSprite != null)
                EditorUtility.CopySerializedManagedFieldsOnly(changeSprite, this);
#endif
        }
        public override IEnumerator ExecuteAction()
        {
            if (objectSource == ObjectSource.UsePlayer)
                image = GameObject.FindGameObjectWithTag("Player").GetComponent<SpriteRenderer>();
            if (!ActionValidationWhilePlaying(image) || !ActionValidationWhilePlaying(newSprite))
                yield break;
            image.sprite = newSprite;
        }

#if UNITY_EDITOR
        public override void CustomEditor(CutSceneNode cutSceneNode, CutsceneGraph graph, List<CutSceneNode> selectedNodes = null)
        {
            var node = cutSceneNode.currentAction as ChangeSpriteAction;

            List<ChangeSpriteAction> actions = new List<ChangeSpriteAction>() { node };
            if (selectedNodes != null)
            {
                actions = selectedNodes.Select(n => n.currentAction as ChangeSpriteAction).ToList();
                var firstNode = actions.First();
                node.image = (SpriteRenderer)GetFieldValue(actions.All(n => n.image == firstNode.image), null, firstNode.image);
                node.newSprite = (Sprite)GetFieldValue(actions.All(n => n.newSprite == firstNode.newSprite), null, firstNode.newSprite);
                node.objectSource = (ObjectSource)GetFieldValue(actions.All(n => n.objectSource == firstNode.objectSource), ObjectSource.AssignObject, firstNode.objectSource);
            }

            GUILayout.BeginHorizontal();
            SpriteRenderer image = null;

            if (node.objectSource == ObjectSource.AssignObject)
                image = (SpriteRenderer)EditorGUILayout.ObjectField("SpriteRenderer", node.image, typeof(SpriteRenderer), true);
            else
            {
                GUILayout.Label("SpriteRenderer");
                GUILayout.Label(new GUIContent("Player SpriteRenderer", "Player-tagged object's SpriteRenderer"));
            }
            var objectSource = (ObjectSource)EditorGUILayout.EnumPopup(node.objectSource, GUILayout.Width(20));
            if (image != node.image || objectSource != node.objectSource)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                {
                    n.image = image;
                    n.objectSource = objectSource;
                }
            }
            GUILayout.EndHorizontal();
            if(node.objectSource == ObjectSource.AssignObject)
                ValidationWarning(node.image, "Sprite Renderer not assigned", graph);

            GUILayout.Space(5);
            node.newSprite = (Sprite)EditorGUILayout.ObjectField("New Sprite", node.newSprite, typeof(Sprite), true);
            if (newSprite != node.newSprite)
            {
                UndoGraph(graph);
                foreach (var n in actions)
                    n.newSprite = newSprite;
            }
            ValidationWarning(node.newSprite,"New Sprite is empty",graph);
        }
        public override bool Validation()
        {
            return (image == null && objectSource == ObjectSource.AssignObject) || (newSprite == null);
        }
#endif
    }
}