using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if inputsystem
using UnityEngine.InputSystem;
#endif

namespace FC_CutsceneSystem
{

    public class InputManager : MonoBehaviour
    {
        CutsceneSystemDatabase db;

#if inputsystem
        [HideInInspector]
        InputActionManager inputActionManager;
#endif
        public static InputManager instance;
        private void Awake()
        {
            instance = this;
            db = (CutsceneSystemDatabase)Resources.Load("Database/FC_Database");
#if inputsystem
            inputActionManager = new InputActionManager();
            inputActionManager.Enable();
#endif
        }
#if inputsystem
        private void OnEnable()
        {
            CutsceneManager.instance.OnCutsceneStart += inputActionManager.Enable;
        }
        private void OnDisable() 
        {
            CutsceneManager.instance.OnCutsceneEnd -= inputActionManager.Enable;
        }
#endif


        #region Input Manager

        public bool CheckInputDown(List<KeyCode> keyCodes)
        {
            if (db.useDefaultInputSystem)
                return keyCodes.Any(k => Input.GetKeyDown(k));
            return false;
        }

        public int NewInput_ChoiceScroll()
        {
#if inputsystem
            if (db.useNewInputSystem && inputActionManager.Cutscene.ChoiceScroll.WasPerformedThisFrame())
                return (int)inputActionManager.Cutscene.ChoiceScroll.ReadValue<float>();
#endif
            return 0;
        }
        /// <summary>
        /// skip dialogue node
        /// </summary>
        /// <returns></returns>
        public bool NewInput_SkipDialogue()
        {
#if inputsystem
            if (db.useNewInputSystem && inputActionManager.Cutscene.SkipDialogue.WasPerformedThisFrame())
                return true;
#endif
            return false;
        }

        public bool NewInput_SelectChoice()
        {
#if inputsystem
            if (db.useNewInputSystem && inputActionManager.Cutscene.ChoiceSelect.WasPerformedThisFrame())
                return true;
#endif
            return false;
        }

        public bool InteractInput()
        {
#if inputsystem
            if (db.useNewInputSystem && inputActionManager.Cutscene.Interact.WasPerformedThisFrame())
                return true;
#endif
            return false;
        }

        #endregion

    }
}