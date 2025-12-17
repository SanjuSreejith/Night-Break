using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent), typeof(Animator))]
public class Ghost2AI_Fixed : MonoBehaviour
{
    public enum GhostState { Idle, Walking, Surprised, CastingSpell, Searching }
    private GhostState currentState = GhostState.Idle;

    [Header("AI Settings")]
    public float walkRadius = 10f;
    public float detectionRadius = 5f;
    public float castSpellTime = 3f;
    public float wanderWaitMin = 2f;
    public float wanderWaitMax = 5f;
    public float walkDurationMin = 3f;
    public float walkDurationMax = 6f;
    public float alertDuration = 5f;
    public float pathRecalculationInterval = 1f;
    public float obstacleAvoidanceDistance = 2f;
    public float turnSmoothTime = 0.3f;
    public float waypointPauseTime = 2f;
    [Range(0, 1)] public float centerPathBias = 0.7f;
    public float fieldOfViewAngle = 120f;

    [Header("References")]
    public Animator ghostAnimator;
    public Transform player;
    public NavMeshAgent agent;  
    public Camera playerCamera;
    public GameObject spellEffect;
    public GameObject darkFogEffect;
    public GameObject gameOverPanel;
    public Transform[] patrolPoints;

    [Header("Audio Effects")]
    public AudioSource ghostLaugh;
    public AudioSource whisperingSound;
    public AudioSource spellCastSound;

    [Header("Cinematic Effects")]
    public float cameraRotateSpeed = 1.5f;
    public float fallSpeed = 0.5f;
    public float blinkInterval = 0.2f;
    public float blinkDuration = 2f;
    public float screenShakeIntensity = 0.2f;
    public float screenShakeDuration = 1f;

    // Navigation variables
    private Queue<Transform> patrolHistory = new Queue<Transform>(3);
    private Transform lastPatrolPoint = null;
    private bool usePatrol = false;
    private Vector3 lastKnownPlayerPosition;
    private Vector3 lastDestination;
    private float sqrDetectionRadius;
    private float turnSmoothVelocity;
    private float searchMemoryDuration = 60f;
    private int currentPatrolIndex = 0;

    // State variables
    private bool playerDetected = false;
    private bool isCastingSpell = false;
    private bool isSearching = false;
    private float alertTimer = 0f;
    private float lastPathRecalculationTime = 0f;
    private float stateTime = 0f;

    // Cinematic variables
    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;

    // Coroutine references
    private IEnumerator wanderCoroutine;
    private IEnumerator spellCoroutine;
    private IEnumerator pathRecalculationCoroutine;

    // Misc
    [Tooltip("How high above the ghost to cast LOS raycasts (use eye height).")]
    public float eyeHeight = 1.6f;
    [Tooltip("Layer mask for obstacles when sampling and LOS raycasts.")]
    public LayerMask obstacleMask = ~0;

    // Performance / stability tuning
    [Header("Movement Tuning")]
    [Tooltip("Minimum distance change to trigger new SetDestination to avoid re-pathing (prevents jitter)")]
    public float destinationChangeThreshold = 0.5f;
    [Tooltip("Tolerance used to detect arrival / avoid rounding-on-spot")]
    public float arrivalTolerance = 0.6f;
    [Tooltip("How long (s) the agent must not move to be considered stuck")]
    public float stuckTimeThreshold = 1.2f;
    [Tooltip("Minimum squared movement to consider that agent moved")]
    public float minSqrMovement = 0.01f;

    private float lastPositionCheckTime;
    private Vector3 lastCheckedPosition;
    private float stuckTimer = 0f;

    private WaitForSeconds pathRecalcWait;
    private WaitForSeconds smallWait = new WaitForSeconds(0.02f);

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (ghostAnimator == null) ghostAnimator = GetComponent<Animator>();

        sqrDetectionRadius = detectionRadius * detectionRadius;

        // Configure NavMeshAgent for better navigation
        agent.autoRepath = true;
        agent.autoTraverseOffMeshLink = true;
        agent.avoidancePriority = Random.Range(30, 70);

        agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        agent.radius = Mathf.Max(0.2f, agent.radius);
        agent.height = Mathf.Max(1.5f, agent.height);
        agent.stoppingDistance = Mathf.Max(0.2f, agent.stoppingDistance);
        agent.autoBraking = true;

        pathRecalcWait = new WaitForSeconds(Mathf.Max(0.5f, pathRecalculationInterval));
    }

    private void Start()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (darkFogEffect) darkFogEffect.SetActive(false);

        ResetAllAnimationBools();
        ghostAnimator.SetBool("isIdle", true);

        usePatrol = patrolPoints != null && patrolPoints.Length > 1 && Random.value > 0.5f;

        // Ensure agent is placed on the NavMesh. If not on NavMesh, warp to nearest sample point.
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
            }
            else
            {
                Debug.LogWarning("Ghost2AI_Fixed: No NavMesh nearby to place agent. Make sure NavMeshSurface is baked.");
            }
        }

        wanderCoroutine = WanderRoutine();
        StartCoroutine(wanderCoroutine);

        pathRecalculationCoroutine = PathRecalculationRoutine();
        StartCoroutine(pathRecalculationCoroutine);

        lastCheckedPosition = transform.position;
        lastPositionCheckTime = Time.time;
    }

    private void Update()
    {
        // Keep Update light — heavy logic lives in coroutines
        if (isCastingSpell) return;

        UpdateAnimations();

        // periodic lightweight checks (every 0.12s)
        if (Time.time - lastPositionCheckTime >= 0.12f)
        {
            float now = Time.time;
            float dt = now - lastPositionCheckTime;
            lastPositionCheckTime = now;

            UpdatePerception();           // cheap sqrMagnitude checks
            UpdateStateBehavior();
            StuckDetectionTick(dt);
        }
    }

    private void StuckDetectionTick(float dt)
    {
        // check movement over time to detect stuck
        float sqrMoved = (transform.position - lastCheckedPosition).sqrMagnitude;
        if (sqrMoved < minSqrMovement)
        {
            stuckTimer += dt;
        }
        else
        {
            stuckTimer = 0f;
            lastCheckedPosition = transform.position;
        }

        if (stuckTimer > stuckTimeThreshold)
        {
            stuckTimer = 0f;
            TryResolveStuck();
        }
    }

    private void TryResolveStuck()
    {
        // prefer to nudge to a nearby valid point, avoid warp unless necessary
        Vector3 fallback = GetRandomPoint(transform.position, Mathf.Clamp(walkRadius * 0.5f, 2f, walkRadius));
        if (fallback != transform.position)
        {
            SetDestinationSafe(fallback);
            return;
        }

        // last resort: warp to nearest NavMesh point
        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }

    private void UpdatePerception()
    {
        if (!player) return;

        Vector3 toPlayer = player.position - transform.position;
        float sqrDist = toPlayer.sqrMagnitude;

        if (sqrDist <= sqrDetectionRadius)
        {
            float angle = Vector3.Angle(transform.forward, toPlayer);
            if (angle < fieldOfViewAngle * 0.5f && CheckLineOfSight(player.position))
            {
                lastKnownPlayerPosition = player.position;
                HandlePlayerDetection();
            }
            else if (playerDetected)
            {
                HandlePlayerLost();
            }
        }
        else if (playerDetected)
        {
            HandlePlayerLost();
        }
    }

    private bool CheckLineOfSight(Vector3 targetPosition)
    {
        Vector3 origin = transform.position + Vector3.up * eyeHeight;
        Vector3 direction = (targetPosition + Vector3.up * 1.0f) - origin;
        if (Physics.Raycast(origin, direction.normalized, out RaycastHit hit, detectionRadius, obstacleMask))
        {
            return hit.transform == player || hit.transform.IsChildOf(player);
        }
        return false;
    }

    private void HandlePlayerDetection()
    {
        if (!playerDetected)
        {
            playerDetected = true;
            ChangeState(GhostState.Surprised);
            if (ghostLaugh) ghostLaugh.Play();

            if (spellCoroutine != null) StopCoroutine(spellCoroutine);
            spellCoroutine = PrepareSpell();
            StartCoroutine(spellCoroutine);
        }

        if (whisperingSound && !whisperingSound.isPlaying)
        {
            whisperingSound.Play();
        }
    }

    private void HandlePlayerLost()
    {
        playerDetected = false;
        StartCoroutine(SearchLastKnownPosition());
        if (whisperingSound && whisperingSound.isPlaying)
        {
            whisperingSound.Stop();
        }
    }

    private void UpdateStateBehavior()
    {
        stateTime += Time.deltaTime;

        switch (currentState)
        {
            case GhostState.Searching:
                alertTimer -= Time.deltaTime;
                if (alertTimer <= 0f)
                {
                    isSearching = false;
                    ChangeState(GhostState.Idle);
                }
                break;

            case GhostState.Surprised:
            case GhostState.CastingSpell:
                FacePlayer();
                break;
        }
    }

    private void UpdateAnimations()
    {
        Vector3 vel = agent.velocity;
        float speed = vel.magnitude;

        if (speed > 0.1f)
        {
            Vector3 desired = agent.desiredVelocity;
            if (desired.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(desired.x, desired.z) * Mathf.Rad2Deg;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle,
                                                ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
            }
        }

        ghostAnimator.SetFloat("Speed", speed);
    }

    private IEnumerator WanderRoutine()
    {
        while (true)
        {
            if (currentState == GhostState.Idle)
            {
                yield return smallWait; // avoid long blocking, distribute waiting
                float wait = Random.Range(wanderWaitMin, wanderWaitMax);
                yield return new WaitForSeconds(wait);

                float choice = Random.value;

                if (choice < 0.3f)
                {
                    ChangeState(GhostState.Surprised);
                    yield return new WaitForSeconds(1.5f);
                    ChangeState(GhostState.Idle);
                }
                else
                {
                    ChangeState(GhostState.Walking);
                }
            }

            if (currentState == GhostState.Walking)
            {
                Vector3 destination = GetValidDestination();
                lastDestination = destination;
                SetDestinationSafe(destination);

                float walkDuration = Random.Range(walkDurationMin, walkDurationMax);
                float elapsed = 0f;

                while (elapsed < walkDuration && currentState == GhostState.Walking)
                {
                    if (agent.pathPending)
                    {
                        yield return null;
                        continue;
                    }

                    if (!agent.isOnNavMesh)
                    {
                        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                            agent.Warp(hit.position);
                    }

                    if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial)
                    {
                        Vector3 alt = GetValidDestination();
                        if ((alt - lastDestination).sqrMagnitude > 0.1f)
                        {
                            lastDestination = alt;
                            SetDestinationSafe(alt);
                        }
                    }

                    // arrival prevention: avoid tiny oscillations by using arrivalTolerance
                    if (!agent.pathPending && agent.remainingDistance <= Mathf.Max(agent.stoppingDistance, arrivalTolerance))
                        break;

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                ChangeState(GhostState.Idle);
            }

            yield return null;
        }
    }

    private Vector3 GetValidDestination()
    {
        Vector3 rawPoint;
        if (usePatrol && patrolPoints != null && patrolPoints.Length > 0)
        {
            // use the next patrol index in a cycle
            Transform nextPoint = patrolPoints[currentPatrolIndex % patrolPoints.Length];
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            lastPatrolPoint = nextPoint;
            AddToPatrolMemory(nextPoint);
            rawPoint = nextPoint.position;
        }
        else
        {
            rawPoint = GetRandomPoint(transform.position, walkRadius);
        }

        Vector3 biased = BiasToNavMesh(rawPoint);

        // ensure new destination is meaningfully different
        if ((biased - lastDestination).sqrMagnitude < 0.04f)
        {
            // try one more time to get different point
            Vector3 alt = GetRandomPoint(transform.position, walkRadius);
            biased = BiasToNavMesh(alt);
        }

        return biased;
    }

    private Vector3 GetRandomPoint(Vector3 origin, float radius)
    {
        // fewer iterations to reduce cost, still robust
        for (int i = 0; i < 20; i++)
        {
            Vector3 randomPoint = origin + Random.insideUnitSphere * radius;
            randomPoint.y = origin.y;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                if (!Physics.CheckSphere(hit.position, obstacleAvoidanceDistance, obstacleMask))
                {
                    if ((hit.position - lastDestination).sqrMagnitude > 0.25f) // avoid too near previous
                        return hit.position;
                }
            }
        }
        return transform.position;
    }

    private Vector3 BiasToNavMesh(Vector3 point)
    {
        if (NavMesh.SamplePosition(point, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            return Vector3.Lerp(point, hit.position, centerPathBias);
        }
        return point;
    }

    private void AddToPatrolMemory(Transform point)
    {
        if (patrolHistory.Contains(point)) return;
        patrolHistory.Enqueue(point);
        if (patrolHistory.Count > 3) patrolHistory.Dequeue();
    }

    private IEnumerator SearchLastKnownPosition()
    {
        isSearching = true;
        alertTimer = alertDuration;

        ChangeState(GhostState.Searching);

        SetDestinationSafe(lastKnownPlayerPosition);

        while (Vector3.Distance(transform.position, lastKnownPlayerPosition) > Mathf.Max(agent.stoppingDistance, arrivalTolerance) && alertTimer > 0f)
        {
            if (Random.value < 0.1f)
            {
                Vector3 randomDirection = Random.insideUnitSphere * 5f;
                randomDirection.y = 0;
                SetDestinationSafe(transform.position + randomDirection);
                yield return new WaitForSeconds(0.5f);
                SetDestinationSafe(lastKnownPlayerPosition);
            }
            yield return null;
        }

        ChangeState(GhostState.Idle);
    }

    private IEnumerator PathRecalculationRoutine()
    {
        WaitForSeconds wait = pathRecalcWait;

        while (true)
        {
            yield return wait;

            if (agent.hasPath && agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance > agent.stoppingDistance)
            {
                Vector3 currentDest = agent.destination;
                // only force re-set when destination changed significantly
                SetDestinationSafe(currentDest);
            }
        }
    }

    private void RecalculatePathIfNeeded()
    {
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                agent.Warp(hit.position);
            return;
        }

        if (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathPending)
        {
            switch (currentState)
            {
                case GhostState.Walking:
                    if (usePatrol && patrolPoints.Length > 0)
                        SetDestinationSafe(patrolPoints[currentPatrolIndex % patrolPoints.Length].position);
                    else
                        SetDestinationSafe(lastDestination);
                    break;
                case GhostState.Searching:
                    SetDestinationSafe(lastKnownPlayerPosition);
                    break;
            }
        }
    }

    // Safely sample a target and set agent destination. Attempts retries and warps if necessary.
    private bool SetDestinationSafe(Vector3 target)
    {
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit selfHit, 5f, NavMesh.AllAreas))
                agent.Warp(selfHit.position);
            else
                return false;
        }

        // avoid re-setting same destination repeatedly (prevents oscillation/lag)
        if (agent.hasPath)
        {
            if ((agent.destination - target).sqrMagnitude < (destinationChangeThreshold * destinationChangeThreshold))
                return true;
        }

        if (NavMesh.SamplePosition(target, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            lastDestination = hit.position;
            return true;
        }
        else
        {
            // fallback: try to find a random nearby valid point
            Vector3 fallback = GetRandomPoint(transform.position, walkRadius);
            if (fallback != transform.position)
            {
                agent.SetDestination(fallback);
                lastDestination = fallback;
                return true;
            }
        }

        return false;
    }

    private IEnumerator PrepareSpell()
    {
        yield return new WaitForSeconds(1f);

        if (playerDetected)
        {
            ChangeState(GhostState.CastingSpell);
            yield return StartCoroutine(CastSpellSequence());
        }
    }

    private IEnumerator CastSpellSequence()
    {
        isCastingSpell = true;

        if (spellCastSound) spellCastSound.Play();
        if (spellEffect) spellEffect.SetActive(true);
        if (darkFogEffect) darkFogEffect.SetActive(true);
        StartFogEffect();

        if (playerCamera != null)
        {
            originalCameraPosition = playerCamera.transform.position;
            originalCameraRotation = playerCamera.transform.rotation;
        }

        yield return StartCoroutine(ScreenShake());

        float elapsed = 0f;
        while (elapsed < 2f)
        {
            elapsed += Time.deltaTime;
            if (playerCamera != null) playerCamera.transform.Rotate(Vector3.forward * cameraRotateSpeed);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < 2f)
        {
            elapsed += Time.deltaTime;
            if (playerCamera != null) playerCamera.transform.position += Vector3.down * fallSpeed * Time.deltaTime;
            yield return null;
        }

        yield return StartCoroutine(BlinkEffect());

        ResetCamera();

        if (gameOverPanel) gameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        isCastingSpell = false;
    }

    private IEnumerator ScreenShake()
    {
        float elapsed = 0f;
        if (playerCamera == null) yield break;
        Vector3 originalPosition = playerCamera.transform.position;

        while (elapsed < screenShakeDuration)
        {
            elapsed += Time.deltaTime;
            playerCamera.transform.position = originalPosition + (Random.insideUnitSphere * screenShakeIntensity);
            yield return null;
        }

        if (playerCamera != null) playerCamera.transform.position = originalPosition;
    }

    private IEnumerator BlinkEffect()
    {
        if (playerCamera == null) yield break;

        float elapsed = 0f;
        bool isBlinking = false;
        while (elapsed < blinkDuration)
        {
            isBlinking = !isBlinking;
            playerCamera.enabled = isBlinking;
            yield return new WaitForSeconds(blinkInterval);
            elapsed += blinkInterval;
        }
        playerCamera.enabled = true;
    }

    private void ResetCamera()
    {
        if (playerCamera != null)
        {
            playerCamera.transform.position = originalCameraPosition;
            playerCamera.transform.rotation = originalCameraRotation;
        }
    }

    private void StartFogEffect()
    {
        RenderSettings.fog = true;
        RenderSettings.fogColor = Color.black;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = 0.1f;
    }

    private void ResetAllAnimationBools()
    {
        ghostAnimator.SetBool("isIdle", false);
        ghostAnimator.SetBool("isWalking", false);
        ghostAnimator.SetBool("isSurprised", false);
        ghostAnimator.SetBool("isCastingSpell", false);
    }

    private void ChangeState(GhostState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        stateTime = 0f;
        ResetAllAnimationBools();

        switch (newState)
        {
            case GhostState.Idle:
                ghostAnimator.SetBool("isIdle", true);
                agent.isStopped = true;
                break;

            case GhostState.Walking:
                ghostAnimator.SetBool("isWalking", true);
                agent.isStopped = false;
                break;

            case GhostState.Surprised:
                ghostAnimator.SetBool("isSurprised", true);
                agent.isStopped = true;
                break;

            case GhostState.CastingSpell:
                ghostAnimator.SetBool("isCastingSpell", true);
                agent.isStopped = true;
                break;

            case GhostState.Searching:
                ghostAnimator.SetBool("isWalking", true);
                agent.isStopped = false;
                break;
        }
    }

    private void FacePlayer()
    {
        if (!player) return;

        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
        }
    }

    private void OnDestroy()
    {
        if (wanderCoroutine != null) StopCoroutine(wanderCoroutine);
        if (spellCoroutine != null) StopCoroutine(spellCoroutine);
        if (pathRecalculationCoroutine != null) StopCoroutine(pathRecalculationCoroutine);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.red;
            Vector3[] pathCorners = agent.path.corners;
            for (int i = 0; i < pathCorners.Length - 1; i++)
            {
                Gizmos.DrawLine(pathCorners[i], pathCorners[i + 1]);
            }
        }
    }
}

