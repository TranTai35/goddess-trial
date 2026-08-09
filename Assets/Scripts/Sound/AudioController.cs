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
    [Tooltip(
        "Thời gian battle music nhỏ dần sau khi giết hết enemy."
    )]
    [SerializeField] private float battleClearFadeDuration = 2.5f;


    private Coroutine changeMusicCoroutine;
    private Coroutine battleFadeCoroutine;

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


        if (battleFadeCoroutine != null)
        {
            StopCoroutine(
                battleFadeCoroutine
            );

            battleFadeCoroutine = null;
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


        if (changeMusicCoroutine != null)
        {
            StopCoroutine(
                changeMusicCoroutine
            );

            changeMusicCoroutine = null;
        }


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
            musicSource.volume = 0f;

            musicSource.Stop();

            musicSource.volume =
                musicVolume;

            battleFadeCoroutine =
                null;


            /*
             * Báo BattleRewardUI mở Panel.
             */
            OnBattleMusicFadeCompleted?.Invoke();

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


        /*
         * Chuẩn bị volume cho scene sau.
         */
        musicSource.volume =
            musicVolume;


        battleFadeCoroutine =
            null;


        /*
         * =====================================================
         * NHẠC FADE XONG
         * -> HIỆN BATTLE REWARD
         * =====================================================
         */
        OnBattleMusicFadeCompleted?.Invoke();
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
    // STOP MUSIC
    // =========================================================

    public void StopMusic()
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


        if (musicSource != null)
        {
            musicSource.Stop();
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