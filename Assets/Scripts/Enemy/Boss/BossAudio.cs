using UnityEngine;

public class BossAudio : MonoBehaviour
{
    [Header("Boss SFX")]
    [SerializeField] private AudioClip walkSFX;
    [SerializeField] private AudioClip rangedAttackSFX;
    [SerializeField] private AudioClip slashSFX;
    [SerializeField] private AudioClip castSFX;
    [SerializeField] private AudioClip teleportSFX;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float walkVolume = 0.5f;

    [Range(0f, 1f)]
    [SerializeField] private float rangedAttackVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float slashVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float castVolume = 0.8f;

    [Range(0f, 1f)]
    [SerializeField] private float teleportVolume = 0.8f;

    [Header("3D Audio")]
    [Range(0f, 1f)]
    [SerializeField] private float spatialBlend = 1f;

    [SerializeField] private float minDistance = 5f;
    [SerializeField] private float maxDistance = 40f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource =
            GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource =
                gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.loop = false;

        audioSource.spatialBlend =
            spatialBlend;

        audioSource.minDistance =
            minDistance;

        audioSource.maxDistance =
            maxDistance;

        audioSource.rolloffMode =
            AudioRolloffMode.Linear;
    }

    // =========================================================
    // WALK / RUN
    // GỌI BẰNG ANIMATION EVENT
    // =========================================================

    public void AnimationEvent_WalkSFX()
    {
        PlaySFX(
            walkSFX,
            walkVolume
        );
    }

    // =========================================================
    // RANGE ATTACK
    // MỖI PROJECTILE BẮN RA PHÁT 1 LẦN
    // =========================================================

    public void PlayRangedAttackSFX()
    {
        PlaySFX(
            rangedAttackSFX,
            rangedAttackVolume
        );
    }

    // =========================================================
    // SLASH
    // =========================================================

    public void PlaySlashSFX()
    {
        PlaySFX(
            slashSFX,
            slashVolume
        );
    }

    // =========================================================
    // CAST / SUMMON
    // =========================================================

    public void PlayCastSFX()
    {
        PlaySFX(
            castSFX,
            castVolume
        );
    }

    // =========================================================
    // TELEPORT
    // =========================================================

    public void PlayTeleportSFX()
    {
        PlaySFX(
            teleportSFX,
            teleportVolume
        );
    }

    // =========================================================
    // COMMON
    // =========================================================

    private void PlaySFX(
        AudioClip clip,
        float volume)
    {
        if (clip == null ||
            audioSource == null)
        {
            return;
        }

        audioSource.PlayOneShot(
            clip,
            Mathf.Clamp01(volume)
        );
    }
}