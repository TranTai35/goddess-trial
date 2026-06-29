using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "PlayerStats",
    menuName = "Data/Player Stats"
)]
public class PlayerStatsData : ScriptableObject
{
    [Header("Health")]
    public float maxHealth = 100;
    public float currentHealth;

    [Header("Mana")]
    public float maxMana = 100;
    public float currentMana;
    public float manaRegen = 0.5f;

    [Header("Combat")]
    public float damage = 10;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Currency")]
    public int gold = 100;
    public int diamond = 190;
}