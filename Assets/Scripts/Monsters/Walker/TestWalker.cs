using UnityEngine;
using UnityEngine.AI;

public class TestWalker : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float fieldOfViewAngle = 45f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private NavMeshAgent _navMeshAgent;
    private float stoppingDistance = 1.5f;
    private float rotationSpeed = 5f;
    private float closeRange = 3f;
    private float attackRange = 1f;
    private float attackCooldown = 2f;

    private Transform targetPlayer;
    private Animator _animator;
    private bool isChasing = false;
    private bool isRunning = false;
    private bool isAttacking = false;
    private float lastAttackTime = -Mathf.Infinity;

    private void Awake()
    {
        _navMeshAgent.speed = moveSpeed;
        _navMeshAgent.stoppingDistance = stoppingDistance;
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        DetectPlayer();
        bool isLookingAround = _animator.GetCurrentAnimatorStateInfo(0).IsName("LookingAround");

        if (isLookingAround)
        {
            _navMeshAgent.isStopped = true;
            return;
        }
        if (isChasing && targetPlayer != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

            if (distanceToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
            else
            {
                isAttacking = false;
                
                if (_navMeshAgent.isStopped) 
                {
                    _navMeshAgent.isStopped = false; // Ensure movement resumes
                }

                if (distanceToPlayer <= closeRange)
                {
                    FacePlayerInstantly();
                }
                else
                {
                    RotateTowardsPlayer();
                }

                _navMeshAgent.SetDestination(targetPlayer.position);

                if (_navMeshAgent.velocity.magnitude > 0.1f) // Check if actually moving
                {
                    if (!isRunning)
                    {
                        _animator.SetBool("Running", true);
                        isRunning = true;
                    }
                }
                else
                {
                    if (isRunning)
                    {
                        _animator.SetBool("Running", false);
                        isRunning = false;
                    }
                }
            }
        }
        else
        {
            if (isRunning)
            {
                _animator.SetBool("Running", false);
                isRunning = false;
            }
        }
    }

    private void DetectPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

        foreach (var hit in hits)
        {
            Vector3 directionToPlayer = (hit.transform.position - transform.position).normalized;
            float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);
            float distanceToPlayer = Vector3.Distance(transform.position, hit.transform.position);

            if (angleToPlayer < fieldOfViewAngle && distanceToPlayer <= detectionRadius)
            {
                targetPlayer = hit.transform;
                isChasing = true;
                _animator.SetBool("Running", true);
                return;
            }
            else
            {
                _animator.SetBool("Running", false);
                _animator.Play("LookingAround");
                return;
            }
        }

        isChasing = false;
        targetPlayer = null;
    }

    private void RotateTowardsPlayer()
    {
        if (targetPlayer == null) return;

        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        direction.y = 0;

        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
    }

    private void FacePlayerInstantly()
    {
        if (targetPlayer == null) return;

        Vector3 direction = (targetPlayer.position - transform.position).normalized;
        direction.y = 0;

        transform.rotation = Quaternion.LookRotation(direction);
    }

    private void AttackPlayer()
    {
        if (!isAttacking)
        {
            isAttacking = true;
            _navMeshAgent.isStopped = true; 
            _animator.SetBool("Running", false);
            _animator.SetTrigger("Attack");

            lastAttackTime = Time.time; // Track last attack
        }
    }
}