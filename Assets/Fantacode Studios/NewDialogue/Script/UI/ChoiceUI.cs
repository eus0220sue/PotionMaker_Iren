using TMPro;
using UnityEngine;

public class ChoiceUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI m_text;
    [SerializeField] GameObject m_highlight;

    public void SetText(string text)
    {
        if (m_text != null)
            m_text.text = text;
    }

    public void SetHighlight(bool active)
    {
        if (m_highlight != null)
            m_highlight.SetActive(active);
    }
}
