using UnityEngine;

public class PlayerBind_GM : MonoBehaviour
{
    void OnEnable()
    {
        if (!GManager.Instance) return;
        GManager.Instance.OnAfterLoad += ApplyFromSave;
        GManager.Instance.OnBeforeSave += CaptureToSave;

        // 타이틀에서 이어하기/처음하기 직후에도 즉시 반영
        ApplyFromSave();
    }

    void OnDisable()
    {
        if (!GManager.Instance) return;
        GManager.Instance.OnAfterLoad -= ApplyFromSave;
        GManager.Instance.OnBeforeSave -= CaptureToSave;
    }

    void ApplyFromSave()
    {
        if (!GManager.Instance) return;
        transform.position = GManager.Instance.GetPlayerPos();
        transform.rotation = GManager.Instance.GetPlayerRot();
    }

    void CaptureToSave()
    {
        if (!GManager.Instance) return;
        GManager.Instance.MarkPlayer(transform);
    }
}
