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


        /*
         * Bình thường trong Village,
         * camera được CameraController điều khiển.
         *
         * Cinemachine chỉ được bật khi có cutscene.
         */
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
    // START CUTSCENE
    // =========================================================

    public void StartCutscene()
    {
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
    }


    // =========================================================
    // END CUTSCENE
    // =========================================================

    public void EndCutscene()
    {
        IsCutscenePlaying = false;


        /*
         * Quan trọng:
         *
         * Phải tắt Cinemachine trước.
         *
         * Nếu không CinemachineBrain vẫn tiếp tục
         * giữ Main Camera ở CM_Intro cuối cùng.
         */
        if (cinemachineBrain != null)
        {
            cinemachineBrain.enabled = false;
        }


        /*
         * Sau khi Cinemachine ngừng điều khiển,
         * trả Main Camera về cho CameraController.
         */
        if (cameraController != null)
        {
            cameraController.enabled = true;
        }


        // -----------------------------------------------------
        // UNLOCK PLAYER
        // -----------------------------------------------------

        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }
}