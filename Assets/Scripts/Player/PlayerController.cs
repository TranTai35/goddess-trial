using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public Animator animator;
    public float moveSpeed = 5f;
    public float rotationSpeed = 15f;

    [Header("Attack")]
    public float attackBufferTime = 0.3f;
    public float attackRange = 2f;
    public LayerMask enemyLayer;

    [Header("Ultimate")]
    public float ultimateDuration = 2f;
    public GameObject swordTrail;

    public float ultimateDamageInterval = 0.2f;

    private bool isCastingSpell;

    private float lastClickTime;

    private bool attackPressed;
    private bool isUltimateActive;
    private string currentAttack = "";

    private bool hasHitThisAttack;

    private SpellCaster spellCaster;
    private PlayerStats playerStats;

    private const string IsMoving = "Moving";
    private const string AttackPressed = "AttackPressed";
    private const string IsAttacking = "IsAttacking";

    private const string Attack01 = "Attack01";
    private const string Attack02 = "Attack02";
    private const string Attack03 = "Attack03";

    private const string Ultimate = "Ultimate";

    public bool IsInvincible
    {
        get;
        private set;
    }

    private void Start()
    {
        spellCaster = GetComponent<SpellCaster>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        Move();

        HandleAttack();

        HandleUltimate();

        HandleSpell();
    }

    #region Movement

    private void Move()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 camForward =
            Camera.main.transform.forward;

        Vector3 camRight =
            Camera.main.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        camForward.Normalize();
        camRight.Normalize();

        Vector3 moveDir =
            camForward * y +
            camRight * x;

        bool isMoving =
            moveDir.sqrMagnitude > 0.01f;

        if (isMoving)
        {
            float targetAngle =
                Mathf.Atan2(
                    moveDir.x,
                    moveDir.z) *
                Mathf.Rad2Deg;

            Quaternion targetRotation =
                Quaternion.Euler(
                    0f,
                    targetAngle,
                    0f);

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed *
                    Time.deltaTime);

            transform.position +=
                moveDir.normalized *
                moveSpeed *
                Time.deltaTime;
        }

        animator.SetBool(
            IsMoving,
            isMoving);
    }

    #endregion

    #region Attack

    private void HandleAttack()
    {
        if (isUltimateActive)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            lastClickTime =
                Time.time;
        }

        attackPressed =
            Time.time -
            lastClickTime <
            attackBufferTime;

        animator.SetBool(
            AttackPressed,
            attackPressed);

        UpdateAttackState();
    }

    private void UpdateAttackState()
    {
        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(1);

        bool isAttackAnim =
            stateInfo.IsName(Attack01) ||
            stateInfo.IsName(Attack02) ||
            stateInfo.IsName(Attack03);

        animator.SetBool(
            IsAttacking,
            isAttackAnim);

        if (!isAttackAnim)
        {
            currentAttack = "";
            hasHitThisAttack = false;
            return;
        }

        string attackName = "";

        if (stateInfo.IsName(Attack01))
            attackName = Attack01;
        else if (stateInfo.IsName(Attack02))
            attackName = Attack02;
        else if (stateInfo.IsName(Attack03))
            attackName = Attack03;

        // Vừa chuyển sang đòn mới
        if (currentAttack != attackName)
        {
            currentAttack = attackName;
            hasHitThisAttack = false;
        }

        if (!hasHitThisAttack &&
            stateInfo.normalizedTime >= 0.5f)
        {
            DealDamage();

            hasHitThisAttack = true;

            Debug.Log("Damage From: " + attackName);
        }
    }

    private void DealDamage()
    {
        Vector3 attackCenter =
            transform.position +
            transform.forward ;

        Collider[] hits =
            Physics.OverlapSphere(
                attackCenter,
                attackRange,
                enemyLayer);
        Debug.Log(hits.Length);

        foreach (Collider hit in hits)
        {
            EnemyController enemy =
                hit.GetComponent<EnemyController>();

            if (enemy == null)
                continue;

            enemy.TakeDamage(
                playerStats.baseStats.damage);
        }
    }

    #endregion

    #region Ultimate

    private void HandleUltimate()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) &&
            !isUltimateActive)
        {
            if (swordTrail != null)
            {
                swordTrail.SetActive(true);
            }

            Invoke(
                nameof(DisableTrail),
                ultimateDuration);

            StartCoroutine(
                UltimateCoroutine());
        }
    }

    private void DisableTrail()
    {
        if (swordTrail != null)
        {
            swordTrail.SetActive(false);
        }
    }

    private void DealUltimateDamage()
    {
        Collider[] hits =
            Physics.OverlapSphere(
                transform.position,
                attackRange * 2f,
                enemyLayer);

        foreach (Collider hit in hits)
        {
            EnemyController enemy =
                hit.GetComponent<EnemyController>();

            if (enemy == null)
                continue;

            enemy.TakeDamage(
                playerStats.baseStats.damage);
        }
    }

    private IEnumerator UltimateCoroutine()
    {
        attackPressed = false;

        animator.SetBool(
            AttackPressed,
            false);

        isUltimateActive = true;

        animator.SetBool(
            Ultimate,
            true);

        float timer = 0f;

        while (timer < ultimateDuration)
        {
            DealUltimateDamage();

            yield return new WaitForSeconds(
                ultimateDamageInterval);

            timer += ultimateDamageInterval;
        }

        animator.SetBool(
            Ultimate,
            false);

        isUltimateActive = false;
    }

    #endregion

    #region Spell

    private void HandleSpell()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            StartCoroutine(
                CastSpellRoutine());
        }
    }

    private IEnumerator CastSpellRoutine()
    {
        isCastingSpell = true;

        IsInvincible = true;

        spellCaster.CastSpell();

        yield return new WaitForSeconds(5f);

        IsInvincible = false;

        isCastingSpell = false;
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Vector3 attackCenter =
            transform.position +
            transform.forward ;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            attackCenter,
            attackRange);
    }

    #endregion
}