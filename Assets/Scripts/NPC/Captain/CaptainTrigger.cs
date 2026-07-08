using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaptainTrigger : MonoBehaviour
{
    public GameObject pressRUI;
    public GameObject upgradeUI;

    private bool playerInside;
    private bool isOpenPress;
    private bool isOpenUpgrade;

    private void Start()
    {
        pressRUI.SetActive(false);
        upgradeUI.SetActive(false);
    }

    private void Update()
    {
        if (!playerInside)
            return;
        if(isOpenPress == true)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                isOpenUpgrade = true;
                isOpenPress = false;
                upgradeUI.SetActive(isOpenUpgrade);
                pressRUI.SetActive(isOpenPress);
                Time.timeScale = 0f;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                isOpenUpgrade = false;
                isOpenPress = true;
                upgradeUI.SetActive(isOpenUpgrade);
                pressRUI.SetActive(isOpenPress);
                Time.timeScale = 1f;
            }
        }
        
    }

    private void OnTriggerEnter(
        Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        isOpenPress = true;
        pressRUI.SetActive(isOpenPress);
    }

    private void OnTriggerExit(
        Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        isOpenPress = false;
        isOpenUpgrade = false;
        pressRUI.SetActive(isOpenPress);
        upgradeUI.SetActive(isOpenUpgrade);
    }
}