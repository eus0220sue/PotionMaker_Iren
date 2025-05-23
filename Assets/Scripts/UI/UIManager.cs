using UnityEngine;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public GameObject CraftUI;
    public GameObject PotionCraftUI;
    public GameObject ShopUI;
    public GameObject DialogueUI;

    /// <summary>
    /// 플래그 세팅
    /// </summary>
    public bool UIOpenFlag
    {
        get
        {
            return CraftUIOpenFlag
                || PotionCraftUIOpenFlag
                || DialogueOpenFlag
                || ShopUIOpenFlag
                || GManager.Instance.IsInventoryUI.isOpen;

        }
    }
    public bool CraftUIOpenFlag = false;
    public bool PotionCraftUIOpenFlag = false;
    public bool DialogueOpenFlag = false;
    public bool ShopUIOpenFlag = false;
    /*void Awake()
    {
        if (CraftUI == null) CraftUI = GameObject.Find("CraftUI");
        if (PotionCraftUI == null) PotionCraftUI = GameObject.Find("PotionCraftUI");
        if (ShopUI == null) ShopUI = GameObject.Find("ShopUI");
        if (DialogueUI == null) DialogueUI = GameObject.Find("DialogueUI");
    }
    */

    void Update()
    {
        CloseUI();
    }
    public void OpenCraftUI()
    {
        CraftUIOpenFlag = true;
        CraftUI.SetActive(true);
        GManager.Instance.IsCraftUI.InitCraftUI();
    }
    public void OpenPotionCraftUI()
    {
        PotionCraftUIOpenFlag = true;
        PotionCraftUI.SetActive(true);
        GManager.Instance.IsPotionCraftUI.InitPotionUI();

    }
    public void OpenShopUI()
    {
        ShopUIOpenFlag = true;
        ShopUI.SetActive(true);
        GManager.Instance.IsShopUI.InitShopUI();
    }

    public void OpenDialogueUI(DialogueNode startNode)
    {
        DialogueOpenFlag = true;

        if (DialogueUI == null)
        {
            Debug.LogError("[UIManager] DialogueUI가 연결되지 않았습니다.");
            return;
        }

        DialogueUI.SetActive(true);

        if (startNode == null)
        {
            Debug.LogWarning("[UIManager] startNode가 null입니다.");
            return;
        }

        GManager.Instance.IsDialogueManager.StartDialogue(startNode);
    }
    public void CloseUI()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (CraftUIOpenFlag)
            {
                CraftUIOpenFlag = false;
                CraftUI.SetActive(false);
            }
            if (PotionCraftUIOpenFlag)
            {
                PotionCraftUIOpenFlag = false;
                PotionCraftUI.SetActive(false);
            }
            if (ShopUIOpenFlag)
            {
                ShopUIOpenFlag = false;
                ShopUI.SetActive(false);
            }
            // 다른 UI들도 추후 여기에 추가
            if (DialogueOpenFlag)
            {
                DialogueOpenFlag = false;
                DialogueUI.SetActive(false);
            }
            GManager.Instance.IsUserController.isInteracting = false; // 인터렉션 가능 상태로 복원

        }
    }
}
