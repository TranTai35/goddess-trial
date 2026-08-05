using UnityEngine;
using UnityEngine.LowLevel;

public class PlayerAudio : MonoBehaviour
{
    [Header("Attack SFX")]
    [SerializeField] private AudioClip attack1SFX;
    [SerializeField] private AudioClip attack2SFX;
    [SerializeField] private AudioClip attack3SFX;

    [Header("Footstep SFX")]
    [SerializeField] private AudioClip footstepSFX;

    [Header("Dash SFX")]
    [SerializeField] private AudioClip dashSFX;

    [Header("Take Dame SFX")]
    [SerializeField] private AudioClip takeDameSFX;

    [Header("Hit Enemy Sounds")]
    [SerializeField] private AudioClip[] hitEnemySounds;

    [Header("Volume")]
    [Range(0f, 1f)]
    [SerializeField] private float attackVolume = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float footstepVolume = 0.6f;

    [Range(0f, 1f)]
    [SerializeField] private float dashVolume = 0.6f;

    [Range(0f, 1f)]
    [SerializeField] private float takeDameVolume = 0.6f;


    [Range(0f, 1f)]
    [SerializeField] private float hitEnemyVolume = 0.9f;



    [Header("Footstep Pitch")]
    [SerializeField] private float minimumFootstepPitch = 0.95f;
    [SerializeField] private float maximumFootstepPitch = 1.05f;

    private AudioSource localAudioSource;

    private void Awake()
    {
        localAudioSource = GetComponent<AudioSource>();

        if (localAudioSource == null)
        {
            localAudioSource = gameObject.AddComponent<AudioSource>();
        }

        localAudioSource.playOnAwake = false;
        localAudioSource.loop = false;

        // Player đi đâu thì người chơi vẫn nghe rõ.
        localAudioSource.spatialBlend = 0f;
    }

    // Gắn Animation Event vào Attack01.
    public void AnimationEvent_Attack1SFX()
    {
        PlayLocalSFX(attack1SFX, attackVolume);
    }

    // Gắn Animation Event vào Attack02.
    public void AnimationEvent_Attack2SFX()
    {
        PlayLocalSFX(attack2SFX, attackVolume);
    }

    // Gắn Animation Event vào Attack03.
    public void AnimationEvent_Attack3SFX()
    {
        PlayLocalSFX(attack3SFX, attackVolume);
    }

    // Gắn vào thời điểm chân chạm đất.
    public void AnimationEvent_FootstepSFX()
    {
        PlayLocalSFX(footstepSFX, footstepVolume);

    }

    public void AnimationEvent_DashSFX()
    {
        
        PlayLocalSFX(dashSFX, dashVolume);
    }

    public void AnimationEvent_TakeDameSFX()
    {

        PlayLocalSFX(takeDameSFX, takeDameVolume);
    }

    public void PlayHitEnemySound()
    {
        if (hitEnemySounds == null || hitEnemySounds.Length == 0)
        {
            return;
        }

        AudioClip clip =
            hitEnemySounds[Random.Range(0, hitEnemySounds.Length)];
        
        PlayLocalSFX(clip, hitEnemyVolume);


    }

    private void PlayLocalSFX(AudioClip clip, float volume)
    {
        if (clip == null)
        {
            return;
        }

        localAudioSource.pitch = 1f;
        localAudioSource.PlayOneShot(clip, volume);
    }
}