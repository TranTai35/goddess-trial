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
    [Tooltip(
        "Kéo tất cả EnemySpawnArea bắt buộc phải hoàn thành " +
        "vào danh sách này.")]
    [SerializeField] private EnemySpawnArea[] requiredAreas;

    [Tooltip(
        "Nếu bật, Portal chỉ hoạt động khi tất cả khu vực " +
        "trong Required Areas đã hoàn thành.")]
    [SerializeField] private bool requireAllAreasCompleted = true;

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
        if (interactTextContent == null &&
            interactText != null)
        {
            interactTextContent =
                interactText.GetComponentInChildren<TMP_Text>(true);
        }

        SetInteractTextVisible(false);
    }

    private void Update()
    {
        if (!playerInside || isTeleporting)
        {
            return;
        }

        /*
         * Cập nhật nội dung UI liên tục.
         *
         * Nếu Player đang đứng trong Portal và vừa tiêu diệt
         * enemy cuối cùng thì dòng thông báo sẽ đổi ngay.
         */
        RefreshPortalMessage();

        if (!Input.GetKeyDown(KeyCode.R))
        {
            return;
        }

        /*
         * Không cho dịch chuyển nếu chưa hoàn thành màn.
         */
        if (!CanUsePortal())
        {
            Debug.Log(
                $"{name}: Player chưa hoàn thành tất cả khu vực.");

            return;
        }

        StartCoroutine(TeleportCutscene());
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

        RefreshPortalMessage();
        SetInteractTextVisible(true);
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null ||
            player != playerController)
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

        SetInteractTextVisible(false);
    }

    private bool CanUsePortal()
    {
        /*
         * Dùng cho Portal không cần điều kiện,
         * ví dụ Portal từ Village sang Level.
         */
        if (!requireAllAreasCompleted)
        {
            return true;
        }

        /*
         * Nếu Portal yêu cầu hoàn thành khu vực nhưng chưa
         * gắn khu vực nào thì không cho đi để tránh bỏ qua màn.
         */
        if (requiredAreas == null ||
            requiredAreas.Length == 0)
        {
            Debug.LogWarning(
                $"{name}: Portal yêu cầu hoàn thành màn nhưng " +
                "Required Areas đang trống.");

            return false;
        }

        foreach (EnemySpawnArea area in requiredAreas)
        {
            if (area == null)
            {
                Debug.LogWarning(
                    $"{name}: Required Areas có phần tử bị trống.");

                return false;
            }

            if (!area.AreaCompleted)
            {
                return false;
            }
        }

        return true;
    }

    private void RefreshPortalMessage()
    {
        if (interactTextContent == null)
        {
            return;
        }

        interactTextContent.text =
            CanUsePortal()
                ? availableMessage
                : lockedMessage;
    }

    private void SetInteractTextVisible(bool visible)
    {
        if (interactText != null)
        {
            interactText.SetActive(visible);
        }
    }

    private IEnumerator TeleportCutscene()
    {
        /*
         * Kiểm tra lại một lần nữa trước khi bắt đầu.
         *
         * Việc này ngăn trường hợp hàm bị gọi từ script khác
         * trong khi màn chưa hoàn thành.
         */
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

        isTeleporting = true;
        SetInteractTextVisible(false);

        // Khóa điều khiển Player.
        playerController.SetControlEnabled(false);
        playerController.SetCutsceneMoving(true);

        // Cho Player tự động đi vào cổng.
        while (GetHorizontalDistance(
                   playerTransform.position,
                   walkTarget.position) > stoppingDistance)
        {
            Vector3 targetPosition = walkTarget.position;

            // Giữ nguyên độ cao của Player.
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

        // Đặt Player chính xác vào điểm giữa Portal.
        Vector3 finalPosition = walkTarget.position;
        finalPosition.y = playerTransform.position.y;
        playerTransform.position = finalPosition;

        // Tạo VFX dịch chuyển.
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

        yield return new WaitForSeconds(disappearDelay);

        SetPlayerVisible(false);

        yield return new WaitForSeconds(loadSceneDelay);

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