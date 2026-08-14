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
        // GAMEPLAY DEFAULT
        // -----------------------------------------------------

        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
        }

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
        if (cameraController != null)
        {
            cameraController.ResetCameraImmediately();
        }
    }


    // =========================================================
    // TIMELINE CUTSCENE 1 / 2
    // =========================================================

    public void StartCutscene()
    {
        if (IsCutscenePlaying)
            return;

        IsCutscenePlaying = true;


        // -----------------------------------------------------
        // RESET + LOCK PLAYER
        // -----------------------------------------------------

        if (playerController != null)
        {
            /*
             * Quan trọng:
             * Không disable PlayerController nữa.
             *
             * SetControlEnabled(false) sẽ:
             * - reset input
             * - tắt trail
             * - hủy aim
             * - reset Animator về Idle
             * - rồi khóa control
             */
            playerController.SetControlEnabled(false);
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


    public void EndCutscene()
    {
        if (!IsCutscenePlaying)
            return;

        IsCutscenePlaying = false;


        // -----------------------------------------------------
        // DISABLE CINEMACHINE
        // -----------------------------------------------------

        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
        }


        // -----------------------------------------------------
        // RESTORE GAMEPLAY CAMERA
        // -----------------------------------------------------

        if (cameraController != null)
        {
            cameraController.ResetCameraImmediately();

            cameraController.enabled = true;
        }


        // -----------------------------------------------------
        // UNLOCK PLAYER
        // -----------------------------------------------------

        if (playerController != null)
        {
            playerController.SetControlEnabled(true);
        }


        Debug.Log(
            "CutsceneManager: End Cutscene"
        );
    }


    // =========================================================
    // ENDING UI CUTSCENE
    // =========================================================

    public void StartEnding()
    {
        if (IsCutscenePlaying)
            return;

        IsCutscenePlaying = true;


        // -----------------------------------------------------
        // RESET + LOCK PLAYER
        // -----------------------------------------------------

        if (playerController != null)
        {
            playerController.SetControlEnabled(false);
        }


        // -----------------------------------------------------
        // ENDING UI KHÔNG DÙNG CINEMACHINE
        // -----------------------------------------------------

        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
        }


        if (cameraController != null)
        {
            cameraController.enabled = true;

            cameraController.ResetCameraImmediately();
        }


        Debug.Log(
            "CutsceneManager: Start Ending"
        );
    }


    public void EndEnding()
    {
        IsCutscenePlaying = false;

        Debug.Log(
            "CutsceneManager: End Ending"
        );
    }
}