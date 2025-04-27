using UnityEngine;

public class CraftBox : MonoBehaviour, IInteractableInterface
{
    public void Interact()
    {
        GManager.Instance.IsUIManager.OpenCraftUI();
    }

}