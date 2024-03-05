using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine;
using UnityEngine.AI;


public enum EnemyState
{
    Idle,
    Moving,
    Attack
}

[RequireComponent(typeof(NavMeshAgent))]


public class EnemyMoving : MonoBehaviour
{
    [Header("Wander")]
    public float wanderDistance = 50f; // How far the animal can Move in one go.
    public float walkSpeed = 5f;
    public float maxWalkTime = 6f;

    [Header("Idle")]
    public float idleTime = 5f; // How Long the animal takes a break for

    protected NavMeshAgent navMeshAgent;
    protected EnemyState currentState = EnemyState.Idle;

    private void Start()
    {
        InitialEnemy();
    }

    protected virtual void InitialEnemy()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.speed = walkSpeed;

        currentState = EnemyState.Idle;
        UpdateState();
    }

    protected virtual void UpdateState()
    {
        switch(currentState)
        {
            case EnemyState.Idle:
                HandlerIdleState();
                break;
            case EnemyState.Moving:
                HandlerMovingState();
                break;
        }
    }

    protected Vector3 GetRandomNavMeshPosition(Vector3 origin, float distance)
    {
        Vector3 randorDirection = Random.insideUnitSphere * distance;
        randorDirection += origin;
        NavMeshHit navMeshHit;

        if (NavMesh.SamplePosition(randorDirection, out navMeshHit, distance, NavMesh.AllAreas))
        {
            return navMeshHit.position;
        }
        else
        {
            return GetRandomNavMeshPosition(origin, distance);
        }
    }

    protected virtual void HandlerMovingState()
    {
        StartCoroutine(WaitToMove());
    }

    private IEnumerator WaitToMove()
    {
        float waitTime = Random.Range(idleTime / 2, idleTime * 2);
        yield return new WaitForSeconds(waitTime);

        Vector3 randomDistantion = GetRandomNavMeshPosition(transform.position, wanderDistance);

        navMeshAgent.SetDestination(randomDistantion);
        //SetState(EnemyState.Moving);
    }

    protected virtual void HandlerIdleState()
    {
        StartCoroutine(WaitReachDistination());
    }

    private IEnumerator WaitReachDistination()
    {
        float startTime = Time.time;

        while(navMeshAgent.remainingDistance > navMeshAgent.stoppingDistance)
        {
            if(Time.time - startTime >= maxWalkTime)
            {
                navMeshAgent.ResetPath();
                 SetState(EnemyState.Idle);
                yield break;
            }

            yield return null;
        }

        //Destination has been reached
        SetState(EnemyState.Idle);
    }

    protected void SetState(EnemyState newState)
    {
        if(currentState == newState)
        {
            return;
        }

        currentState = newState;
        OnStateChanged(newState);
    }

    protected virtual void OnStateChanged(EnemyState newState)
    {
        UpdateState();
    }
}
