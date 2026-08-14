using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform targer;

    [Header("Camera Settings")]
    [SerializeField]
    private Vector3 offset = new Vector3(12f, 20f, -15f);

    [SerializeField]
    private float smoothSpeed = 0.125f;

    [Header("Lens")]
    [SerializeField]
    private float gameplayFOV = 40f;

    [Header("Target Finding")]
    [SerializeField]
    private string playerTag = "Player";

    private Camera cachedCamera;


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        offset = new Vector3(12f, 20f, -15f);
        cachedCamera = GetComponent<Camera>();

        if (cachedCamera != null)
        {
            cachedCamera.fieldOfView = gameplayFOV;
        }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        ResolveTarget();

        ResetCameraImmediately();
    }


    // =========================================================
    // ON ENABLE
    // =========================================================

    private void OnEnable()
    {
        if (cachedCamera == null)
        {
            cachedCamera = GetComponent<Camera>();
        }

        ResolveTarget();

        ResetCameraImmediately();
    }


    // =========================================================
    // LATE UPDATE
    // =========================================================

    private void LateUpdate()
    {
        /*
         * Nếu target bị mất sau Load Game,
         * tự tìm lại Player.
         */
        if (targer == null)
        {
            ResolveTarget();

            if (targer == null)
            {
                return;
            }

            /*
             * Khi vừa tìm lại được Player,
             * đưa camera về đúng vị trí ngay lập tức.
             */
            ResetCameraImmediately();

            return;
        }


        Vector3 desiredPosition =
            targer.position + offset;

        transform.position =
            Vector3.Lerp(
                transform.position,
                desiredPosition,
                smoothSpeed
            );

        transform.LookAt(targer);


        // -----------------------------------------------------
        // GIỮ FOV GAMEPLAY
        // -----------------------------------------------------

        if (cachedCamera != null)
        {
            cachedCamera.fieldOfView =
                gameplayFOV;
        }
    }


    // =========================================================
    // FIND PLAYER
    // =========================================================

    private void ResolveTarget()
    {
        /*
         * Reference cũ vẫn còn hợp lệ
         * thì không cần tìm lại.
         */
        if (targer != null)
        {
            return;
        }


        /*
         * Tìm Player bằng Tag.
         */
        GameObject playerObject =
            GameObject.FindGameObjectWithTag(
                playerTag
            );


        if (playerObject != null)
        {
            targer =
                playerObject.transform;

            Debug.Log(
                "CameraController: Đã tìm thấy Player mới."
            );

            return;
        }


        /*
         * Không tìm thấy Player.
         */
        Debug.LogWarning(
            "CameraController: Không tìm thấy GameObject có Tag = "
            + playerTag
        );
    }


    // =========================================================
    // RESET CAMERA
    // =========================================================

    public void ResetCameraImmediately()
    {
        /*
         * Thử tìm Player trước.
         */
        ResolveTarget();


        if (targer == null)
        {
            return;
        }


        if (cachedCamera == null)
        {
            cachedCamera = GetComponent<Camera>();
        }


        // -----------------------------------------------------
        // POSITION
        // -----------------------------------------------------

        transform.position =
            targer.position + offset;


        // -----------------------------------------------------
        // ROTATION
        // -----------------------------------------------------

        transform.LookAt(targer);


        // -----------------------------------------------------
        // FOV
        // -----------------------------------------------------

        if (cachedCamera != null)
        {
            cachedCamera.fieldOfView =
                gameplayFOV;
        }
    }


    // =========================================================
    // SET TARGET
    // =========================================================

    public void SetTarget(Transform newTarget)
    {
        targer = newTarget;

        ResetCameraImmediately();
    }
}