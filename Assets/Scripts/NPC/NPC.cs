using UnityEngine;

public class NPC : MonoBehaviour
{
    [Header("Common")]
    public string npcName;

    protected Animator animator;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    
}