using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpellBase : MonoBehaviour
{
    [Header("Info")]
    public string spellName;

    public Sprite icon;

    public int manaCost = 10;

    [Header("Cooldown")]
    public float cooldown = 5f;

    public float lastCastTime = -100f;
    protected virtual void Awake()
    {
        lastCastTime = -cooldown;
    }

 
    
    public bool CanCast()
    {
        Debug.Log($"Cooldown = {cooldown}");
        Debug.Log($"LastCastTime = {lastCastTime}");
        Debug.Log($"Time.time = {Time.time}");

        return Time.time >= lastCastTime + cooldown;
    }

    public float GetRemainingCooldown()
    {
        return Mathf.Max(
            0,
            lastCastTime + cooldown - Time.time);
    }

    protected void StartCooldown()
    {
        lastCastTime = Time.time;
    }

    public abstract void Cast(PlayerController player);
}