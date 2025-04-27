using UnityEngine;

public class PotionCraftBox : MonoBehaviour, IInteractableInterface
{
    public void Interact()
    {
        GManager.Instance.IsUIManager.OpenPotionCraftUI();
    }

}