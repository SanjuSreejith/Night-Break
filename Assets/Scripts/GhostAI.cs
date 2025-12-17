using GLTFast.Schema;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GhostAI : MonoBehaviour
{
    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float walkSpeed = 3.5f;
    public float runSpeed = 6.0f;
    public float investigationSpeed = 4.0f;
    public float chaseSpeed = 7.0f;
    public float waypointPauseTime = 2f;
    [Range(0, 1)] public float centerPathBias = 0.7f;

    [Header("Navigation Settings")]
    public float stuckCheckInterval = 1f;
    public float minMovementThreshold = 0.5f;
    public int maxStuckCount = 3;
    public float pathRecalculationInterval = 2f;
    public float obstacleAvoidanceRadius = 2f;
    public LayerMask obstacleLayer;

    [Header("Detection Settings")]
    public float detectionRange = 10f;
    public float warningRange = 3f;
    public float deathRange = 0.75f;
    public float fieldOfViewAngle = 120f;
    public float hearingRange = 8f;
    public LayerMask detectionLayers;
    [Header("Death Sequence Settings")]
    public AudioClip deathFaceSound; // Assign a loud jumpscare sound
    public float facePlayerSpeed = 5f; // How fast ghost turns to face player
    public float faceDistance = 1.5f; // How close ghost gets before stopping
    public float postFaceDelay = 0.5f; // Short pause before screen effects

    [Header("Behavior Settings")]
    public float idleBeforeNextPatrol = 2f;
    public float stareBeforeChase = 1.5f;
    public float adaptiveChaseRange = 10f;
    public float adaptiveChaseTime = 5f;
    public float searchDuration = 5f;
    public float forgetPlayerTime = 5f;
    public float chaseDuration = 10f;
    public float speedBurstChance = 0.1f;
    public float speedBurstDuration = 1f;
    public float speedBurstMultiplier = 1f;
    public float randomPauseChance = 0.05f;
    public float randomPauseDuration = 2f;
    public bool firstTimeLaughEnabled = true;
    public float laughDuration = 3f;
    public float doorInteractionTime = 3f;
    public float doorStandTime = 5f;
    public float doorCheckDistance = 2f;
    public float turnSmoothTime = 0.3f;

    [Header("Dialogue Settings")]
    public float dialogueRange = 10f;
    public float dialogueCooldown = 5f;
    public AudioSource dialogueAudioSource;

    [Header("Audio/Visual Settings")]
    public Animator animator;
    public AudioSource gameOverSound;
    public AudioSource warningSound;
    public AudioSource footstepSound;
    public AudioSource backgroundMusic;
    public AudioSource ghostCatchSound;
    public AudioSource ghostLaughSound;
    public AudioClip walkingFootsteps;
    public AudioClip runningFootsteps;
    public AudioClip[] ghostDialogues;
    public GameObject gameOverPanel;
    public GameObject warningImage;
    public GameObject ghostAttackImage;
    public Transform attackImageTarget;
    public float footstepInterval = 0.5f;

    [Header("Difficulty Settings")]
    public float easySpeed = 2.5f;
    public float mediumSpeed = 4f;
    public float hardSpeed = 6f;
    public float easyDetectionRange = 8f;
    public float mediumDetectionRange = 12f;
    public float hardDetectionRange = 15f;
    public float easyChaseDuration = 5f;
    public float hardChaseDuration = 10f;

    // Internal State
    private NavMeshAgent agent;
    private Transform player;
    private Light playerTorchLight;
    private int currentPatrolIndex = 0;
    private int lastPatrolIndex = -1;
    private float lastStuckCheckTime;
    private Vector3 lastPosition;
    private int stuckCount = 0;
    private float lastPathRecalculationTime;
    private bool isChasing = false;
    private bool isGameOver = false;
    private bool isWarningShown = false;
    private bool isPlayingFootsteps = false;
    private float timePlayerInRange = 0f;
    private bool isAdaptiveChasing = false;
    private bool isSpeedBurstActive = false;
    private bool isRandomlyPaused = false;
    private bool isSearching = false;
    private float lastDialogueTime;
    private bool hasLaughed = false;
    private Vector3 lastHeardPosition;
    private Vector3 lastKnownPosition;
    private bool isInvestigatingSound = false;
    private float lightExposure = 0f;
    private float stateTime = 0f;
    private bool hasSeenLight = false;
    private int lightDetectionCount = 0;
    private Vector3 lastSeenLightPosition;
    private float forgetTimer = 0f;
    private float turnSmoothVelocity;
    private Vector3 currentDirection;
    private float doorStandTimer = 0f;
    private bool isAtDoor = false;
    private GameObject currentDoor;
    private float lastFootstepTime;
    private float lastCoordinationTime = 0f;
    private bool isPlayerHidden = false;
    public float forgetTime = 10f; // You can change 10f to your preferred time in seconds
    [Header("Chase Audio")]
    public AudioSource chaseAudioSource;
    public AudioClip chaseSound;
    private Coroutine chaseCoroutine;
    private Coroutine patrolCoroutine;



    // AI States
    private enum GhostState { Patrolling, Investigating, Chasing, Searching, Distracted }
    private GhostState currentState = GhostState.Patrolling;

    // Multi-Ghost Coordination
    public static List<GhostAI> allGhosts = new List<GhostAI>();
    public float coordinationRange = 20f;
    public float coordinationCooldown = 5f;
    [Header("Capture Effects")]
    public GameObject darkFog; // Assign a particle system or fog plane
    public float fogFadeSpeed = 2f; // Faster fog appearance
    public float fogMaxDensity = 0.97f; // Nearly opaque
    public float cameraShakeDuration = 0.5f;
    public float cameraShakeMagnitude = 0.2f;
    public float panelShakeMagnitude = 0.3f; // Stronger shake for panel reveal
    public string attackAnimationTrigger = "Attack";
    public float freezePlayerDelay = 0.3f;
    public float panelDelay = 1.5f; // Delay before showing panel
    public float postPanelShakeDuration = 0.4f; // Shake when panel appears

    private MonoBehaviour playerMovementScript;
    private bool isInDeathSequence = false;
    // Add these new variables to your class
    private Dictionary<Vector3, float> searchedPositions = new Dictionary<Vector3, float>();
    private float searchMemoryDuration = 60f; // Remember searched positions for 60 seconds
    private float lastSearchUpdateTime = 0f;
    [Header("Death Appearance")]
    public GameObject ghostModel; // The normal ghost model to hide
    public GameObject deathAppearancePrefab; // The prefab to show during death sequence
    public float deathAppearanceDuration = 2f; // How long the death appearance stays
    public TorchSystem torchSystem; // Assign via Inspector or dynamically





    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoRepath = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        var torchSystem = player?.GetComponentInChildren<TorchSystem>();
        playerTorchLight = torchSystem?.GetComponentInChildren<Light>();
        playerMovementScript = player.GetComponent<PlayerMovement>();
        InitializeAudioVisuals();
        UpgradeDifficulty(PlayerPrefs.GetInt("GameDifficulty", 1));

        if (firstTimeLaughEnabled && !hasLaughed)
        {
            StartCoroutine(PlayFirstTimeLaugh());
        }

        allGhosts.Add(this);
        lastPosition = transform.position;

        if (dialogueAudioSource == null)
        {
            dialogueAudioSource = gameObject.AddComponent<AudioSource>();
            dialogueAudioSource.spatialBlend = 1f;
            dialogueAudioSource.minDistance = 2f;
            dialogueAudioSource.maxDistance = dialogueRange;
        }

        if (patrolPoints.Length > 0)
        {
            StartCoroutine(PatrolBehavior());
        }
    }

    void OnDestroy()
    {
        allGhosts.Remove(this);
    }

    void Update()
    {
        if (isGameOver) return;

        UpdatePerception();
        UpdateStateMachine();
        UpdateAnimations();
        PlayMovementSounds();

        float distance = Vector3.Distance(transform.position, player.position);
        UpdateAdaptiveChasing(distance);
        HandleDynamicBehaviors();
        HandleWarningSystem(distance);
        HandleDialogues(distance);
        CheckInstantDeath(distance);
        DetectFlashlight();
        CheckDoors();
        HandleDoorBehavior();

        if (Time.time - lastCoordinationTime > coordinationCooldown)
        {
            CoordinateWithOtherGhosts();
            lastCoordinationTime = Time.time;
        }
        if (Time.time - lastSearchUpdateTime > 10f && currentState == GhostState.Patrolling)
        {
            // Mark current patrol point as searched
            if (patrolPoints.Length > 0 && currentPatrolIndex >= 0 && currentPatrolIndex < patrolPoints.Length)
            {
                searchedPositions[patrolPoints[currentPatrolIndex].position] = Time.time;
            }
            lastSearchUpdateTime = Time.time;
        }
        if (Time.time - lastStuckCheckTime > stuckCheckInterval)
        {
            CheckIfStuck();
            lastStuckCheckTime = Time.time;
        }

        if (Time.time - lastPathRecalculationTime > pathRecalculationInterval)
        {
            RecalculatePathIfNeeded();
            lastPathRecalculationTime = Time.time;
        }
    }

    #region Navigation Systems
    private void UpdateAdaptiveChasing(float distance)
    {
        if (distance <= adaptiveChaseRange)
        {
            timePlayerInRange += Time.deltaTime;
            if (timePlayerInRange >= adaptiveChaseTime && !isAdaptiveChasing)
            {
                StartAdaptiveChase();
            }
        }
        else
        {
            ResetAdaptiveChase();
        }
    }


    private void StartAdaptiveChase()
    {
        if (!player || BarrelHidingSpot.IsPlayerHiding)
            return;
        isAdaptiveChasing = true;
        agent.speed = runSpeed;
        animator.SetBool("isRunning", true);
        agent.SetDestination(player.position);

        // Play chase sound if not already playing
        if (chaseAudioSource && chaseSound && !chaseAudioSource.isPlaying)
        {
            chaseAudioSource.clip = chaseSound;
            chaseAudioSource.loop = true;
            chaseAudioSource.spatialBlend = 1f; // 3D effect
            chaseAudioSource.Play();
        }
    }


    private void ResetAdaptiveChase()
    {
        timePlayerInRange = 0f;
        if (isAdaptiveChasing)
        {
            isAdaptiveChasing = false;
            agent.speed = walkSpeed;
            animator.SetBool("isRunning", false);
            TransitionToState(GhostState.Patrolling);
        }
    }

    private void HandleDynamicBehaviors()
    {
        if (!isSpeedBurstActive && Random.value < speedBurstChance && currentState == GhostState.Chasing)
        {
            StartCoroutine(SpeedBurstRoutine());
        }

        if (!isRandomlyPaused && Random.value < randomPauseChance && currentState == GhostState.Patrolling)
        {
            StartCoroutine(RandomPauseRoutine());
        }
    }

    IEnumerator SpeedBurstRoutine()
    {
        isSpeedBurstActive = true;
        float originalSpeed = agent.speed;
        agent.speed *= speedBurstMultiplier;
        yield return new WaitForSeconds(speedBurstDuration);
        agent.speed = originalSpeed;
        isSpeedBurstActive = false;
    }

    IEnumerator RandomPauseRoutine()
    {
        isRandomlyPaused = true;
        agent.isStopped = true;
        animator.SetBool("isWalking", false);
        yield return new WaitForSeconds(randomPauseDuration);
        agent.isStopped = false;
        animator.SetBool("isWalking", true);
        isRandomlyPaused = false;
    }
    #endregion

    #region Enhanced Patrol System
    IEnumerator PatrolBehavior()
    {
        while (currentState == GhostState.Patrolling)
        {
            if (patrolPoints.Length == 0) yield break;

            currentPatrolIndex = GetNextPatrolIndex();
            lastPatrolIndex = currentPatrolIndex;

            Vector3 targetPosition = GetCenterBiasedPatrolPosition(patrolPoints[currentPatrolIndex].position);
            agent.SetDestination(targetPosition);
            animator.SetBool("isWalking", true);

            // Wait until close or state changes, but check more frequently
            while (agent.remainingDistance > agent.stoppingDistance && currentState == GhostState.Patrolling)
            {
                yield return new WaitForSeconds(0.2f); // More frequent checks
            }

            animator.SetBool("isWalking", false);

            // Variable wait time at waypoints
            float pauseTime = waypointPauseTime * (0.8f + Random.value * 0.4f); // ±20% variation
            yield return new WaitForSeconds(pauseTime);
        }
    }
    IEnumerator LookAround()
    {
        float lookDuration = 2f;
        float lookTime = 0f;
        Vector3 originalForward = transform.forward;

        while (lookTime < lookDuration)
        {
            float progress = lookTime / lookDuration;
            float angle = Mathf.Lerp(0, 360, progress);
            transform.rotation = Quaternion.Euler(0, angle, 0);

            if (isPlayerVisible())
            {
                TransitionToState(GhostState.Chasing);
                yield break;
            }

            lookTime += Time.deltaTime;
            yield return null;
        }

        transform.forward = originalForward;
        yield return new WaitForSeconds(1f);
    }

    IEnumerator SearchBehavior()
    {
        float searchRadius = 8f; // Increased search radius
        Vector3 basePosition = lastKnownPosition;

        // If we're near a patrol point, search around it
        foreach (var point in patrolPoints)
        {
            if (Vector3.Distance(basePosition, point.position) < 5f)
            {
                basePosition = point.position;
                break;
            }
        }

        // Generate search points, avoiding recently searched areas
        Vector3[] searchPoints = new Vector3[5];
        int attempts = 0;
        int validPoints = 0;

        while (validPoints < 5 && attempts < 20)
        {
            attempts++;
            Vector3 potentialPoint = basePosition + Random.insideUnitSphere * searchRadius;
            potentialPoint.y = basePosition.y;

            // Check if this area was recently searched
            bool recentlySearched = false;
            foreach (var searchedPos in searchedPositions)
            {
                if (Vector3.Distance(potentialPoint, searchedPos.Key) < 3f &&
                    Time.time - searchedPos.Value < searchMemoryDuration)
                {
                    recentlySearched = true;
                    break;
                }
            }

            if (!recentlySearched)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(potentialPoint, out hit, searchRadius, NavMesh.AllAreas))
                {
                    searchPoints[validPoints] = hit.position;
                    validPoints++;
                    searchedPositions[hit.position] = Time.time;
                }
            }
        }

        // Search the valid points we found
        for (int i = 0; i < validPoints; i++)
        {
            agent.SetDestination(searchPoints[i]);
            yield return new WaitUntil(() => agent.remainingDistance < 1f || isPlayerVisible());

            if (isPlayerVisible())
            {
                TransitionToState(GhostState.Chasing);
                yield break;
            }

            // Look around while at search point
            yield return StartCoroutine(LookAround());
        }

        // Clean up old search memory
        if (Time.time - lastSearchUpdateTime > 10f)
        {
            List<Vector3> toRemove = new List<Vector3>();
            foreach (var entry in searchedPositions)
            {
                if (Time.time - entry.Value > searchMemoryDuration)
                    toRemove.Add(entry.Key);
            }
            foreach (var key in toRemove)
            {
                searchedPositions.Remove(key);
            }
            lastSearchUpdateTime = Time.time;
        }

        TransitionToState(GhostState.Patrolling);
    }



    private int GetNextPatrolIndex()
    {
        if (patrolPoints.Length <= 1) return 0;

        List<int> availableIndices = new List<int>();
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (i != lastPatrolIndex)
                availableIndices.Add(i);
        }

        // In endless mode, prioritize points near player and avoid other ghosts
        if (SessionSettings.OverrideHardInEndless)
        {
            // Remove points that other ghosts are heading to
            foreach (var ghost in allGhosts)
            {
                if (ghost != this && ghost.currentState == GhostState.Patrolling)
                {
                    int ghostIndex = ghost.currentPatrolIndex;
                    if (availableIndices.Contains(ghostIndex))
                    {
                        availableIndices.Remove(ghostIndex);
                    }
                }
            }

            // If no points left, just pick any
            if (availableIndices.Count == 0)
            {
                for (int i = 0; i < patrolPoints.Length; i++)
                {
                    if (i != lastPatrolIndex)
                        availableIndices.Add(i);
                }
            }

            // Find point closest to player
            int nearestIndex = availableIndices[0];
            float minDistance = float.MaxValue;

            foreach (int index in availableIndices)
            {
                float distance = Vector3.Distance(player.position, patrolPoints[index].position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestIndex = index;
                }
            }
            return nearestIndex;
        }

        // Original behavior for non-endless mode
        float randomValue = Random.value;

        if (randomValue < 0.7f) // 70% chance for random point
        {
            return availableIndices[Random.Range(0, availableIndices.Count)];
        }
        else // 30% chance for furthest point
        {
            int furthestIndex = availableIndices[0];
            float maxDistance = 0;

            foreach (int index in availableIndices)
            {
                float distance = Vector3.Distance(transform.position, patrolPoints[index].position);
                if (distance > maxDistance)
                {
                    maxDistance = distance;
                    furthestIndex = index;
                }
            }
            return furthestIndex;
        }
    }

    private Vector3 GetCenterBiasedPatrolPosition(Vector3 targetPosition)
    {
        Vector3 centerDirection = (Vector3.zero - transform.position).normalized;
        return Vector3.Lerp(targetPosition, targetPosition + centerDirection * 5f, centerPathBias);
    }
    #endregion

    #region Navigation Recovery
    private void CheckIfStuck()
    {
        float distanceMoved = Vector3.Distance(transform.position, lastPosition);
        lastPosition = transform.position;

        if (distanceMoved < minMovementThreshold)
        {
            stuckCount++;
            if (stuckCount >= maxStuckCount)
            {
                HandleStuckSituation();
                stuckCount = 0;
            }
        }
        else
        {
            stuckCount = 0;
        }
    }

    private void RecalculatePathIfNeeded()
    {
        if (agent.pathStatus == NavMeshPathStatus.PathPartial || agent.pathPending)
        {
            switch (currentState)
            {
                case GhostState.Patrolling:
                    agent.SetDestination(patrolPoints[currentPatrolIndex].position);
                    break;
                case GhostState.Chasing:
                    agent.SetDestination(player.position);
                    break;
                case GhostState.Investigating:
                    agent.SetDestination(lastKnownPosition);
                    break;
            }
        }
    }

    private void HandleStuckSituation()
    {
        switch (currentState)
        {
            case GhostState.Patrolling:
                for (int i = 0; i < patrolPoints.Length; i++)
                {
                    int testIndex = (currentPatrolIndex + i) % patrolPoints.Length;
                    if (testIndex != lastPatrolIndex)
                    {
                        NavMeshPath path = new NavMeshPath();
                        if (agent.CalculatePath(patrolPoints[testIndex].position, path))
                        {
                            if (path.status == NavMeshPathStatus.PathComplete)
                            {
                                currentPatrolIndex = testIndex;
                                agent.SetDestination(patrolPoints[testIndex].position);
                                break;
                            }
                        }
                    }
                }
                break;

            case GhostState.Chasing:
                FindAlternativePathToPlayer();
                break;

            case GhostState.Investigating:
                MoveToNearbyPosition();
                break;
        }

        StartCoroutine(UnstuckBoost());
    }

    private void FindAlternativePathToPlayer()
    {
        if (!player) return;

        for (int i = 1; i <= 3; i++)
        {
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            Vector3 alternativePosition = player.position + Quaternion.Euler(0, 45 * i, 0) * directionToPlayer * 2f;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(alternativePosition, out hit, 2f, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path))
                {
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        agent.SetDestination(hit.position);
                        return;
                    }
                }
            }
        }

        agent.SetDestination(player.position);
    }

    private void MoveToNearbyPosition()
    {
        for (int i = 0; i < 5; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * obstacleAvoidanceRadius;
            randomDirection += transform.position;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, obstacleAvoidanceRadius, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path))
                {
                    if (path.status == NavMeshPathStatus.PathComplete)
                    {
                        agent.SetDestination(hit.position);
                        return;
                    }
                }
            }
        }
    }

    IEnumerator UnstuckBoost()
    {
        float originalSpeed = agent.speed;
        float originalAngularSpeed = agent.angularSpeed;
        agent.speed *= 1.5f;
        agent.angularSpeed *= 1.5f;
        yield return new WaitForSeconds(1f);
        agent.speed = originalSpeed;
        agent.angularSpeed = originalAngularSpeed;
    }
    #endregion

    #region Core Systems
    void UpdatePerception()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        bool playerInSight = false;
        if (distanceToPlayer < detectionRange && angleToPlayer < fieldOfViewAngle * 0.5f)
        {
            if (!Physics.Raycast(transform.position, directionToPlayer.normalized, distanceToPlayer, obstacleLayer))
            {
                playerInSight = true;
                lastKnownPosition = player.position;
            }
        }

        bool playerAudible = (distanceToPlayer < hearingRange);

        if (playerTorchLight != null && playerTorchLight.enabled)
        {
            float torchEffect = Vector3.Dot(directionToPlayer.normalized, playerTorchLight.transform.forward);
            if (torchEffect > 0.5f && distanceToPlayer < detectionRange)
            {
                lightExposure += Time.deltaTime;
                if (lightExposure > 1f)
                {
                    lastKnownPosition = player.position;
                    if (currentState != GhostState.Chasing)
                    {
                        TransitionToState(GhostState.Investigating);
                    }
                }
            }
            else
            {
                lightExposure = Mathf.Max(0, lightExposure - Time.deltaTime);
            }
        }

        if (playerAudible && !playerInSight)
        {
            lastHeardPosition = player.position;
            if (!isInvestigatingSound && !isChasing)
            {
                isInvestigatingSound = true;
                TransitionToState(GhostState.Investigating);
            }
        }
    }

    void UpdateStateMachine()
    {
        stateTime += Time.deltaTime;

        switch (currentState)
        {
            case GhostState.Patrolling:
                if (isPlayerVisible())
                {
                    TransitionToState(GhostState.Chasing);
                }
                else if (isPlayerAudible() || lightExposure > 0.5f)
                {
                    TransitionToState(GhostState.Investigating);
                }
                break;

            case GhostState.Investigating:
                agent.SetDestination(lastKnownPosition);

                if (isPlayerVisible())
                {
                    TransitionToState(GhostState.Chasing);
                }
                else if (agent.remainingDistance < 1f)
                {
                    TransitionToState(GhostState.Searching);
                }
                break;

            case GhostState.Chasing:
                agent.SetDestination(player.position);

                if (!isPlayerVisible())
                {
                    if (stateTime > chaseDuration)
                    {
                        TransitionToState(GhostState.Searching);
                    }
                }
                else
                {
                    stateTime = 0;
                }
                break;

            case GhostState.Searching:
                if (isPlayerVisible())
                {
                    TransitionToState(GhostState.Chasing);
                }
                else if (stateTime > searchDuration)
                {
                    TransitionToState(GhostState.Patrolling);
                }
                break;

            case GhostState.Distracted:
                if (stateTime > doorInteractionTime)
                {
                    TransitionToState(GhostState.Investigating);
                }
                break;
        }
    }

    void TransitionToState(GhostState newState)
    {
        switch (currentState)
        {
            case GhostState.Patrolling:
                StopCoroutine(PatrolBehavior());
                break;
            case GhostState.Searching:
                StopCoroutine(SearchBehavior());
                break;
        }

        currentState = newState;
        stateTime = 0f;

        switch (newState)
        {
            case GhostState.Patrolling:
                agent.speed = walkSpeed;
                StartCoroutine(PatrolBehavior());
                break;

            case GhostState.Investigating:
                agent.speed = investigationSpeed;
                agent.SetDestination(lastKnownPosition);
                PlayRandomDialogue(0.3f);
                break;



            case GhostState.Chasing:
                // Make chasing less precise on easier difficulties
                if (PlayerPrefs.GetInt("GameDifficulty", 1) == 0) // Easy
                {
                    agent.stoppingDistance = 1.5f; // Stop further away
                    agent.autoBraking = false; // More fluid movement
                }
                else
                {
                    agent.stoppingDistance = 0.5f;
                }
                agent.speed = chaseSpeed;
                isChasing = true;
                break;

            case GhostState.Searching:
                agent.speed = walkSpeed;
                StartCoroutine(SearchBehavior());
                break;

            case GhostState.Distracted:
                agent.isStopped = true;
                break;
        }
    }

    private bool isPlayerVisible()
    {
        if (!player || BarrelHidingSpot.IsPlayerHiding)
            return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle < fieldOfViewAngle * 0.5f &&
            Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            return !Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, detectionRange, obstacleLayer)
                   || hit.collider.CompareTag("Player");
        }

        return false;
    }


    private bool isPlayerAudible()
    {
        if (!player || BarrelHidingSpot.IsPlayerHiding)
            return false;
        if (!player) return false;
        return Vector3.Distance(transform.position, player.position) <= hearingRange;
        

    }

    void DetectFlashlight()
    {
        if (!player || BarrelHidingSpot.IsPlayerHiding)
            return;

        if (playerTorchLight == null || player == null) return;


        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (playerTorchLight.enabled)
        {
            float torchEffect = Vector3.Dot(directionToPlayer.normalized, playerTorchLight.transform.forward);
            if (torchEffect > 0.7f && distanceToPlayer < detectionRange)
            {
                if (Physics.Raycast(transform.position, directionToPlayer.normalized, distanceToPlayer, detectionLayers))
                {
                    lastSeenLightPosition = player.position;
                    hasSeenLight = true;
                    lightDetectionCount++;

                    if (lightDetectionCount >= 3)
                    {
                        agent.speed = chaseSpeed;
                        TransitionToState(GhostState.Chasing);
                    }
                }
            }
        }
        else if (hasSeenLight)
        {
            forgetTimer += Time.deltaTime;
            if (forgetTimer > forgetTime)
            {
                hasSeenLight = false;
                lightDetectionCount = 0;
                agent.speed = walkSpeed;
            }
        }
    }
    #endregion

    #region Audio/Visual
    private void InitializeAudioVisuals()
    {
        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (warningImage) warningImage.SetActive(false);
        if (ghostAttackImage) ghostAttackImage.SetActive(false);

        if (backgroundMusic)
        {
            backgroundMusic.volume = 0;
            backgroundMusic.Play();
            StartCoroutine(FadeIn(backgroundMusic, 2.0f));
        }
    }

    private void UpdateAnimations()
    {
        Vector3 velocity = agent.velocity;
        float speed = velocity.magnitude;

        if (speed > 0.1f)
        {
            float targetAngle = Mathf.Atan2(velocity.x, velocity.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle,
                                            ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }

        animator.SetFloat("Speed", speed);
        animator.SetBool("isChasing", isChasing);
        animator.SetBool("isAtDoor", isAtDoor);
    }

    private void PlayMovementSounds()
    {
        if (agent.velocity.magnitude > 0.1f && Time.time - lastFootstepTime > footstepInterval)
        {
            footstepSound.clip = currentState == GhostState.Chasing ? runningFootsteps : walkingFootsteps;
            footstepSound.Play();
            lastFootstepTime = Time.time;
        }
    }

    private void HandleWarningSystem(float distance)
    {
        bool shouldShowWarning = distance <= warningRange;
        if (shouldShowWarning != isWarningShown)
        {
            isWarningShown = shouldShowWarning;
            if (warningImage) warningImage.SetActive(shouldShowWarning);
            if (warningSound)
            {
                if (shouldShowWarning && !warningSound.isPlaying) warningSound.Play();
                else if (!shouldShowWarning) warningSound.Stop();
            }
        }
    }

    IEnumerator FadeIn(AudioSource audioSource, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration)
        {
            audioSource.volume = Mathf.Lerp(0, 1, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        audioSource.volume = 1;
    }
    #endregion

    #region Player Interaction



    private void CheckInstantDeath(float distance)
    {
        if (!player || BarrelHidingSpot.IsPlayerHiding)
            return;
        if (!player || isInDeathSequence) return;

     

        if (distance <= deathRange && !isPlayerHidden)
        {
            agent.isStopped = true;
            isInDeathSequence = true;

            // Hide original ghost model
            ghostModel.SetActive(false);
             if (torchSystem != null && torchSystem.isPicked && torchSystem.torchLight.enabled)
        {
            torchSystem.TurnOffTorch();
        }

            // Show scary death model instantly
            if (deathAppearancePrefab)
                deathAppearancePrefab.SetActive(true);

            // Play intense game over sound
            gameOverSound?.Play();

            // Start intense fear shake
            StartCoroutine(ShakeCamera(0.5f, 0.3f));

            // Run death sequence simultaneously
            StartCoroutine(DeathSequence());
        }
    }

    IEnumerator DeathSequence()
    {
        isInDeathSequence = true;

        // 1. Play ghost animation and sound
        animator.SetTrigger(attackAnimationTrigger);
        ghostCatchSound?.Play();

        // 2. Disable player movement
        if (playerMovementScript)
            playerMovementScript.enabled = false;

        // 3. Camera shake with fear
        StartCoroutine(ShakeCamera(0.5f, 0.3f));

        // 4. Activate fog instantly
        if (darkFog)
        {
            darkFog.SetActive(true);
            StartCoroutine(FadeInFog(0.1f));
        }

        // 5. Show death prefab (instantiate so it fully plays animation/audio)
        GameObject deathVisual = null;
        if (deathAppearancePrefab)
        {
            deathVisual = Instantiate(deathAppearancePrefab, transform.position, transform.rotation);
            Destroy(deathVisual, 5f);
        }

        // 6. Play game over sound
        float waitTime = 0f;
        if (gameOverSound && gameOverSound.clip)
        {
            gameOverSound.Play();
            waitTime = gameOverSound.clip.length;
        }

        // 7. Wait for the sound to finish
        yield return new WaitForSeconds(waitTime);

        // 8. Show correct Game Over panel
        EscapeGameController escapeController = FindObjectOfType<EscapeGameController>();
        if (escapeController != null)
        {
            if (GameSettings.CurrentMode == GameSettings.GameMode.Endless)
            {
                escapeController.ShowEndlessLoseScreen();
            }
            else
            {
                escapeController.ShowNormalLoseScreen(true); // Player caught by ghost
            }
        }
        else
        {
            Debug.LogWarning("EscapeGameController not found in scene!");
        }

        // 9. Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator FadeInFog(float duration = 1f) // Default to 1s if not specified
    {
        float elapsed = 0f;
        CanvasGroup fogGroup = darkFog.GetComponent<CanvasGroup>();

        if (fogGroup == null)
        {
            fogGroup = darkFog.AddComponent<CanvasGroup>();
            fogGroup.alpha = 0f;
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fogGroup.alpha = Mathf.Clamp01(elapsed / duration);
            yield return null;
        }

        fogGroup.alpha = 1f;
    }



    IEnumerator ShakeCamera(float duration, float magnitude)
    {
        Transform camTransform = UnityEngine.Camera.main.transform;

        Vector3 originalPos = camTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // More intense at start, tapering off
            float progress = elapsed / duration;
            float currentMagnitude = magnitude * (1 - progress * 0.7f);

            camTransform.localPosition = originalPos +
                new Vector3(
                    Random.Range(-1f, 1f) * currentMagnitude,
                    Random.Range(-1f, 1f) * currentMagnitude,
                    0
                );

            elapsed += Time.deltaTime;
            yield return null;
        }

        camTransform.localPosition = originalPos;
    }
    IEnumerator PlayFirstTimeLaugh()
    {
        if (ghostLaughSound)
        {
            ghostLaughSound.Play();
            yield return new WaitForSeconds(laughDuration);
            hasLaughed = true;
        }
    }
    public void UpgradeDifficulty(int difficulty)
    {
        switch (difficulty)
        {
            case 0: // Easy
                walkSpeed = easySpeed;
                runSpeed = easySpeed * 1.1f;
                chaseSpeed = easySpeed * 1.3f; // ~4.5f
                investigationSpeed = easySpeed * 1.4f; // ~3f
                detectionRange = easyDetectionRange;
                adaptiveChaseTime = easyChaseDuration;
                break;
            case 1: // Medium
                walkSpeed = mediumSpeed;
                runSpeed = mediumSpeed * 1.5f;
                chaseSpeed = mediumSpeed * 1.8f; // ~7.2f
                investigationSpeed = mediumSpeed * 1.2f; // ~4.8f
                detectionRange = mediumDetectionRange;
                adaptiveChaseTime = (easyChaseDuration + hardChaseDuration) / 2;
                break;
            case 2: // Hard
                walkSpeed = hardSpeed;
                runSpeed = hardSpeed * 1.5f;
                chaseSpeed = hardSpeed * 1.8f; // ~10.8f
                investigationSpeed = hardSpeed * 1.2f; // ~7.2f
                detectionRange = hardDetectionRange;
                adaptiveChaseTime = hardChaseDuration;
                break;
        }
    }
    private void HandleDialogues(float distance)
    {
        if (distance <= dialogueRange && Time.time - lastDialogueTime > dialogueCooldown)
        {
            PlayRandomDialogue(0.5f);
            lastDialogueTime = Time.time;
        }
    }

    private void PlayRandomDialogue(float probability)
    {
        if (ghostDialogues.Length > 0 && !dialogueAudioSource.isPlaying && Random.value < probability)
        {
            int index = Random.Range(0, ghostDialogues.Length);
            dialogueAudioSource.clip = ghostDialogues[index];
            dialogueAudioSource.volume = 1f;
            dialogueAudioSource.Play();
        }
    }

    public void SetPlayerHidden(bool hidden)
    {
        
        isPlayerHidden = hidden;
        if (!player || BarrelHidingSpot.IsPlayerHiding)
            return;
        if (hidden && isChasing)
        {
            TransitionToState(GhostState.Searching);
        }
    }

    public void Distract(Vector3 position)
    {
        lastKnownPosition = position;
        if (currentState != GhostState.Chasing)
        {
            TransitionToState(GhostState.Investigating);
        }
    }
    #endregion

    #region Multi-Ghost Coordination
    private void CoordinateWithOtherGhosts()
    {
        foreach (var ghost in allGhosts)
        {
            if (ghost != this && Vector3.Distance(transform.position, ghost.transform.position) <= coordinationRange)
            {
                if (ghost.isChasing && !this.isChasing)
                {
                    this.lastKnownPosition = ghost.lastKnownPosition;
                    TransitionToState(GhostState.Chasing);
                }
                else if (this.isChasing && !ghost.isChasing)
                {
                    ghost.lastKnownPosition = this.lastKnownPosition;
                    ghost.TransitionToState(GhostState.Chasing);
                }
            }
        }
    }
    #endregion

    #region Door System
    private void CheckDoors()
    {
        if (isChasing || isInvestigatingSound || isAdaptiveChasing) return;

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, doorCheckDistance);
        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Door"))
            {
                currentDoor = hitCollider.gameObject;
                isAtDoor = true;
                doorStandTimer = 0f;
                agent.isStopped = true;
                animator.SetBool("isAtDoor", true);
                return;
            }
        }
    }

    private void HandleDoorBehavior()
    {
        if (!isAtDoor) return;

        doorStandTimer += Time.deltaTime;

        if (currentDoor == null ||
            Vector3.Distance(player.position, currentDoor.transform.position) < 3f ||
            doorStandTimer > doorStandTime)
        {
            isAtDoor = false;
            agent.isStopped = false;
            animator.SetBool("isAtDoor", false);
            TransitionToState(GhostState.Patrolling);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Door"))
        {
            currentDoor = other.gameObject;
            if (currentState != GhostState.Chasing)
            {
                StartCoroutine(InteractWithDoor());
            }
        }
    }

    IEnumerator InteractWithDoor()
    {
        TransitionToState(GhostState.Distracted);
        yield return new WaitForSeconds(doorInteractionTime);

        if (currentDoor != null && Random.value > 0.5f)
        {
            currentDoor.transform.Rotate(0, 90, 0);
        }

        TransitionToState(GhostState.Investigating);
    }
    #endregion

    public void StartChase(Transform player)
    {
        if (chaseCoroutine != null) StopCoroutine(chaseCoroutine);
        chaseCoroutine = StartCoroutine(ChasePlayer(player));
    }

    private IEnumerator ChasePlayer(Transform player)
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        while (player != null && agent.enabled)
        {
            agent.SetDestination(player.position);
            yield return new WaitForSeconds(0.2f); // update path less frequently for performance
        }
    }

    public void StartPatrol()
    {
        if (patrolCoroutine != null) StopCoroutine(patrolCoroutine);
        patrolCoroutine = StartCoroutine(PatrolRoutine());
    }

    private IEnumerator PatrolRoutine()
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        while (patrolPoints.Length > 0 && agent.enabled)
        {
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
            while (agent.pathPending || agent.remainingDistance > 0.2f)
            {
                yield return null;
            }

            // Optional random pause
            float pauseTime = Random.Range(1f, 3f);
            yield return new WaitForSeconds(pauseTime);

            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    public bool IsPlayerVisible(Transform player, float detectionRange, float fovAngle, LayerMask obstaclesLayer)
    {
        Vector3 dir = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dir);

        if (angle < fovAngle / 2f && Vector3.Distance(transform.position, player.position) <= detectionRange)
        {
            if (!Physics.Linecast(transform.position + Vector3.up, player.position + Vector3.up, obstaclesLayer))
            {
                return true;
            }
        }
        return false;
    }

    public void InvestigatePosition(Vector3 position)
    {
        if (chaseCoroutine != null) StopCoroutine(chaseCoroutine);
        StartCoroutine(InvestigateRoutine(position));
    }

    private IEnumerator InvestigateRoutine(Vector3 position)
    {
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        agent.SetDestination(position);

        while (agent.pathPending || agent.remainingDistance > 0.2f)
        {
            yield return null;
        }

        // Wait a moment before resuming patrol
        yield return new WaitForSeconds(Random.Range(1f, 2f));
        StartPatrol();
    }

    public void GhostDeath()
    {
        if (chaseCoroutine != null) StopCoroutine(chaseCoroutine);
        if (patrolCoroutine != null) StopCoroutine(patrolCoroutine);


        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;

        // Optional visual effects
        Animator anim = GetComponent<Animator>();
        if (anim != null) anim.SetTrigger("Death");

        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // Destroy after effect
        Destroy(gameObject, 3f);
    }
}