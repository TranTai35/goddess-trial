using UnityEngine;

public class KnightNPC : NPC
{
    [Header("Knight SFX")]
    [SerializeField] private AudioClip knightSFX;

    [Range(0f, 1f)]
    [SerializeField] private float knightSFXVolume = 1f;


    protected override void Awake()
    {
        base.Awake();
    }


    // =========================================================
    // ANIMATION EVENT
    // =========================================================

    /// <summary>
    /// Gọi hàm này bằng Animation Event
    /// tại frame muốn phát âm thanh của Knight.
    /// </summary>
    public void PlayKnightSFX()
    {
        if (knightSFX == null)
            return;


        if (AudioController.Instance == null)
            return;


        AudioController.Instance.PlaySFX(
            knightSFX,
            knightSFXVolume
        );
    }
}