using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HPUI : MonoBehaviour
{
    public List<Image> hpImages; // 총 9개 세팅 (에디터에서 순서대로 넣기)
    public Sprite fullHPSprite;
    public Sprite emptyHPSprite;
    public Sprite lockedHPSprite;

    public int maxHP = 4;     // 현재 최대 HP
    public int currentHP = 4; // 현재 남은 HP

    public void SetHP(int current, int max)
    {
        currentHP = current;
        maxHP = max;

        for (int i = 0; i < hpImages.Count; i++)
        {
            if (i < maxHP)
            {
                hpImages[i].enabled = true;
                hpImages[i].sprite = (i < currentHP) ? fullHPSprite : emptyHPSprite;
            }
            else
            {
                hpImages[i].enabled = false; // 또는 lockedHPSprite로 잠김 상태 표현
                // hpImages[i].sprite = lockedHPSprite;
            }
        }
    }

    public void AddMaxHP()
    {
        if (maxHP < hpImages.Count)
            maxHP++;

        // 체력도 함께 증가시킬지 여부는 선택
        if (currentHP < maxHP)
            currentHP++;

        SetHP(currentHP, maxHP);
    }
}

