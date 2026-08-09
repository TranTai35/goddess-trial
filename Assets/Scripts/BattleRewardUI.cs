using UnityEngine;

public class BattleRewardUI : MonoBehaviour
{
    public static BattleRewardUI Instance { get; private set; }

    [Header("References")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private PlayerStats playerStats;

    [Header("Reward Interact UI")]
    [Tooltip("Text kiểu: Nhấn R để nhận phần thưởng")]
    [SerializeField] private GameObject rewardInteractText;

    [Header("Reward VFX")]
    [Tooltip("Prefab VFX xuất hiện dưới Player sau khi clear enemy.")]
    [SerializeField] private GameObject rewardVfxPrefab;

    [Tooltip("Offset của VFX so với Player.")]
    [SerializeField]
    private Vector3 rewardVfxOffset =
        new Vector3(0f, 0.05f, 0f);

    [Header("HP Reward")]
    [Range(0f, 1f)]
    [SerializeField] private float healthRestorePercent = 0.20f;

    [Header("Mana Reward")]
    [Range(0f, 1f)]
    [SerializeField] private float manaRestorePercent = 0.10f;

    [Header("Gold Reward")]
    [SerializeField] private int goldReward = 50;

    [Header("Diamond Reward")]
    [SerializeField] private int diamondReward = 20;

    [Header("Settings")]
    [SerializeField] private bool pauseGameWhileChoosing = true;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = true;


    private GameObject spawnedRewardVfx;

    /*
     * Enemy đã clear, reward đang nằm dưới Player,
     * chờ người chơi nhấn R.
     */
    private bool rewardAvailable = false;

    /*
     * Panel 4 lựa chọn đang mở.
     */
    private bool rewardPanelOpen = false;

    /*
     * Đã lấy reward của màn này.
     */
    private bool rewardSelected = false;


    // =========================================================
    // PUBLIC STATE
    // =========================================================

    public bool RewardAvailable =>
        rewardAvailable;

    public bool RewardPanelOpen =>
        rewardPanelOpen;

    /// <summary>
    /// Portal dùng biến này để biết có cần nhường
    /// quyền tương tác cho Reward hay không.
    /// </summary>
    public static bool IsBlockingPortalInteraction
    {
        get
        {
            if (Instance == null)
                return false;

            return Instance.rewardAvailable ||
                   Instance.rewardPanelOpen;
        }
    }


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        Instance = this;

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }

        if (rewardInteractText != null)
        {
            rewardInteractText.SetActive(false);
        }
    }


    // =========================================================
    // ENABLE
    // =========================================================

    private void OnEnable()
    {
        AudioController.OnBattleMusicFadeCompleted +=
            PrepareReward;
    }


    // =========================================================
    // DISABLE
    // =========================================================

    private void OnDisable()
    {
        AudioController.OnBattleMusicFadeCompleted -=
            PrepareReward;
    }


    // =========================================================
    // DESTROY
    // =========================================================

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        FindPlayer();
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        /*
         * Reward đã xuất hiện dưới Player
         * nhưng chưa mở panel.
         */
        if (!rewardAvailable)
            return;

        if (rewardPanelOpen)
            return;

        if (rewardSelected)
            return;


        /*
         * Reward được ưu tiên phím R trước Portal.
         */
        if (Input.GetKeyDown(KeyCode.R))
        {
            OpenRewardPanel();
        }
    }


    // =========================================================
    // FIND PLAYER
    // =========================================================

    private void FindPlayer()
    {
        if (playerStats != null)
            return;


        playerStats =
            FindFirstObjectByType<PlayerStats>();


        if (playerStats != null)
        {
            Log(
                $"Tìm thấy PlayerStats: {playerStats.gameObject.name}"
            );

            return;
        }


        GameObject playerObject = null;

        try
        {
            playerObject =
                GameObject.FindGameObjectWithTag("Player");
        }
        catch
        {
        }


        if (playerObject != null)
        {
            playerStats =
                playerObject.GetComponent<PlayerStats>();
        }


        if (playerStats == null)
        {
            Debug.LogWarning(
                "BattleRewardUI không tìm thấy PlayerStats."
            );
        }
    }


    // =========================================================
    // PREPARE REWARD
    // =========================================================

    private void PrepareReward()
    {
        /*
         * Không tạo lại nếu đã tạo reward rồi.
         */
        if (rewardAvailable ||
            rewardPanelOpen ||
            rewardSelected)
        {
            return;
        }


        FindPlayer();


        if (playerStats == null)
        {
            Debug.LogError(
                "Không tìm thấy Player nên không thể tạo Battle Reward."
            );

            return;
        }


        rewardAvailable = true;


        // =====================================================
        // CREATE VFX UNDER PLAYER
        // =====================================================

        if (rewardVfxPrefab != null)
        {
            spawnedRewardVfx =
                Instantiate(
                    rewardVfxPrefab,
                    playerStats.transform
                );


            spawnedRewardVfx.transform.localPosition =
                rewardVfxOffset;


            spawnedRewardVfx.transform.localRotation =
                Quaternion.identity;
        }


        // =====================================================
        // SHOW "PRESS R"
        // =====================================================

        if (rewardInteractText != null)
        {
            rewardInteractText.SetActive(true);
        }


        Log(
            "Reward đã xuất hiện. Nhấn R để nhận."
        );
    }


    // =========================================================
    // OPEN REWARD PANEL
    // =========================================================

    private void OpenRewardPanel()
    {
        if (!rewardAvailable)
            return;

        if (rewardSelected)
            return;


        rewardAvailable = false;
        rewardPanelOpen = true;


        // =====================================================
        // REMOVE VFX
        // =====================================================

        if (spawnedRewardVfx != null)
        {
            Destroy(spawnedRewardVfx);

            spawnedRewardVfx = null;
        }


        // =====================================================
        // HIDE R TEXT
        // =====================================================

        if (rewardInteractText != null)
        {
            rewardInteractText.SetActive(false);
        }


        // =====================================================
        // SHOW PANEL
        // =====================================================

        if (rewardPanel != null)
        {
            rewardPanel.SetActive(true);

            rewardPanel.transform.SetAsLastSibling();
        }


        if (pauseGameWhileChoosing)
        {
            Time.timeScale = 0f;
        }


        Log(
            "Mở Battle Reward Panel."
        );
    }


    // =========================================================
    // HEALTH
    // =========================================================

    public void ChooseHealth()
    {
        if (!CanSelectReward())
            return;


        playerStats.HealPercent(
            healthRestorePercent
        );


        CompleteSelection("HP");
    }


    // =========================================================
    // MANA
    // =========================================================

    public void ChooseMana()
    {
        if (!CanSelectReward())
            return;


        playerStats.RestoreManaPercent(
            manaRestorePercent
        );


        CompleteSelection("Mana");
    }


    // =========================================================
    // GOLD
    // =========================================================

    public void ChooseGold()
    {
        if (!CanSelectReward())
            return;


        playerStats.baseStats.gold +=
            goldReward;


        playerStats.SaveRunStats();


        CompleteSelection("Gold");
    }


    // =========================================================
    // DIAMOND
    // =========================================================

    public void ChooseDiamond()
    {
        if (!CanSelectReward())
            return;


        playerStats.baseStats.diamond +=
            diamondReward;


        playerStats.SaveRunStats();


        CompleteSelection("Diamond");
    }


    // =========================================================
    // CAN SELECT
    // =========================================================

    private bool CanSelectReward()
    {
        if (rewardSelected)
            return false;


        if (!rewardPanelOpen)
            return false;


        if (playerStats == null)
        {
            FindPlayer();
        }


        if (playerStats == null ||
            playerStats.baseStats == null)
        {
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
        rewardSelected = true;
        rewardPanelOpen = false;


        if (rewardPanel != null)
        {
            rewardPanel.SetActive(false);
        }


        if (pauseGameWhileChoosing)
        {
            Time.timeScale = 1f;
        }


        Log(
            $"Player chọn Reward: {rewardName}"
        );
    }


    // =========================================================
    // LOG
    // =========================================================

    private void Log(
        string message)
    {
        if (!showDebugLogs)
            return;


        Debug.Log(
            $"[BattleRewardUI] {message}"
        );
    }
}