using UnityEngine;
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
    }

    private void Start()
    {
        if (!isKilled)
        {
            StartCoroutine(RoamingRoutine());
        }
    }

    private void Update()
    {
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
            yield return new WaitForSeconds(2f);
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
            isKilled = true;
            anim.SetBool("isKill", true);
            StopAllCoroutines();
            enemyPathfinding.StopMoving();

            Destroy(gameObject, 0.5f);
            FindObjectOfType<EnemySpawner>().OnEnemyKilled();
        }
    }
}
