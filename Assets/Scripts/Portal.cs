using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject interactText;

    [Header("Destination")]
    [SerializeField] private string destinationScene = "Level1";

    [Header("Cutscene Movement")]
    [SerializeField] private Transform walkTarget;
    [SerializeField] private float cutsceneMoveSpeed = 3f;
    [SerializeField] private float cutsceneRotationSpeed = 10f;
    [SerializeField] private float stoppingDistance = 0.05f;

    [Header("Teleport VFX")]
    [SerializeField] private GameObject teleportVfxPrefab;
    [SerializeField] private Transform vfxSpawnPoint;
    [SerializeField] private float disappearDelay = 0.6f;
    [SerializeField] private float loadSceneDelay = 0.8f;

    private PlayerController playerController;
    private Transform playerTransform;

    private bool playerInside;
    private bool isTeleporting;

    private void Start()
    {
        if (interactText != null)
        {
            interactText.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInside || isTeleporting)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(TeleportCutscene());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTeleporting)
        {
            return;
        }

        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null)
        {
            return;
        }

        playerController = player;
        playerTransform = player.transform;
        playerInside = true;

        if (interactText != null)
        {
            interactText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null || player != playerController)
        {
            return;
        }

        if (isTeleporting)
        {
            return;
        }

        playerInside = false;
        playerController = null;
        playerTransform = null;

        if (interactText != null)
        {
            interactText.SetActive(false);
        }
    }

    private IEnumerator TeleportCutscene()
    {
        if (playerController == null ||
            playerTransform == null ||
            walkTarget == null)
        {
            yield break;
        }

        isTeleporting = true;

        if (interactText != null)
        {
            interactText.SetActive(false);
        }

        // Khóa điều khiển Player
        playerController.SetControlEnabled(false);
        playerController.SetCutsceneMoving(true);

        // Cho Player tự động đi vào cổng
        while (GetHorizontalDistance(
                   playerTransform.position,
                   walkTarget.position) > stoppingDistance)
        {
            Vector3 targetPosition = walkTarget.position;

            // Giữ nguyên độ cao của Player
            targetPosition.y = playerTransform.position.y;

            Vector3 direction =
                targetPosition - playerTransform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation =
                    Quaternion.LookRotation(direction);

                playerTransform.rotation =
                    Quaternion.Slerp(
                        playerTransform.rotation,
                        targetRotation,
                        cutsceneRotationSpeed * Time.deltaTime);
            }

            playerTransform.position =
                Vector3.MoveTowards(
                    playerTransform.position,
                    targetPosition,
                    cutsceneMoveSpeed * Time.deltaTime);

            yield return null;
        }

        playerController.SetCutsceneMoving(false);

        // Đặt Player chính xác vào điểm giữa portal
        Vector3 finalPosition = walkTarget.position;
        finalPosition.y = playerTransform.position.y;
        playerTransform.position = finalPosition;

        // Tạo VFX dịch chuyển
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
                spawnRotation);
        }

        // Chờ VFX chạy một lúc
        yield return new WaitForSeconds(disappearDelay);

        // Làm Player biến mất nhưng không tắt cả GameObject
        SetPlayerVisible(false);

        // Chờ thêm trước khi load scene
        yield return new WaitForSeconds(loadSceneDelay);

        PlayerStats stats = playerTransform.GetComponent<PlayerStats>();
        PersistentPlayerState state = PersistentPlayerState.EnsureExists();

        // Tiền luôn được giữ. Chỉ lưu stat vĩnh viễn khi rời Village.
        state.SaveCurrencyFrom(stats);
        if (SceneManager.GetActiveScene().name == PersistentPlayerState.VillageSceneName)
            state.SavePermanentStatsFrom(stats);

        SceneManager.LoadScene(destinationScene);
    }

    private float GetHorizontalDistance(
        Vector3 firstPosition,
        Vector3 secondPosition)
    {
        firstPosition.y = 0f;
        secondPosition.y = 0f;

        return Vector3.Distance(
            firstPosition,
            secondPosition);
    }

    private void SetPlayerVisible(bool visible)
    {
        if (playerTransform == null)
        {
            return;
        }

        Renderer[] renderers =
            playerTransform.GetComponentsInChildren<Renderer>();

        foreach (Renderer playerRenderer in renderers)
        {
            playerRenderer.enabled = visible;
        }
    }
}