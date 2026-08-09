using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }


    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Background Music")]
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] private AudioClip villageMusic;
    [SerializeField] private AudioClip battleMusic;
    [SerializeField] private AudioClip bossMusic;

    [Header("UI Button SFX")]
    [SerializeField] private AudioClip buttonClickSFX;

    [Range(0f, 1f)]
    [SerializeField] private float buttonClickVolume = 1f;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    [Header("Music Transition")]
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Battle Music")]
    [Tooltip("Thời gian battle music nhỏ dần sau khi giết hết enemy.")]
    [SerializeField] private float battleClearFadeDuration = 2.5f;

    private Coroutine changeMusicCoroutine;
    private Coroutine battleFadeCoroutine;

    private bool currentSceneIsBattle;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene();

        PlayMusicForScene(
            currentScene.name,
            true
        );

        RegisterButtonClickSounds(currentScene);
    }

    private void SetupAudioSources()
    {
        if (musicSource == null)
        {
            musicSource =
                gameObject.AddComponent<AudioSource>();
        }

        if (sfxSource == null)
        {
            sfxSource =
                gameObject.AddComponent<AudioSource>();
        }

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.spatialBlend = 0f;
        sfxSource.volume = sfxVolume;
    }

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        // Dừng coroutine fade của scene cũ.
        if (battleFadeCoroutine != null)
        {
            StopCoroutine(battleFadeCoroutine);
            battleFadeCoroutine = null;
        }

        if (changeMusicCoroutine != null)
        {
            StopCoroutine(changeMusicCoroutine);
            changeMusicCoroutine = null;
        }

        musicSource.volume = musicVolume;

        /*
         * true:
         * Nếu đây là battle scene thì bắt đầu
         * battle music lại từ đầu.
         */
        PlayMusicForScene(
            scene.name,
            true
        );

        RegisterButtonClickSounds(scene);
    }

    private void RegisterButtonClickSounds(
        Scene scene)
    {
        if (!scene.IsValid() ||
            !scene.isLoaded)
        {
            return;
        }

        GameObject[] rootObjects =
            scene.GetRootGameObjects();

        foreach (GameObject rootObject
                 in rootObjects)
        {
            Button[] buttons =
                rootObject.GetComponentsInChildren<Button>(
                    true);

            foreach (Button button in buttons)
            {
                if (button == null)
                    continue;

                button.onClick.RemoveListener(
                    PlayButtonClickSFX);

                button.onClick.AddListener(
                    PlayButtonClickSFX);
            }
        }
    }

    public void PlayButtonClickSFX()
    {
        if (buttonClickSFX == null)
            return;

        PlaySFX(
            buttonClickSFX,
            buttonClickVolume
        );
    }

    private void PlayMusicForScene(
        string sceneName,
        bool restartBattleMusic)
    {
        AudioClip targetMusic =
            GetMusicForScene(sceneName);

        currentSceneIsBattle =
            IsBattleScene(sceneName);

        if (targetMusic == null)
        {
            Debug.LogWarning(
                $"AudioController: Chưa xác định nhạc cho scene '{sceneName}'."
            );

            return;
        }

        /*
         * Battle scene:
         * luôn chạy lại nhạc từ 0 khi vào scene mới.
         */
        if (currentSceneIsBattle &&
            restartBattleMusic)
        {
            PlayMusicFromBeginning(targetMusic);
            return;
        }

        PlayMusic(targetMusic);
    }

    private bool IsBattleScene(
        string sceneName)
    {
        string lowerSceneName =
            sceneName.ToLower();

        /*
         * Boss phải kiểm tra trước Level
         * nếu tên scene boss có chứa "level".
         */
        if (lowerSceneName.Contains("boss"))
            return false;

        return lowerSceneName.Contains("level");
    }

    private AudioClip GetMusicForScene(
        string sceneName)
    {
        string lowerSceneName =
            sceneName.ToLower();

        if (lowerSceneName.Contains("mainmenu") ||
            lowerSceneName.Contains("main_menu") ||
            lowerSceneName == "menu")
        {
            return mainMenuMusic;
        }

        if (lowerSceneName.Contains("village"))
        {
            return villageMusic;
        }

        if (lowerSceneName.Contains("boss"))
        {
            return bossMusic;
        }

        if (lowerSceneName.Contains("level"))
        {
            return battleMusic;
        }

        return null;
    }

    /// <summary>
    /// Dùng khi chuyển giữa các Level.
    /// Battle Music luôn bắt đầu lại từ giây 0.
    /// </summary>
    private void PlayMusicFromBeginning(
        AudioClip clip)
    {
        if (clip == null ||
            musicSource == null)
        {
            return;
        }

        if (changeMusicCoroutine != null)
        {
            StopCoroutine(changeMusicCoroutine);
            changeMusicCoroutine = null;
        }

        if (battleFadeCoroutine != null)
        {
            StopCoroutine(battleFadeCoroutine);
            battleFadeCoroutine = null;
        }

        musicSource.Stop();

        musicSource.clip = clip;
        musicSource.time = 0f;
        musicSource.volume = musicVolume;
        musicSource.loop = true;

        musicSource.Play();
    }

    public void PlayMusic(
        AudioClip clip)
    {
        if (clip == null ||
            musicSource == null)
        {
            return;
        }

        if (battleFadeCoroutine != null)
        {
            StopCoroutine(battleFadeCoroutine);
            battleFadeCoroutine = null;
        }

        /*
         * Với MainMenu/Village/Boss:
         * nếu đang đúng bài thì không restart.
         */
        if (musicSource.clip == clip &&
            musicSource.isPlaying)
        {
            musicSource.volume =
                musicVolume;

            return;
        }

        if (changeMusicCoroutine != null)
        {
            StopCoroutine(changeMusicCoroutine);
        }

        changeMusicCoroutine =
            StartCoroutine(
                ChangeMusicRoutine(clip)
            );
    }

    private IEnumerator ChangeMusicRoutine(
        AudioClip newClip)
    {
        float originalVolume =
            musicVolume;

        if (musicSource.isPlaying &&
            fadeDuration > 0f)
        {
            float timer = 0f;

            float startVolume =
                musicSource.volume;

            while (timer < fadeDuration)
            {
                timer +=
                    Time.unscaledDeltaTime;

                musicSource.volume =
                    Mathf.Lerp(
                        startVolume,
                        0f,
                        timer / fadeDuration
                    );

                yield return null;
            }
        }

        musicSource.Stop();

        musicSource.clip =
            newClip;

        musicSource.time = 0f;
        musicSource.volume = 0f;
        musicSource.loop = true;

        musicSource.Play();

        if (fadeDuration > 0f)
        {
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer +=
                    Time.unscaledDeltaTime;

                musicSource.volume =
                    Mathf.Lerp(
                        0f,
                        originalVolume,
                        timer / fadeDuration
                    );

                yield return null;
            }
        }

        musicSource.volume =
            originalVolume;

        changeMusicCoroutine = null;
    }

    /// <summary>
    /// EnemySpawnArea gọi hàm này mỗi khi một area
    /// vừa được hoàn thành.
    ///
    /// Chỉ fade Battle Music nếu TẤT CẢ area
    /// trong scene đều đã completed.
    /// </summary>
    public void CheckAndFadeBattleMusicIfAllAreasCompleted()
    {
        if (!currentSceneIsBattle)
            return;

        EnemySpawnArea[] areas =
            FindObjectsByType<EnemySpawnArea>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );

        /*
         * Không có EnemySpawnArea thì không tự fade.
         */
        if (areas == null ||
            areas.Length == 0)
        {
            return;
        }

        foreach (EnemySpawnArea area in areas)
        {
            if (area == null)
                continue;

            if (!area.AreaCompleted)
            {
                // Vẫn còn ít nhất một khu chưa clear.
                return;
            }
        }

        FadeOutBattleMusic();
    }

    public void FadeOutBattleMusic()
    {
        if (!currentSceneIsBattle)
            return;

        if (musicSource == null ||
            !musicSource.isPlaying)
        {
            return;
        }

        if (musicSource.clip != battleMusic)
            return;

        if (changeMusicCoroutine != null)
        {
            StopCoroutine(changeMusicCoroutine);
            changeMusicCoroutine = null;
        }

        if (battleFadeCoroutine != null)
        {
            StopCoroutine(battleFadeCoroutine);
        }

        battleFadeCoroutine =
            StartCoroutine(
                FadeOutBattleMusicRoutine()
            );
    }

    private IEnumerator FadeOutBattleMusicRoutine()
    {
        float startVolume =
            musicSource.volume;

        if (battleClearFadeDuration <= 0f)
        {
            musicSource.volume = 0f;
            musicSource.Stop();

            battleFadeCoroutine = null;
            yield break;
        }

        float timer = 0f;

        while (timer <
               battleClearFadeDuration)
        {
            timer +=
                Time.unscaledDeltaTime;

            musicSource.volume =
                Mathf.Lerp(
                    startVolume,
                    0f,
                    timer /
                    battleClearFadeDuration
                );

            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();

        /*
         * Đặt volume lại để scene sau
         * có thể phát bình thường.
         */
        musicSource.volume =
            musicVolume;

        battleFadeCoroutine = null;
    }

    public void PlaySFX(
        AudioClip clip)
    {
        PlaySFX(
            clip,
            1f
        );
    }

    public void PlaySFX(
        AudioClip clip,
        float volumeScale)
    {
        if (clip == null ||
            sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volumeScale) *
            sfxVolume
        );
    }

    public void SetMusicVolume(
        float value)
    {
        musicVolume =
            Mathf.Clamp01(value);

        if (musicSource != null)
        {
            musicSource.volume =
                musicVolume;
        }
    }

    public void SetSFXVolume(
        float value)
    {
        sfxVolume =
            Mathf.Clamp01(value);

        if (sfxSource != null)
        {
            sfxSource.volume =
                sfxVolume;
        }
    }

    public void StopMusic()
    {
        if (changeMusicCoroutine != null)
        {
            StopCoroutine(
                changeMusicCoroutine
            );

            changeMusicCoroutine = null;
        }

        if (battleFadeCoroutine != null)
        {
            StopCoroutine(
                battleFadeCoroutine
            );

            battleFadeCoroutine = null;
        }

        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void PlayBossMusic()
    {
        currentSceneIsBattle = false;

        PlayMusic(bossMusic);
    }

    public void PlayBattleMusic()
    {
        currentSceneIsBattle = true;

        PlayMusicFromBeginning(
            battleMusic
        );
    }


}
