using UnityEngine;

public class BattleRewardUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private GameObject rewardPanel;

    [SerializeField]
    private PlayerStats playerStats;


    [Header("HP Reward")]
    [Range(0f, 1f)]
    [SerializeField]
    private float healthRestorePercent =
        0.20f;


    [Header("Mana Reward")]
    [Range(0f, 1f)]
    [SerializeField]
    private float manaRestorePercent =
        0.10f;


    [Header("Gold Reward")]
    [SerializeField]
    private int goldReward =
        50;


    [Header("Diamond Reward")]
    [SerializeField]
    private int diamondReward =
        20;


    [Header("Settings")]
    [Tooltip(
        "Có khóa game khi bảng reward hiện ra hay không."
    )]
    [SerializeField]
    private bool pauseGameWhileChoosing =
        true;


    private bool rewardShown =
        false;

    private bool rewardSelected =
        false;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (rewardPanel != null)
        {
            rewardPanel.SetActive(
                false
            );
        }
    }


    // =========================================================
    // EVENT SUBSCRIBE
    // =========================================================

    private void OnEnable()
    {
        AudioController.OnBattleMusicFadeCompleted +=
            ShowRewardPanel;
    }


    private void OnDisable()
    {
        AudioController.OnBattleMusicFadeCompleted -=
            ShowRewardPanel;
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        FindPlayer();
    }


    // =========================================================
    // FIND PLAYER
    // =========================================================

    private void FindPlayer()
    {
        if (playerStats != null)
            return;


        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                "Player"
            );


        if (playerObject != null)
        {
            playerStats =
                playerObject.GetComponent<PlayerStats>();
        }
    }


    // =========================================================
    // SHOW REWARD
    // =========================================================

    private void ShowRewardPanel()
    {
        /*
         * Không hiện lại lần 2.
         */
        if (rewardShown)
            return;


        rewardShown =
            true;


        rewardSelected =
            false;


        FindPlayer();


        if (rewardPanel == null)
        {
            Debug.LogWarning(
                "BattleRewardUI chưa được gắn Reward Panel."
            );

            return;
        }


        rewardPanel.SetActive(
            true
        );


        if (pauseGameWhileChoosing)
        {
            Time.timeScale =
                0f;
        }


        Debug.Log(
            "Battle Reward Panel xuất hiện."
        );
    }


    // =========================================================
    // CHOOSE HEALTH
    // =========================================================

    public void ChooseHealth()
    {
        if (!CanSelectReward())
            return;


        playerStats.HealPercent(
            healthRestorePercent
        );


        CompleteSelection(
            "HP"
        );
    }


    // =========================================================
    // CHOOSE MANA
    // =========================================================

    public void ChooseMana()
    {
        if (!CanSelectReward())
            return;


        playerStats.RestoreManaPercent(
            manaRestorePercent
        );


        CompleteSelection(
            "Mana"
        );
    }


    // =========================================================
    // CHOOSE GOLD
    // =========================================================

    public void ChooseGold()
    {
        if (!CanSelectReward())
            return;


        playerStats.baseStats.gold +=
            goldReward;


        playerStats.SaveRunStats();


        CompleteSelection(
            "Gold"
        );
    }


    // =========================================================
    // CHOOSE DIAMOND
    // =========================================================

    public void ChooseDiamond()
    {
        if (!CanSelectReward())
            return;


        playerStats.baseStats.diamond +=
            diamondReward;


        playerStats.SaveRunStats();


        CompleteSelection(
            "Diamond"
        );
    }


    // =========================================================
    // CAN SELECT
    // =========================================================

    private bool CanSelectReward()
    {
        /*
         * Đã chọn rồi thì không cho click thêm.
         */
        if (rewardSelected)
            return false;


        if (playerStats == null)
        {
            FindPlayer();
        }


        if (
            playerStats == null ||
            playerStats.baseStats == null
        )
        {
            Debug.LogWarning(
                "BattleRewardUI không tìm thấy PlayerStats."
            );

            return false;
        }


        return true;
    }


    // =========================================================
    // COMPLETE
    // =========================================================

    private void CompleteSelection(
        string rewardName)
    {
        rewardSelected =
            true;


        Debug.Log(
            $"Player chọn Battle Reward: {rewardName}"
        );


        if (rewardPanel != null)
        {
            rewardPanel.SetActive(
                false
            );
        }


        if (pauseGameWhileChoosing)
        {
            Time.timeScale =
                1f;
        }
    }
}