using UnityEngine;
using UnityEngine.Playables;

public class BossIntroCutscene : MonoBehaviour
{
    [Header("Timeline")]
    [SerializeField] private PlayableDirector director;

    [Header("Boss")]
    [SerializeField] private BossController bossController;

    [Header("Player")]
    [SerializeField] private PlayerController playerController;

    [Header("UI")]
    [SerializeField] private GameObject gameplayUI;

    [Header("Settings")]
    [SerializeField] private bool playOnStart = true;
    [SerializeField] private bool allowSkip = true;
    [SerializeField] private KeyCode skipKey = KeyCode.Space;

    private bool isPlaying;
    private bool hasFinished;

    private void Awake()
    {
        if (director == null)
        {
            Debug.LogError(
                "BossIntroCutscene: PlayableDirector chưa được gắn.",
                this
            );

            return;
        }

        // Rất quan trọng:
        // Script sẽ tự Play, không cho Director tự Play.
        director.playOnAwake = false;
    }

    private void Start()
    {
        if (!playOnStart)
            return;

        PlayBossIntro();
    }

    private void Update()
    {
        if (!isPlaying)
            return;

        if (!allowSkip)
            return;

        if (Input.GetKeyDown(skipKey))
        {
            SkipBossIntro();
        }
    }

    public void PlayBossIntro()
    {
        if (director == null)
            return;

        if (isPlaying)
            return;

        isPlaying = true;
        hasFinished = false;

        // -------------------------------------------------
        // Khóa Player + camera gameplay
        // -------------------------------------------------

        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.StartCutscene();
        }
        else
        {
            Debug.LogWarning(
                "BossIntroCutscene: Không tìm thấy CutsceneManager."
            );
        }

        // -------------------------------------------------
        // Khóa Boss AI
        // -------------------------------------------------

        if (bossController != null)
        {
            bossController.enabled = false;
        }

        // -------------------------------------------------
        // Ẩn gameplay UI
        // -------------------------------------------------

        if (gameplayUI != null)
        {
            gameplayUI.SetActive(false);
        }

        // -------------------------------------------------
        // Đăng ký callback
        // -------------------------------------------------

        director.stopped -= OnTimelineStopped;
        director.stopped += OnTimelineStopped;

        // -------------------------------------------------
        // Chạy từ đầu
        // -------------------------------------------------

        director.time = 0;
        director.Evaluate();
        director.Play();
    }

    private void SkipBossIntro()
    {
        if (director == null)
            return;

        if (!isPlaying)
            return;

        if (director.state != PlayState.Playing)
            return;

        // Nhảy đến cuối Timeline
        director.time = director.duration;
        director.Evaluate();
        director.Stop();
    }

    private void OnTimelineStopped(PlayableDirector stoppedDirector)
    {
        FinishBossIntro();
    }

    private void FinishBossIntro()
    {
        if (hasFinished)
            return;

        hasFinished = true;
        isPlaying = false;

        // -------------------------------------------------
        // Hủy callback
        // -------------------------------------------------

        if (director != null)
        {
            director.stopped -= OnTimelineStopped;
        }

        // -------------------------------------------------
        // Hiện gameplay UI
        // -------------------------------------------------

        if (gameplayUI != null)
        {
            gameplayUI.SetActive(true);
        }

        // -------------------------------------------------
        // Bật Boss AI
        // -------------------------------------------------

        if (bossController != null)
        {
            bossController.enabled = true;
        }

        // -------------------------------------------------
        // Trả camera + Player về gameplay
        // -------------------------------------------------

        if (CutsceneManager.Instance != null)
        {
            CutsceneManager.Instance.EndCutscene();
        }

        Debug.Log("Boss Intro Cutscene Finished.");
    }

    private void OnDestroy()
    {
        if (director != null)
        {
            director.stopped -= OnTimelineStopped;
        }
    }
}