using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FC_CutsceneSystem
{
    public class OnMouseOverChecker : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler
    {
        public Action<int> choiceActionEnter;
        public Action<int> choiceActionClick;
        //[HideInInspector]
        public int index;
        public void OnPointerClick(PointerEventData eventData)
        {
            choiceActionClick?.Invoke(index);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            choiceActionEnter?.Invoke(index);
        }
    }
}