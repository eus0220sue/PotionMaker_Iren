using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Example_door : MonoBehaviour
{
    bool isOpen;

    // SavableObject_GM → Apply()에서 호출
    void OnApplyOpenState(bool opened)
    {
        isOpen = opened;
        // 애니메이션/콜라이더/사운드 등 반영
    }

    // SavableObject_GM → Capture()에서 호출
    void QueryOpenState(System.Action<bool> cb) => cb?.Invoke(isOpen);

    public void Open() { isOpen = true;  /* 비주얼 반영 */ }
    public void Close() { isOpen = false; /* 비주얼 반영 */ }
}
