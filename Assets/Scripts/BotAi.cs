using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(PlayerFearEffect))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class HumanLikeCompanionAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public Transform player;
    public float minComfortDistance = 1.8f;
    public float maxComfortDistance = 4f;
    public float leadThreshold = 6f;
    public float walkSpeed = 2f;
    public float jogSpeed = 3.8f;
    public float runSpeed = 5.5f;
    public float rotationSpeed = 12f;
    public float pathVarianceAngle = 25f;

    [Header("Behavior Settings")]
    public float reactionTime = 0.3f;
    public float decisionInterval = 0.5f;
    public float safetyCheckInterval = 5f;
    public float calloutRange = 12f;

    [Header("Personality Traits")]
    [Range(0, 1)] public float bravery = 0.6f;
    [Range(0, 1)] public float curiosity = 0.5f;
    [Range(0, 1)] public float energy = 0.7f;

    // Private components
    private Animator animator;
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private PlayerFearEffect fearEffect;

    // Movement state
    private Vector3 currentTarget;
    private Vector3 lastPlayerForward;
    private float nextDecisionTime;
    private bool isLeading;
    private float currentSideOffset;
    private float pathVarianceTimer;

    // Behavior state
    private CompanionBehavior currentBehavior;
    private float behaviorTimer;
    private float nextSafetyCheck;
    private List<Vector3> recentPlayerPositions = new List<Vector3>();

    // Animation hashes
    private readonly int IsWalkingHash = Animator.StringToHash("isWalking");
    private readonly int IsRunningHash = Animator.StringToHash("isRunning");
    private readonly int CallOutHash = Animator.StringToHash("CallOut");
    private readonly int LookAroundHash = Animator.StringToHash("LookAround");
    private readonly int StumbleHash = Animator.StringToHash("Stumble");

    private enum CompanionBehavior
    {
        Following,
        Leading,
        Waiting,
        Investigating,
        Alerted
    }

    void Start()
    {
        InitializeComponents();
        GeneratePersonality();
        SetInitialBehavior();
    }

    void InitializeComponents()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        fearEffect = GetComponent<PlayerFearEffect>();

        agent.speed = walkSpeed;
        agent.angularSpeed = rotationSpeed;
        agent.autoBraking = true;
        agent.stoppingDistance = minComfortDistance;
    }

    void GeneratePersonality()
    {
        bravery = Random.Range(0.4f, 0.8f);
        curiosity = Random.Range(0.3f, 0.7f);
        energy = Random.Range(0.5f, 0.9f);

        // Adjust speeds based on energy
        walkSpeed *= Mathf.Lerp(0.8f, 1.2f, energy);
        jogSpeed *= Mathf.Lerp(0.9f, 1.3f, energy);
        runSpeed *= Mathf.Lerp(1f, 1.5f, energy);

        currentSideOffset = Random.Range(-1f, 1f);
    }

    void SetInitialBehavior()
    {
        currentBehavior = CompanionBehavior.Following;
        behaviorTimer = Random.Range(5f, 15f);
    }

    void Update()
    {
        UpdatePlayerTracking();
        UpdateBehavior();
        UpdateMovement();
        UpdateAnimations();
        CheckEnvironment();
    }

    void UpdatePlayerTracking()
    {
        // Track recent player positions for prediction
        recentPlayerPositions.Add(player.position);
        if (recentPlayerPositions.Count > 10)
        {
            recentPlayerPositions.RemoveAt(0);
        }
    }

    void UpdateBehavior()
    {
        behaviorTimer -= Time.deltaTime;
        if (behaviorTimer <= 0)
        {
            DecideNextBehavior();
            behaviorTimer = Random.Range(5f, 15f);
        }
    }

    void DecideNextBehavior()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 playerMovementDirection = (player.position - recentPlayerPositions[0]).normalized;

        // Check if we should lead
        if (distanceToPlayer > leadThreshold &&
            Vector3.Dot(player.forward, playerMovementDirection) > 0.7f &&
            Random.value < bravery)
        {
            currentBehavior = CompanionBehavior.Leading;
            return;
        }

        // Check if we should investigate something
        if (Random.value < curiosity * 0.3f)
        {
            currentBehavior = CompanionBehavior.Investigating;
            return;
        }

        // Default to following
        currentBehavior = CompanionBehavior.Following;
    }

    void UpdateMovement()
    {
        if (Time.time < nextDecisionTime) return;
        nextDecisionTime = Time.time + decisionInterval;

        switch (currentBehavior)
        {
            case CompanionBehavior.Following:
                FollowPlayer();
                break;
            case CompanionBehavior.Leading:
                LeadPlayer();
                break;
            case CompanionBehavior.Waiting:
                WaitForPlayer();
                break;
            case CompanionBehavior.Investigating:
                InvestigateArea();
                break;
            case CompanionBehavior.Alerted:
                ReactToThreat();
                break;
        }

        ApplyMovementVariance();
        AvoidBackwardMovement();
    }

    void FollowPlayer()
    {
        // Calculate position slightly behind and to the side
        Vector3 followOffset = -player.forward * Mathf.Lerp(minComfortDistance, maxComfortDistance, 0.7f) +
                             player.right * currentSideOffset;

        currentTarget = player.position + followOffset;
        agent.speed = walkSpeed;
        agent.SetDestination(currentTarget);
    }

    void LeadPlayer()
    {
        // Move ahead and to the side of player
        Vector3 leadOffset = player.forward * Mathf.Lerp(leadThreshold, leadThreshold * 1.5f, bravery) +
                           player.right * currentSideOffset * 1.5f;

        currentTarget = player.position + leadOffset;
        agent.speed = jogSpeed;
        agent.SetDestination(currentTarget);

        // Occasionally check if player is following
        if (Random.value < 0.1f)
        {
            animator.SetTrigger(LookAroundHash);
        }
    }

    void WaitForPlayer()
    {
        agent.isStopped = true;
        animator.SetTrigger(LookAroundHash);
    }

    void InvestigateArea()
    {
        // Find a random point within a cone in front of the player
        Vector3 investigationPoint = player.position +
                                   Quaternion.Euler(0, Random.Range(-45f, 45f), 0) *
                                   player.forward * Random.Range(3f, 8f);

        if (NavMesh.SamplePosition(investigationPoint, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            currentTarget = hit.position;
            agent.speed = walkSpeed;
            agent.SetDestination(currentTarget);
        }
    }

    void ReactToThreat()
    {
        // Implement threat reaction logic
    }

    void ApplyMovementVariance()
    {
        // Occasionally vary the path
        pathVarianceTimer += Time.deltaTime;
        if (pathVarianceTimer > Random.Range(2f, 5f))
        {
            pathVarianceTimer = 0;
            currentSideOffset = Mathf.Clamp(currentSideOffset + Random.Range(-0.5f, 0.5f), -1f, 1f);

            if (NavMesh.SamplePosition(
                currentTarget + Random.insideUnitSphere * 2f,
                out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                currentTarget = hit.position;
            }
        }
    }

    void AvoidBackwardMovement()
    {
        Vector3 toTarget = currentTarget - transform.position;
        float angleToTarget = Vector3.Angle(transform.forward, toTarget);

        // If target is behind us, adjust position to force forward movement
        if (angleToTarget > 100f)
        {
            Vector3 adjustedTarget = transform.position + transform.forward * 2f +
                                   transform.right * (Random.value > 0.5f ? 1f : -1f);

            if (NavMesh.SamplePosition(adjustedTarget, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                currentTarget = hit.position;
                agent.SetDestination(currentTarget);
            }
        }
    }

    void UpdateAnimations()
    {
        bool shouldWalk = agent.velocity.magnitude > 0.1f && agent.velocity.magnitude < jogSpeed * 0.8f;
        bool shouldRun = agent.velocity.magnitude >= jogSpeed * 0.8f;

        animator.SetBool(IsWalkingHash, shouldWalk);
        animator.SetBool(IsRunningHash, shouldRun);

        // Match animation speed to movement speed
        float speedRatio = agent.velocity.magnitude / (shouldRun ? runSpeed : jogSpeed);
        animator.speed = Mathf.Lerp(0.9f, 1.1f, speedRatio);
    }

    void CheckEnvironment()
    {
        if (Time.time < nextSafetyCheck) return;
        nextSafetyCheck = Time.time + safetyCheckInterval;

        // Check for threats
        Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, calloutRange);
        foreach (var collider in nearbyColliders)
        {
            if (collider.CompareTag("Ghost"))
            {
                OnThreatDetected(collider.transform);
                break;
            }
        }

        // Random environmental checks
        if (Random.value < curiosity * 0.2f)
        {
            animator.SetTrigger(LookAroundHash);
        }
    }

    void OnThreatDetected(Transform threat)
    {
        if (Random.value < bravery)
        {
            // Brave reaction - call out and face threat
            animator.SetTrigger(CallOutHash);
            FaceTarget(threat.position);
        }
        else
        {
            // Scared reaction
            currentBehavior = CompanionBehavior.Alerted;
            behaviorTimer = Random.Range(3f, 8f);
        }
    }

    void FaceTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation,
                rotationSpeed * Time.deltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(currentTarget, 0.3f);
        Gizmos.DrawLine(transform.position, currentTarget);

        if (currentBehavior == CompanionBehavior.Leading)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, 0.5f);
        }
    }
}