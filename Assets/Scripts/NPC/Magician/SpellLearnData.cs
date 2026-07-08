using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class SpellLearnData
{
    public string spellName;
    public int diamondCost = 10;

    [Header("Chỉ gắn 1 trong 2 ô dưới đây")]
    public SpellBase utilitySpellPrefab;       // Phép bổ trợ (phím Q)
    public AttackSpellBase attackSpellPrefab;  // Phép tấn công (phím E, Chuột)

    [HideInInspector]
    public bool isBought = false; // Đánh dấu đã mua chưa
}