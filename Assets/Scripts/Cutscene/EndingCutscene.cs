using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class EndingCutscene : MonoBehaviour
{
    [System.Serializable]
    public class EndingSlide
    {
        [Header("Image")]
        public Sprite image;

        [Header("Text")]
        [TextArea(2, 6)]
        public string text;

        [Header("Timing")]
        public float duration = 4f;
    }

    [Header("Slides")]
    [SerializeField]
    private EndingSlide[] slides;

    [Header("Ending UI")]
    [Tooltip("Canvas hoặc parent chứa toàn bộ UI Ending.")]
    [SerializeField]
    private GameObject endingUI;

    [SerializeField]
    private Image storyImage;

    [SerializeField]
    private GameObject textBackground;

    [SerializeField]
    private TMP_Text storyText;

    [SerializeField]
    private CanvasGroup fadeGroup;

    [SerializeField]
    private TMP_Text skipText;

    [Header("Gameplay UI")]
    [Tooltip("UI gameplay của Boss Scene. Sẽ tắt khi Ending bắt đầu.")]
    [SerializeField]
    private GameObject gameplayUI;

    [Header("Fade To Ending")]
    [Tooltip("Thời gian boss music fade out đồng thời với màn hình chuyển sang đen.")]
    [SerializeField]
    private float fadeToBlackDuration = 1.2f;

    [Tooltip("Thời gian fade in của ending music sau khi màn hình đã đen.")]
    [SerializeField]
    private float endingMusicFadeInDuration = 1.5f;

    [Tooltip("Thời gian giữ màn hình đen trước khi hiện ảnh đầu tiên.")]
    [SerializeField]
    private float blackScreenHoldDuration = 0.15f;

    [Header("Slide Fade")]
    [SerializeField]
    private float slideFadeDuration = 0.7f;

    [Header("End")]
    [SerializeField]
    private float endScreenDuration = 4f;

    [SerializeField]
    private string mainMenuSceneName = "MainMenu";

    private bool isPlaying;
    private bool skipRequested;

    private void Awake()
    {
        // Không tự Play khi Boss Scene load.
        // Chỉ ẩn nội dung Ending bằng CanvasGroup.
        isPlaying = false;
        skipRequested = false;

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = false;
            fadeGroup.interactable = false;
        }

        if (storyImage != null)
        {
            storyImage.enabled = false;
        }

        if (textBackground != null)
        {
            textBackground.SetActive(false);
        }

        if (storyText != null)
        {
            storyText.text = string.Empty;
        }

        if (skipText != null)
        {
            skipText.text = string.Empty;
            skipText.gameObject.SetActive(false);
        }

        // endingUI phải được giữ Active để script còn nhận PlayEnding().
        if (endingUI != null)
        {
            endingUI.SetActive(true);
        }
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            skipRequested = true;
        }
    }

    // =========================================================
    // CALL ONLY WHEN BOSS DIES
    // =========================================================

    public void PlayEnding()
    {
        if (isPlaying)
            return;

        isPlaying = true;
        skipRequested = false;

        if (endingUI != null)
        {
            endingUI.SetActive(true);
        }

        if (gameplayUI != null)
        {
            gameplayUI.SetActive(false);
        }

        if (skipText != null)
        {
            skipText.text = "SPACE - Skip";
            skipText.gameObject.SetActive(false);
        }

        if (textBackground != null)
        {
            textBackground.SetActive(false);
        }

        if (storyImage != null)
        {
            storyImage.enabled = false;
        }

        if (storyText != null)
        {
            storyText.text = string.Empty;
        }

        StartCoroutine(EndingRoutine());
    }

    private IEnumerator EndingRoutine()
    {
        // Ending 3 là slideshow UI, không dùng Cinemachine.
        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.StartEnding();
        }

        // -----------------------------------------------------
        // CHUẨN BỊ MÀN HÌNH ĐEN
        // -----------------------------------------------------

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            fadeGroup.blocksRaycasts = true;
            fadeGroup.interactable = true;
        }

        // -----------------------------------------------------
        // BOSS MUSIC FADE OUT + ENDING MUSIC
        // chạy song song với fade màn hình sang đen.
        // -----------------------------------------------------

        if (AudioController.Instance != null)
        {
            StartCoroutine(
                AudioController.Instance.TransitionToEndingMusicRoutine(
                    fadeToBlackDuration,
                    endingMusicFadeInDuration
                )
            );
        }

        // Gameplay -> đen dần.
        yield return Fade(0f, 1f, fadeToBlackDuration);

        // Khi màn hình đã đen, chờ một chút để ending music bắt đầu.
        if (blackScreenHoldDuration > 0f)
        {
            yield return new WaitForSecondsRealtime(
                blackScreenHoldDuration
            );
        }

        // -----------------------------------------------------
        // SLIDES
        // -----------------------------------------------------

        for (int i = 0; i < slides.Length; i++)
        {
            skipRequested = false;

            EndingSlide slide = slides[i];

            if (storyImage != null)
            {
                storyImage.sprite = slide.image;
                storyImage.enabled = slide.image != null;
            }

            bool hasText =
                !string.IsNullOrWhiteSpace(slide.text);

            if (storyText != null)
            {
                storyText.text =
                    hasText ? slide.text : string.Empty;
            }

            if (textBackground != null)
            {
                textBackground.SetActive(hasText);
            }

            if (skipText != null)
            {
                skipText.gameObject.SetActive(true);
            }

            // Đen -> slide.
            yield return Fade(1f, 0f, slideFadeDuration);

            float timer = 0f;

            while (
                timer < slide.duration &&
                !skipRequested
            )
            {
                timer += Time.unscaledDeltaTime;
                yield return null;
            }

            // Slide -> đen.
            yield return Fade(0f, 1f, slideFadeDuration);
        }

        // -----------------------------------------------------
        // THANK YOU SCREEN
        // -----------------------------------------------------

        if (storyImage != null)
        {
            storyImage.sprite = null;
            storyImage.enabled = false;
        }

        if (textBackground != null)
        {
            textBackground.SetActive(false);
        }

        if (skipText != null)
        {
            skipText.gameObject.SetActive(false);
        }

        if (storyText != null)
        {
            
            storyText.text =
                "GODDESS TRIAL-THANK YOU FOR PLAYING";
        }

        skipRequested = false;

        yield return Fade(1f, 0f, slideFadeDuration);

        float endTimer = 0f;

        while (
            endTimer < endScreenDuration &&
            !skipRequested
        )
        {
            endTimer += Time.unscaledDeltaTime;
            yield return null;
        }

        // -----------------------------------------------------
        // FADE TO BLACK
        // -----------------------------------------------------

        yield return Fade(0f, 1f, fadeToBlackDuration);

        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.EndEnding();
        }

        // -----------------------------------------------------
        // RETURN MAIN MENU
        // -----------------------------------------------------

        isPlaying = false;


        // Sau khi hoàn thành game,
        // save point quay về Village.
        SaveGameManager.SetSavedScene(
            SaveGameManager.DefaultScene
        );


        SceneManager.LoadScene(
            mainMenuSceneName
        );
    }

    // =========================================================
    // FADE
    // =========================================================

    private IEnumerator Fade(
        float from,
        float to,
        float duration
    )
    {
        if (fadeGroup == null)
            yield break;

        if (duration <= 0f)
        {
            fadeGroup.alpha = to;
            yield break;
        }

        float timer = 0f;

        fadeGroup.alpha = from;

        while (timer < duration)
        {
            timer += Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    timer / duration
                );

            fadeGroup.alpha =
                Mathf.Lerp(
                    from,
                    to,
                    t
                );

            yield return null;
        }

        fadeGroup.alpha = to;
    }
}
