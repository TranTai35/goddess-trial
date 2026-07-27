using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerStats : MonoBehaviour
{
    public PlayerStatsData baseStats;
    private Animator animator;
    private const string TakeDamageTrigger = "TakeDamage";
    private bool isTakingDamage;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        // Mỗi Player/scene dùng một bản runtime riêng.
        // Nhờ vậy nâng chỉ số trong Level không sửa vào ScriptableObject gốc.
        if (baseStats != null)
            baseStats = Instantiate(baseStats);

        PersistentPlayerState state = PersistentPlayerState.EnsureExists();
        state.ApplyPersistentState(this);
    }

    private void Start()
    {
        // Chờ tất cả caster Awake xong rồi mới gắn lại spell đã lưu.
        PersistentPlayerState.Instance?.ApplyEquippedSpells(gameObject);
    }

    public void TakeDamage(float damage)
    {
        PlayerController player = GetComponent<PlayerController>();

        if (player != null && player.IsInvincible)
            return;

        baseStats.currentHealth -= damage;
        Debug.Log("Player HP: " + baseStats.currentHealth);

        if (!isTakingDamage && animator != null)
            StartCoroutine(PlayDamageAnimationRoutine());

        if (baseStats.currentHealth <= 0)
            Die();
    }

    private IEnumerator PlayDamageAnimationRoutine()
    {
        isTakingDamage = true;
        animator.SetTrigger(TakeDamageTrigger);
        yield return new WaitForSeconds(1f);
        isTakingDamage = false;
    }

    private void Die()
    {
        baseStats.currentHealth = 0;
        Debug.Log("PLAYER DEAD");
    }

    private void OnDestroy()
    {
        // Luôn giữ tiền hiện tại. Không lưu các stat tạm của level.
        if (PersistentPlayerState.Instance != null)
            PersistentPlayerState.Instance.SaveCurrencyFrom(this);
    }
}
