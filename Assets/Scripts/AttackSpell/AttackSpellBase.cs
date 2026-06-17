using UnityEngine;

public abstract class AttackSpellBase : MonoBehaviour
{
    public string spellName;
    public Sprite icon;

    public int manaCost = 10;
    public float attackRange = 8f;

    public abstract void StartAim(PlayerController player);

    public abstract void Cast(PlayerController player);

    public abstract void CancelAim();
}