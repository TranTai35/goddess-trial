using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject interactText;
    [SerializeField] private TMP_Text interactTextContent;

    [TextArea]
    [SerializeField]
    private string availableMessage =
        "Nhấn R để đi qua Portal";

    [TextArea]
    [SerializeField]
    private string lockedMessage =
        "Hãy tiêu diệt toàn bộ kẻ địch trước";


    [Header("Level Completion")]
    [SerializeField]
    private EnemySpawnArea[] requiredAreas;

    [SerializeField]
    private bool requireAllAreasCompleted = true;


    [Header("Destination")]
    [SerializeField]
    private string destinationScene = "Level1";


    [Header("Cutscene Movement")]
    [SerializeField]
    private Transform walkTarget;

    [SerializeField]
    private float cutsceneMoveSpeed = 3f;

    [SerializeField]
    private float cutsceneRotationSpeed = 10f;

    [SerializeField]
    private float stoppingDistance = 0.05f;


    [Header("Teleport VFX")]
    [SerializeField]
    private GameObject teleportVfxPrefab;

    [SerializeField]
    private Transform vfxSpawnPoint;

    [SerializeField]
    private float disappearDelay = 0.6f;

    [SerializeField]
    private float loadSceneDelay = 0.8f;


    private PlayerController playerController;
    private Transform playerTransform;

    private bool playerInside;
    private bool isTeleporting;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (interactTextContent == null &&
            interactText != null)
        {
            interactTextContent =
                interactText.GetComponentInChildren<TMP_Text>(
                    true
                );
        }


        SetInteractTextVisible(false);
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!playerInside ||
            isTeleporting)
        {
            return;
        }


        /*
         * =====================================================
         * REWARD CÓ ĐỘ ƯU TIÊN CAO HƠN PORTAL
         * =====================================================
         *
         * Khi Reward đang:
         * - chờ nhấn R
         * hoặc
         * - đang mở Panel
         *
         * Portal sẽ:
         * - ẩn text
         * - không nhận R
         */
        if (BattleRewardUI.IsBlockingPortalInteraction)
        {
            SetInteractTextVisible(false);

            return;
        }


        /*
         * Reward đã xử lý xong.
         * Nếu Player vẫn đứng trong Portal thì hiện text lại.
         */
        RefreshPortalMessage();

        SetInteractTextVisible(true);


        if (!Input.GetKeyDown(KeyCode.R))
        {
            return;
        }


        if (!CanUsePortal())
        {
            Debug.Log(
                $"{name}: Player chưa hoàn thành tất cả khu vực."
            );

            return;
        }


        StartCoroutine(
            TeleportCutscene()
        );
    }


    // =========================================================
    // TRIGGER ENTER
    // =========================================================

    private void OnTriggerEnter(
        Collider other)
    {
        if (isTeleporting)
            return;


        PlayerController player =
            other.GetComponentInParent<PlayerController>();


        if (player == null)
            return;


        playerController =
            player;

        playerTransform =
            player.transform;

        playerInside =
            true;


        /*
         * Nếu Reward đang cần tương tác thì
         * Portal không được hiện text.
         */
        if (BattleRewardUI.IsBlockingPortalInteraction)
        {
            SetInteractTextVisible(false);

            return;
        }


        RefreshPortalMessage();

        SetInteractTextVisible(true);
    }


    // =========================================================
    // TRIGGER EXIT
    // =========================================================

    private void OnTriggerExit(
        Collider other)
    {
        PlayerController player =
            other.GetComponentInParent<PlayerController>();


        if (player == null ||
            player != playerController)
        {
            return;
        }


        if (isTeleporting)
            return;


        playerInside =
            false;

        playerController =
            null;

        playerTransform =
            null;


        SetInteractTextVisible(false);
    }


    // =========================================================
    // CAN USE PORTAL
    // =========================================================

    private bool CanUsePortal()
    {
        if (!requireAllAreasCompleted)
        {
            return true;
        }


        if (requiredAreas == null ||
            requiredAreas.Length == 0)
        {
            Debug.LogWarning(
                $"{name}: Portal yêu cầu hoàn thành màn nhưng Required Areas đang trống."
            );

            return false;
        }


        foreach (
            EnemySpawnArea area
            in requiredAreas
        )
        {
            if (area == null)
            {
                return false;
            }


            if (!area.AreaCompleted)
            {
                return false;
            }
        }


        return true;
    }


    // =========================================================
    // REFRESH MESSAGE
    // =========================================================

    private void RefreshPortalMessage()
    {
        if (interactTextContent == null)
            return;


        interactTextContent.text =
            CanUsePortal()
                ? availableMessage
                : lockedMessage;
    }


    // =========================================================
    // TEXT
    // =========================================================

    private void SetInteractTextVisible(
        bool visible)
    {
        if (interactText != null)
        {
            interactText.SetActive(
                visible
            );
        }
    }


    // =========================================================
    // TELEPORT
    // =========================================================

    private IEnumerator TeleportCutscene()
    {
        /*
         * Không cho đi Portal nếu Reward vẫn chưa xử lý xong.
         */
        if (BattleRewardUI.IsBlockingPortalInteraction)
        {
            yield break;
        }


        if (!CanUsePortal())
        {
            yield break;
        }


        if (playerController == null ||
            playerTransform == null ||
            walkTarget == null)
        {
            yield break;
        }


        isTeleporting =
            true;


        SetInteractTextVisible(false);


        playerController.SetControlEnabled(
            false
        );


        playerController.SetCutsceneMoving(
            true
        );


        // =====================================================
        // WALK TO PORTAL
        // =====================================================

        while (
            GetHorizontalDistance(
                playerTransform.position,
                walkTarget.position
            )
            >
            stoppingDistance
        )
        {
            Vector3 targetPosition =
                walkTarget.position;


            targetPosition.y =
                playerTransform.position.y;


            Vector3 direction =
                targetPosition -
                playerTransform.position;


            direction.y =
                0f;


            if (direction.sqrMagnitude >
                0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(
                        direction
                    );


                playerTransform.rotation =
                    Quaternion.Slerp(
                        playerTransform.rotation,
                        targetRotation,
                        cutsceneRotationSpeed *
                        Time.deltaTime
                    );
            }


            playerTransform.position =
                Vector3.MoveTowards(
                    playerTransform.position,
                    targetPosition,
                    cutsceneMoveSpeed *
                    Time.deltaTime
                );


            yield return null;
        }


        // =====================================================
        // VFX
        // =====================================================

        if (teleportVfxPrefab != null)
        {
            Vector3 spawnPosition =
                vfxSpawnPoint != null
                    ? vfxSpawnPoint.position
                    : playerTransform.position;


            Quaternion spawnRotation =
                vfxSpawnPoint != null
                    ? vfxSpawnPoint.rotation
                    : Quaternion.identity;


            Instantiate(
                teleportVfxPrefab,
                spawnPosition,
                spawnRotation
            );
        }


        // =====================================================
        // PLAYER DISAPPEAR
        // =====================================================

        yield return new WaitForSeconds(
            disappearDelay
        );


        if (playerController != null)
        {
            playerController.gameObject.SetActive(
                false
            );
        }


        // =====================================================
        // LOAD SCENE
        // =====================================================

        yield return new WaitForSeconds(
            loadSceneDelay
        );


        SceneManager.LoadScene(
            destinationScene
        );
    }


    // =========================================================
    // DISTANCE
    // =========================================================

    private float GetHorizontalDistance(
        Vector3 a,
        Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;


        return Vector3.Distance(
            a,
            b
        );
    }
}