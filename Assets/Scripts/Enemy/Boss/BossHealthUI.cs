using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossHealthUI : MonoBehaviour
{
    public static BossHealthUI Instance { get; private set; }

    [Header("UI References")]
    [SerializeField] private GameObject bossHealthPanel;
    [SerializeField] private Slider bossHealthSlider;
    [SerializeField] private TMP_Text bossNameText;
    [SerializeField] private TMP_Text bossHealthText;

    private BossController currentBoss;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        HideBossHealth();
    }

    public void ShowBossHealth(
        BossController boss,
        string bossName,
        float currentHealth,
        float maxHealth)
    {
        currentBoss = boss;

        if (bossHealthPanel != null)
        {
            bossHealthPanel.SetActive(true);
        }

        if (bossNameText != null)
        {
            bossNameText.text = bossName;
        }

        UpdateBossHealth(currentHealth, maxHealth);
    }

    public void UpdateBossHealth(float currentHealth, float maxHealth)
    {
        if (bossHealthSlider != null)
        {
            bossHealthSlider.minValue = 0f;
            bossHealthSlider.maxValue = Mathf.Max(1f, maxHealth);
            bossHealthSlider.value =
                Mathf.Clamp(currentHealth, 0f, maxHealth);
        }

        if (bossHealthText != null)
        {
            bossHealthText.text =
                $"{Mathf.CeilToInt(Mathf.Max(0f, currentHealth))}" +
                $" / {Mathf.CeilToInt(Mathf.Max(1f, maxHealth))}";
        }
    }

    public void HideBossHealth(BossController boss)
    {
        if (currentBoss != null && currentBoss != boss)
        {
            return;
        }

        HideBossHealth();
    }

    public void HideBossHealth()
    {
        currentBoss = null;

        if (bossHealthPanel != null)
        {
            bossHealthPanel.SetActive(false);
        }
    }
}
