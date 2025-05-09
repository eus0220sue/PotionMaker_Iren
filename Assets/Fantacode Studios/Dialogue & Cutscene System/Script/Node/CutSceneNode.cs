using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;

namespace FC_CutsceneSystem
{

    [Serializable]
    public class CutSceneNode : NodeBase
    {
        #region variables
        [HideInInspector]
        [SerializeReference] public CutSceneAction currentAction = new PlayAnimationAction();

        #endregion

        public CutSceneNode(CutSceneNode cutSceneNode = null) 
        {
            NodeName = "Cutscene";
            nodeIndicator = "CutsceneIndicator";
            if (cutSceneNode != null)
                currentAction = CutSceneAction.CreateCutSceneAction((int)cutSceneNode.currentAction.actionType, cutSceneNode.currentAction);
        }

#if UNITY_EDITOR
        public override void InitNode()
        {
            base.InitNode();
            nodeType = NodeType.Cutscene;
            input.connectionType = ConnectionType.Multiple; 
            output.connectionType = ConnectionType.Multiple;
        }

        public override void UpdateNodeGUI(Event e, Rect viewRect, GUISkin viewSkin)
        {
            nodeStyle = IsSelected ? "CutsceneNodeSelected" : "CutsceneNodeDefault";
            base.UpdateNodeGUI(e, viewRect, viewSkin);
            
            GUILayout.BeginArea(new Rect(NodeRect.x + 25, NodeRect.y + (NodeRect.height * 0.46f), NodeRect.width * .8f, NodeRect.height * .8f));
            EditorGUILayout.LabelField("Choose an action", viewSkin.GetStyle("Label"));
            GUILayout.EndArea();
            inputFieldRect = new Rect(NodeRect.x + 25, NodeRect.y + (NodeRect.height * 0.6f), NodeRect.width * .8f, NodeRect.height * .19f);
            GUILayout.BeginArea(inputFieldRect);
            if (GUILayout.Button(CutSceneAction.GetActionDisplayName(currentAction.actionType), viewSkin.GetStyle("PopUp"), GUILayout.Height(30)))
                GenericMenu(e);
            GUILayout.EndArea();
        }

        void GenericMenu(Event e)
        {
            GenericMenu menu = new GenericMenu();
            EditorZoom.End();

            for (int i = 0; i < Enum.GetNames(typeof(ActionType)).Length; i++)
            {
                menu.AddItem(new GUIContent(CutSceneAction.GetActionDisplayName((ActionType)i)), false, ContextCallBack, i);
            }

            menu.ShowAsContext();
            e.Use();
            EditorZoom.Begin(parentGraph.zoom, parentGraph.windowRect);
            parentGraph.MouseUp();
        }

        private void ContextCallBack(object obj)
        {
            if (currentAction.actionType == (ActionType)obj)
                return;
            Undo.RegisterCompleteObjectUndo(parentGraph, "Change Action");
            currentAction = CutSceneAction.CreateCutSceneAction((int)obj);

            parentGraph.ActiveObject = this;
            parentGraph.multipleNodeSelection = false;
        }

        public override void Validation()
        {
            warning = currentAction.Validation();
        }
#endif
    }
}



