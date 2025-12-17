using UnityEngine;
using System.Collections;

public class PlayerFearEffect : MonoBehaviour
{
    // ---------------------------
    // PLAYER STATE
    // ---------------------------
    [Header("Player State")]
    [Range(0f, 1f)] public float fearLevel;
    [Range(0f, 1f)] public float exhaustionLevel;
    public bool isSprinting;
    public bool isMoving;
    public bool isGrounded;

    // ---------------------------
    // FEAR SETTINGS
    // ---------------------------
    [Header("Fear Settings")]
    public float fearIncreaseSpeed = 0.9f;
    public float fearDecreaseSpeed = 0.6f;

    private bool enemyIsNear = false;
    private bool isInScaryZone = false;
    private bool darknessIsHigh = false;
    private bool jumpScareTriggered = false;

    public void SetEnemyNear(bool state) => enemyIsNear = state;
    public void SetScaryZone(bool state) => isInScaryZone = state;
    public void SetDarkness(bool state) => darknessIsHigh = state;
    public void TriggerJumpScare() => jumpScareTriggered = true;

    // ---------------------------
    // GLOBAL AUDIO VOLUME SYSTEM
    // ---------------------------
    [Header("Global Volume Scaling")]
    public float MasterSFXVolume = 1f; // For settings menu later

    [SerializeField, Range(0.5f, 3f)]
    public float footstepVolumeMultiplier = 1.8f; // NEW - makes footsteps louder safely

    [Tooltip("Fear → volume scaling curve")]
    public AnimationCurve fearToVolume = AnimationCurve.Linear(0, 0, 1, 1);

    [Tooltip("Exhaustion → volume scaling curve")]
    public AnimationCurve exhaustionToVolume = AnimationCurve.Linear(0, 0, 1, 1);

    public float sprintVolumeBoost = 0.15f;

    [Header("Audio Bounds")]
    [Range(0.1f, 2f)] public float minPitch = 0.9f;
    [Range(0.1f, 2f)] public float maxPitch = 1.3f;
    [Range(0f, 1f)] public float minVolume = 0.15f;
    [Range(0f, 1f)] public float maxVolume = 1f;

    private float ComputeVolume(float baseValue)
    {
        float v = baseValue;

        // Fear makes SFX louder
        v *= (1f + fearToVolume.Evaluate(fearLevel));

        // Exhaustion increases breathing + footsteps loudness
        v *= (1f + exhaustionToVolume.Evaluate(exhaustionLevel));

        // Sprinting → louder impacts
        if (isSprinting)
            v *= (1f + sprintVolumeBoost);

        // Master volume (settings menu)
        v *= MasterSFXVolume;

        return Mathf.Clamp(v, minVolume, maxVolume);
    }

    // ---------------------------
    // FOOTSTEPS
    // ---------------------------
    [Header("Footsteps")]
    public AudioSource footstepSource;
    public AudioClip[] footstepClips;
    public AudioClip[] snowFootstepClips;

    public float walkStepInterval = 0.5f;
    public float runStepInterval = 0.32f;

    [Header("Surface Detection")]
    public float rayOriginYOffset = 0.5f;
    public float rayDistance = 3f;
    public LayerMask surfaceLayerMask = ~0;

    private float nextStepTime = 0f;
    private string currentSurfaceTag = "Default";
    private int lastStepIndex = -1;
    private AudioClip[] currentSurfaceClips;

    // ---------------------------
    // BREATHING
    // ---------------------------
    [Header("Breathing")]
    public AudioSource breathingSource;
    public AudioClip[] calmBreathing;
    public AudioClip[] stressedBreathing;
    public AudioClip[] exhaustedBreathing;

    private float breathCooldown = 0f;
    private int lastBreathIndex = -1;

    // ---------------------------
    // HEARTBEAT
    // ---------------------------
    [Header("Heartbeat")]
    public AudioSource heartbeatSource;
    public AudioClip[] heartbeatClips; // Calm → Panic

    private float currentHeartRate = 70f;
    private float targetHeartRate = 70f;
    private bool panicCoroutineRunning = false;

    // ---------------------------
    // OPTIONAL ANIMATOR
    // ---------------------------
    [Header("Animator (Optional)")]
    public Animator playerAnimator;

    // ---------------------------
    // DEBUG
    // ---------------------------
    public bool debugLogs = false;

    // ---------------------------
    // UNITY LOOP
    // ---------------------------
    private void Update()
    {
        UpdateFearLevel();
        HandleFootsteps();
        HandleBreathing();
        HandleHeartbeat();
    }

    // ---------------------------
    // FEAR SYSTEM
    // ---------------------------
    private void UpdateFearLevel()
    {
        float targetFear = 0f;

        if (enemyIsNear) targetFear += 0.6f;
        if (isInScaryZone) targetFear += 0.25f;
        if (darknessIsHigh) targetFear += 0.35f;

        if (jumpScareTriggered) targetFear = 1f;

        targetFear = Mathf.Clamp01(targetFear);

        // Smooth transitions
        float rate = (targetFear > fearLevel) ? fearIncreaseSpeed : fearDecreaseSpeed;
        fearLevel = Mathf.MoveTowards(fearLevel, targetFear, rate * Time.deltaTime);

        if (jumpScareTriggered && fearLevel >= 0.95f)
            jumpScareTriggered = false;
    }

    // ---------------------------
    // FOOTSTEPS
    // ---------------------------
    private void HandleFootsteps()
    {
        if (footstepSource == null) return;
        if (!isGrounded || !isMoving) return;

        DetectSurface();

        float interval = isSprinting ? runStepInterval : walkStepInterval;
        if (fearLevel > 0.6f) interval *= 0.85f;
        if (exhaustionLevel > 0.7f) interval *= 1.15f;

        if (Time.time >= nextStepTime)
        {
            PlayFootstep();
            nextStepTime = Time.time + Mathf.Max(0.05f, interval);
        }
    }

    private void DetectSurface()
    {
        RaycastHit hit;
        Vector3 origin = transform.position + Vector3.up * rayOriginYOffset;

        if (Physics.Raycast(origin, Vector3.down, out hit, rayDistance, surfaceLayerMask))
            currentSurfaceTag = hit.collider.tag;
        else
            currentSurfaceTag = "Default";
    }

    private void PlayFootstep()
    {
        // choose clip array
        if (currentSurfaceTag == "Snow" && snowFootstepClips != null && snowFootstepClips.Length > 0)
            currentSurfaceClips = snowFootstepClips;
        else
            currentSurfaceClips = footstepClips;

        if (currentSurfaceClips == null || currentSurfaceClips.Length == 0)
        {
            if (debugLogs) Debug.LogWarning("[Footsteps] No clips found for current surface.");
            return;
        }

        int index;
        do
        {
            index = Random.Range(0, currentSurfaceClips.Length);
        } while (index == lastStepIndex && currentSurfaceClips.Length > 1);

        lastStepIndex = index;

        // base pitch/volume
        float pitch = 1f;
        float volume = 1f;

        // sprinting
        if (isSprinting) { pitch *= 1.08f; volume *= 1.15f; }

        // fear makes steps sharper or heavier depending on exhaustion
        if (fearLevel > 0.5f) { pitch *= 0.97f; volume *= 1.15f; }
        if (exhaustionLevel > 0.7f) { pitch *= 0.9f; volume *= 1.25f; }

        // slight randomization
        pitch *= Random.Range(0.96f, 1.04f);
        volume *= Random.Range(0.9f, 1.1f);

        // 🔥 NEW louder volume (multiply AFTER all logic)
        volume *= footstepVolumeMultiplier;

        // clamp to safe ranges
        footstepSource.pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        footstepSource.volume = Mathf.Clamp(volume, minVolume, maxVolume);

        // PlayOneShot is correct here
        AudioClip clip = currentSurfaceClips[index];
        if (clip != null)
        {
            footstepSource.PlayOneShot(clip);
            if (debugLogs)
                Debug.Log($"[Footsteps] Playing '{clip.name}' loud:{footstepSource.volume:F2} pitch:{footstepSource.pitch:F2}");
        }
    }
    // ---------------------------
    // BREATHING
    // ---------------------------
    private void HandleBreathing()
    {
        if (breathingSource == null) return;

        breathCooldown -= Time.deltaTime;

        if (breathCooldown <= 0f || !breathingSource.isPlaying)
            PlayBreath();
    }

    private void PlayBreath()
    {
        AudioClip[] clips = calmBreathing;

        if (exhaustionLevel > 0.75f && exhaustedBreathing.Length > 0)
            clips = exhaustedBreathing;
        else if (fearLevel > 0.4f && stressedBreathing.Length > 0)
            clips = stressedBreathing;

        if (clips.Length == 0) return;

        int index;
        do { index = Random.Range(0, clips.Length); }
        while (index == lastBreathIndex && clips.Length > 1);
        lastBreathIndex = index;

        float baseVolume = Mathf.Lerp(0.35f, 1f, Mathf.Max(fearLevel, exhaustionLevel));
        float pitch = Mathf.Lerp(1f, 1.25f, fearLevel);

        breathingSource.pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        breathingSource.volume = ComputeVolume(baseVolume);

        breathingSource.PlayOneShot(clips[index]);

        breathCooldown = Mathf.Lerp(3.5f, 1.3f, Mathf.Max(fearLevel, exhaustionLevel));
    }

    // ---------------------------
    // HEARTBEAT
    // ---------------------------
    private void HandleHeartbeat()
    {
        if (heartbeatSource == null || heartbeatClips.Length == 0) return;

        targetHeartRate = Mathf.Lerp(65f, 160f, fearLevel) + (isSprinting ? 15f : 0f);
        currentHeartRate = Mathf.Lerp(currentHeartRate, targetHeartRate, Time.deltaTime * 3f);

        int clipIndex = Mathf.Clamp(Mathf.RoundToInt(fearLevel * (heartbeatClips.Length - 1)), 0, heartbeatClips.Length - 1);

        if (heartbeatSource.clip != heartbeatClips[clipIndex] || !heartbeatSource.isPlaying)
        {
            heartbeatSource.clip = heartbeatClips[clipIndex];
            heartbeatSource.loop = true;
            heartbeatSource.Play();
        }

        heartbeatSource.pitch = Mathf.Clamp(1f + fearLevel * 0.28f, minPitch, maxPitch);
        heartbeatSource.volume = ComputeVolume(Mathf.Lerp(0.15f, 1f, fearLevel));

        if (fearLevel > 0.95f && !panicCoroutineRunning)
            StartCoroutine(PanicBeat());
    }

    private IEnumerator PanicBeat()
    {
        panicCoroutineRunning = true;

        float originalPitch = heartbeatSource.pitch;
        heartbeatSource.pitch = Mathf.Clamp(originalPitch * 0.85f, minPitch, maxPitch);
        yield return new WaitForSeconds(0.08f);

        heartbeatSource.pitch = Mathf.Clamp(originalPitch * 1.4f, minPitch, maxPitch);
        yield return new WaitForSeconds(0.08f);

        heartbeatSource.pitch = originalPitch;
        yield return new WaitForSeconds(1f);

        panicCoroutineRunning = false;
    }

    // ---------------------------
    // API — FIRST PERSON CONTROLLER MOVEMENT INPUT
    // ---------------------------
    public void UpdateMovementState(float currentSpeed, bool isMovingInput, bool isSprintingInput, bool grounded)
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetFloat("Speed", currentSpeed);
            playerAnimator.SetBool("isWalking", isMovingInput && !isSprintingInput);
            playerAnimator.SetBool("isRunning", isMovingInput && isSprintingInput);
            playerAnimator.SetBool("isGrounded", grounded);
        }

        isMoving = isMovingInput;
        isSprinting = isSprintingInput;
        isGrounded = grounded;
    }
}
