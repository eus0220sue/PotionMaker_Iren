using UnityEngine;

public class PlayerBind_GM : MonoBehaviour
{
    // 저장 직전 호출
    public void Capture()
    {
        var ex = GManager.Instance?.IsExchangeManager;

        // 골드/등급
        int gold = ex != null ? ex.GetPlayerGold() : 0;
        int grade = ex != null ? ex.GetPlayerGradeInt() : 0;

        SaveLoad.SetInt(GManager.Keys.Gold, gold);
        SaveLoad.SetInt(GManager.Keys.Grade, grade);

        // 위치/회전
        var t = GManager.Instance?.IsUserController ? GManager.Instance.IsUserController.transform : null;
        if (t != null)
        {
            SaveLoad.SetVector3(GManager.Keys.Pos, t.position);
            SaveLoad.SetQuaternion(GManager.Keys.Rot, t.rotation);
        }
        else
        {
        }

    }

    // 로드 직후 호출
    public void Apply()
    {
        // 골드/등급
        int gold = SaveLoad.GetInt(GManager.Keys.Gold, 0);
        int grade = SaveLoad.GetInt(GManager.Keys.Grade, 0);

        var ex = GManager.Instance?.IsExchangeManager;
        if (ex != null)
        {
            ex.SetPlayerGold(gold);
            ex.SetPlayerGrade((GradeType.Type)grade);
        }
        GManager.Instance?.IsHUDUI?.UpdateGold(gold);

        // 위치/회전
        Vector3 pos = SaveLoad.GetVector3(GManager.Keys.Pos, Vector3.zero);
        Quaternion rot = SaveLoad.GetQuaternion(GManager.Keys.Rot, Quaternion.identity);

        var t = GManager.Instance?.IsUserController ? GManager.Instance.IsUserController.transform : null;
        if (t != null)
        {
            t.SetPositionAndRotation(pos, rot);
        }
        else
        {
        }

    }
}
