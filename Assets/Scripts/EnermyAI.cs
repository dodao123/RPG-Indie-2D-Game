using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float roamingRadius = 5f;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float attackRadius = 2f;

    [Header("Attack Settings")]
    [SerializeField] private float attackSpeed = 4f;
    [SerializeField] private float attackDuration = 0.3f;
    [SerializeField] private float attackCooldown = 1f;

    [Header("Combat Settings")]
    [SerializeField] private int maxHealth = 10;
    [SerializeField] private float knockbackForce = 8f;
    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField] private float stunDuration = 0.2f;

    [Header("UI")]
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Vector3 healthBarOffset = new Vector3(0, 0.5f, 0);

    // Component references
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private EnemyPathfinding pathfinding;
    private Transform player;

    // State tracking
    private Vector2 startPosition;
    private Vector2 moveDirection;
    private int currentHealth;
    private bool isAttacking;
    private bool isStunned;
    private bool isDead;
    private bool canAttack = true;

    private void Awake()
    {
        // Get all required components
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        pathfinding = GetComponent<EnemyPathfinding>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Set up Rigidbody2D
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        // Initialize state
        startPosition = transform.position;
        currentHealth = maxHealth;
    }

    private void Start()
    {
        if (!healthSlider)
        {
            Debug.LogWarning($"Health slider not assigned to {gameObject.name}");
            return;
        }

        UpdateHealthUI();
        StartCoroutine(RoamingRoutine());
    }

    private void Update()
    {
        if (isDead) return;

        UpdateHealthBarPosition();
        HandleEnemyBehavior();
    }

    private void HandleEnemyBehavior()
    {
        if (isStunned || isAttacking) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        // Update facing direction
        FlipSprite(player.position.x - transform.position.x);

        // Handle behavior based on distance to player
        if (distanceToPlayer <= attackRadius && canAttack)
        {
            StartCoroutine(PerformAttack());
        }
        else if (distanceToPlayer <= detectionRadius)
        {
            ChasePlayer();
        }
    }

    private void ChasePlayer()
    {
        if (isStunned || isAttacking) return;

        moveDirection = (player.position - transform.position).normalized;
        pathfinding.MoveTo(player.position);

        // Move towards player using Rigidbody2D
        rb.velocity = moveDirection * moveSpeed;
    }

    private IEnumerator RoamingRoutine()
    {
        while (!isDead)
        {
            if (!isStunned && !isAttacking && Vector2.Distance(transform.position, player.position) > detectionRadius)
            {
                Vector2 roamPosition = startPosition + Random.insideUnitCircle * roamingRadius;
                pathfinding.MoveTo(roamPosition);
                yield return new WaitForSeconds(2f);
            }
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator PerformAttack()
    {
        isAttacking = true;
        canAttack = false;
        animator.SetBool("isAttack", true);

        // Store original position and calculate target position
        Vector2 startPos = transform.position;
        Vector2 targetPos = (Vector2)player.position;
        Vector2 attackDirection = (targetPos - startPos).normalized;

        // Perform attack dash
        float elapsedTime = 0;
        while (elapsedTime < attackDuration)
        {
            if (isDead) yield break;

            rb.velocity = attackDirection * attackSpeed;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset after attack
        rb.velocity = Vector2.zero;
        animator.SetBool("isAttack", false);
        isAttacking = false;

        // Attack cooldown
        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    public void TakeDamage(int damage, Vector2 attackerPosition)
    {
        if (isDead) return;

        // Apply damage
        currentHealth -= damage;
        UpdateHealthUI();

        // Handle stun and knockback
        StartCoroutine(HandleDamageEffects(attackerPosition));

        // Check for death
        if (currentHealth <= 0)
        {
            // Thay vì StartCoroutine(Die()), ta gọi Die() trực tiếp
            Die();
        }
    }

    private IEnumerator HandleDamageEffects(Vector2 attackerPosition)
    {
        // Set states
        isStunned = true;
        isAttacking = false;
        canAttack = false;

        // Stop current actions
        StopCoroutine(PerformAttack());
        pathfinding.StopMoving();
        animator.SetBool("isAttack", false);
        animator.SetBool("isAttacked", true);

        // Calculate knockback direction
        Vector2 knockbackDirection = ((Vector2)transform.position - attackerPosition).normalized;

        // Apply knockback
        float elapsedTime = 0;
        while (elapsedTime < knockbackDuration)
        {
            rb.velocity = knockbackDirection * knockbackForce;
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset states after knockback
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(stunDuration);

        // Reset all states if not dead
        if (!isDead)
        {
            isStunned = false;
            canAttack = true;
            animator.SetBool("isAttacked", false);
        }
    }

    private void Die()
    {
        isDead = true;
        animator.SetBool("isKill", true);

        // Stop all movement and actions
        StopAllCoroutines();
        pathfinding.StopMoving();
        rb.velocity = Vector2.zero;

        // Notify spawner
        var spawner = FindObjectOfType<EnemySpawner>();
        if (spawner) spawner.OnEnemyKilled();

        // Sử dụng Invoke để delay việc destroy object
        Invoke("DestroyEnemy", 0.5f);
    }

    private void DestroyEnemy()
    {
        Destroy(gameObject);
    }

    private void UpdateHealthUI()
    {
        if (healthSlider)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    private void UpdateHealthBarPosition()
    {
        if (healthSlider)
        {
            healthSlider.transform.position = Camera.main.WorldToScreenPoint(transform.position + healthBarOffset);
        }
    }

    private void FlipSprite(float directionX)
    {
        if (directionX != 0)
        {
            spriteRenderer.flipX = directionX < 0;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

        if (collision.CompareTag("Sword"))
        {
            TakeDamage(2, collision.transform.position);
        }
        else if (collision.CompareTag("Skill Q") || collision.CompareTag("Skill E"))
        {
            TakeDamage(5, collision.transform.position);
        }
    }
}