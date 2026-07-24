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
    public float attackRange = 1.5f;
    public LayerMask enemyLayer;

    [Header("Ultimate")]
    public float ultimateDuration = 2f;
    public GameObject swordTrail;
    public float ultimateDamageInterval = 0.2f;
    float lastUltiTime = -999f;
    float cooldownUlti = 10f;

    [Header("Dash")]
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.8f;

    [Header("Dash Trail")]
    [SerializeField] private TrailRenderer dashTrail1;
    [SerializeField] private TrailRenderer dashTrail2;

    public float dashRadius = 0.4f;
    public float dashSkinWidth = 0.05f;

    [Header("Combo")]
    [SerializeField] private float comboInputOpenTime = 0.45f;
    [SerializeField] private float comboInputCloseTime = 0.9f;

    private bool attackQueued;
    private bool attackInputPulse;

    public LayerMask obstacleLayer;

    private bool isDashing;
    private float nextDashTime;

    private const string Dash = "Dash";

    private Rigidbody rb;

    private bool isCastingSpell;

    

    private bool isUltimateActive;
    private string currentAttack = "";

    private bool hasHitThisAttack;

    private SpellCaster spellCaster;

    private AttackSpellCaster attackSpellCaster;
    private bool isAimingAttackSpell;

    private PlayerStats playerStats;

    private bool canControl = true;

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

    public float BaseMoveSpeed
    {
        get
        {
            if (playerStats != null &&
                playerStats.baseStats != null)
            {
                return playerStats.baseStats.moveSpeed;
            }

            return moveSpeed;
        }
    }

    private void Start()
    {
        spellCaster = GetComponent<SpellCaster>();
        attackSpellCaster = GetComponent<AttackSpellCaster>();
        playerStats = GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody>();

        if (dashTrail1 != null)
        {
            dashTrail1.emitting = false;
            dashTrail1.Clear();
        }

        if (dashTrail2 != null)
        {
            dashTrail2.emitting = false;
            dashTrail2.Clear();
        }
    }

    private void Update()
    {
        if (!canControl)
        {
            return;
        }

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

    public void UpdateMoveSpeed()
    {
        moveSpeed = playerStats.baseStats.moveSpeed;
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
        {
            attackQueued = false;

            animator.SetBool(
                AttackPressed,
                false
            );

            return;
        }

        /*
         * AttackPressed chỉ bật đúng một frame.
         * Nhờ vậy một lần click không thể làm Animator
         * chạy xuyên qua nhiều đòn.
         */
        animator.SetBool(
            AttackPressed,
            false
        );

        attackInputPulse = false;

        if (Input.GetMouseButtonDown(0))
        {
            attackQueued = true;
        }

        AnimatorStateInfo stateInfo =
            animator.GetCurrentAnimatorStateInfo(1);

        bool isAttack01 =
            stateInfo.IsName(Attack01);

        bool isAttack02 =
            stateInfo.IsName(Attack02);

        bool isAttack03 =
            stateInfo.IsName(Attack03);

        bool isAttackAnim =
            isAttack01 ||
            isAttack02 ||
            isAttack03;

        /*
         * Khi chưa đánh:
         * một click sẽ bắt đầu Attack01.
         */
        if (!isAttackAnim && attackQueued)
        {
            SendAttackInput();

            attackQueued = false;
        }
        /*
         * Trong Attack01 và Attack02:
         * chỉ tiêu thụ click tiếp theo khi combo window mở.
         */
        else if (
            (isAttack01 || isAttack02) &&
            attackQueued &&
            stateInfo.normalizedTime >= comboInputOpenTime &&
            stateInfo.normalizedTime <= comboInputCloseTime
        )
        {
            SendAttackInput();

            attackQueued = false;
        }

        /*
         * Attack03 là đòn cuối.
         * Không cho click cũ tiếp tục kích hoạt combo mới.
         */
        if (isAttack03 &&
            stateInfo.normalizedTime >= comboInputCloseTime)
        {
            attackQueued = false;
        }

        UpdateAttackState();
    }

    private void ResetAttackInput()
    {
        attackQueued = false;
        attackInputPulse = false;

        currentAttack = "";
        hasHitThisAttack = false;

        animator.SetBool(
            AttackPressed,
            false);
    }

    private void SendAttackInput()
    {
        attackInputPulse = true;

        animator.SetBool(
            AttackPressed,
            true
        );
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
            isAttackAnim
        );

        if (!isAttackAnim)
        {
            currentAttack = "";
            hasHitThisAttack = false;
            return;
        }

        string attackName = "";

        if (stateInfo.IsName(Attack01))
        {
            attackName = Attack01;
        }
        else if (stateInfo.IsName(Attack02))
        {
            attackName = Attack02;
        }
        else if (stateInfo.IsName(Attack03))
        {
            attackName = Attack03;
        }

        // Vừa bước sang animation tấn công mới
        if (currentAttack != attackName)
        {
            currentAttack = attackName;
            hasHitThisAttack = false;
        }

        float hitTime =
            currentAttack == Attack03
                ? 0.46f
                : 0.5f;

        if (!hasHitThisAttack &&
            stateInfo.normalizedTime >= hitTime)
        {
            DealDamage();

            hasHitThisAttack = true;
        }
    }


    private void DealDamage()
    {
        bool isCritical =
            currentAttack == Attack03;

        float damage =
            playerStats.baseStats.damage;

        if (isCritical)
        {
            damage *= 2f;
        }

        Vector3 attackCenter =
            transform.position +
            transform.forward;

        Collider[] hits =
            Physics.OverlapSphere(
                attackCenter,
                attackRange,
                enemyLayer
            );

        if (hits.Length > 0 &&
            FeedbackManager.Instance != null)
        {
            if (isCritical)
            {
                /*
                 * Attack03:
                 * dừng hình lâu hơn,
                 * slow motion mạnh hơn,
                 * camera rung lâu và mạnh hơn.
                 */
                FeedbackManager.Instance.PlayHitFeedback(
                    0.09f, // Thời gian hit-stop
                    0.03f, // Time scale
                    0.18f, // Thời gian rung
                    0.18f  // Độ mạnh rung
                );
            }
            else
            {
                FeedbackManager.Instance.PlayHitFeedback(
                    0.035f,
                    0.2f,
                    0.07f,
                    0.055f
                );
            }
        }

        foreach (Collider hit in hits)
        {
            EnemyController enemy =
                hit.GetComponentInParent<EnemyController>();

            if (enemy == null)
            {
                continue;
            }

            enemy.TakeDamage(
                damage,
                isCritical
            );
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
        ResetAttackInput();

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
        if (isDashing) return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            // KIỂM TRA: Phép bổ trợ phải tồn tại và ĐÃ HỒI CHIÊU XONG mới cho bấm
            if (spellCaster.equippedSpell != null && spellCaster.equippedSpell.CanCast())
            {
                StartCoroutine(CastSpellRoutine());
            }
            else
            {
                Debug.Log("Phép bổ trợ (Q) chưa hồi chiêu xong!");
            }
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
            // SỬA TẠI ĐÂY: Kiểm tra phép tấn công tồn tại VÀ phải hồi chiêu xong mới cho ngắm bắn
            if (attackSpellCaster.equippedSpell != null && attackSpellCaster.equippedSpell.CanCast())
            {
                attackSpellCaster.StartAim();
                isAimingAttackSpell = true;
            }
            else
            {
                Debug.Log("Phép tấn công (E) đang hồi chiêu, không thể ngắm bắn!");
            }
        }

        if (!isAimingAttackSpell) return;

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


    private IEnumerator DistancleAim()
    {
        yield return new WaitForSeconds(1f);
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

    private bool IsBlocked(Vector3 direction)
    {
        Vector3 origin =
            transform.position + Vector3.up * 0.5f;


        return Physics.SphereCast(
            origin,
            dashRadius,
            direction,
            out RaycastHit hit,
            dashRadius + dashSkinWidth,
            obstacleLayer
        );
    }

    private IEnumerator DashRoutine()
    {
        isDashing = true;
        IsInvincible = true;

        animator.SetTrigger(Dash);

        EnableDashTrails();


        Vector3 direction;

        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");


        Vector3 camForward = Camera.main.transform.forward;
        Vector3 camRight = Camera.main.transform.right;


        camForward.y = 0;
        camRight.y = 0;


        direction =
            (camForward * y +
            camRight * x).normalized;


        if (direction == Vector3.zero)
        {
            direction = transform.forward;
        }


        float dashSpeed =
            dashDistance / dashDuration;


        float timer = 0;


        while (timer < dashDuration)
        {
            float moveDistance =
                dashSpeed * Time.deltaTime;


            Vector3 origin =
                rb.position + Vector3.up * 0.5f;


            RaycastHit hit;


            // kiểm tra trước khi di chuyển
            if (Physics.SphereCast(
                origin,
                dashRadius,
                direction,
                out hit,
                moveDistance + dashSkinWidth,
                obstacleLayer))
            {

                // đứng cách wall một khoảng nhỏ
                Vector3 stopPosition =
                    hit.point -
                    direction * dashRadius;


                rb.MovePosition(stopPosition);

                break;
            }


            rb.MovePosition(
                rb.position +
                direction * moveDistance
            );


            timer += Time.deltaTime;


            yield return null;
        }


        DisableDashTrails();


        IsInvincible = false;
        isDashing = false;
    }

    private void EnableDashTrails()
    {
        if (dashTrail1 != null)
        {
            dashTrail1.Clear();
            dashTrail1.emitting = true;
        }

        if (dashTrail2 != null)
        {
            dashTrail2.Clear();
            dashTrail2.emitting = true;
        }
    }

    private void DisableDashTrails()
    {
        if (dashTrail1 != null)
        {
            dashTrail1.emitting = false;
        }

        if (dashTrail2 != null)
        {
            dashTrail2.emitting = false;
        }
    }


    public void SetControlEnabled(bool enabled)
    {
        canControl = enabled;

        if (!enabled)
        {
            ResetAttackInput();

            isAimingAttackSpell = false;

            animator.SetBool(
                IsAttacking,
                false);

            animator.SetBool(
                IsMoving,
                false);

            if (attackSpellCaster != null)
            {
                attackSpellCaster.CancelAim();
            }
        }
    }

    public void SetCutsceneMoving(bool isMoving)
    {
        animator.SetBool(IsMoving, isMoving);
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