using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FC_CutsceneSystem
{
    public class ViewBase
    {
        #region public variables
        public string viewTitle;
        public Rect viewRect;
        public CutsceneGraph curGraph;
        #endregion

        #region protected variable
        protected GUISkin viewSkin;
        #endregion

        #region Constructors
        public ViewBase(string title)
        {
            viewTitle = title;
            GetEditorSkinns();
        }
        #endregion

        #region main methods 
        public virtual void UpdateView(Event e, CutsceneGraph curGraph)
        {

            if (viewSkin == null)
            {
                GetEditorSkinns();
                return;
            }

            this.curGraph = curGraph;
            if (curGraph != null)
                viewTitle = curGraph.graphName;
            else
                viewTitle = "No Graph";
        }
        public virtual void ProcessEvents(Event e)
        {

        }
        #endregion

        #region utility Methods
        protected void GetEditorSkinns()
        {
            viewSkin = (GUISkin)Resources.Load("EditorSkin/NodeEditorSkin");
        }
        #endregion
    }
}
