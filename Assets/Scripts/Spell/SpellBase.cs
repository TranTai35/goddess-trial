using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpellBase : MonoBehaviour
{
    [Header("Info")]
    public string spellName;

    public Sprite icon;

    public int manaCost = 10;

    public abstract void Cast(PlayerController player);
}