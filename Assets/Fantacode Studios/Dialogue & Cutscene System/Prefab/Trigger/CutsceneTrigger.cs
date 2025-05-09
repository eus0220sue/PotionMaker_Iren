using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace FC_CutsceneSystem
{

    public enum TriggerType { None, OnStart, OnEnable, OnDestroy,OnTrigger,OnTrigger2D,OnTriggerWithKeyDown, OnTrigger2DWithKeyDown}

    public class CutsceneTrigger : MonoBehaviour
    {
        [HideInInspector]
        public TriggerType triggerType;
        [HideInInspector]
        public List<Component> componentStack = new List<Component>();

        public void ChangeTrigger()
        {
            foreach (var obj in componentStack)
                DestroyImmediate(obj);
            
            componentStack = new List<Component>();
            BoxCollider collider;
            BoxCollider2D collider2D;
            switch (triggerType)
            {
#if UNITY_EDITOR
                case TriggerType.OnStart:
                    componentStack.Add(Undo.AddComponent(gameObject, typeof(OnStart)));
                    break;
                case TriggerType.OnEnable:
                    componentStack.Add(Undo.AddComponent(gameObject, typeof(OnEnabled)));
                    break;
                case TriggerType.OnDestroy:
                    componentStack.Add(Undo.AddComponent(gameObject, typeof(OnDestroyed)));
                    break;
                case TriggerType.OnTrigger:
                    componentStack.Add(Undo.AddComponent(gameObject, typeof(OnTrigger)));
                    collider = Undo.AddComponent(gameObject, typeof(BoxCollider)) as BoxCollider;
                    componentStack.Add(collider);
                    collider.isTrigger = true;
                    break;
                case TriggerType.OnTrigger2D:
                    componentStack.Add(Undo.AddComponent(gameObject, typeof(OnTrigger2D)));
                    collider2D = Undo.AddComponent(gameObject, typeof(BoxCollider2D)) as BoxCollider2D;
                    componentStack.Add(collider2D);
                    collider2D.isTrigger = true;
                    break;
                case TriggerType.OnTriggerWithKeyDown:
                    componentStack.Add(Undo.AddComponent(gameObject, typeof(OnTriggerWithKeyDown)));
                    collider = Undo.AddComponent(gameObject, typeof(BoxCollider)) as BoxCollider;
                    componentStack.Add(collider);
                    collider.isTrigger = true;
                    break;
                case TriggerType.OnTrigger2DWithKeyDown:
                    componentStack.Add(Undo.AddComponent(gameObject, typeof(OnTrigger2DWithKeyDown)));
                    collider2D = Undo.AddComponent(gameObject, typeof(BoxCollider2D)) as BoxCollider2D;
                    componentStack.Add(collider2D);
                    collider2D.isTrigger = true;
                    break;
#endif
                default:
                    break;
            }
        }

    }
}
