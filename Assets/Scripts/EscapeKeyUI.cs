using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EscapeKeyUI : MonoBehaviour
{
    public Slider volumeSlider;
    public TMP_Text volumeValueText;
    public Sprite[] keyMapSprites;      // ����Ű ĭ ����/���� �̹��� [0:����, 1:����]
    public Sprite[] toTitleSprites;     // Ÿ��Ʋ�� ���ư��� ĭ ����/���� �̹��� [0:����, 1:����]
    public Image keyMapImage;
    public Image toTitleImage;
    private float holdDelay = 0.2f;
    private float holdTimer = 0f;
    public GameObject KeyGuideGameObj;

    private int selectedIndex = 0;

    void OnEnable()
    {
        selectedIndex = 0;  // ���� �����̴� ���� ���·� �ʱ�ȭ
        UpdateSelectionVisuals();
        UpdateUI();

        if (KeyGuideGameObj != null)
            KeyGuideGameObj.SetActive(false);  // UI �� �� Ű���̵� ����
    }

    void OnDisable()
    {
        if (KeyGuideGameObj != null)
            KeyGuideGameObj.SetActive(false);  // UI �� �� Ű���̵� ����
    }

    void Start()
    {
        if (SoundManager.Instance != null)
        {
            volumeSlider.value = SoundManager.Instance.m_systemVolume;
            UpdateVolumeText();
        }
        else
        {
            volumeSlider.value = 0.5f; // fallback ��
            UpdateVolumeText();
        }
    }

    void Update()
    {

        bool isHolding = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);

        // �ܹ� �Է�: GetKeyDown���� �� ���� ����
        if (selectedIndex == 0)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                AdjustVolume(-0.01f);
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                AdjustVolume(0.01f);
        }

        // ���� �Է� ó��
        if (selectedIndex == 0 && isHolding)
        {
            holdTimer -= Time.deltaTime;
            if (holdTimer <= 0f)
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                    AdjustVolume(-0.01f);
                else if (Input.GetKey(KeyCode.RightArrow))
                    AdjustVolume(0.01f);

                holdTimer = holdDelay;
            }
        }
        else
        {
            holdTimer = holdDelay;  // �Է� ������ �ʱ�ȭ
        }
        HandleInput();
        UpdateSelectionVisuals();
        UpdateUI();
    }


    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            selectedIndex = (selectedIndex + 1) % 3;
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            selectedIndex = (selectedIndex + 3 - 1) % 3;
        }

        if (selectedIndex == 0)
        {
            float step = 0.01f;
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                AdjustVolume(-step);
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                AdjustVolume(step);
        }

        if (selectedIndex == 1)
        {
            // Space ������ KeyGuideGameObj �ѱ�
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (KeyGuideGameObj != null)
                    KeyGuideGameObj.SetActive(true);
            }

            // Z ������ KeyGuideGameObj ����
            if (Input.GetKeyDown(KeyCode.Z))
            {
                if (KeyGuideGameObj != null && KeyGuideGameObj.activeSelf)
                    KeyGuideGameObj.SetActive(false);
            }
        }


        if (selectedIndex == 2)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                if (currentSceneName != "Title")
                {
                    UnityEngine.SceneManagement.SceneManager.LoadScene("Title");
                    GManager.Instance.IsUIManager.escapeKeyUIOpenFlag = false;

                }
                else
                {
                    gameObject.SetActive(false);
                }
            }
        }


    }

    void UpdateSelectionVisuals()
    {
        keyMapImage.sprite = (selectedIndex == 1) ? keyMapSprites[1] : keyMapSprites[0];
        toTitleImage.sprite = (selectedIndex == 2) ? toTitleSprites[1] : toTitleSprites[0];
    }

    void UpdateUI()
    {
        volumeSlider.interactable = (selectedIndex == 0);
    }

    void AdjustVolume(float delta)
    {
        volumeSlider.value = Mathf.Clamp(volumeSlider.value + delta, volumeSlider.minValue, volumeSlider.maxValue);
        volumeValueText.text = Mathf.RoundToInt(volumeSlider.value * 100).ToString();

        float normalizedVolume = volumeSlider.value;

        SoundManager.Instance.SetSystemVolume(normalizedVolume);
        SoundManager.Instance.SetBGMVolume(normalizedVolume);
    }

    private void UpdateVolumeText()
    {
        volumeValueText.text = Mathf.RoundToInt(volumeSlider.value * 100).ToString();
    }
}
