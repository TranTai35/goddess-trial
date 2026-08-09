using UnityEngine;
using UnityEngine.UI;

public class MagicianTrigger : MonoBehaviour
{
    [Header("UI")]
    public GameObject pressRUI;
    public GameObject boardUI;
    public GameObject boardSpell;
    public GameObject boardAttackSpell;

    public Image imageSpell;
    public Image imageAttackSpell;


    [Header("Magician SFX")]
    [SerializeField] private AudioClip magicianInteractSFX;

    [Range(0f, 1f)]
    [SerializeField] private float magicianInteractVolume = 1f;


    [Header("Colors")]
    public Color normalColor = Color.white;

    public Color choiceColor =
        new Color(
            0.5f,
            1f,
            0.5f
        );


    private bool playerInside;
    private bool isOpenPress;
    private bool isOpenBoard;


    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        if (pressRUI != null)
        {
            pressRUI.SetActive(false);
        }


        if (boardUI != null)
        {
            boardUI.SetActive(false);
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (!playerInside)
            return;


        // =====================================================
        // OPEN MAGICIAN
        // =====================================================

        if (isOpenPress)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                isOpenBoard = true;
                isOpenPress = false;


                if (boardUI != null)
                {
                    boardUI.SetActive(true);
                }


                if (pressRUI != null)
                {
                    pressRUI.SetActive(false);
                }


                if (boardSpell != null)
                {
                    boardSpell.SetActive(true);
                }


                if (boardAttackSpell != null)
                {
                    boardAttackSpell.SetActive(false);
                }


                if (imageSpell != null)
                {
                    imageSpell.color =
                        choiceColor;
                }


                if (imageAttackSpell != null)
                {
                    imageAttackSpell.color =
                        normalColor;
                }


                /*
                 * Phát đúng 1 lần khi mở shop.
                 */
                PlayMagicianInteractSFX();


                Time.timeScale = 0f;
            }
        }


        // =====================================================
        // CLOSE MAGICIAN
        // =====================================================

        else
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                isOpenBoard = false;
                isOpenPress = true;


                if (boardUI != null)
                {
                    boardUI.SetActive(false);
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
    // MAGICIAN SFX
    // =========================================================

    private void PlayMagicianInteractSFX()
    {
        if (magicianInteractSFX == null)
            return;


        if (AudioController.Instance == null)
            return;


        AudioController.Instance.PlaySFX(
            magicianInteractSFX,
            magicianInteractVolume
        );
    }


    // =========================================================
    // ENTER
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
    // EXIT
    // =========================================================

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;


        playerInside = false;
        isOpenPress = false;
        isOpenBoard = false;


        if (pressRUI != null)
        {
            pressRUI.SetActive(false);
        }


        if (boardUI != null)
        {
            boardUI.SetActive(false);
        }


        Time.timeScale = 1f;
    }


    // =========================================================
    // SPELL TAB
    // =========================================================

    public void OnClickTabSpell()
    {
        if (boardSpell != null)
        {
            boardSpell.SetActive(true);
        }


        if (boardAttackSpell != null)
        {
            boardAttackSpell.SetActive(false);
        }


        if (imageSpell != null)
        {
            imageSpell.color =
                choiceColor;
        }


        if (imageAttackSpell != null)
        {
            imageAttackSpell.color =
                normalColor;
        }
    }


    // =========================================================
    // ATTACK SPELL TAB
    // =========================================================

    public void OnClickTabAttackSpell()
    {
        if (boardSpell != null)
        {
            boardSpell.SetActive(false);
        }


        if (boardAttackSpell != null)
        {
            boardAttackSpell.SetActive(true);
        }


        if (imageSpell != null)
        {
            imageSpell.color =
                normalColor;
        }


        if (imageAttackSpell != null)
        {
            imageAttackSpell.color =
                choiceColor;
        }
    }
}