using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    [Header("UI")]
    public GameObject uiPortal;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;
        
        uiPortal.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        uiPortal.SetActive(false);

    }

    public void EnterLevel()
    {
        SceneManager.LoadScene("Level1");
    }
}