using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MagicianTrigger : MonoBehaviour
{
    [Header("UI")]
    public GameObject pressRUI;
    public GameObject boardUI;
    public GameObject boardSpell;
    public GameObject boardAttackSpell;

    public Image imageSpell;
    public Image imageAttackSpell;

    private bool playerInside;
    private bool isOpenPress;
    private bool isOpenBoard;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color choiceColor = new Color(0.5f, 1f, 0.5f);


    private void Start()
    {
        pressRUI.SetActive(false);
        boardUI.SetActive(false);
    }

    private void Update()
    {
        if (!playerInside)
            return;
        if (isOpenPress == true)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                isOpenBoard = true;
                isOpenPress = false;
                boardUI.SetActive(isOpenBoard);
                pressRUI.SetActive(isOpenPress);
                boardSpell.SetActive(true);
                boardAttackSpell.SetActive(false);
                imageSpell.color = choiceColor;
                imageAttackSpell.color = normalColor;
                Time.timeScale = 0f;
            }
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                isOpenBoard = false;
                isOpenPress = true;
                boardUI.SetActive(isOpenBoard);
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
        isOpenBoard = false;
        pressRUI.SetActive(isOpenPress);
        boardUI.SetActive(isOpenBoard);
    }


    public void OnClickTabSpell()
    {
        boardSpell.SetActive(true);
        boardAttackSpell.SetActive(false);
        imageSpell.color = choiceColor;
        imageAttackSpell.color = normalColor;
    }

    public void OnClickTabAttackSpell()
    {
        boardSpell.SetActive(false);
        boardAttackSpell.SetActive(true);
        imageSpell.color = normalColor;
        imageAttackSpell.color = choiceColor;
    }
}
