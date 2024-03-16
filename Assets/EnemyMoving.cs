using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Moving,
    Pursuing,
    Attack
}

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMoving : MonoBehaviour
{
    public float wanderDistance = 50f;
    public float walkSpeed = 5f;
    public float pursueSpeed = 8f; // Скорость преследования
    public float maxWalkTime = 6f;
    public float idleTime = 5f;
    public float pursueDistance = 10f; // Дистанция для начала преследования
    public float attackDistance = 2f; // Дистанция для начала атаки

    protected NavMeshAgent navMeshAgent;
    protected EnemyState currentState = EnemyState.Idle;

    Animator animator;
    bool isAttacking = false;

    private void Start()
    {
        InitialEnemy();
    }

    protected virtual void InitialEnemy()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = walkSpeed;
        animator = GetComponent<Animator>();

        StartCoroutine(EnemyStateMachine());
    }

    protected virtual IEnumerator EnemyStateMachine()
    {
        while (true)
        {
            switch (currentState)
            {
                case EnemyState.Idle:
                    yield return StartCoroutine(HandlerIdleState());
                    break;
                case EnemyState.Moving:
                    yield return StartCoroutine(HandlerMovingState());
                    break;
                case EnemyState.Pursuing:
                    yield return StartCoroutine(HandlerPursuingState());
                    break;
                case EnemyState.Attack:
                    yield return StartCoroutine(HandlerAttackState());
                    break;
            }
        }
    }

    protected Vector3 GetRandomNavMeshPosition(Vector3 origin, float distance)
    {
        Vector3 randomDirection = Random.insideUnitSphere * distance;
        randomDirection += origin;
        NavMeshHit navMeshHit;

        if (NavMesh.SamplePosition(randomDirection, out navMeshHit, distance, NavMesh.AllAreas))
        {
            return navMeshHit.position;
        }
        else
        {
            return GetRandomNavMeshPosition(origin, distance);
        }
    }

    protected virtual IEnumerator HandlerIdleState()
    {
        yield return new WaitForSeconds(idleTime);

        if (PlayerInRange())
        {
            SetState(EnemyState.Pursuing);
        }
        else
        {
            SetState(EnemyState.Moving);
            PlayAnimation("Walk");
        }
    }

    protected bool IsDestinationReached()
    {
        return !navMeshAgent.pathPending && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance;
    }

    protected virtual IEnumerator HandlerMovingState()
    {
        Vector3 randomDestination = GetRandomNavMeshPosition(transform.position, wanderDistance);

        // Output debug information
        Debug.Log("Moving to destination: " + randomDestination);

        // Поворачиваем персонаж в сторону цели
        transform.LookAt(randomDestination);

        navMeshAgent.SetDestination(randomDestination);
        yield return new WaitUntil(() => IsDestinationReached()); // Ждем, пока персонаж не достигнет цели

        // Персонаж достиг цели, включаем анимацию покоя
        PlayAnimation("Idle");
        SetState(EnemyState.Idle);
    }

    protected virtual IEnumerator HandlerPursuingState()
    {
        if (!PlayerInRange())
        {
            SetState(EnemyState.Idle);
            yield break;
        }

        navMeshAgent.speed = pursueSpeed;
        Vector3 playerPosition = GetPlayerPosition();

        // Output debug information
        Debug.Log("Pursuing player to position: " + playerPosition);

        // Поворачиваем персонаж в сторону игрока
        transform.LookAt(playerPosition);

        navMeshAgent.SetDestination(playerPosition);
        yield return new WaitUntil(() => IsDestinationReached()); // Ждем, пока персонаж не достигнет игрока

        if (PlayerInRange(attackDistance))
        {
            SetState(EnemyState.Attack);
        }
        else
        {
            SetState(EnemyState.Pursuing);
        }
    }

    protected virtual IEnumerator HandlerAttackState()
    {
        // Атакуем бесконечно, пока игрок находится в зоне атаки
        while (PlayerInRange(attackDistance))
        {
            // Начинаем атаку, если не в процессе атаки
            if (!isAttacking)
            {
                isAttacking = true;
                PlayAnimation("Attack");

                // Ждем завершения анимации атаки
                yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

                isAttacking = false;
            }

            // Короткая пауза перед следующей атакой
            yield return new WaitForSeconds(0.1f);
        }

        // Игрок вышел из зоны атаки, переключаемся в режим преследования
        SetState(EnemyState.Pursuing);
    }

    private void PlayAnimation(string animationName)
    {
        if (animator != null)
        {
            animator.Play(animationName);
        }
    }

    protected void SetState(EnemyState newState)
    {
        if (currentState == newState)
        {
            return;
        }

        currentState = newState;
    }

    protected bool PlayerInRange()
    {
        return Vector3.Distance(transform.position, GetPlayerPosition()) <= pursueDistance;
    }

    protected bool PlayerInRange(float range)
    {
        return Vector3.Distance(transform.position, GetPlayerPosition()) <= range;
    }

    protected Vector3 GetPlayerPosition()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
        {
            return playerObject.transform.position;
        }
        else
        {
            Debug.LogError("Player not found! Make sure the player has the 'Player' tag.");
            return Vector3.zero; // Возвращаем позицию (0, 0, 0) в случае ошибки
        }
    }
}
