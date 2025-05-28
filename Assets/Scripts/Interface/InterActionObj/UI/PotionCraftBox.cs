using UnityEngine;

public class PotionCraftBox : MonoBehaviour, IInteractableInterface
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite highlightSprite;
    [SerializeField] private GameObject guideUI;

    public void Interact()
    {
        GManager.Instance.IsUIManager.OpenPotionCraftUI();
    }

    public void OnFocusEnter()
    {
        SetHighlight(true);
    }

    public void OnFocusExit()
    {
        SetHighlight(false);
    }
    public void SetHighlight(bool active)
    {
        if (spriteRenderer != null)
            spriteRenderer.sprite = active ? highlightSprite : normalSprite;

        if (guideUI != null)
            guideUI.SetActive(active);
    }
}
