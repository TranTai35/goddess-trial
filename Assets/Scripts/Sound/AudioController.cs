using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.7f;

    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 1f;

    [Header("Music Transition")]
    [SerializeField] private float fadeDuration = 0.5f;

    private Coroutine changeMusicCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Giữ AudioController khi chuyển scene.
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
        // Phát nhạc đúng cho scene đầu tiên.
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void SetupAudioSources()
    {
        if (musicSource != null)
        {
            musicSource.playOnAwake = false;
            musicSource.loop = true;
            musicSource.spatialBlend = 0f;
            musicSource.volume = musicVolume;
        }

        if (sfxSource != null)
        {
            sfxSource.playOnAwake = false;
            sfxSource.loop = false;
            sfxSource.spatialBlend = 0f;
            sfxSource.volume = sfxVolume;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void PlayMusicForScene(string sceneName)
    {
        AudioClip targetMusic = GetMusicForScene(sceneName);

        if (targetMusic == null)
        {
            Debug.LogWarning(
                $"AudioController: Chưa xác định nhạc cho scene '{sceneName}'."
            );
            return;
        }

        PlayMusic(targetMusic);
    }

    private AudioClip GetMusicForScene(string sceneName)
    {
        string lowerSceneName = sceneName.ToLower();

        // Sửa các điều kiện này theo đúng tên scene trong project.
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

        // Các scene level đánh quái thường.
        if (lowerSceneName.Contains("level"))
        {
            return battleMusic;
        }

        return null;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || musicSource == null)
        {
            return;
        }

        // Đang phát đúng bài thì không phát lại từ đầu.
        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            return;
        }

        if (changeMusicCoroutine != null)
        {
            StopCoroutine(changeMusicCoroutine);
        }

        changeMusicCoroutine = StartCoroutine(ChangeMusicRoutine(clip));
    }

    private IEnumerator ChangeMusicRoutine(AudioClip newClip)
    {
        float originalVolume = musicVolume;

        // Fade out bài cũ.
        if (musicSource.isPlaying && fadeDuration > 0f)
        {
            float timer = 0f;
            float startVolume = musicSource.volume;

            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;

                musicSource.volume = Mathf.Lerp(
                    startVolume,
                    0f,
                    timer / fadeDuration
                );

                yield return null;
            }
        }

        musicSource.Stop();
        musicSource.clip = newClip;
        musicSource.volume = 0f;
        musicSource.Play();

        // Fade in bài mới.
        if (fadeDuration > 0f)
        {
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.unscaledDeltaTime;

                musicSource.volume = Mathf.Lerp(
                    0f,
                    originalVolume,
                    timer / fadeDuration
                );

                yield return null;
            }
        }

        musicSource.volume = originalVolume;
        changeMusicCoroutine = null;
    }

    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(clip, 1f);
    }

    public void PlaySFX(AudioClip clip, float volumeScale)
    {
        if (clip == null || sfxSource == null)
        {
            return;
        }

        sfxSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volumeScale) * sfxVolume
        );
    }

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);

        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);

        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }

    public void StopMusic()
    {
        if (changeMusicCoroutine != null)
        {
            StopCoroutine(changeMusicCoroutine);
            changeMusicCoroutine = null;
        }

        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    // Dùng nếu boss xuất hiện trong cùng một scene đánh thường.
    public void PlayBossMusic()
    {
        PlayMusic(bossMusic);
    }

    // Dùng sau khi boss chết và muốn trở lại nhạc battle.
    public void PlayBattleMusic()
    {
        PlayMusic(battleMusic);
    }
}