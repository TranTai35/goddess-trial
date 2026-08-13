using System.Collections;
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

    [Header("Attack Sword Trail")]
    [SerializeField] private GameObject attackSwordTrail;

    [Header("Ultimate")]
    public float ultimateDuration = 2f;
    public GameObject swordTrail;
    public float ultimateDamageInterval = 0.2f;

    private float lastUltiTime = -999f;
    private float cooldownUlti = 10f;

    [Header("Dash")]
    public float dashDistance = 5f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Dash Trail")]
    [SerializeField] private TrailRenderer dashTrail1;
    [SerializeField] private TrailRenderer dashTrail2;

    public float dashRadius = 0.4f;
    public float dashSkinWidth = 0.05f;

    [Header("Combo")]
    [SerializeField] private float comboInputOpenTime = 0.45f;
    [SerializeField] private float comboInputCloseTime = 0.9f;

    public LayerMask obstacleLayer;

    private bool attackQueued;
    private bool attackInputPulse;

    private bool isDashing;
    private float nextDashTime;

    private bool isCastingSpell;
    private bool isUltimateActive;
    private bool isAimingAttackSpell;
    private bool canControl = true;

    private string currentAttack = "";
    private bool hasHitThisAttack;

    private Rigidbody rb;

    private SpellCaster spellCaster;
    private AttackSpellCaster attackSpellCaster;
    private PlayerStats playerStats;

    // Audio của Player.
    private PlayerAudio playerAudio;

    private const string Dash = "Dash";

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

        /*
         * Dùng GetComponentInChildren để tìm được PlayerAudio
         * nếu script nằm trên object con chứa Animator/model.
         */
        playerAudio = GetComponentInChildren<PlayerAudio>();

        if (playerAudio == null)
        {
            Debug.LogWarning(
                "PlayerController: Không tìm thấy PlayerAudio trên Player hoặc object con."
            );
        }

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

        if (attackSwordTrail != null)
        {
            attackSwordTrail.SetActive(false);
        }

        if (swordTrail != null)
        {
            swordTrail.SetActive(false);
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

        camForward.y = 0f;
        camRight.y = 0f;

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
                    moveDir.z
                ) * Mathf.Rad2Deg;

            Quaternion targetRotation =
                Quaternion.Euler(
                    0f,
                    targetAngle,
                    0f
                );

            transform.rotation =
                Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );

            transform.position +=
                moveDir.normalized *
                moveSpeed *
                Time.deltaTime;
        }

        animator.SetBool(
            IsMoving,
            isMoving
        );
    }

    public void UpdateMoveSpeed()
    {
        if (playerStats == null ||
            playerStats.baseStats == null)
        {
            return;
        }

        moveSpeed =
            playerStats.baseStats.moveSpeed;
    }

    #endregion

    #region Attack

    private void RotateTowardsMouse()
    {
        Ray ray =
            Camera.main.ScreenPointToRay(
                Input.mousePosition
            );

        Plane plane =
            new Plane(
                Vector3.up,
                transform.position
            );

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint =
                ray.GetPoint(enter);

            Vector3 direction =
                hitPoint -
                transform.position;

            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                transform.rotation =
                    Quaternion.LookRotation(
                        direction
                    );
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
            false
        );

        SetAttackSwordTrail(false);
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

        SetAttackSwordTrail(isAttackAnim);

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

        // Vừa bước sang animation tấn công mới.
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

    private void SetAttackSwordTrail(bool active)
    {
        if (attackSwordTrail == null)
        {
            return;
        }

        if (attackSwordTrail.activeSelf != active)
        {
            attackSwordTrail.SetActive(active);
        }
    }

    private void DealDamage()
    {
        bool isCritical =
            currentAttack == Attack03;

        if (playerStats == null ||
            playerStats.baseStats == null)
        {
            return;
        }

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

        /*
         * Một Enemy hoặc Boss có thể có nhiều Collider,
         * nên chỉ gây damage một lần trong mỗi đòn.
         */
        System.Collections.Generic.HashSet<EnemyController>
            damagedEnemies =
                new System.Collections.Generic.HashSet<EnemyController>();

        System.Collections.Generic.HashSet<BossController>
            damagedBosses =
                new System.Collections.Generic.HashSet<BossController>();

        bool hitAtLeastOneTarget = false;

        foreach (Collider hit in hits)
        {
            EnemyController enemy =
                hit.GetComponentInParent<EnemyController>();

            if (enemy != null &&
                damagedEnemies.Add(enemy))
            {
                enemy.TakeDamage(
                    damage,
                    isCritical
                );

                hitAtLeastOneTarget = true;
                continue;
            }

            BossController boss =
                hit.GetComponentInParent<BossController>();

            if (boss != null &&
                damagedBosses.Add(boss))
            {
                boss.TakeDamage(
                    damage,
                    isCritical
                );

                hitAtLeastOneTarget = true;
            }
        }

        /*
         * Chỉ phát tiếng va chạm khi thật sự gây damage.
         * Chém trúng nhiều Enemy cùng lúc vẫn chỉ phát
         * một tiếng hit cho một nhát chém.
         */
        if (hitAtLeastOneTarget &&
            playerAudio != null)
        {
            playerAudio.PlayHitEnemySound();
        }

        /*
         * Chỉ chạy hit-stop và camera shake
         * khi thật sự đánh trúng Enemy hoặc Boss.
         */
        if (hitAtLeastOneTarget &&
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
                    0.09f,
                    0.03f,
                    0.18f,
                    0.18f
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
    }

    #endregion

    #region Ultimate

    public bool CanUltimate()
    {
        return
            Time.time >=
            lastUltiTime +
            cooldownUlti;
    }

    public float GetUltiCooldown()
    {
        return Mathf.Max(
            0f,
            lastUltiTime +
            cooldownUlti -
            Time.time
        );
    }

    protected void StartUltiCooldown()
    {
        lastUltiTime = Time.time;
    }

    private void HandleUltimate()
    {
        if (isDashing)
        {
            return;
        }

        if (!CanUltimate())
        {
            return;
        }

        if (Input.GetMouseButtonDown(1) &&
            !isUltimateActive)
        {
            StartUltiCooldown();

            if (swordTrail != null)
            {
                swordTrail.SetActive(true);
            }

            Invoke(
                nameof(DisableTrail),
                ultimateDuration
            );

            StartCoroutine(
                UltimateCoroutine()
            );
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
                enemyLayer
            );

        if (playerStats == null ||
            playerStats.baseStats == null)
        {
            return;
        }

        float ultimateDamage =
            playerStats.baseStats.damage *
            1.5f;

        System.Collections.Generic.HashSet<EnemyController>
            damagedEnemies =
                new System.Collections.Generic.HashSet<EnemyController>();

        System.Collections.Generic.HashSet<BossController>
            damagedBosses =
                new System.Collections.Generic.HashSet<BossController>();

        bool hitAtLeastOneTarget = false;

        foreach (Collider hit in hits)
        {
            EnemyController enemy =
                hit.GetComponentInParent<EnemyController>();

            if (enemy != null &&
                damagedEnemies.Add(enemy))
            {
                enemy.TakeDamage(
                    ultimateDamage,
                    true
                );

                hitAtLeastOneTarget = true;
                continue;
            }

            BossController boss =
                hit.GetComponentInParent<BossController>();

            if (boss != null &&
                damagedBosses.Add(boss))
            {
                boss.TakeDamage(
                    ultimateDamage,
                    true
                );

                hitAtLeastOneTarget = true;
            }
        }

        /*
         * Ultimate gây damage nhiều lần theo thời gian.
         * Mỗi lần tick trúng mục tiêu sẽ phát một tiếng hit.
         *
         * Nếu nghe quá dày, có thể xóa phần này hoặc
         * tạo cooldown riêng cho tiếng Ultimate.
         */
        if (hitAtLeastOneTarget &&
            playerAudio != null)
        {
            playerAudio.PlayHitEnemySound();
        }
    }

    private IEnumerator UltimateCoroutine()
    {
        ResetAttackInput();
        SetAttackSwordTrail(false);

        isUltimateActive = true;
        IsInvincible = true;

        animator.SetBool(
            Ultimate,
            true
        );

        float timer = 0f;

        while (timer < ultimateDuration)
        {
            DealUltimateDamage();

            yield return new WaitForSeconds(
                ultimateDamageInterval
            );

            timer += ultimateDamageInterval;
        }

        animator.SetBool(
            Ultimate,
            false
        );

        isUltimateActive = false;
        IsInvincible = false;
    }

    #endregion

    #region Spell

    private void HandleSpell()
    {
        if (isDashing)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (spellCaster != null &&
                spellCaster.equippedSpell != null &&
                spellCaster.equippedSpell.CanCast())
            {
                StartCoroutine(
                    CastSpellRoutine()
                );
            }
            else
            {
                Debug.Log(
                    "Phép bổ trợ (Q) chưa hồi chiêu xong!"
                );
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
        if (attackSpellCaster == null)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E) &&
            !isAimingAttackSpell)
        {
            if (attackSpellCaster.equippedSpell != null &&
                attackSpellCaster.equippedSpell.CanCast())
            {
                attackSpellCaster.StartAim();

                isAimingAttackSpell = true;
                return;
            }

            Debug.Log(
                "Phép tấn công (E) đang hồi chiêu, không thể ngắm bắn!"
            );
        }

        if (!isAimingAttackSpell)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            attackSpellCaster.CastSpell();

            isAimingAttackSpell = false;
        }

        if (Input.GetKeyDown(KeyCode.E) &&
            isAimingAttackSpell)
        {
            attackSpellCaster.CancelAim();

            isAimingAttackSpell = false;
        }
    }

    private IEnumerator DistancleAim()
    {
        yield return new WaitForSeconds(1f);
    }

    #endregion

    #region Dash

    private void HandleDash()
    {
        if (isUltimateActive)
        {
            return;
        }

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
        {
            return;
        }

        nextDashTime =
            Time.time +
            dashCooldown;

        StartCoroutine(
            DashRoutine()
        );
    }

    private Vector3 CalculateDashDestination()
    {
        Vector3 origin =
            transform.position +
            Vector3.up * 0.5f;

        // Nếu đang đứng đè hoặc quá sát obstacle thì không cho dash.
        if (Physics.CheckSphere(
            origin,
            dashRadius,
            obstacleLayer
        ))
        {
            return transform.position;
        }

        Vector3 direction;

        float x =
            Input.GetAxisRaw("Horizontal");

        float y =
            Input.GetAxisRaw("Vertical");

        Vector3 camForward =
            Camera.main.transform.forward;

        Vector3 camRight =
            Camera.main.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        direction =
            (camForward * y +
            camRight * x).normalized;

        if (direction == Vector3.zero)
        {
            direction = transform.forward;
        }

        origin =
            transform.position +
            Vector3.up * 0.5f;

        float distance =
            dashDistance;

        if (Physics.SphereCast(
            origin,
            dashRadius,
            direction,
            out RaycastHit hit,
            dashDistance,
            obstacleLayer
        ))
        {
            distance =
                Mathf.Max(
                    0f,
                    hit.distance -
                    dashSkinWidth
                );
        }

        return
            transform.position +
            direction *
            distance;
    }

    private bool IsBlocked(Vector3 direction)
    {
        Vector3 origin =
            transform.position +
            Vector3.up * 0.5f;

        return Physics.SphereCast(
            origin,
            dashRadius,
            direction,
            out RaycastHit hit,
            dashRadius +
            dashSkinWidth,
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

        float x =
            Input.GetAxisRaw("Horizontal");

        float y =
            Input.GetAxisRaw("Vertical");

        Vector3 camForward =
            Camera.main.transform.forward;

        Vector3 camRight =
            Camera.main.transform.right;

        camForward.y = 0f;
        camRight.y = 0f;

        direction =
            (camForward * y +
            camRight * x).normalized;

        if (direction == Vector3.zero)
        {
            direction =
                transform.forward;
        }

        float dashSpeed =
            dashDistance /
            dashDuration;

        float timer = 0f;

        while (timer < dashDuration)
        {
            float moveDistance =
                dashSpeed *
                Time.deltaTime;

            Vector3 origin =
                rb.position +
                Vector3.up * 0.5f;

            /*
             * Kiểm tra vật cản trước khi di chuyển.
             */
            if (Physics.SphereCast(
                origin,
                dashRadius,
                direction,
                out RaycastHit hit,
                moveDistance +
                dashSkinWidth,
                obstacleLayer
            ))
            {
                Vector3 stopPosition =
                    hit.point -
                    direction *
                    dashRadius;

                rb.MovePosition(
                    stopPosition
                );

                break;
            }

            rb.MovePosition(
                rb.position +
                direction *
                moveDistance
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

    #endregion

    #region Control

    public void SetControlEnabled(bool enabled)
    {
        canControl = enabled;

        if (!enabled)
        {
            ResetAttackInput();
            SetAttackSwordTrail(false);

            isAimingAttackSpell = false;

            animator.SetBool(
                IsAttacking,
                false
            );

            animator.SetBool(
                IsMoving,
                false
            );

            if (attackSpellCaster != null)
            {
                attackSpellCaster.CancelAim();
            }
        }
    }

    public void SetCutsceneMoving(bool isMoving)
    {
        animator.SetBool(
            IsMoving,
            isMoving
        );
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmosSelected()
    {
        Vector3 attackCenter =
            transform.position +
            transform.forward;

        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(
            attackCenter,
            attackRange
        );
    }

    #endregion
}