using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class SpotlightWithGhost : MonoBehaviour
{
    private Light spotlight;
    private ReflectionProbe reflectionProbe;

    [Header("Light Flicker Settings")]
    public float minOnDuration = 1f;
    public float maxOnDuration = 30f;
    public float minOffDuration = 1f;
    public float maxOffDuration = 30f;
    public int blinkCount = 3;
    public float blinkInterval = 0.3f;

    [Header("Start Delay")]
    public float minStartDelay = 0f;
    public float maxStartDelay = 5f;

    [Header("Audio")]
    public AudioSource flickerAudioSource;
    public AudioClip flickerSound;

    public AudioSource ghostAudioSource;
    public AudioClip ghostVoice;
    public float ghostVoiceIntervalMin = 10f;
    public float ghostVoiceIntervalMax = 30f;

    private void Start()
    {
        spotlight = GetComponent<Light>();
     

        // Set baked lighting
        LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
        spotlight.shadows = LightShadows.None;

        // Set baked reflection probe
        reflectionProbe = FindObjectOfType<ReflectionProbe>();
        if (reflectionProbe)
            reflectionProbe.mode = ReflectionProbeMode.Baked;

        SetupFlickerAudio(); // Automatically setup audio

        // Start coroutines
        Invoke(nameof(StartLightRoutine), Random.Range(minStartDelay, maxStartDelay));
        StartCoroutine(GhostVoiceRoutine());
    }

    private void SetupFlickerAudio()
    {
        // Create a dedicated child GameObject for flicker sound
        GameObject flickerAudioObj = new GameObject("FlickerAudioSource");
        flickerAudioObj.transform.SetParent(transform);
        flickerAudioObj.transform.localPosition = Vector3.zero;

        // Add and configure AudioSource
        flickerAudioSource = flickerAudioObj.AddComponent<AudioSource>();
        flickerAudioSource.playOnAwake = false;
        flickerAudioSource.spatialBlend = 1f; // 3D audio
        flickerAudioSource.minDistance = 1f;
        flickerAudioSource.maxDistance = 15f;

        // Load audio clip from Resources if not already set
        if (flickerSound == null)
        {
            flickerSound = Resources.Load<AudioClip>("Flickering");
            if (flickerSound == null)
                Debug.LogWarning("Flickering.wav not found in Resources folder!");
        }

        flickerAudioSource.clip = flickerSound;
    }


    private void StartLightRoutine()
    {
        StartCoroutine(LightRoutine());
    }

    private IEnumerator LightRoutine()
    {
        while (true)
        {
            spotlight.enabled = true;
            yield return new WaitForSeconds(Random.Range(minOnDuration, maxOnDuration));

            for (int i = 0; i < blinkCount; i++)
            {
                spotlight.enabled = false;
                PlayFlickerSound();
                yield return new WaitForSeconds(blinkInterval);

                spotlight.enabled = true;
                PlayFlickerSound();
                yield return new WaitForSeconds(blinkInterval);
            }

            spotlight.enabled = false;
            yield return new WaitForSeconds(Random.Range(minOffDuration, maxOffDuration));
        }
    }

    private void PlayFlickerSound()
    {
        if (flickerAudioSource && flickerSound)
        {
            flickerAudioSource.PlayOneShot(flickerSound);
        }
    }

    private IEnumerator GhostVoiceRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(ghostVoiceIntervalMin, ghostVoiceIntervalMax));

            if (ghostAudioSource && ghostVoice)
            {
                ghostAudioSource.PlayOneShot(ghostVoice);
            }
        }
    }
}
