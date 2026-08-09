using UnityEngine;
using UnityEngine.UI;

public class SettingUIManager : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider bgVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider brightnessSlider;

    [Header("Brightness Overlay")]
    [Tooltip("Image màu đen phủ toàn màn hình để chỉnh độ sáng.")]
    [SerializeField] private Image brightnessOverlay;

    [Header("Brightness Settings")]
    [Range(0f, 0.9f)]
    [SerializeField] private float maxDarkness = 0.7f;

    private const string MusicVolumeKey = "MusicVolume";
    private const string SFXVolumeKey = "SFXVolume";
    private const string BrightnessKey = "Brightness";


    private void Start()
    {
        LoadSettings();

        SetupListeners();
    }


    // =========================================================
    // SETUP
    // =========================================================

    private void SetupListeners()
    {
        if (bgVolumeSlider != null)
        {
            bgVolumeSlider.onValueChanged.RemoveListener(
                SetBackgroundVolume
            );

            bgVolumeSlider.onValueChanged.AddListener(
                SetBackgroundVolume
            );
        }


        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.onValueChanged.RemoveListener(
                SetSFXVolume
            );

            sfxVolumeSlider.onValueChanged.AddListener(
                SetSFXVolume
            );
        }


        if (brightnessSlider != null)
        {
            brightnessSlider.onValueChanged.RemoveListener(
                SetBrightness
            );

            brightnessSlider.onValueChanged.AddListener(
                SetBrightness
            );
        }
    }


    // =========================================================
    // BACKGROUND MUSIC
    // =========================================================

    public void SetBackgroundVolume(float value)
    {
        value = Mathf.Clamp01(value);

        if (AudioController.Instance != null)
        {
            AudioController.Instance.SetMusicVolume(value);
        }

        PlayerPrefs.SetFloat(
            MusicVolumeKey,
            value
        );

        PlayerPrefs.Save();
    }


    // =========================================================
    // SFX / VFX SOUND
    // =========================================================

    public void SetSFXVolume(float value)
    {
        value = Mathf.Clamp01(value);

        if (AudioController.Instance != null)
        {
            AudioController.Instance.SetSFXVolume(value);
        }

        PlayerPrefs.SetFloat(
            SFXVolumeKey,
            value
        );

        PlayerPrefs.Save();
    }


    // =========================================================
    // BRIGHTNESS
    // =========================================================

    public void SetBrightness(float value)
    {
        value = Mathf.Clamp01(value);

        /*
         * Slider:
         * 0 = tối nhất
         * 1 = sáng nhất
         *
         * Overlay đen:
         * alpha cao = tối
         * alpha thấp = sáng
         */

        if (brightnessOverlay != null)
        {
            Color color =
                brightnessOverlay.color;

            color.a =
                Mathf.Lerp(
                    maxDarkness,
                    0f,
                    value
                );

            brightnessOverlay.color =
                color;
        }

        PlayerPrefs.SetFloat(
            BrightnessKey,
            value
        );

        PlayerPrefs.Save();
    }


    // =========================================================
    // LOAD SETTINGS
    // =========================================================

    private void LoadSettings()
    {
        float musicVolume =
            PlayerPrefs.GetFloat(
                MusicVolumeKey,
                0.7f
            );


        float sfxVolume =
            PlayerPrefs.GetFloat(
                SFXVolumeKey,
                1f
            );


        float brightness =
            PlayerPrefs.GetFloat(
                BrightnessKey,
                1f
            );


        if (bgVolumeSlider != null)
        {
            bgVolumeSlider.SetValueWithoutNotify(
                musicVolume
            );
        }


        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(
                sfxVolume
            );
        }


        if (brightnessSlider != null)
        {
            brightnessSlider.SetValueWithoutNotify(
                brightness
            );
        }


        if (AudioController.Instance != null)
        {
            AudioController.Instance.SetMusicVolume(
                musicVolume
            );

            AudioController.Instance.SetSFXVolume(
                sfxVolume
            );
        }


        ApplyBrightnessImmediately(
            brightness
        );
    }


    // =========================================================
    // APPLY BRIGHTNESS
    // =========================================================

    private void ApplyBrightnessImmediately(
        float brightness)
    {
        if (brightnessOverlay == null)
            return;


        Color color =
            brightnessOverlay.color;


        color.a =
            Mathf.Lerp(
                maxDarkness,
                0f,
                brightness
            );


        brightnessOverlay.color =
            color;
    }
}