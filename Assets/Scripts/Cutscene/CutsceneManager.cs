using UnityEngine;
using Cinemachine;

public class CutsceneManager : MonoBehaviour
{
    public static CutsceneManager Instance;

    [Header("Player")]
    [SerializeField]
    private PlayerController playerController;

    [Header("Gameplay Camera")]
    [SerializeField]
    private CameraController cameraController;

    [Header("Main Camera")]
    [SerializeField]
    private Camera mainCamera;

    [Header("Cinemachine")]
    [SerializeField]
    private CinemachineBrain cinemachineBrain;


    public bool IsCutscenePlaying
    {
        get;
        private set;
    }


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;


        // -----------------------------------------------------
        // FIND MAIN CAMERA
        // -----------------------------------------------------

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }


        // -----------------------------------------------------
        // FIND CINEMACHINE BRAIN
        // -----------------------------------------------------

        if (cinemachineBrain == null &&
            mainCamera != null)
        {
            cinemachineBrain =
                mainCamera.GetComponent<CinemachineBrain>();
        }


        // -----------------------------------------------------
        // QUAN TRỌNG:
        // GAMEPLAY MẶC ĐỊNH KHÔNG DÙNG CINEMACHINE
        // -----------------------------------------------------

        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
        }


        // -----------------------------------------------------
        // CAMERA GAMEPLAY MẶC ĐỊNH ĐƯỢC BẬT
        // -----------------------------------------------------

        if (cameraController != null)
        {
            cameraController.enabled = true;
        }
    }


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        /*
         * Đảm bảo khi scene load,
         * camera luôn trở về gameplay camera.
         */

        if (cameraController != null)
        {
            cameraController.ResetCameraImmediately();
        }
    }


    // =========================================================
    // START CUTSCENE
    // =========================================================

    public void StartCutscene()
    {
        if (IsCutscenePlaying)
            return;

        IsCutscenePlaying = true;


        // -----------------------------------------------------
        // LOCK PLAYER
        // -----------------------------------------------------

        if (playerController != null)
        {
            playerController.enabled = false;
        }


        // -----------------------------------------------------
        // DISABLE GAMEPLAY CAMERA
        // -----------------------------------------------------

        if (cameraController != null)
        {
            cameraController.enabled = false;
        }


        // -----------------------------------------------------
        // ENABLE CINEMACHINE
        // -----------------------------------------------------

        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = true;
        }


        Debug.Log(
            "CutsceneManager: Start Cutscene"
        );
    }


    // =========================================================
    // END CUTSCENE
    // =========================================================

    public void EndCutscene()
    {
        if (!IsCutscenePlaying)
            return;

        IsCutscenePlaying = false;


        // -----------------------------------------------------
        // 1. TẮT CINEMACHINE
        // -----------------------------------------------------

        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
        }


        // -----------------------------------------------------
        // 2. RESET GAMEPLAY CAMERA
        // -----------------------------------------------------

        if (cameraController != null)
        {
            cameraController.ResetCameraImmediately();

            cameraController.enabled = true;
        }


        // -----------------------------------------------------
        // 3. PLAYER ON
        // -----------------------------------------------------

        if (playerController != null)
        {
            playerController.enabled = true;
        }


        Debug.Log(
            "CutsceneManager: End Cutscene"
        );
    }
}