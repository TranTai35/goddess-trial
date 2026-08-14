using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AudioController : MonoBehaviour
{
    public static AudioController Instance { get; private set; }

    public static event Action OnBattleMusicFadeCompleted;


    [Header("Audio Sources")]
    [SerializeField]
    private AudioSource musicSource;

    [SerializeField]
    private AudioSource sfxSource;


    [Header("Background Music")]
    [SerializeField]
    private AudioClip mainMenuMusic;

    [SerializeField]
    private AudioClip villageMusic;

    [SerializeField]
    private AudioClip battleMusic;

    [SerializeField]
    private AudioClip bossMusic;


    [Header("Battle Clear Music")]
    [Tooltip(
        "Nhạc nền ngắn phát sau khi toàn bộ enemy trong Level đã bị tiêu diệt."
    )]
    [SerializeField]
    private AudioClip battleClearMusic;

    [Tooltip(
        "Thời gian fade in của Battle Clear Music."
    )]
    [SerializeField]
    private float battleClearMusicFadeInDuration = 0.5f;


    [Header("Ending Music")]
    [Tooltip(
        "Nhạc phát khi bắt đầu Ending Cutscene."
    )]
    [SerializeField]
    private AudioClip endingMusic;

    [Tooltip(
        "Thời gian fade in nhạc ending nếu gọi trực tiếp từ AudioController."
    )]
    [SerializeField]
    private float endingMusicFadeInDuration = 1.5f;


    [Header("UI Button SFX")]
    [SerializeField]
    private AudioClip buttonClickSFX;

    [Range(0f, 1f)]
    [SerializeField]
    private float buttonClickVolume = 1f;


    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField]
    private float musicVolume = 0.7f;

    [Range(0f, 1f)]
    [SerializeField]
    private float sfxVolume = 1f;


    [Header("Music Transition")]
    [SerializeField]
    private float fadeDuration = 0.5f;


    [Header("Battle Music")]
    [Tooltip(
        "Thời gian battle music nhỏ dần sau khi giết hết enemy."
    )]
    [SerializeField]
    private float battleClearFadeDuration = 2.5f;


    private Coroutine changeMusicCoroutine;
    private Coroutine battleFadeCoroutine;
    private Coroutine battleClearMusicCoroutine;

    private bool currentSceneIsBattle;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        SetupAudioSources();
    }


    // =========================================================
    // EVENT
    // =========================================================

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }


    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        Scene currentScene =
            SceneManager.GetActiveScene();

        PlayMusicForScene(
            currentScene.name,
            true
        );

        RegisterButtonClickSounds(
            currentScene
        );
    }


    // =========================================================
    // AUDIO SOURCE
    // =========================================================

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


    // =========================================================
    // SCENE LOADED
    // =========================================================

    private void OnSceneLoaded(
        Scene scene,
        LoadSceneMode mode)
    {
        if (battleFadeCoroutine != null)
        {
            StopCoroutine(
                battleFadeCoroutine
            );

            battleFadeCoroutine = null;
        }


        if (changeMusicCoroutine != null)
        {
            StopCoroutine(
                changeMusicCoroutine
            );

            changeMusicCoroutine = null;
        }


        if (battleClearMusicCoroutine != null)
        {
            StopCoroutine(
                battleClearMusicCoroutine
            );

            battleClearMusicCoroutine = null;
        }


        musicSource.volume =
            musicVolume;


        PlayMusicForScene(
            scene.name,
            true
        );


        RegisterButtonClickSounds(
            scene
        );
    }


    // =========================================================
    // BUTTON SFX
    // =========================================================

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


        foreach (
            GameObject rootObject
            in rootObjects
        )
        {
            Button[] buttons =
                rootObject.GetComponentsInChildren<Button>(
                    true
                );


            foreach (Button button in buttons)
            {
                if (button == null)
                    continue;


                button.onClick.RemoveListener(
                    PlayButtonClickSFX
                );


                button.onClick.AddListener(
                    PlayButtonClickSFX
                );
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


    // =========================================================
    // MUSIC FOR SCENE
    // =========================================================

    private void PlayMusicForScene(
        string sceneName,
        bool restartBattleMusic)
    {
        AudioClip targetMusic =
            GetMusicForScene(
                sceneName
            );


        currentSceneIsBattle =
            IsBattleScene(
                sceneName
            );


        if (targetMusic == null)
        {
            Debug.LogWarning(
                $"AudioController: Chưa xác định nhạc cho scene '{sceneName}'."
            );

            return;
        }


        /*
         * Battle Scene luôn restart nhạc từ đầu.
         */
        if (currentSceneIsBattle &&
            restartBattleMusic)
        {
            PlayMusicFromBeginning(
                targetMusic
            );

            return;
        }


        PlayMusic(
            targetMusic
        );
    }


    // =========================================================
    // IS BATTLE SCENE
    // =========================================================

    private bool IsBattleScene(
        string sceneName)
    {
        string lowerSceneName =
            sceneName.ToLower();


        /*
         * Boss Scene không dùng Battle Clear Music.
         */
        if (lowerSceneName.Contains("boss"))
        {
            return false;
        }


        return lowerSceneName.Contains(
            "level"
        );
    }


    // =========================================================
    // GET MUSIC
    // =========================================================

    private AudioClip GetMusicForScene(
        string sceneName)
    {
        string lowerSceneName =
            sceneName.ToLower();


        if (
            lowerSceneName.Contains("mainmenu")
            ||
            lowerSceneName.Contains("main_menu")
            ||
            lowerSceneName == "menu"
        )
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


    // =========================================================
    // PLAY MUSIC FROM BEGINNING
    // =========================================================

    private void PlayMusicFromBeginning(
        AudioClip clip)
    {
        if (clip == null ||
            musicSource == null)
        {
            return;
        }


        StopAllMusicCoroutines();


        musicSource.Stop();

        musicSource.clip =
            clip;

        musicSource.time =
            0f;

        musicSource.volume =
            musicVolume;

        musicSource.loop =
            true;

        musicSource.Play();
    }


    // =========================================================
    // PLAY MUSIC
    // =========================================================

    public void PlayMusic(
        AudioClip clip)
    {
        if (clip == null ||
            musicSource == null)
        {
            return;
        }


        if (
            musicSource.clip == clip &&
            musicSource.isPlaying
        )
        {
            musicSource.volume =
                musicVolume;

            return;
        }


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


        if (battleClearMusicCoroutine != null)
        {
            StopCoroutine(
                battleClearMusicCoroutine
            );

            battleClearMusicCoroutine = null;
        }


        changeMusicCoroutine =
            StartCoroutine(
                ChangeMusicRoutine(
                    clip
                )
            );
    }


    // =========================================================
    // CHANGE MUSIC ROUTINE
    // =========================================================

    private IEnumerator ChangeMusicRoutine(
        AudioClip newClip)
    {
        float originalVolume =
            musicVolume;


        if (
            musicSource.isPlaying &&
            fadeDuration > 0f
        )
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

        musicSource.time =
            0f;

        musicSource.volume =
            0f;

        musicSource.loop =
            true;

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


        changeMusicCoroutine =
            null;
    }


    // =========================================================
    // CHECK ALL ENEMY AREAS
    // =========================================================

    public void CheckAndFadeBattleMusicIfAllAreasCompleted()
    {
        if (!currentSceneIsBattle)
            return;


        EnemySpawnArea[] areas =
            FindObjectsByType<EnemySpawnArea>(
                FindObjectsInactive.Include,
                FindObjectsSortMode.None
            );


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
                return;
            }
        }


        FadeOutBattleMusic();
    }


    // =========================================================
    // FADE BATTLE MUSIC
    // =========================================================

    public void FadeOutBattleMusic()
    {
        if (!currentSceneIsBattle)
            return;


        if (
            musicSource == null ||
            !musicSource.isPlaying
        )
        {
            return;
        }


        if (musicSource.clip != battleMusic)
            return;


        if (battleFadeCoroutine != null)
        {
            return;
        }


        battleFadeCoroutine =
            StartCoroutine(
                FadeOutBattleMusicRoutine()
            );
    }


    // =========================================================
    // BATTLE MUSIC FADE ROUTINE
    // =========================================================

    private IEnumerator FadeOutBattleMusicRoutine()
    {
        float startVolume =
            musicSource.volume;


        if (battleClearFadeDuration <= 0f)
        {
            musicSource.volume =
                0f;

            musicSource.Stop();

            battleFadeCoroutine =
                null;


            StartBattleClearMusic();

            yield break;
        }


        float timer = 0f;


        while (
            timer <
            battleClearFadeDuration
        )
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


        musicSource.volume =
            0f;


        musicSource.Stop();


        battleFadeCoroutine =
            null;


        // -----------------------------------------------------
        // PHÁT BATTLE CLEAR MUSIC
        // -----------------------------------------------------

        StartBattleClearMusic();


        /*
         * Báo cho BattleRewardUI hoặc hệ thống khác
         * rằng Battle Music đã fade xong.
         */
        OnBattleMusicFadeCompleted?.Invoke();
    }


    // =========================================================
    // START BATTLE CLEAR MUSIC
    // =========================================================

    private void StartBattleClearMusic()
    {
        if (battleClearMusic == null)
        {
            Debug.LogWarning(
                "AudioController: Chưa gắn Battle Clear Music."
            );

            return;
        }


        if (musicSource == null)
            return;


        if (battleClearMusicCoroutine != null)
        {
            StopCoroutine(
                battleClearMusicCoroutine
            );

            battleClearMusicCoroutine = null;
        }


        battleClearMusicCoroutine =
            StartCoroutine(
                BattleClearMusicRoutine()
            );
    }


    // =========================================================
    // BATTLE CLEAR MUSIC ROUTINE
    // =========================================================

    private IEnumerator BattleClearMusicRoutine()
    {
        musicSource.Stop();


        musicSource.clip =
            battleClearMusic;

        musicSource.time =
            0f;

        musicSource.loop =
            false;

        musicSource.volume =
            0f;

        musicSource.Play();


        float targetVolume =
            musicVolume;


        if (
            battleClearMusicFadeInDuration <= 0f
        )
        {
            musicSource.volume =
                targetVolume;
        }
        else
        {
            float timer = 0f;


            while (
                timer <
                battleClearMusicFadeInDuration
            )
            {
                timer +=
                    Time.unscaledDeltaTime;


                musicSource.volume =
                    Mathf.Lerp(
                        0f,
                        targetVolume,
                        timer /
                        battleClearMusicFadeInDuration
                    );


                yield return null;
            }


            musicSource.volume =
                targetVolume;
        }


        /*
         * Chờ Battle Clear Music chạy xong.
         */
        while (musicSource.isPlaying)
        {
            yield return null;
        }


        /*
         * Trả volume về mức Music Volume.
         * Không phát lại nhạc.
         */
        musicSource.volume =
            musicVolume;


        battleClearMusicCoroutine =
            null;
    }


    // =========================================================
    // SFX
    // =========================================================

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
        if (
            clip == null ||
            sfxSource == null
        )
        {
            return;
        }


        sfxSource.PlayOneShot(
            clip,
            Mathf.Clamp01(
                volumeScale
            ) *
            sfxVolume
        );
    }


    // =========================================================
    // MUSIC VOLUME
    // =========================================================

    public void SetMusicVolume(
        float value)
    {
        musicVolume =
            Mathf.Clamp01(
                value
            );


        if (musicSource != null)
        {
            musicSource.volume =
                musicVolume;
        }
    }


    // =========================================================
    // SFX VOLUME
    // =========================================================

    public void SetSFXVolume(
        float value)
    {
        sfxVolume =
            Mathf.Clamp01(
                value
            );


        if (sfxSource != null)
        {
            sfxSource.volume =
                sfxVolume;
        }
    }


    // =========================================================
    // ENDING MUSIC TRANSITION
    // =========================================================

    public IEnumerator TransitionToEndingMusicRoutine(
        float fadeOutDuration,
        float fadeInDuration
    )
    {
        if (musicSource == null)
            yield break;


        if (endingMusic == null)
        {
            Debug.LogWarning(
                "AudioController: Chưa gắn Ending Music."
            );

            yield break;
        }


        StopAllMusicCoroutines();


        float startVolume =
            musicSource.volume;


        // -----------------------------------------------------
        // FADE OUT CURRENT MUSIC
        // -----------------------------------------------------

        if (
            musicSource.isPlaying &&
            fadeOutDuration > 0f
        )
        {
            float timer = 0f;


            while (
                timer <
                fadeOutDuration
            )
            {
                timer +=
                    Time.unscaledDeltaTime;


                musicSource.volume =
                    Mathf.Lerp(
                        startVolume,
                        0f,
                        timer /
                        fadeOutDuration
                    );


                yield return null;
            }
        }
        else
        {
            musicSource.volume =
                0f;
        }


        musicSource.Stop();


        // -----------------------------------------------------
        // ENDING MUSIC
        // -----------------------------------------------------

        musicSource.clip =
            endingMusic;

        musicSource.time =
            0f;

        musicSource.loop =
            true;

        musicSource.volume =
            0f;

        musicSource.Play();


        // -----------------------------------------------------
        // FADE IN ENDING MUSIC
        // -----------------------------------------------------

        float targetVolume =
            musicVolume;


        float fadeInTime =
            fadeInDuration > 0f
                ? fadeInDuration
                : endingMusicFadeInDuration;


        if (fadeInTime > 0f)
        {
            float timer = 0f;


            while (
                timer <
                fadeInTime
            )
            {
                timer +=
                    Time.unscaledDeltaTime;


                musicSource.volume =
                    Mathf.Lerp(
                        0f,
                        targetVolume,
                        timer /
                        fadeInTime
                    );


                yield return null;
            }
        }


        musicSource.volume =
            targetVolume;
    }


    // =========================================================
    // STOP MUSIC
    // =========================================================

    public void StopMusic()
    {
        StopAllMusicCoroutines();


        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }


    // =========================================================
    // STOP MUSIC COROUTINES
    // =========================================================

    private void StopAllMusicCoroutines()
    {
        if (changeMusicCoroutine != null)
        {
            StopCoroutine(
                changeMusicCoroutine
            );

            changeMusicCoroutine =
                null;
        }


        if (battleFadeCoroutine != null)
        {
            StopCoroutine(
                battleFadeCoroutine
            );

            battleFadeCoroutine =
                null;
        }


        if (battleClearMusicCoroutine != null)
        {
            StopCoroutine(
                battleClearMusicCoroutine
            );

            battleClearMusicCoroutine =
                null;
        }
    }


    // =========================================================
    // BOSS MUSIC
    // =========================================================

    public void PlayBossMusic()
    {
        currentSceneIsBattle =
            false;


        PlayMusic(
            bossMusic
        );
    }


    // =========================================================
    // BATTLE MUSIC
    // =========================================================

    public void PlayBattleMusic()
    {
        currentSceneIsBattle =
            true;


        PlayMusicFromBeginning(
            battleMusic
        );
    }
}