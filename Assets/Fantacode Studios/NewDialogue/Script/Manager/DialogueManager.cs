using System;
using System.Collections.Generic;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public DialogueUI m_dialougeUI => GManager.Instance.IsDialougeUI;
    public DialogueNode m_currentNode;

    private DialogueNode m_pendingNextNode = null;
    private bool m_isCutScene = false;
    public event Action OnDialogueEnded;

    public Dictionary<string, bool> m_flagTable = new Dictionary<string, bool>();
    private Dictionary<string, DialogueNode> m_dialogueNodes = new Dictionary<string, DialogueNode>();
    public event Action OnDialogueStarted;

    private void Update()
    {
        // �ƾ� ������ ��
        if (m_isCutScene)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                m_isCutScene = false;
                m_dialougeUI.HideCutScene();
                ContinueDialogue(m_pendingNextNode); // ���� ���� �̵�
            }
            return; // �ƾ� ���¿����� �ٸ� ���� �ǳʶ�
        }

        if (m_currentNode == null) return;
        if (m_dialougeUI.IsSelecting) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            m_currentNode.Execute(this);
        }
    }

    public void StartDialogue(DialogueNode startNode)
    {
        m_currentNode = startNode;
        m_currentNode.Execute(this);

        OnDialogueStarted?.Invoke();  // ��ȭ ���� ��ȣ �߻�
    }
    public void ShowDialogue(string speaker, string text, Sprite portrait, SpeakerPosition position, DialogueNode nextNode)
    {
        m_dialougeUI.UpdateDialogue(speaker, text, portrait, position);
        m_currentNode = nextNode;
    }
    public void ShowDialogueWithChoices(string speaker, string text, Sprite portrait, SpeakerPosition pos, List<DialogueChoice> choices)
    {
        m_dialougeUI.UpdateDialogue(speaker, text, portrait, pos);
        m_dialougeUI.ShowSelection(choices, OnChoiceSelected);
    }
    public void ShowChoices(List<DialogueChoice> choices)
    {
        m_dialougeUI.ShowSelection(choices, OnChoiceSelected);
    }
    public void ShowCutScene(Sprite sprite, DialogueNode nextNode)
    {
        m_isCutScene = true;
        m_pendingNextNode = nextNode;
        m_dialougeUI.ShowCutScene(sprite);
    }
    // ��ȭ ��� ��ȸ �Լ�
    public DialogueNode GetDialogueNodeByID(string dialogueID)
    {
        if (m_dialogueNodes.TryGetValue(dialogueID, out var node))
        {
            return node;
        }
        return null;
    }

    private void OnChoiceSelected(int index)
    {
        var next = m_currentNode.choices[index].nextNode;
        ContinueDialogue(next);
    }

    public void ContinueDialogue(DialogueNode next)
    {
        if (next == null)
        {
            EndDialogue();
            return;
        }

        m_currentNode = next;
        m_currentNode.Execute(this);
    }

    public void EndDialogue()
    {
        m_dialougeUI.HideDialogue();
        m_currentNode = null;

        GManager.Instance.IsUIManager.DialogueOpenFlag = false;
        GManager.Instance.IsUserController.isInteracting = false;
        GManager.Instance.IsUserController.StartInteractCooldown();
        OnDialogueEnded?.Invoke();  // �ݵ�� ȣ��Ǿ�� ��
    }

    public void SetFlag(string flagName, bool value)
    {
        m_flagTable[flagName] = value;
    }

    public bool GetFlag(string flagName)
    {
        return GManager.Instance.IsQuestManager.GetQuestFlag(flagName);
    }
}