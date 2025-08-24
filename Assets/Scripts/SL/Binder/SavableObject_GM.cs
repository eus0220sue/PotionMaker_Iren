using UnityEngine;

public class SavableObject_GM : MonoBehaviour
{
    [Tooltip("씬 내 유일한 ID")] public string objectId;
    public enum Mode { OpenClose, Destroyable, Pickup }
    public Mode mode;

    void OnEnable()
    {
        if (!GManager.Instance) return;
        GManager.Instance.OnAfterLoad += Apply;
        GManager.Instance.OnBeforeSave += Capture;

        Apply(); // 씬 로드 직후가 아니어도 즉시 상태 반영
    }
    void OnDisable()
    {
        if (!GManager.Instance) return;
        GManager.Instance.OnAfterLoad -= Apply;
        GManager.Instance.OnBeforeSave -= Capture;
    }

    void Apply()
    {
        if (string.IsNullOrEmpty(objectId)) return;

        switch (mode)
        {
            case Mode.OpenClose:
                bool opened = SaveLoad.ContainsInStringSet(GManager.Keys.OpenedSet, objectId);
                // 문/상자 스크립트에 전달(선택)
                SendMessage("OnApplyOpenState", opened, SendMessageOptions.DontRequireReceiver);
                gameObject.SetActive(true); // 열림/닫힘은 개체 비활성화가 아니라 비주얼로 표현
                break;

            case Mode.Destroyable:
                bool destroyed = SaveLoad.ContainsInStringSet(GManager.Keys.DestroyedSet, objectId);
                gameObject.SetActive(!destroyed); // 파괴되었으면 비활성화
                break;

            case Mode.Pickup:
                bool picked = SaveLoad.ContainsInStringSet(GManager.Keys.PickedSet, objectId);
                gameObject.SetActive(!picked); // 줍고 사라졌으면 비활성화
                break;
        }
    }

    void Capture()
    {
        if (string.IsNullOrEmpty(objectId)) return;

        switch (mode)
        {
            case Mode.OpenClose:
                // 현재 열림 상태를 문/상자 스크립트에서 콜백으로 받음
                bool opened = false;
                SendMessage("QueryOpenState", (System.Action<bool>)(b => opened = b), SendMessageOptions.DontRequireReceiver);
                if (opened) SaveLoad.AddToStringSet(GManager.Keys.OpenedSet, objectId);
                break;

            case Mode.Destroyable:
                if (!gameObject.activeSelf)
                    SaveLoad.AddToStringSet(GManager.Keys.DestroyedSet, objectId);
                break;

            case Mode.Pickup:
                if (!gameObject.activeSelf)
                    SaveLoad.AddToStringSet(GManager.Keys.PickedSet, objectId);
                break;
        }
    }
}
