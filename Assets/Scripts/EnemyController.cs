using UnityEngine;
using System.Collections;

public class EnemyController : MonoBehaviour
{
    [Header("Status")]
    public int maxHP = 10;
    public int currentHP;
    public int damagePerHit = 5;
    public float attackInterval = 2f;

    [Header("Movimento (mundo)")]
    public float moveSpeed = 10f;
    public float stopDistance = 1.5f;
    [Tooltip("Margem extra para evitar encostar no player")]
    public float stopBuffer = 0.2f;

    [Header("Movimento (UI)")]
    public float uiMoveSpeed = 400f;      // pixels por segundo
    public float uiStopDistance = 20f;    // pixels
    public bool faceTarget = true;
    public float waitDistance = 2.5f; // distância extra para fila de espera
    public float moveSpeedPerRound = 0.1f; // multiplicador incremental por round

    [Header("Animação via EquipmentPart")]
    public EnemyEquipment enemyEquipment;
    public EquipmentPart walkPart;
    public EquipmentPart attackPart;
    public float walkFrameRate = 0.16f;
    public float attackFrameRate = 0.12f;
    public bool adjustScalePerState = false;
    public float walkScale = 1f;
    public float attackScale = 1f;
    public EnemyHealthBar healthBar;

    private RoundManager roundManager;
    private PlayerXPClick playerXP;
    private PlayerHealth playerHealth;
    private PlayerEquipment playerEquipment;
    private int roundNumber;
    private Transform target;
    private RectTransform rectSelf;
    private RectTransform rectTarget;
    private bool useUIPositioning;
    private Canvas uiCanvas;
    private Camera uiCamera;
    private SpriteRenderer spriteRenderer;
    private bool initialFlipX;
    private bool hasAttackSlot = false;
    private bool isInRange = false;
    private AnimState currentAnimState = AnimState.None;
    private Vector3 baseScale;

    private enum AnimState
    {
        None,
        Walk,
        Attack
    }

    public void Init(RoundManager manager, PlayerXPClick xp, PlayerHealth health, int round, Transform moveTarget)
    {
        roundManager = manager;
        playerXP = xp;
        playerHealth = health;
        playerEquipment = xp != null ? xp.playerEquipment : null;
        roundNumber = round;
        target = moveTarget;
        rectSelf = GetComponent<RectTransform>();
        rectTarget = moveTarget != null ? moveTarget.GetComponent<RectTransform>() : null;
        uiCanvas = rectSelf != null ? rectSelf.GetComponentInParent<Canvas>() : null;
        uiCamera = uiCanvas != null ? uiCanvas.worldCamera : Camera.main;
        useUIPositioning = rectSelf != null && uiCanvas != null; // mover em UI se este inimigo é UI
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            initialFlipX = spriteRenderer.flipX;
        if (enemyEquipment == null)
            enemyEquipment = GetComponent<EnemyEquipment>();
        if (healthBar == null)
            healthBar = GetComponentInChildren<EnemyHealthBar>();
        baseScale = transform.localScale;

        // escalar dificuldade por round
        int r = Mathf.Max(1, round);
        maxHP = Mathf.RoundToInt(20 + (r - 1) * 8f);
        currentHP = maxHP;
        damagePerHit = Mathf.RoundToInt(3 + (r - 1) * 1.5f);
        moveSpeed *= 1f + moveSpeedPerRound * (r - 1);
        uiMoveSpeed *= 1f + moveSpeedPerRound * (r - 1);

        if (healthBar != null)
        {
            healthBar.enemy = this;
            healthBar.SetValue(currentHP, maxHP);
        }

        StartCoroutine(AttackRoutine());
    }

    void Update()
    {
        if (currentHP <= 0) return;

        if (useUIPositioning)
        {
            if (uiCanvas == null) return;
            Vector2 current = rectSelf.anchoredPosition;
            Vector2 targetPos;

            if (rectTarget != null && rectTarget.GetComponentInParent<Canvas>() == uiCanvas)
            {
                targetPos = rectTarget.anchoredPosition;
            }
            else if (target != null)
            {
                RectTransform canvasRect = uiCanvas.transform as RectTransform;
                if (canvasRect == null) return;
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCamera, target.position);
                RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, uiCamera, out targetPos);
            }
            else
            {
                return;
            }

            Vector2 dir = targetPos - current;
            float distSqr = dir.sqrMagnitude;
            float stopDistSqr = uiStopDistance * uiStopDistance;

            if (distSqr > stopDistSqr)
            {
                Vector2 desired = targetPos - dir.normalized * uiStopDistance;
                rectSelf.anchoredPosition = Vector2.MoveTowards(current, desired, uiMoveSpeed * Time.deltaTime);
                SetAnimStates(walk: true, attack: false);
            }
            else
            {
                SetAnimStates(walk: false, attack: false);
            }
        }
        else
        {
            if (target == null) return;
            Vector3 dir = target.position - transform.position;
            float dist = dir.magnitude;
            float effectiveStop = Mathf.Max(0f, stopDistance + stopBuffer);
            isInRange = dist <= effectiveStop;

            float waitStop = Mathf.Max(effectiveStop, waitDistance);

            if (dist > (hasAttackSlot ? effectiveStop : waitStop))
            {
                Vector3 desired = target.position - dir.normalized * (hasAttackSlot ? effectiveStop : waitStop);
                transform.position = Vector3.MoveTowards(transform.position, desired, moveSpeed * Time.deltaTime);
                SetAnimStates(walk: true, attack: false);
            }
            else
            {
                SetAnimStates(walk: false, attack: hasAttackSlot && isInRange);
                TryAcquireAttackSlot();
            }

            UpdateFacing(dir.x);
        }
    }

    private void UpdateFacing(float dirX)
    {
        if (!faceTarget || spriteRenderer == null) return;

        if (dirX > 0.05f)
            spriteRenderer.flipX = initialFlipX;
        else if (dirX < -0.05f)
            spriteRenderer.flipX = !initialFlipX;
    }

    IEnumerator AttackRoutine()
    {
        while (currentHP > 0 && playerHealth != null && playerHealth.currentHP > 0)
        {
            yield return new WaitForSeconds(attackInterval);
            if (currentHP <= 0 || playerHealth == null) break;

            bool inRange = true;
            if (useUIPositioning && rectSelf != null)
            {
                Vector2 current = rectSelf.anchoredPosition;
                Vector2 targetPos;

                if (rectTarget != null && rectTarget.GetComponentInParent<Canvas>() == uiCanvas)
                {
                    targetPos = rectTarget.anchoredPosition;
                }
                else if (target != null && uiCanvas != null)
                {
                    RectTransform canvasRect = uiCanvas.transform as RectTransform;
                    Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCamera, target.position);
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screen, uiCamera, out targetPos);
                }
                else
                {
                    targetPos = current;
                }

                float distSqr = (targetPos - current).sqrMagnitude;
                inRange = distSqr <= (uiStopDistance * uiStopDistance);
            }
            else if (target != null)
            {
                float effectiveStop = Mathf.Max(0f, stopDistance + stopBuffer);
                float distSqr = (target.position - transform.position).sqrMagnitude;
                inRange = distSqr <= (effectiveStop * effectiveStop);
            }

            if (inRange && EnsureAttackSlot())
            {
                playerHealth.TakeDamage(damagePerHit);
                playerEquipment?.PlayHitReaction();
            }
        }
    }

    public void TakeDamage(int amount)
    {
        currentHP -= amount;
        if (currentHP <= 0)
        {
            Die();
        }
        else
        {
            if (healthBar != null)
                healthBar.SetValue(currentHP, maxHP);
        }
    }

    private void Die()
    {
        // recompensa de XP por inimigo
        if (playerXP != null)
        {
            int reward = 5 * roundNumber;
            playerXP.xp += reward;
            playerXP.UpdateXPText();
        }

        if (roundManager != null)
            roundManager.EnemyDefeated();

        ReleaseAttackSlot();
        if (healthBar != null)
            Destroy(healthBar.gameObject);
        Destroy(gameObject);
    }

    private bool EnsureAttackSlot()
    {
        if (hasAttackSlot) return true;
        return TryAcquireAttackSlot();
    }

    private bool TryAcquireAttackSlot()
    {
        if (roundManager == null) return false;
        hasAttackSlot = roundManager.TrySetAttacker(this);
        SetAnimStates(walk: !hasAttackSlot, attack: hasAttackSlot && isInRange);
        return hasAttackSlot;
    }

    public void ReleaseAttackSlot()
    {
        if (hasAttackSlot)
        {
            hasAttackSlot = false;
            roundManager?.ReleaseAttacker(this);
            SetAnimStates(walk: false, attack: false);
        }
    }

    private void SetAnimStates(bool walk, bool attack)
    {
        AnimState desired =
            attack && attackPart != null ? AnimState.Attack :
            walk && walkPart != null ? AnimState.Walk :
            AnimState.None;

        if (desired == currentAnimState) return;
        currentAnimState = desired;

        if (enemyEquipment == null) return;

        switch (desired)
        {
            case AnimState.Attack:
                if (attackPart != null) enemyEquipment.SetPart(attackPart, attackFrameRate);
                if (adjustScalePerState) transform.localScale = baseScale * attackScale;
                break;
            case AnimState.Walk:
                if (walkPart != null) enemyEquipment.SetPart(walkPart, walkFrameRate);
                if (adjustScalePerState) transform.localScale = baseScale * walkScale;
                break;
            case AnimState.None:
                // mantém o último frame; não troca part para evitar reset
                if (adjustScalePerState) transform.localScale = baseScale;
                break;
        }
    }
}
