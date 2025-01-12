using UnityEngine;
using UnityEngine.AI;

public class EnemyPathfinding : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private bool isMoving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void MoveTo(Vector2 position)
    {
        targetPosition = position;
        isMoving = true;
    }

    private void FixedUpdate()
    {
        if (isMoving)
        {
            Vector2 currentPosition = rb.position;
            Vector2 moveDirection = (targetPosition - currentPosition).normalized;

            rb.MovePosition(currentPosition + moveDirection * moveSpeed * Time.fixedDeltaTime);

            // Check if we've reached the target position
            if (Vector2.Distance(currentPosition, targetPosition) < 0.1f)
            {
                isMoving = false;
            }
        }
    }
    public void StopMoving()
    {
        isMoving = false;
        // Dừng tất cả các hành động di chuyển
        // Ví dụ: Dừng NavMeshAgent nếu bạn đang sử dụng
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true; // Dừng NavMeshAgent
        }
    }
}