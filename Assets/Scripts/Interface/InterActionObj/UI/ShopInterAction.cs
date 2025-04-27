using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInterAction : MonoBehaviour, IInteractableInterface
{
    public void Interact()
    {
        GManager.Instance.IsUIManager.OpenShopUI();
    }
}
