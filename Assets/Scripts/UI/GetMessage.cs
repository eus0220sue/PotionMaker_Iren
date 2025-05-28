using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Linq;


public class GetMessage : MonoBehaviour
{

    [SerializeField] GameObject itemEntryPrefab;
    [SerializeField] Transform itemListParent;
    [SerializeField] int maxEntries = 2;

    private Queue<GameObject> activeItemEntries = new Queue<GameObject>();

    public void AddItemMessage(string itemName, int count)
    {
        if (activeItemEntries.Count >= maxEntries)
        {
            var oldEntry = activeItemEntries.Dequeue();
            Destroy(oldEntry);
        }

        var entry = Instantiate(itemEntryPrefab, itemListParent);
        var textComp = entry.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (textComp != null)
            textComp.text = $"{itemName} {count}개 획득";
        else
            Debug.LogWarning("TextMeshProUGUI 컴포넌트를 찾을 수 없습니다.");

        activeItemEntries.Enqueue(entry);

        StartCoroutine(FadeOutAndRemove(entry, 5f));
    }

    private IEnumerator FadeOutAndRemove(GameObject entry, float delay)
    {
        yield return new WaitForSeconds(delay);

        CanvasGroup cg = entry.GetComponent<CanvasGroup>();
        if (cg == null) cg = entry.AddComponent<CanvasGroup>();

        float t = 0f;
        float fadeDuration = 1f; // 페이드 아웃 시간

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        // 페이드 아웃 끝난 후 큐에서 제거 (혹시 중복 제거 방지)
        if (activeItemEntries.Contains(entry))
            activeItemEntries = new Queue<GameObject>(activeItemEntries.Where(e => e != entry));

        Destroy(entry);
    }
}
