using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public Animator animator;
    public float moveSpeed = 5f;
    public float rotationSpeed = 50f;

    [Header("Attack")]
    public float attackBufferTime = 0.3f;
    public float attackRange = 1f;
    public LayerMask enemyLayer;

    [Header("Ultimate")]
    public float ultimateDuration = 2f;
    public GameObject swordTrail;
    public float ultimateDamageInterval = 0.2f;
    float lastUltiTime = -999f;
    float cooldownUlti = 10f;

    [Header("Dash")]
    public float dashDistance = 3f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.8f;

    public float dashRadius = 0.4f;
    public float dashSkinWidth = 0.05f;

    public LayerMask obstacleLayer;

    private bool isDashing;
    private float nextDashTime;

    private const string Dash = "Dash";

    private bool isCastingSpell;

    private float lastClickTime;

    private bool attackPressed;
    private bool isUltimateActive;
    private string currentAttack = "";

    private bool hasHitThisAttack;

    private SpellCaster spellCaster;

    private AttackSpellCaster attackSpellCaster;
    private bool isAimingAttackSpell;

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
        attackSpellCaster = GetComponent<AttackSpellCaster>();
        playerStats = GetComponent<PlayerStats>();
    }

    private void Update()
    {
        if (!isDashing)
        {
            Move();
        }

        HandleAttack();

        HandleUltimate();

        HandleSpell();

        HandleAttackSpell();

        HandleDash();
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

    private void RotateTowardsMouse()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Plane plane = new Plane(Vector3.up, transform.position);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);

            Vector3 direction = hitPoint - transform.position;
            direction.y = 0;

            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
    private void HandleAttack()
    {
        if (isUltimateActive || isDashing)
            return;


        if (Input.GetMouseButtonDown(0))
        {
            //RotateTowardsMouse();

            lastClickTime = Time.time;
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

    public bool CanUltimate()
    {
        return Time.time >= lastUltiTime + cooldownUlti;
    }

    public float GetUltiCooldown()
    {
        return Mathf.Max(
            0,
            lastUltiTime + cooldownUlti - Time.time);
    }

    protected void StartUltiCooldown()
    {
        lastUltiTime = Time.time;
    }
    private void HandleUltimate()
    {
        if (isDashing)
            return;
        if (!CanUltimate())
        {
            Debug.Log(
                $"Cooldown: {GetUltiCooldown():F1}s");
            return;
        }

        

        if (Input.GetMouseButtonDown(1) && !isUltimateActive)
        {
            StartUltiCooldown();
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
        if (isDashing)
            return;
        if (Input.GetKeyDown(KeyCode.Q))
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

    private void HandleAttackSpell()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            attackSpellCaster.StartAim();
            isAimingAttackSpell = true;
        }

        if (!isAimingAttackSpell)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            attackSpellCaster.CastSpell();
            isAimingAttackSpell = false;
        }

        if (Input.GetMouseButtonDown(1))
        {
            attackSpellCaster.CancelAim();
            isAimingAttackSpell = false;
        }
    }
    private void HandleDash()
    {

        if (  isUltimateActive)
            return;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TryDash();
        }
    }

    private bool CanDash()
    {
        return
            !isDashing &&
            !isUltimateActive &&
            Time.time >= nextDashTime;
    }

    private void TryDash()
    {
        if (!CanDash())
            return;

        nextDashTime =
            Time.time + dashCooldown;

        StartCoroutine(DashRoutine());
    }

    private Vector3 CalculateDashDestination()
    {

        Vector3 origin =  transform.position + Vector3.up * 0.5f;

        // Nếu đang đứng đè hoặc quá sát obstacle thì không cho dash
        if (Physics.CheckSphere(origin, dashRadius, obstacleLayer))
        {
            return transform.position;
        }
        Vector3 direction;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");

        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;

        camForward.y = 0;
        camRight.y = 0;

        direction =
            (camForward * y + camRight * x).normalized;

        if (direction == Vector3.zero)
        {
            direction = transform.forward;
        }
        //Vector3 direction = transform.forward;

        origin =
            transform.position +
            Vector3.up * 0.5f;

        float distance =
            dashDistance;

        RaycastHit hit;

        if (Physics.SphereCast(
            origin,
            dashRadius,
            direction,
            out hit,
            dashDistance,
            obstacleLayer))
        {
            distance =
                Mathf.Max(
                    0,
                    hit.distance - dashSkinWidth);
        }

        return
            transform.position +
            direction * distance;
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;

        IsInvincible = true;

        attackPressed = false;

        animator.SetBool(
            AttackPressed,
            false);

        animator.SetTrigger(Dash);


        Vector3 start =
            transform.position;

        Vector3 end =
            CalculateDashDestination();

        float timer = 0;

        while (timer < dashDuration)
        {
            transform.position =
                Vector3.Lerp(
                    start,
                    end,
                    timer / dashDuration);

            timer += Time.deltaTime;

            yield return null;
        }

        transform.position = end;

        IsInvincible = false;

        isDashing = false;
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