using UnityEngine;
using UnityEngine.UI;
using System.Collections;



public class MonsterAI : MonoBehaviour
{
    [SerializeField] private EnemySpawner Enemy;
    public GameObject TextMassage;
    public Transform player;

    [Header("Attack Colliders")]
    public GameObject attackCollider;
    public Collider2D attackColliderComponent;

    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float attackRange = 1.5f;
    public float slowDownRange = 5f;

    [Header("Combat Settings")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private Slider healthSlider;
    [SerializeField] private Vector3 sliderOffset = new Vector3(0, 0.5f, 0);

    private Animator animator;
    private EnemyPathfinding enemyPathfinding;
    private EnemySpawner spawner;
    private int currentHealth;
    private bool playerCanAttack = true;
    private bool isAttacking = false;
    private bool canAttack = true;
    private bool isKilled = false;
    private float messageDuration = 1.5f;
    private float lastAttackTime = -Mathf.Infinity;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        spawner = FindObjectOfType<EnemySpawner>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        if (attackColliderComponent != null)
        {
            attackColliderComponent.enabled = false;
        }

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogWarning("Player reference not set in " + gameObject.name);
                enabled = false;
                return;
            }
        }

        // Kiểm tra waves completed khi spawn
    }
 

    private void Update()
    {
        // Monster chỉ di chuyển và tấn công khi playerCanAttack = true
        if (!playerCanAttack || player == null || isKilled) return;

        Vector2 direction = (player.position - transform.position);
        float distanceToPlayer = direction.magnitude;
        Vector2 normalizedDirection = direction.normalized;

        if (distanceToPlayer <= attackRange)
        {
            HandleAttack();
        }
        else
        {
            HandleMovement(normalizedDirection, distanceToPlayer);
        }

        UpdateHealthBarPosition();
    }

    private void HandleAttack()
    {
        animator?.SetBool("isAttacking", true);
        animator?.SetFloat("Speed", 0f);
    }

    private void HandleMovement(Vector2 normalizedDirection, float distanceToPlayer)
    {
        animator?.SetBool("isAttacking", false);
        MoveTowardsPlayer(normalizedDirection, distanceToPlayer);
    }

    private void MoveTowardsPlayer(Vector2 normalizedDirection, float distanceToPlayer)
    {
        float speed = CalculateSpeed(distanceToPlayer);
        UpdateFacing(normalizedDirection);
        UpdatePosition(normalizedDirection, speed);
        UpdateAnimationSpeed(speed);
    }

    private float CalculateSpeed(float distanceToPlayer)
    {
        float speed = moveSpeed;
        if (distanceToPlayer <= slowDownRange)
        {
            speed *= (distanceToPlayer - attackRange) / (slowDownRange - attackRange);
            animator?.SetBool("isAttacking", true);
            Invoke(nameof(EnableAttackCollider), 0.04f);
        }
        else
        {
            DisableAttackCollider();
        }
        return speed;
    }

    private void UpdateFacing(Vector2 normalizedDirection)
    {
        if (normalizedDirection.x != 0)
        {
            transform.localScale = new Vector3(normalizedDirection.x > 0 ? -1 : 1, 1, 1);
        }
    }

    private void UpdatePosition(Vector2 normalizedDirection, float speed)
    {
        transform.position += (Vector3)(normalizedDirection * speed * Time.deltaTime);
    }

    private void UpdateAnimationSpeed(float speed)
    {
        animator?.SetFloat("Speed", speed);
    }

    private void UpdateHealthBarPosition()
    {
        if (healthSlider != null)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + sliderOffset);
            healthSlider.transform.position = screenPosition;
        }
    }

    private void EnableAttackCollider()
    {
        if (attackColliderComponent != null && !isKilled)
        {
            attackColliderComponent.enabled = true;
        }
    }

    private void DisableAttackCollider()
    {
        if (attackColliderComponent != null)
        {
            attackColliderComponent.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Monster không nhận damage khi playerCanAttack = false
        if (isKilled || !playerCanAttack) return;

        switch (collision.tag)
        {
            case "Sword":
                TakeDamage(2);
                animator.SetBool("isDamged", true);
                Invoke(nameof(ResetDamageAnimation), 0.5f);
                break;
            case "Skill Q":
            case "Skill E":
                TakeDamage(5);
                animator.SetBool("isDamged", true);
                Invoke(nameof(ResetDamageAnimation), 0.5f);
                break;
        }
    }

    private void ResetDamageAnimation()
    {
        animator.SetBool("isDamged", false);
    }

    private void TakeDamage(int damage)
    {
        if (isKilled) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        isKilled = true;
        animator.SetBool("isDie", true);
        StopAllCoroutines();

        if (enemyPathfinding != null)
        {
            enemyPathfinding.StopMoving();
        }

        var spawner = FindObjectOfType<EnemySpawner>();
        if (spawner != null)
        {
            spawner.OnEnemyKilled();
        }

        Destroy(gameObject, 1f);
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    public void AllowPlayerToAttack()
    {
        playerCanAttack = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Hiện thông báo khi chạm vào player và playerCanAttack = false
        if (collision.gameObject.CompareTag("Player") && !playerCanAttack)
        {
            ShowTextMessage();
        }
    }

    private void ShowTextMessage()
    {
        if (TextMassage != null)
        {
            TextMassage.SetActive(true);
            Invoke(nameof(HideTextMessage), messageDuration);
        }
    }

    private void HideTextMessage()
    {
        if (TextMassage != null)
        {
            TextMassage.SetActive(false);
        }
    }
}