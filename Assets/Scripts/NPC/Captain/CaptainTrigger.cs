using UnityEngine;

public class CaptainTrigger : MonoBehaviour
{
    [Header("UI")]
    public GameObject pressRUI;
    public GameObject upgradeUI;

    [Header("Captain SFX")]
    [SerializeField] private AudioClip captainInteractSFX;

    [Range(0f, 1f)]
    [SerializeField] private float captainInteractVolume = 1f;

    private bool playerInside;
    private bool isOpenPress;
    private bool isOpenUpgrade;


    private void Start()
    {
        if (pressRUI != null)
        {
            pressRUI.SetActive(false);
        }

        if (upgradeUI != null)
        {
            upgradeUI.SetActive(false);
        }
    }


    private void Update()
    {
        if (!playerInside)
            return;


        // =====================================================
        // PRESS R TO OPEN CAPTAIN UPGRADE
        // =====================================================

        if (isOpenPress)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                isOpenUpgrade = true;
                isOpenPress = false;

                if (upgradeUI != null)
                {
                    upgradeUI.SetActive(true);
                }

                if (pressRUI != null)
                {
                    pressRUI.SetActive(false);
                }


                /*
                 * Chỉ phát 1 lần khi mở bảng Captain.
                 */
                PlayCaptainInteractSFX();


                Time.timeScale = 0f;
            }
        }


        // =====================================================
        // PRESS R TO CLOSE
        // =====================================================

        else
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                isOpenUpgrade = false;
                isOpenPress = true;

                if (upgradeUI != null)
                {
                    upgradeUI.SetActive(false);
                }

                if (pressRUI != null)
                {
                    pressRUI.SetActive(true);
                }


                Time.timeScale = 1f;
            }
        }
    }


    // =========================================================
    // CAPTAIN SFX
    // =========================================================

    private void PlayCaptainInteractSFX()
    {
        if (captainInteractSFX == null)
            return;


        if (AudioController.Instance == null)
            return;


        AudioController.Instance.PlaySFX(
            captainInteractSFX,
            captainInteractVolume
        );
    }


    // =========================================================
    // TRIGGER ENTER
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;


        playerInside = true;
        isOpenPress = true;


        if (pressRUI != null)
        {
            pressRUI.SetActive(true);
        }
    }


    // =========================================================
    // TRIGGER EXIT
    // =========================================================

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;


        playerInside = false;
        isOpenPress = false;
        isOpenUpgrade = false;


        if (pressRUI != null)
        {
            pressRUI.SetActive(false);
        }


        if (upgradeUI != null)
        {
            upgradeUI.SetActive(false);
        }


        Time.timeScale = 1f;
    }
}