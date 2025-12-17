using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public AudioSource dialogueAudioSource; // AudioSource for playing voiceovers
    public AudioClip gameStartDialogue; // Voiceover for game start
    public AudioClip lightsOffDialogue; // Voiceover for lights off
    public float gameStartDelay = 5f; // Delay before playing the game start dialogue
    public Light[] lights; // Array of lights to check if they are off
    public Transform player; // Reference to the player
    public float lightDetectionRadius = 10f; // Radius to detect nearby lights

    public float volumeMultiplier = 12f; // Volume multiplier to increase the audio level
    private bool lightsOffTriggered = false;

    void Start()
    {
        // Set the AudioSource volume, clamping the result to 1
        dialogueAudioSource.volume = Mathf.Clamp(dialogueAudioSource.volume * volumeMultiplier, 0, 1);

        // Start the game start dialogue after a delay
        Invoke(nameof(PlayGameStartDialogue), gameStartDelay);
    }

    void Update()
    {
        // Check if all nearby lights are off
        if (!lightsOffTriggered && AreAllLightsOff())
        {
            lightsOffTriggered = true;
            PlayLightsOffDialogue();
        }
    }

    void PlayGameStartDialogue()
    {
        // Play the game start voiceover
        PlayAudioClip(gameStartDialogue);
    }

    void PlayLightsOffDialogue()
    {
        // Play the lights-off voiceover
        PlayAudioClip(lightsOffDialogue);
    }

    void PlayAudioClip(AudioClip clip)
    {
        if (clip != null && dialogueAudioSource != null)
        {
            dialogueAudioSource.clip = clip;
            dialogueAudioSource.Play();
        }
    }

    bool AreAllLightsOff()
    {
        foreach (Light light in lights)
        {
            // Check if the light is within the detection radius and is turned on
            if (light != null && Vector3.Distance(player.position, light.transform.position) <= lightDetectionRadius && light.enabled)
            {
                return false; // If any light is on, return false
            }
        }
        return true; // All nearby lights are off
    }
}
