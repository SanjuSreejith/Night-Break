using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

[RequireComponent(typeof(NavMeshAgent))]
public class CreepyDollAI : MonoBehaviour
{
    private enum DollState { Idle, Stalking, Chasing, Teleporting }
    private DollState state = DollState.Idle;

    [Header("References")]
    public Transform player;
    public AudioSource walkSound;
    public AudioSource chaseMusic;
    public AudioSource movementMusic;
    public Animator animator;
    public List<Transform> teleportPoints;

    [Header("Movement Settings")]
    public float followDistance = 3f;
    public float walkDelay = 5f;
    public float normalSpeed = 1.5f;
    public float chaseSpeed = 4.5f;
    public float chaseStopDistance = 1.5f;

    [Header("Teleport Settings")]
    public float teleportInterval = 20f;
    [Range(0f, 1f)] public float skipTeleportChance = 0.3f;
    public float chaseNearPlayerThreshold = 3f;

    [Header("Sound Settings")]
    public float minMoveVolume = 0.1f;
    public float maxMoveVolume = 0.6f;

    [Header("Chase Triggers")]
    public Vector2Int lookTriggerRange = new Vector2Int(2, 5);
    private int idleSeenCount;
    private int requiredIdleSeen = 3;
    private bool chaseTriggered;

    private NavMeshAgent agent;
    private float lastSeenTime;
    private float nextTeleportTime;
    private float nextWalkTime;
    private bool wasSeen;
    private bool movementCaughtThisCycle;
    private float chaseNearPlayerStartTime;

    private List<Transform> shuffledTeleportPoints = new();
    private int lastTeleportIndex = -1;
    private Camera mainCam;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = normalSpeed;

        mainCam = Camera.main;
        ShuffleTeleportPoints();
        ScheduleTeleport();
        nextWalkTime = Time.time + walkDelay;
        SetAnimIdle();
        ResetChaseTriggers();
    }

    void Update()
    {
        if (!player || !agent) return;

        Vector3 dollPos = transform.position;
        Vector3 playerPos = player.position;
        bool isSeen = CheckPlayerVisibility(dollPos);

        if (isSeen)
        {
            lastSeenTime = Time.time;
            wasSeen = true;

            if (state == DollState.Stalking)
            {
                if (IsActuallyMoving() && !movementCaughtThisCycle && !chaseTriggered)
                {
                    movementCaughtThisCycle = true;
                    StartChase();
                    return;
                }
                else if (!IsActuallyMoving() && !chaseTriggered)
                {
                    idleSeenCount++;
                    if (idleSeenCount >= requiredIdleSeen)
                    {
                        StartChase();
                        return;
                    }
                }
            }

            StopWalkSound();
            SetAnimIdle();
        }

        switch (state)
        {
            case DollState.Idle:
                if (!isSeen && Time.time >= nextWalkTime)
                    StartStalk();
                break;

            case DollState.Stalking:
                HandleStalk(isSeen, playerPos, dollPos);
                break;

            case DollState.Chasing:
                HandleChase(playerPos);
                float sqrDist = (dollPos - playerPos).sqrMagnitude;

                if (sqrDist <= chaseStopDistance * chaseStopDistance)
                {
                    if (chaseNearPlayerStartTime == 0f)
                        chaseNearPlayerStartTime = Time.time;

                    if (Time.time - chaseNearPlayerStartTime >= chaseNearPlayerThreshold)
                    {
                        Teleport();
                        chaseNearPlayerStartTime = 0f;
                    }
                }
                else
                {
                    chaseNearPlayerStartTime = 0f;
                }
                break;
        }

        SyncFootstepSound();
        UpdateMovementMusic();
    }

    void HandleStalk(bool isSeen, Vector3 playerPos, Vector3 dollPos)
    {
        if (Time.time - lastSeenTime < 1.5f)
        {
            agent.ResetPath();
            SetAnimIdle();
            return;
        }

        Vector3 toDoll = (dollPos - playerPos).normalized;
        float angleDot = Vector3.Dot(player.forward, toDoll);

        if (angleDot > -0.4f || Random.value < 0.4f)
        {
            agent.ResetPath();
            SetAnimIdle();
            return;
        }

        Vector3 behind = playerPos - player.forward * followDistance;
        agent.speed = normalSpeed;
        agent.SetDestination(behind);
        SetAnimWalk();
    }

    void HandleChase(Vector3 playerPos)
    {
        agent.speed = chaseSpeed;
        agent.SetDestination(playerPos);
        SetAnimRun();
    }

    void StartStalk()
    {
        state = DollState.Stalking;
        SetAnimWalk();
        movementCaughtThisCycle = false;
    }

    void StartChase()
    {
        state = DollState.Chasing;
        agent.speed = chaseSpeed;
        SetAnimRun();
        PlayChaseMusic();
    }

    void Teleport()
    {
        state = DollState.Teleporting;

        if (shuffledTeleportPoints.Count > 0)
        {
            lastTeleportIndex = (lastTeleportIndex + 1) % shuffledTeleportPoints.Count;
            Transform point = shuffledTeleportPoints[lastTeleportIndex];

            if (NavMesh.SamplePosition(point.position, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                transform.position = hit.position;

            agent.ResetPath();
        }

        agent.speed = normalSpeed;
        wasSeen = false;
        movementCaughtThisCycle = false;
        ScheduleTeleport();
        nextWalkTime = Time.time + walkDelay;
        state = DollState.Idle;
        SetAnimIdle();
        StopWalkSound();
        StopChaseMusic();
        ResetChaseTriggers();
    }

    void ScheduleTeleport()
    {
        float delay = teleportInterval + Random.Range(-2f, 2f);

        if (state == DollState.Idle && Time.timeSinceLevelLoad < 5f && Random.value < skipTeleportChance)
            delay += teleportInterval;

        nextTeleportTime = Time.time + delay;
    }

    void ShuffleTeleportPoints()
    {
        shuffledTeleportPoints = new List<Transform>(teleportPoints);
        for (int i = 0; i < shuffledTeleportPoints.Count; i++)
        {
            int rand = Random.Range(i, shuffledTeleportPoints.Count);
            (shuffledTeleportPoints[i], shuffledTeleportPoints[rand]) = (shuffledTeleportPoints[rand], shuffledTeleportPoints[i]);
        }
    }

    void ResetChaseTriggers()
    {
        idleSeenCount = 0;
        requiredIdleSeen = Random.Range(lookTriggerRange.x, lookTriggerRange.y + 1);
        chaseTriggered = false;
    }

    bool CheckPlayerVisibility(Vector3 dollPos)
    {
        if (!mainCam) return false;

        Vector3 viewportPos = mainCam.WorldToViewportPoint(dollPos);
        if (viewportPos.z < 0 || viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
            return false;

        Vector3 dir = dollPos - mainCam.transform.position;
        return Physics.Raycast(mainCam.transform.position, dir.normalized, out RaycastHit hit) && hit.transform == transform;
    }

    bool IsActuallyMoving() => agent.velocity.sqrMagnitude > 0.01f;

    void SyncFootstepSound()
    {
        bool shouldPlay = IsActuallyMoving() && state != DollState.Teleporting;
        if (shouldPlay && !walkSound.isPlaying) walkSound.Play();
        else if (!shouldPlay && walkSound.isPlaying) walkSound.Stop();
    }

    void UpdateMovementMusic()
    {
        if (!movementMusic) return;

        if (state == DollState.Stalking && !CheckPlayerVisibility(transform.position))
        {
            float speed = agent.velocity.magnitude;
            movementMusic.volume = Mathf.Lerp(minMoveVolume, maxMoveVolume, speed / chaseSpeed);
            if (!movementMusic.isPlaying && speed > 0.1f)
                movementMusic.Play();
        }
        else if (movementMusic.isPlaying)
        {
            movementMusic.Stop();
        }
    }

    void StopWalkSound() { if (walkSound?.isPlaying == true) walkSound.Stop(); }
    void PlayChaseMusic() { if (chaseMusic && !chaseMusic.isPlaying) chaseMusic.Play(); }
    void StopChaseMusic() { if (chaseMusic?.isPlaying == true) chaseMusic.Stop(); }

    void SetAnimWalk() { if (animator) { animator.SetBool("IsWalking", true); animator.ResetTrigger("Run"); } }
    void SetAnimRun() { if (animator) { animator.SetBool("IsWalking", false); animator.SetTrigger("Run"); } }
    void SetAnimIdle() { if (animator) { animator.SetBool("IsWalking", false); animator.ResetTrigger("Run"); } }
}
