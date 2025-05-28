using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EscapeKeyUI : MonoBehaviour
{
    public Slider volumeSlider;
    public TMP_Text volumeValueText;
    public Sprite[] keyMapSprites;      // 조작키 칸 비선택/선택 이미지 [0:비선택, 1:선택]
    public Sprite[] toTitleSprites;     // 타이틀로 돌아가기 칸 비선택/선택 이미지 [0:비선택, 1:선택]
    public Image keyMapImage;
    public Image toTitleImage;
    private float holdDelay = 0.2f;
    private float holdTimer = 0f;
    public GameObject KeyGuideGameObj;

    private int selectedIndex = 0;

    void OnEnable()
    {
        selectedIndex = 0;  // 사운드 슬라이더 선택 상태로 초기화
        UpdateSelectionVisuals();
        UpdateUI();

        if (KeyGuideGameObj != null)
            KeyGuideGameObj.SetActive(false);  // UI 켤 때 키가이드 끄기
    }

    void OnDisable()
    {
        if (KeyGuideGameObj != null)
            KeyGuideGameObj.SetActive(false);  // UI 끌 때 키가이드 끄기
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
            volumeSlider.value = 0.5f; // fallback 값
            UpdateVolumeText();
        }
    }

    void Update()
    {

        bool isHolding = Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow);

        // 단발 입력: GetKeyDown으로 한 번만 반응
        if (selectedIndex == 0)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                AdjustVolume(-0.01f);
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                AdjustVolume(0.01f);
        }

        // 연속 입력 처리
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
            holdTimer = holdDelay;  // 입력 없으면 초기화
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
            // Space 누르면 KeyGuideGameObj 켜기
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (KeyGuideGameObj != null)
                    KeyGuideGameObj.SetActive(true);
            }

            // Z 누르면 KeyGuideGameObj 끄기
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
