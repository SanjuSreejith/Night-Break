using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent), typeof(AudioSource), typeof(Animator))]
public class UltimateGhostAI : MonoBehaviour
{
    [Header("Detection Settings")]
    public float visionRange = 15f;  // How far the ghost can see
    public float visionAngle = 110f; // Field of view in degrees
    public float hearingRange = 10f; // How far sounds can be heard
    public float lightDetectionRange = 20f; // How far flashlight can be detected
 
    public LayerMask detectionLayers; // What layers block vision
    public LayerMask obstacleLayers; // What counts as obstacles
    

    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 6f;
    public float investigationSpeed = 3.5f;
    public float acceleration = 8f;
    public float angularSpeed = 120f;
    public float stoppingDistance = 0.5f;

    [Header("Behavior Settings")]
    public float minStateDuration = 5f;
    public float maxStateDuration = 15f;
    public float aggressionLevel = 0.5f;
    [Range(0, 1)] public float insanityLevel = 0.3f;
    public float detectionCooldown = 2f;

    [Header("Horror Effects")]
    public AudioClip[] ambientSounds;
    public AudioClip[] chaseSounds;
    public AudioClip[] attackSounds;
    public ParticleSystem apparitionEffect;
    public Light ghostLight;
    public float lightIntensity = 2f;
    public float flickerSpeed = 10f;

    [Header("Player Interaction")]
    public float killDistance = 1f;
    public float jumpscareDistance = 3f;
    public float sanityDrainRate = 0.1f;
    public float sanityRecoverRate = 0.05f;
    public GameObject gameOverScreen;
    public AudioSource jumpscareSound;
    public Image jumpscareImage;
    public float jumpscareDuration = 2f;

    [Header("Door Settings")]
    public float doorOpenAngle = 90f;
    public float doorOpenSpeed = 2f;
    public float doorInteractionRange = 3f;
    public float doorInteractionChance = 0.3f;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    public float waypointWaitTime = 2f;
    [Range(0, 1)] public float waypointSkipChance = 0.1f;

    [Header("Animation Settings")]
    public string walkAnimParam = "isWalking";
    public string runAnimParam = "isRunning";
    public string attackAnimParam = "Attack";
    public string doorAnimParam = "AtDoor";
    public float turnSmoothTime = 0.3f;

    // Private variables
    private NavMeshAgent agent;
    private AudioSource audioSource;
    private Animator animator;
    private Transform player;
    private Light playerFlashlight;
    private float stateTimer;
    private float detectionTimer;
    private Vector3 lastKnownPosition;
    private bool isPlayerVisible;
    private bool isPlayerAudible;
    private int currentPatrolIndex;
    private List<Light> nearbyLights = new List<Light>();
    private List<Transform> nearbyDoors = new List<Transform>();
    private float insanityEffectTimer;
    private float lightFlickerTimer;
    private float playerSanity = 1f;
    private bool isActive = true;
    private bool isInJumpscare;
    private float turnSmoothVelocity;
    private Vector3 currentDirection;

    // AI States
    private enum GhostState { Patrolling, Investigating, Chasing, Hunting, Distracted }
    private GhostState currentState = GhostState.Patrolling;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerFlashlight = player.GetComponentInChildren<Light>();

        agent.speed = patrolSpeed;
        agent.acceleration = acceleration;
        agent.angularSpeed = angularSpeed;
        agent.stoppingDistance = stoppingDistance;

        if (gameOverScreen) gameOverScreen.SetActive(false);
        if (jumpscareImage) jumpscareImage.gameObject.SetActive(false);
    }

    void Update()
    {
        if (!isActive || isInJumpscare) return;

        stateTimer -= Time.deltaTime;
        detectionTimer -= Time.deltaTime;

        UpdatePlayerDetection();
        UpdateEnvironmentalInteractions();
        UpdateHorrorEffects();
        UpdateAnimations();
        UpdateMovementRotation();

        switch (currentState)
        {
            case GhostState.Patrolling: PatrolBehavior(); break;
            case GhostState.Investigating: InvestigateBehavior(); break;
            case GhostState.Chasing: ChaseBehavior(); break;
            case GhostState.Hunting: HuntBehavior(); break;
            case GhostState.Distracted: DistractedBehavior(); break;
        }

        if (Vector3.Distance(transform.position, player.position) < killDistance)
        {
            StartCoroutine(ExecuteJumpscare());
        }
    }

    void UpdateMovementRotation()
    {
        if (agent.velocity.magnitude > 0.1f)
        {
            float targetAngle = Mathf.Atan2(agent.velocity.x, agent.velocity.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

    void UpdateAnimations()
    {
        animator.SetBool(walkAnimParam, currentState == GhostState.Patrolling || currentState == GhostState.Investigating);
        animator.SetBool(runAnimParam, currentState == GhostState.Chasing || currentState == GhostState.Hunting);

        bool nearDoor = nearbyDoors.Count > 0 && currentState != GhostState.Chasing && currentState != GhostState.Hunting;
        animator.SetBool(doorAnimParam, nearDoor);
    }

    void UpdatePlayerDetection()
    {
        if (detectionTimer > 0) return;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float angleToPlayer = Vector3.Angle(transform.forward, directionToPlayer);

        // Vision check
        isPlayerVisible = false;
        if (distanceToPlayer < visionRange && angleToPlayer < visionAngle * 0.5f)
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, directionToPlayer, out hit, visionRange, detectionLayers))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    isPlayerVisible = true;
                    lastKnownPosition = player.position;
                }
            }
        }

        // Hearing check
        isPlayerAudible = distanceToPlayer < hearingRange;

        // Light detection
        if (playerFlashlight != null && playerFlashlight.enabled && distanceToPlayer < lightDetectionRange)
        {
            lastKnownPosition = player.position;
            if (currentState != GhostState.Chasing && currentState != GhostState.Hunting)
            {
                ChangeState(GhostState.Investigating);
            }
        }

        detectionTimer = detectionCooldown;
    }

    void UpdateEnvironmentalInteractions()
    {
        // Light flickering
        if (Random.value < insanityLevel * 0.1f)
        {
            foreach (Light light in nearbyLights)
            {
                if (light != null && Random.value < 0.3f)
                {
                    StartCoroutine(FlickerLight(light));
                }
            }
        }

        // Door interactions
        if (Random.value < insanityLevel * doorInteractionChance && nearbyDoors.Count > 0)
        {
            Transform randomDoor = nearbyDoors[Random.Range(0, nearbyDoors.Count)];
            if (randomDoor != null)
            {
                StartCoroutine(OpenDoor(randomDoor));
            }
        }
    }

    void UpdateHorrorEffects()
    {
        // Ghost light effects
        if (ghostLight != null)
        {
            lightFlickerTimer += Time.deltaTime * flickerSpeed;
            ghostLight.intensity = lightIntensity * (0.8f + Mathf.PerlinNoise(lightFlickerTimer, 0) * 0.4f);
        }

        // Random sounds
        if (Random.value < insanityLevel * 0.02f && !audioSource.isPlaying)
        {
            PlayRandomSound(ambientSounds, 0.3f);
        }

        // Apparition effects
        if (Random.value < insanityLevel * 0.01f)
        {
            Apparate();
        }
    }

    void PatrolBehavior()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);

        if (agent.remainingDistance < agent.stoppingDistance)
        {
            if (Random.value > waypointSkipChance)
            {
                StartCoroutine(WaitAtWaypoint());
            }
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }

        if (isPlayerVisible)
        {
            ChangeState(GhostState.Chasing);
        }
        else if (isPlayerAudible || stateTimer <= 0)
        {
            ChangeState(Random.value < aggressionLevel ? GhostState.Hunting : GhostState.Investigating);
        }
    }

    void InvestigateBehavior()
    {
        agent.SetDestination(lastKnownPosition);

        if (agent.remainingDistance < agent.stoppingDistance)
        {
            StartCoroutine(LookAround());
            ChangeState(Random.value < aggressionLevel * 0.5f ? GhostState.Hunting : GhostState.Patrolling);
        }

        if (isPlayerVisible)
        {
            ChangeState(GhostState.Chasing);
        }
    }

    void ChaseBehavior()
    {
        agent.SetDestination(player.position);

        if (Random.value < 0.1f && !audioSource.isPlaying)
        {
            PlayRandomSound(chaseSounds, 0.5f);
        }

        if (!isPlayerVisible && stateTimer <= 0)
        {
            ChangeState(Random.value < aggressionLevel ? GhostState.Hunting : GhostState.Investigating);
        }
    }

    void HuntBehavior()
    {
        agent.SetDestination(GetHuntingPosition());
        insanityLevel = Mathf.Min(1f, insanityLevel + Time.deltaTime * 0.05f);

        if (stateTimer <= 0)
        {
            insanityLevel *= 0.5f;
            ChangeState(GhostState.Patrolling);
        }
    }

    void DistractedBehavior()
    {
        if (stateTimer <= 0)
        {
            ChangeState(GhostState.Patrolling);
        }
    }

    Vector3 GetHuntingPosition()
    {
        Vector3 predictedPosition = player.position + player.GetComponent<CharacterController>().velocity * 2f;
        predictedPosition += Random.insideUnitSphere * 3f;

        NavMeshHit hit;
        if (NavMesh.SamplePosition(predictedPosition, out hit, 5f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        return player.position;
    }

    void ChangeState(GhostState newState)
    {
        currentState = newState;
        stateTimer = Random.Range(minStateDuration, maxStateDuration);

        switch (newState)
        {
            case GhostState.Patrolling:
                agent.speed = patrolSpeed;
                break;
            case GhostState.Investigating:
                agent.speed = investigationSpeed;
                PlayRandomSound(ambientSounds, 0.5f);
                break;
            case GhostState.Chasing:
                agent.speed = chaseSpeed;
                PlayRandomSound(chaseSounds, 0.7f);
                break;
            case GhostState.Hunting:
                agent.speed = chaseSpeed * 1.2f;
                PlayRandomSound(chaseSounds, 1f);
                Apparate();
                break;
            case GhostState.Distracted:
                agent.speed = 0;
                break;
        }
    }

    void PlayRandomSound(AudioClip[] clips, float volume)
    {
        if (clips.Length > 0 && !audioSource.isPlaying)
        {
            audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
        }
    }

    void Apparate()
    {
        if (apparitionEffect != null)
        {
            apparitionEffect.Play();
        }

        if (Random.value < insanityLevel * 0.3f)
        {
            Vector3 randomOffset = Random.insideUnitSphere * 5f;
            randomOffset.y = 0;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position + randomOffset, out hit, 5f, NavMesh.AllAreas))
            {
                transform.position = hit.position;
            }
        }
    }

    IEnumerator ExecuteJumpscare()
    {
        isInJumpscare = true;

        // Freeze everything
        Time.timeScale = 0.3f;
        agent.isStopped = true;
        if (player.GetComponent<PlayerMovement>())
            player.GetComponent<PlayerMovement>().enabled = false;

        // Face player directly
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(directionToPlayer);

        // Trigger attack animation
        animator.SetTrigger(attackAnimParam);

        // Play jumpscare sound
        if (jumpscareSound) jumpscareSound.Play();

        // Show jumpscare image
        if (jumpscareImage)
        {
            jumpscareImage.gameObject.SetActive(true);
            jumpscareImage.color = new Color(1, 1, 1, 0);
            float fadeTimer = 0;
            while (fadeTimer < 0.2f)
            {
                fadeTimer += Time.unscaledDeltaTime;
                jumpscareImage.color = new Color(1, 1, 1, fadeTimer / 0.2f);
                yield return null;
            }
        }

        // Wait for animation
        yield return new WaitForSecondsRealtime(0.5f);

        // Show game over screen
        if (gameOverScreen) gameOverScreen.SetActive(true);

        // Reset timescale
        Time.timeScale = 1f;

        // Disable controls
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    IEnumerator WaitAtWaypoint()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(waypointWaitTime);
        agent.isStopped = false;
    }

    IEnumerator LookAround()
    {
        float duration = Random.Range(2f, 5f);
        float timer = 0f;
        float startAngle = transform.eulerAngles.y;
        float targetAngle = startAngle + Random.Range(90f, 270f);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration;
            float currentAngle = Mathf.LerpAngle(startAngle, targetAngle, progress);
            transform.rotation = Quaternion.Euler(0, currentAngle, 0);
            yield return null;
        }
    }

    IEnumerator FlickerLight(Light light)
    {
        float originalIntensity = light.intensity;
        int flickerCount = Random.Range(2, 5);

        for (int i = 0; i < flickerCount; i++)
        {
            light.intensity = 0;
            yield return new WaitForSeconds(Random.Range(0.05f, 0.2f));
            light.intensity = originalIntensity;
            yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
        }
    }

    IEnumerator OpenDoor(Transform door)
    {
        Quaternion startRot = door.rotation;
        Quaternion endRot = startRot * Quaternion.Euler(0, doorOpenAngle, 0);

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * doorOpenSpeed;
            door.rotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Light"))
        {
            Light light = other.GetComponent<Light>();
            if (light != null && !nearbyLights.Contains(light))
            {
                nearbyLights.Add(light);
            }
        }
        else if (other.CompareTag("Door"))
        {
            if (!nearbyDoors.Contains(other.transform))
            {
                nearbyDoors.Add(other.transform);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Light"))
        {
            Light light = other.GetComponent<Light>();
            if (light != null && nearbyLights.Contains(light))
            {
                nearbyLights.Remove(light);
            }
        }
        else if (other.CompareTag("Door"))
        {
            if (nearbyDoors.Contains(other.transform))
            {
                nearbyDoors.Remove(other.transform);
            }
        }
    }
}