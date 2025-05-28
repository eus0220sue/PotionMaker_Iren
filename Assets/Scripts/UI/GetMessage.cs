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
            textComp.text = $"{itemName} {count}�� ȹ��";
        else
            Debug.LogWarning("TextMeshProUGUI ������Ʈ�� ã�� �� �����ϴ�.");

        activeItemEntries.Enqueue(entry);

        StartCoroutine(FadeOutAndRemove(entry, 5f));
    }

    private IEnumerator FadeOutAndRemove(GameObject entry, float delay)
    {
        yield return new WaitForSeconds(delay);

        CanvasGroup cg = entry.GetComponent<CanvasGroup>();
        if (cg == null) cg = entry.AddComponent<CanvasGroup>();

        float t = 0f;
        float fadeDuration = 1f; // ���̵� �ƿ� �ð�

        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }

        // ���̵� �ƿ� ���� �� ť���� ���� (Ȥ�� �ߺ� ���� ����)
        if (activeItemEntries.Contains(entry))
            activeItemEntries = new Queue<GameObject>(activeItemEntries.Where(e => e != entry));

        Destroy(entry);
    }
}
