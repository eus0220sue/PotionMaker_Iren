using TMPro;
using UnityEngine;

public class ErrorMessage : MonoBehaviour
{

    [SerializeField] private GameObject errorMessagePrefab;

    private GameObject currentErrorMessage;


    /// <summary>
    /// Ư�� �θ� Transform �Ʒ��� �޽��� ǥ�� (�ߺ� ���� + �ڵ� ����)
    /// </summary>
    public void ShowErrorMessage(string message, Transform parent)
    {
        if (errorMessagePrefab == null)
        {
            Debug.LogWarning("ErrorMessage Prefab�� �Ҵ�Ǿ� ���� �ʽ��ϴ�.");
            return;
        }

        // ���� �޽��� ����
        if (currentErrorMessage != null)
        {
            Destroy(currentErrorMessage);
        }

        currentErrorMessage = Instantiate(errorMessagePrefab, parent);
        var tmp = currentErrorMessage.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (tmp != null) tmp.text = message;

        Destroy(currentErrorMessage, 2f);
    }
}
