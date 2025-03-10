using UnityEngine;
using UnityEngine.AI;

public class Walker : MonoBehaviour
{
    [SerializeField] private float detectionRadius = 10f;
    [SerializeField] private float fieldOfViewAngle = 45f;
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private NavMeshAgent _navMeshAgent;
    [SerializeField] private Transform headBone;
    [SerializeField] private Transform armTransform;
    [SerializeField] private float attackConeAngle = 30f;
    [SerializeField] private float armRayLength = 1f;
    [SerializeField] private int attackDamage = 25;
    private float stoppingDistance = 1.5f;
    private float rotationSpeed = 5f;
    private float closeRange = 3f;
    private float attackRange = 1f;
    private float attackCooldown = 1f;

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
        //Vector3 origin = armTransform.position;
        //Vector3 direction = -armTransform.forward; 
        //Debug.DrawRay(origin, direction * armRayLength, Color.red);
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
            lastAttackTime = Time.time; 
        }
    }

    public void TriggerDamage()
    {
        
        //Debug.Log("Animation event triggered: TriggerDamage"); 
        Vector3 origin = transform.position;
        Vector3 attackDirection = (targetPlayer.position - origin).normalized;  
        //float angleToTarget = Vector3.Angle(attackDirection, armTransform.forward);

        Collider[] hits = Physics.OverlapSphere(origin, armRayLength, playerLayer);

        foreach (var hit in hits)
        {
            Vector3 directionToTarget = (hit.transform.position - origin).normalized;
            float angleToTarget = Vector3.Angle(transform.forward, directionToTarget);
            float distanceToTarget = Vector3.Distance(origin, hit.transform.position);

            
            Debug.Log(angleToTarget);
            if (angleToTarget <= attackConeAngle * 0.5f && distanceToTarget <= armRayLength )
            {
                DealDamage(); 
                break; 
            }
        }
    }

    private void DealDamage()
    {
        Debug.Log("DealDamage");
        PlayerHealth.Instance.ReceiveDamage(attackDamage);
    }
    private void OnDrawGizmos()
    {
        if (headBone == null) return;

        //Gizmos.color = Color.yellow;
        //Gizmos.DrawWireSphere(headBone.position, detectionRadius);

        Vector3 forward = headBone.forward * detectionRadius;
        Quaternion leftRayRotation = Quaternion.Euler(0, -fieldOfViewAngle, 0);
        Quaternion rightRayRotation = Quaternion.Euler(0, fieldOfViewAngle, 0);

        Vector3 leftRayDirection = leftRayRotation * forward;
        Vector3 rightRayDirection = rightRayRotation * forward;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(headBone.position, headBone.position + leftRayDirection);
        Gizmos.DrawLine(headBone.position, headBone.position + rightRayDirection);

        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.DrawWireArc(headBone.position, Vector3.up, leftRayDirection.normalized, fieldOfViewAngle * 2, detectionRadius);
        
        if (armTransform != null)
        {
            Vector3 attackForward = armTransform.forward * armRayLength;
            Quaternion leftAttackRotation = Quaternion.Euler(0, -attackConeAngle, 0);
            Quaternion rightAttackRotation = Quaternion.Euler(0, attackConeAngle, 0);

            Vector3 leftAttackDirection = leftAttackRotation * attackForward;
            Vector3 rightAttackDirection = rightAttackRotation * attackForward;

            Gizmos.color = Color.blue; // Color for the attack cone
            Gizmos.DrawLine(armTransform.position, armTransform.position + leftAttackDirection);
            Gizmos.DrawLine(armTransform.position, armTransform.position + rightAttackDirection);

            UnityEditor.Handles.color = Color.blue;
            UnityEditor.Handles.DrawWireArc(armTransform.position, Vector3.up, leftAttackDirection.normalized, attackConeAngle * 2, armRayLength);
        }
    }
}
