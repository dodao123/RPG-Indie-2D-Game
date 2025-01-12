using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    private enum State
    {
        Roaming,
        Chasing
    }

    [SerializeField] private float roamingRadius = 5f;
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private int maxHealth = 10; // Máu tối đa
    private int currentHealth; // Máu hiện tại

    [SerializeField] private Slider healthSlider; // Tham chiếu đến Slider
    [SerializeField] private Vector3 sliderOffset = new Vector3(0, 0.5f, 0); // Để thanh trượt ở trên đầu quái vật

    private State state;
    private EnemyPathfinding enemyPathfinding;
    private Vector2 startingPosition;
    private Animator anim;
    private Transform player;

    private bool isKilled = false;

    private void Awake()
    {
        enemyPathfinding = GetComponent<EnemyPathfinding>();
        anim = GetComponent<Animator>();
        state = State.Roaming;
        startingPosition = transform.position;

        player = GameObject.FindWithTag("Player").transform;

        if (anim == null)
        {
            Debug.LogError("Animator component is missing from the game object.");
        }

        // Kiểm tra nếu thanh máu chưa được gán
        if (healthSlider == null)
        {
            Debug.LogError("Health Slider is not assigned in the Inspector!");
        }
    }

    private void Start()
    {
        currentHealth = maxHealth; // Đặt máu ban đầu
        UpdateHealthUI(); // Cập nhật UI máu

        if (!isKilled)
        {
            StartCoroutine(RoamingRoutine());
        }
    }

    private void Update()
    {
        // Cập nhật vị trí của thanh trượt để nó theo quái vật
        if (healthSlider != null)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + sliderOffset); // Cập nhật lại vị trí thanh máu
            healthSlider.transform.position = screenPosition;
        }

        // Kiểm tra xem quái vật có gần người chơi hay không
        if (Vector2.Distance(transform.position, player.position) < detectionRadius)
        {
            state = State.Chasing;
            enemyPathfinding.MoveTo(player.position);
        }
    }

    private IEnumerator RoamingRoutine()
    {
        while (state == State.Roaming && !isKilled)
        {
            Vector2 roamPosition = GetRoamingPosition();
            enemyPathfinding.MoveTo(roamPosition);
            yield return new WaitForSeconds(2f); // Dừng 2 giây trước khi tiếp tục
        }
    }

    private Vector2 GetRoamingPosition()
    {
        Vector2 randomDirection = Random.insideUnitCircle * roamingRadius;
        return startingPosition + randomDirection;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isKilled && collision.CompareTag("Sword"))
        {
            TakeDamage(2); // Giảm máu khi bị chạm
        }
        if (!isKilled && collision.CompareTag("Skill Q"))
        {
            TakeDamage(5); // Giảm máu khi bị chạm
        }
        if (!isKilled && collision.CompareTag("Skill E"))
        {
            TakeDamage(5); // Giảm máu khi bị chạm
        }
    }

    private void TakeDamage(int damage)
    {
        currentHealth -= damage; // Giảm máu
        UpdateHealthUI(); // Cập nhật UI máu

        if (currentHealth <= 0)
        {
            isKilled = true;
            anim.SetBool("isKill", true);
            StopAllCoroutines();
            enemyPathfinding.StopMoving();

            Destroy(gameObject, 0.5f);
            FindObjectOfType<EnemySpawner>().OnEnemyKilled();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth; // Đặt giá trị tối đa
            healthSlider.value = currentHealth; // Cập nhật giá trị hiện tại
        }
    }
}
