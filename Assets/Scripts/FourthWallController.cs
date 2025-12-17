using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using StarterAssets;

public class IdleTimeDialogue : MonoBehaviour
{
    [System.Serializable]
    public class DialogueAudio
    {
        public AudioClip englishClip;
        public AudioClip hindiClip;
        public AudioClip malayalamClip;
        [TextArea(3, 5)] public string englishSubtitle;
        [TextArea(3, 5)] public string hindiSubtitle;
        [TextArea(3, 5)] public string malayalamSubtitle;
    }

    [Header("Dialogues")]
    [SerializeField] private List<DialogueAudio> idleDialogues = new List<DialogueAudio>();
    [SerializeField] private List<DialogueAudio> torchDialogues = new List<DialogueAudio>();

    [Header("Timers")]
    [SerializeField] private float minIdleTime = 10f;
    [SerializeField] private float maxIdleTime = 30f;
    [SerializeField] private float torchDialogueMinTime = 40f;
    [SerializeField] private float torchDialogueMaxTime = 60f;
    [SerializeField] private float interactionTimeout = 3f;
    [SerializeField] private float dialogueCooldownTime = 3f;

    [Header("References")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SubtitleDisplay subtitleDisplay;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private GameObject torchObject;

    private GameSettings.Language language;
    private FirstPersonController playerController;
    private float lastInteractionTime;
    private bool isPlayerIdle;
    private bool isPlayingDialogue;
    private bool torchDialoguePlayed;
    private bool dialogueCooldown;

    private void Start()
    {
        language = GameSettings.GetCurrentLanguage();
        playerController = FindObjectOfType<FirstPersonController>();
        lastInteractionTime = Time.time;

        StartCoroutine(IdleDialogueRoutine());
        StartCoroutine(TorchDialogueRoutine());
    }

    private void Update()
    {
        if (isPlayingDialogue || IsGameOver()) return;

        if (Input.anyKeyDown || Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.1f)
        {
            lastInteractionTime = Time.time;
            isPlayerIdle = false;
        }
        else if (Time.time - lastInteractionTime > interactionTimeout)
        {
            isPlayerIdle = true;
        }
    }

    private IEnumerator IdleDialogueRoutine()
    {
        
        while (true)
        {
            if (isPlayerIdle && idleDialogues.Count > 0 && !isPlayingDialogue && !dialogueCooldown && !IsGameOver())
            {
                float waitTime = Random.Range(minIdleTime, maxIdleTime);
                float timer = 0f;

                while (timer < waitTime)
                {
                    if (!isPlayerIdle || isPlayingDialogue || IsGameOver()) break;
                    timer += Time.deltaTime;
                    yield return null;
                }

                if (isPlayerIdle && !isPlayingDialogue && !dialogueCooldown && !IsGameOver())
                {
                    DialogueAudio dialogue = idleDialogues[Random.Range(0, idleDialogues.Count)];
                    yield return StartCoroutine(PlayDialogue(dialogue));
                    yield return StartCoroutine(StartDialogueCooldown());
                }
            }
            yield return null;
        }
    }

    private IEnumerator TorchDialogueRoutine()
    {
        while (!torchDialoguePlayed)
        {
            float waitTime = Random.Range(torchDialogueMinTime, torchDialogueMaxTime);
            float timer = 0f;

            while (timer < waitTime)
            {
                if (IsGameOver() || IsTorchEquipped()) yield break;
                timer += Time.deltaTime;
                yield return null;
            }

            // Wait for any other dialogue cooldown if necessary
            while (isPlayingDialogue || dialogueCooldown)
                yield return null;

            if (!torchDialoguePlayed && !IsTorchEquipped() && torchObject != null && torchObject.activeInHierarchy && torchDialogues.Count > 0 && !IsGameOver())
            {
                torchDialoguePlayed = true;
                DialogueAudio dialogue = torchDialogues[Random.Range(0, torchDialogues.Count)];
                yield return StartCoroutine(PlayDialogue(dialogue));
                yield return StartCoroutine(StartDialogueCooldown());
            }
        }
    }

    private bool IsTorchEquipped()
    {
        TorchSystem torchController = torchObject?.GetComponent<TorchSystem>();
        return torchController != null && torchController.isPicked;
    }

    private IEnumerator PlayDialogue(DialogueAudio dialogue)
    {
        isPlayingDialogue = true;
        if (playerController != null)
            playerController.enabled = false;

        AudioClip clip = null;
        string subtitle = "";

        switch (language)
        {
            case GameSettings.Language.Hindi:
                clip = dialogue.hindiClip;
                subtitle = dialogue.hindiSubtitle;
                break;
            case GameSettings.Language.Malayalam:
                clip = dialogue.malayalamClip;
                subtitle = dialogue.malayalamSubtitle;
                break;
            default:
                clip = dialogue.englishClip;
                subtitle = dialogue.englishSubtitle;
                break;
        }

        Quaternion originalRotation = cameraTransform.localRotation;

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
            subtitleDisplay?.Show(subtitle, clip.length);
        }

        float effectTime = 0f;
        float maxEffectTime = Mathf.Min(clip.length, 8f);

        while (effectTime < maxEffectTime)
        {
            float sway = 0.1f * Mathf.Sin(effectTime * 2f);
            float vertical = 0.05f * Mathf.Cos(effectTime * 1.5f);
            float roll = Mathf.Sin(effectTime * 0.9f) * 1f;

            Quaternion target = Quaternion.Euler(
                originalRotation.eulerAngles.x + vertical * 10f,
                originalRotation.eulerAngles.y + sway * 15f,
                roll
            );

            cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, target, Time.deltaTime * 3f);
            effectTime += Time.deltaTime;
            yield return null;
        }

        float returnTime = 0f;
        while (returnTime < 1f)
        {
            cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, originalRotation, returnTime);
            returnTime += Time.deltaTime * 2f;
            yield return null;
        }

        if (clip != null)
            yield return new WaitForSeconds(Mathf.Max(0, clip.length - maxEffectTime));

        if (playerController != null)
            playerController.enabled = true;

        subtitleDisplay?.Hide();
        isPlayingDialogue = false;
    }

    private IEnumerator StartDialogueCooldown()
    {
        dialogueCooldown = true;
        yield return new WaitForSeconds(dialogueCooldownTime);
        dialogueCooldown = false;
    }

    private bool IsGameOver()
    {
        return GameStateManager.Instance != null && GameStateManager.Instance.IsGameOver;
    }

    public void StopAllDialogues()
    {
        if (audioSource != null)
            audioSource.Stop();
        subtitleDisplay?.Hide();
        StopAllCoroutines();
    }

    private void OnDestroy()
    {
        StopAllDialogues();
    }

    public static void ResetForNewScene()
    {
        // Not using singleton pattern anymore
    }
}
