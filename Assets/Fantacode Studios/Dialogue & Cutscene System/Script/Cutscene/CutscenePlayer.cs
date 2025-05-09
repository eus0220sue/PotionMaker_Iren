using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace FC_CutsceneSystem
{
    public class CutscenePlayer : MonoBehaviour
    {
        CutsceneGraph curGraph;
        TextMeshProUGUI choicePrefab;

        GameObject choiceListParent;
        GameObject choiceListBGObject;

        TextMeshProUGUI dialogText;
        TextMeshProUGUI speakerText;

        Image speakerImage;
        Image speakerNameBG;
        Canvas UICanvas;

        string currentLanguage;
        AudioSource audioSource;
        AudioSource defaultAudioSource;
        AudioClip defaultAudioClip;

        NodeBase curBaseNode;
        List<TextMeshProUGUI> choiceLists = new List<TextMeshProUGUI>(); 


        GameObject dialogBG;
        GameObject choiceScrollView;

        CutsceneSystemDatabase db;


        int selectedChoiceIndex = 0;
        bool choiceSelected = true;
        int maxItemsInScrollView = 8;
        float choiceSpacing = 0;
        int setChoiceBG = 0;
        float choiceListRectHeight;
        int letterPerSecond;

        void Update()
        {
            ChoiceSelecting();
        }
        private void OnEnable()
        {
            LanguageManager.languageChangeAction += SetLanguage; 
        }
        private void OnDisable()
        {
            LanguageManager.languageChangeAction -= SetLanguage;
        }
        private void Awake()
        {
            db = (CutsceneSystemDatabase)Resources.Load("Database/FC_Database");
            curGraph = GetComponent<CutsceneGraph>();

            speakerImage = CutsceneManager.instance.speakerImage;

            UICanvas = CutsceneManager.instance.canvas;
            dialogText = CutsceneManager.instance.dialogText;
            speakerText = CutsceneManager.instance.speakerText;


            speakerNameBG = CutsceneManager.instance.speakerNameBG;
            audioSource = CutsceneManager.instance.audioSource;
            defaultAudioSource = CutsceneManager.instance.defaultAudioSource;
            defaultAudioClip = db.defaultAudio;
            choicePrefab = CutsceneManager.instance.choicePrefab;
            choiceScrollView = CutsceneManager.instance.choiceScrollView;
            choiceListParent = CutsceneManager.instance.choiceListParent;
            choiceListBGObject = CutsceneManager.instance.choiceListBGObject;
            dialogBG = CutsceneManager.instance.dialogBG.gameObject;


            choicePrefab.font = dialogText.font;

            letterPerSecond = db.letterPerSecond;

            //set current language
            currentLanguage = PlayerPrefs.GetString("FC_CutsceneSystem_Language");

        }


        #region cutscenePlayer 
        public void StartCutscene()
        {
            if (curGraph.IsExecuting)
                return;

            SetCutsceneCamera();
            UICanvas.gameObject.SetActive(true);
            curBaseNode = curGraph.nodes.FirstOrDefault(n => n.nodeType == NodeType.Start);
            CutsceneManager.instance.OnCutsceneStart?.Invoke();
            curGraph.IsExecuting = true;
            StartCoroutine(ExecuteChildNodes(curBaseNode));
        }
        void SetCutsceneCamera()
        {
            Camera startingCamera = curGraph.startingCamera;
            if (startingCamera == null)
                startingCamera = Camera.main;
            if (startingCamera == null && Camera.allCameras.Length > 0)
                startingCamera = Camera.allCameras[0];
            if(startingCamera == null)
            {
                Debug.LogError("There is no active camera in scene");
                return;
            }
            if (curGraph.CutsceneEditorManager == null)
                curGraph.CutsceneEditorManager = FindObjectOfType<CutsceneEditorManager>();
            curGraph.CutsceneEditorManager.currentCamera = startingCamera.gameObject;

        }
        IEnumerator ExecuteChildNodes(NodeBase curBaseNode)
        {
            // Get the dialogue node if there is any
            var childNodes = curBaseNode.output.connections.Where(n => CheckSatisfied(n) && n.Node.nodeType == NodeType.Dialog).ToList();
            if (curBaseNode.nodeType == NodeType.Random)
            {
                //childNodes = new List<PortConnection>();
                //var randomIndex = UnityEngine.Random.Range(0, curBaseNode.output.connections.Count);
                //childNodes.Add(curBaseNode.output.connections[randomIndex]);

                var randomIndex = UnityEngine.Random.Range(0, curBaseNode.output.connections.Count);
                childNodes = new List<PortConnection>();
                if (CheckSatisfied(curBaseNode.output.connections[randomIndex]))
                    childNodes.Add(curBaseNode.output.connections[randomIndex]);
            }
            else
            {
                // Get all other nodes
                foreach (var port in curBaseNode.output.connections)
                {
                    if (CheckSatisfied(port) && port.Node.nodeType != NodeType.Choice && !childNodes.Contains(port))
                        childNodes.Add(port);
                }
            }

            List<Coroutine> coroutine = new List<Coroutine>();
            foreach (var port in childNodes)
            {
                if (port.Node.working)
                    continue;
                var cr = StartCoroutine(ExecuteNode(port.Node));

                //focus current node
                port.Node.parentGraph.windowOrigin = port.Node.RectPos - new Vector2(port.Node.parentGraph.windowRect.width * .5f, port.Node.parentGraph.windowRect.height * .5f);
                port.Node.parentGraph.zoom = 0.8f;

                port.Node.working = true;
                coroutine.Add(cr);
            }



            for (int i = 0; i < childNodes.Count; i++)
            {
                StartCoroutine(WaitToExecuteNextNode(childNodes[i].Node, coroutine[i]));
            }

            yield break;
        }

        private IEnumerator ExecuteNode(NodeBase curNode)
        {
            //DIALOGUE NODE
            if (curNode.nodeType == NodeType.Dialog)
                yield return RunDialogue(curNode);

            //SET CUTSCENE NODE
            else if (curNode.nodeType == NodeType.Cutscene)
                yield return RunCutscene(curNode);

            //SET FACT NODE
            else if (curNode.nodeType == NodeType.SetFact)
                yield return RunFact(curNode);

            //SET EVENTTRIGGER NODE
            else if (curNode.nodeType == NodeType.TriggerEvent)
                yield return RunEventTrigger(curNode);
        }

        IEnumerator WaitToExecuteNextNode(NodeBase node, Coroutine cr)
        {
            yield return cr;
            node.working = false;
            if (node.output.connections.Count == 0 && curGraph.nodes.All(n => !n.working))
            {
                curGraph.IsExecuting = false;
                CutsceneManager.instance.OnCutsceneEnd?.Invoke();
            } 
            else if (node.output.connections.Count > 0)
                StartCoroutine(ExecuteChildNodes(node));
        }
        #endregion
         
        #region Set Dialogue and choice

        void EnableDialogueUI(DialogueNode currentNode)
        {
            dialogBG.gameObject.SetActive(true);
            if (!String.IsNullOrEmpty(currentNode.speaker))
                speakerNameBG.gameObject.SetActive(true);
            speakerText.text = "";
        }
        void SetFieldValues(DialogueNode currentNode)
        {
            if (currentNode.SpeakerImage != null)
            {
                speakerImage.gameObject.SetActive(true);
                speakerImage.sprite = currentNode.SpeakerImage;
            }
            dialogText.text = "";

            if (!String.IsNullOrEmpty(currentNode.speaker))
                speakerText.text = currentNode.speaker;
        }
        void PlayDialogueAnimation(DialogueNode currentNode)
        {
            if (currentNode.wantToPlayAnm)
            {
                if (currentNode.objectSource == ObjectSource.UsePlayer)
                    currentNode.animator = GameObject.FindGameObjectWithTag("Player").GetComponentInChildren<Animator>();
                if (currentNode.animator != null)
                    currentNode.animator.CrossFadeInFixedTime(currentNode.anmName, currentNode.crossFadeTime, currentNode.anmLayerIndex);
            }
        }
        void PlayDialogueDefaultAudio()
        {
            if (defaultAudioSource != null)
            {
                defaultAudioSource.clip = defaultAudioClip;
                defaultAudioSource.Play();
            }
        }
        IEnumerator ChoiceSetUp(List<PortConnection> choices,NodeBase curBaseNode, DialogueNode currentNode)
        {
            choiceSelected = false;
            SetChoice(choices);
            setChoiceBG = 0;
            yield return new WaitUntil(() => InputManager.instance.CheckInputDown(db.selectChoice) || choiceSelected || InputManager.instance.NewInput_SelectChoice());
            selectedChoiceIndex = choiceLists.Count - selectedChoiceIndex - 1;
            StartCoroutine(ExecuteChildNodes(curBaseNode.output.connections[selectedChoiceIndex].Node));
            DestroyChoices();
            foreach (var c in choices)
                c.Node.working = false;
            choiceSelected = true;
            currentNode.working = false;

            if (curBaseNode.output.connections[selectedChoiceIndex].Node.output.connections.Count == 0 && curGraph.nodes.All(n => !n.working))
            {
                curGraph.IsExecuting = false;
                CutsceneManager.instance.OnCutsceneEnd?.Invoke();
            }
        }
        IEnumerator RunDialogue(NodeBase curBaseNode)
        { 
            var currentNode = curBaseNode as DialogueNode;

            EnableDialogueUI(currentNode);

            yield return new WaitForSecondsRealtime(.5f);

            SetFieldValues(currentNode);

            PlayDialogueAnimation(currentNode);

            var dialog = GetTextAndAudio(currentNode.DialogText, currentNode);

            audioSource.Play();

            PlayDialogueDefaultAudio();


            StartCoroutine(TypeText(dialogText, dialog,letterPerSecond, currentNode));
            yield return SkipDialogueTextTyping(dialogText, dialog, currentNode);
            var choices = currentNode.output.connections.Where(p => p.Node.nodeType == NodeType.Choice && CheckSatisfied(p)).ToList();

            //set choice
            if (choices.Count > 0)
            {
                yield return ChoiceSetUp(choices, curBaseNode, currentNode);
            }
            else
            {
                yield return GoToNextNode(currentNode);
            }

            EndOfDialogue(currentNode);

        }
        void SetChoice(List<PortConnection> choices)
        {
            GetMaxItemsInScrollView();
            selectedChoiceIndex = choices.Count - 1;
            choiceScrollView.SetActive(true);
            for (int i = 0; i < choices.Count; i++)
            {
                var choice = choices[i];
                var currentNode = choice.Node as ChoiceNode;
                currentNode.working = true;
                var curChoice = Instantiate(choicePrefab, choiceListParent.transform);
                choiceLists.Add(curChoice);
                var choiceText = GetTextAndAudio(currentNode.ChoiceText,currentNode);
                curChoice.text = choiceText;
                
            }
            choiceLists.Reverse();
            for (int i = 0; i < choiceLists.Count; i++)
            {
                var choice = choiceLists[i];
                choice.GetComponent<OnMouseOverChecker>().index = i;
                choice.GetComponent<OnMouseOverChecker>().choiceActionEnter += MouseHoverOnChoice;
                choice.GetComponent<OnMouseOverChecker>().choiceActionClick += ChoiceSelected;
            }
            HandleChoiceScrolling();
        }
        IEnumerator TypeText(TextMeshProUGUI dialogText, string text, int letterPerSecond = 75, DialogueNode currentNode = null)
        {
            foreach (var letter in text.ToCharArray())
            {
                if (dialogText.text.Equals(text))
                    break;
                dialogText.text += letter;
                yield return new WaitForSeconds(1f / letterPerSecond);
            }
        }
        IEnumerator SkipDialogueTextTyping(TextMeshProUGUI dialogText, string text,DialogueNode currentNode)
        {
            while (!dialogText.text.Equals(text))
            {
                if (InputManager.instance.CheckInputDown(db.skipDialogue) || InputManager.instance.NewInput_SkipDialogue()
                    && currentNode != null)
                    dialogText.text = text;
                yield return null;
            }
            yield break;
        }
        string GetTextAndAudio(string defaultText, NodeBase node)
        {
            if(node is DialogueNode)
            {
                var dialogNode = node as DialogueNode;
                if (currentLanguage == "" || currentLanguage == "default")
                {
                    audioSource.clip = dialogNode.Audio;
                    return defaultText;
                }

                for (int i = 0; i < dialogNode.localization.Count; i++)
                {
                    if (dialogNode.localization[i].language == currentLanguage)
                    {
                        audioSource.clip = dialogNode.localization[i].audioClip;
                        return dialogNode.localization[i].languageText;
                    }
                }

                audioSource.clip = dialogNode.Audio;
                return defaultText;
            }

            else if(node is ChoiceNode)
            {
                var choiceNode = node as ChoiceNode;
                if (currentLanguage == "" || currentLanguage == "default")
                    return defaultText;

                for (int i = 0; i < choiceNode.localization.Count; i++)
                {
                    if (choiceNode.localization[i].language == currentLanguage)
                    {
                        audioSource.clip = choiceNode.localization[i].audioClip;
                        return choiceNode.localization[i].languageText;
                    }
                }
                return defaultText;
            }

            return "";
        }
        void ChoiceSelecting()
        {
            if (!choiceSelected)
            {
                //KEY EVENTS
                if (InputManager.instance.CheckInputDown(db.choiceDown))
                    selectedChoiceIndex--;
                if (InputManager.instance.CheckInputDown(db.choiceUp))
                    selectedChoiceIndex++;

                selectedChoiceIndex += InputManager.instance.NewInput_ChoiceScroll();

                selectedChoiceIndex = Mathf.Clamp(selectedChoiceIndex, 0, choiceLists.Count - 1);

                //changing selected choice text colour
                foreach (var choice in choiceLists)
                    choice.color = db.choiceTextColor;
                if (choiceLists.Count > 0)
                    choiceLists[selectedChoiceIndex].color = db.selectedChoiceColor;
                if (InputManager.instance.CheckInputDown(db.selectChoice) || InputManager.instance.NewInput_SelectChoice())
                {
                    dialogText.text = "";
                    dialogBG.gameObject.SetActive(false);
                    speakerNameBG.gameObject.SetActive(false);
                }

                if (setChoiceBG < 2)
                {
                    SetChoiceBG();
                    setChoiceBG++;
                }

                if (choiceLists.Count > maxItemsInScrollView)
                {
                    if (InputManager.instance.CheckInputDown(db.choiceDown) || InputManager.instance.CheckInputDown(db.choiceUp) || InputManager.instance.NewInput_ChoiceScroll() != 0)
                        HandleChoiceScrolling();
                }
            }
        }
        void SetChoiceBG()
        {
            var choiceListParentRect = choiceListParent.GetComponent<RectTransform>();
            var choiceListBGObjectRect = choiceListBGObject.GetComponent<RectTransform>();
            var choiceScrollViewRect = choiceScrollView.GetComponent<RectTransform>();



            if (choiceListParentRect.rect.height > choiceScrollViewRect.rect.height + 20 && choiceListParentRect.rect.width > choiceScrollViewRect.rect.width + 20)
            {
                choiceListBGObjectRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, choiceScrollViewRect.rect.width + 20);
                choiceListBGObjectRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, choiceScrollViewRect.rect.height + 20);
            }
            else if (choiceListParentRect.rect.height > choiceScrollViewRect.rect.height + 20)
                choiceListBGObjectRect.sizeDelta = new Vector2(choiceListParentRect.rect.width, choiceScrollViewRect.rect.height + 20);
            else if (choiceListParentRect.rect.width > choiceScrollViewRect.rect.width + 20)
                choiceListBGObjectRect.sizeDelta = new Vector2(choiceScrollViewRect.rect.width + 20, choiceListParentRect.rect.height);
            else
            {
                choiceListBGObjectRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, choiceListParentRect.rect.width);
                choiceListBGObjectRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, choiceListParentRect.rect.height);
            }
        }
        void HandleChoiceScrolling()
        {
            if (choiceLists.Count <= maxItemsInScrollView)
                return;
            float scrollPos =  - choiceListRectHeight - (Mathf.Clamp(selectedChoiceIndex - maxItemsInScrollView / 2, 0, selectedChoiceIndex) * (choiceLists[0].rectTransform.rect.height + choiceSpacing));
            choiceListParent.GetComponent<RectTransform>().localPosition = new Vector2(choiceListParent.GetComponent<RectTransform>().localPosition.x, scrollPos);
        }
        void MouseHoverOnChoice(int index)
        {
            selectedChoiceIndex = index;
        }
        void ChoiceSelected(int index)
        {
            selectedChoiceIndex = index;
            choiceSelected = true;
            dialogText.text = "";
            dialogBG.gameObject.SetActive(false);
            speakerNameBG.gameObject.SetActive(false);
        }
        void DestroyChoices()
        {
            choiceScrollView.SetActive(false);
            if (choiceLists.Count > 0)
            {
                foreach (var choice in choiceLists)
                    Destroy(choice.gameObject);
            }
            choiceLists = new List<TextMeshProUGUI>();
        }
        IEnumerator GoToNextNode(DialogueNode currentNode)
        {
            yield return new WaitUntil(() => InputManager.instance.CheckInputDown(db.skipDialogue) || InputManager.instance.NewInput_SkipDialogue());
            dialogText.text = "";
            if (!currentNode.output.connections.Any(n => n.Node.nodeType == NodeType.Dialog))
            {
                dialogBG.gameObject.SetActive(false);
                speakerNameBG.gameObject.SetActive(false);
            }
        }
        void EndOfDialogue(DialogueNode currentNode)
        {
            if (currentNode.wantToPlayAnm && currentNode.animator != null)
                currentNode.animator.Rebind();

            audioSource.Stop();
            defaultAudioSource.Stop();

            speakerImage.gameObject.SetActive(false);
            currentNode.working = false;
        }
        #endregion

        #region setfact
        IEnumerator RunFact(NodeBase curBaseNode)
        {
            var currentNode = curBaseNode as SetFactNode;
            CutsceneManager.SetFact(currentNode.factKey, currentNode.factState);
            yield break;
        }
        #endregion

        #region cutscene
        IEnumerator RunCutscene(NodeBase curBaseNode)
        {
            var currentNode = curBaseNode as CutSceneNode;
            yield return currentNode.currentAction.ExecuteAction();
        }
        #endregion

        #region Event trigger
        IEnumerator RunEventTrigger(NodeBase curBaseNode)
        {
            var currentNode = curBaseNode as TriggerEventNode;
            currentNode.Event.Invoke();

            yield break;
        }
        #endregion

        #region utility methods
      
        bool CheckSatisfied(PortConnection connection)
        {
            if (connection.conditions.Count == 0 || connection.conditions.All(c => c.CheckSatisfied()))
                return true;
            return false;
        }
   
        void SetLanguage(string newLanguage)
        {
            currentLanguage = newLanguage;
        }
        void GetMaxItemsInScrollView()
        {
            //Vector3[] scrollViewCorners = new Vector3[4];
            //choiceScrollView.GetComponent<RectTransform>().GetWorldCorners(scrollViewCorners);
            //choiceListRectHeight = scrollViewCorners[1].y - scrollViewCorners[0].y;
            choiceListRectHeight = choiceScrollView.GetComponent<RectTransform>().rect.height;
            var verticalgroup = choiceListParent.GetComponent<VerticalLayoutGroup>();
            var totalHeight = choiceListRectHeight - verticalgroup.padding.bottom - verticalgroup.padding.top;
            var choiceHeight = choicePrefab.GetComponent<RectTransform>().rect.height;
            maxItemsInScrollView = Mathf.RoundToInt((totalHeight / (choiceHeight + verticalgroup.spacing)));
            choiceSpacing = verticalgroup.spacing;

        }
        #endregion

        

    }
}