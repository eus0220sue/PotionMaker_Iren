using TMPro;
using UnityEngine;

public class ErrorMessage : MonoBehaviour
{

    [SerializeField] private GameObject errorMessagePrefab;

    private GameObject currentErrorMessage;


    /// <summary>
    /// 특정 부모 Transform 아래에 메시지 표시 (중복 제거 + 자동 삭제)
    /// </summary>
    public void ShowErrorMessage(string message, Transform parent)
    {
        if (errorMessagePrefab == null)
        {
            Debug.LogWarning("ErrorMessage Prefab이 할당되어 있지 않습니다.");
            return;
        }

        // 기존 메시지 삭제
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
