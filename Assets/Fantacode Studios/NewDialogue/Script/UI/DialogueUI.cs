using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject m_DialogueUI;
    [SerializeField] TextMeshProUGUI m_speakerName;
    [SerializeField] TextMeshProUGUI m_dialogueText;
    [SerializeField] Image m_leftPortrait;
    [SerializeField] Image m_rightPortrait;



    [Header("컷씬 처리용")]
    [SerializeField] GameObject m_nameBox;
    [SerializeField] GameObject m_dialogueBox;
    [SerializeField] GameObject m_Illust;
    [SerializeField] GameObject m_cutScenePanel;
    [SerializeField] Image m_cutSceneDisplay;

    [Header("Selection UI")]
    [SerializeField] GameObject m_selectionRoot;
    [SerializeField] GameObject m_choicePrefabs;

    public List<GameObject> m_choiceObjects = new List<GameObject>();
    public int m_selectedIndex = 0;
    public Action<int> m_onSelect;
    public bool m_isSelecting = false;
    public bool IsSelecting => m_isSelecting;

    private void Start()
    {
        gameObject.SetActive(false);
    }
    private void Update()
    {
        HandleSelectionInput();
    }

    public void UpdateDialogue(string speaker, string dialogue, Sprite portrait, SpeakerPosition position)
    {
        m_speakerName.text = speaker;
        m_dialogueText.text = dialogue;

        m_leftPortrait.gameObject.SetActive(position == SpeakerPosition.Left && portrait != null);
        m_rightPortrait.gameObject.SetActive(position == SpeakerPosition.Right && portrait != null);

        if (position == SpeakerPosition.Left) 
        {
            m_leftPortrait.sprite = portrait;
        } 
        else if (position == SpeakerPosition.Right)
        {
            m_rightPortrait.sprite = portrait;

        }
        //  대사 갱신 시 선택지 UI 꺼주기
        m_selectionRoot.SetActive(false);
        m_isSelecting = false;
    }


    public void ShowSelection(List<DialogueChoice> choices, Action<int> onSelect)
    {
        m_onSelect = onSelect;
        m_selectedIndex = 0;
        m_choiceObjects.Clear();

        foreach (Transform child in m_selectionRoot.transform)
            Destroy(child.gameObject);

        for (int i = 0; i < choices.Count; i++)
        {
            var go = Instantiate(m_choicePrefabs, m_selectionRoot.transform);
            var choiceUI = go.GetComponent<ChoiceUI>();
            choiceUI.SetText(choices[i].choiceText);
            choiceUI.SetHighlight(i == 0); // 첫 번째만 강조
            m_choiceObjects.Add(go);
        }

        m_selectionRoot.SetActive(true);
        m_isSelecting = true;
    }

    public void HandleSelectionInput()
    {
        if (!m_isSelecting || m_choiceObjects.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            m_selectedIndex = (m_selectedIndex - 1 + m_choiceObjects.Count) % m_choiceObjects.Count;
            HighlightChoice(m_selectedIndex);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            m_selectedIndex = (m_selectedIndex + 1) % m_choiceObjects.Count;
            HighlightChoice(m_selectedIndex);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            SelectChoice(m_selectedIndex);
        }
    }

    private void HighlightChoice(int index)
    {
        for (int i = 0; i < m_choiceObjects.Count; i++)
        {
            var choiceUI = m_choiceObjects[i].GetComponent<ChoiceUI>();
            choiceUI?.SetHighlight(i == index);
        }
    }

    private void SelectChoice(int index)
    {
        m_isSelecting = false;
        m_selectionRoot.SetActive(false);
        m_onSelect?.Invoke(index);
    }

    public void HideDialogue()
    {
        gameObject.SetActive(false);
        m_isSelecting = false;
        GManager.Instance.IsUIManager.DialogueOpenFlag = false;
    }

    public void ShowCutScene(Sprite sprite)
    {
        m_nameBox.SetActive(false);
        m_dialogueBox.SetActive(false);
        m_choicePrefabs.SetActive(false);

        m_cutSceneDisplay.sprite = sprite;
        m_cutScenePanel.SetActive(true);
    }

    public void HideCutScene()
    {
        m_cutScenePanel.SetActive(false);
        m_cutSceneDisplay.sprite = null;

        m_nameBox.SetActive(true);
        m_dialogueBox.SetActive(true);
    }

}
