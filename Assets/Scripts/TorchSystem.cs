using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class TorchSystem : MonoBehaviour
{
    [Header("Setup")]
    public Transform handPosition;
    public Camera playerCamera;
    public GameObject pickButton;
    public GameObject toggleTorchButton;
    public Image chargeStateImage;
    public Sprite[] chargeStateSprites;

    [Header("Torch Settings")]
    public Light torchLight;
    public float torchDuration = 10f;
    public float rechargeTime = 5f;
    public int maxCharges = 3;

    [Header("Sounds")]
    public AudioClip pickSound;
    public AudioClip torchOnSound;
    public AudioClip torchOffSound;
    public AudioClip flickerSound;

    [Header("Detection")]
    public float detectionRange = 2f;
    public float torchVisibleAngle = 75f;

    [Header("Outline")]
    public Color defaultColor = Color.yellow;
    public Color detectedColor = Color.green;

    private AudioSource audioSource;
    public bool isPicked = false;
    private bool isLit = false;
    private float currentCharge = 0;
    private int remainingCharges;
    private float rechargeTimer = 0f;

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;
    private float torchMoveLimit = 0.1f;
    private float smoothSpeed = 5f;

    private Outline outline;
    private int lastChargePoint = 3;
    private bool isPlayerNearby = false;
    private bool isFlickering = false;

    private void Start()
    {
        pickButton.SetActive(false);
        toggleTorchButton.SetActive(false);

        if (torchLight != null)
            torchLight.enabled = false;

        currentCharge = torchDuration;
        remainingCharges = maxCharges;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            Debug.LogError("AudioSource missing on " + gameObject.name);

        if (pickButton != null)
            pickButton.GetComponent<Button>().onClick.AddListener(PickTorch);

        if (chargeStateImage != null)
            chargeStateImage.gameObject.SetActive(false);

        outline = GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
            outline.OutlineColor = defaultColor;
        }
    }

    private void Update()
    {
        if (!isPicked)
        {
            DetectTorch();
            UpdateOutlineState();
        }
        else
        {
            HandleTorchMovement();

            if (Input.GetKeyDown(KeyCode.F))
                ToggleTorch();

            if (isLit)
            {
                currentCharge -= Time.deltaTime;

                int currentPoint = Mathf.CeilToInt(currentCharge / (torchDuration / maxCharges));

                if (currentPoint < lastChargePoint && currentPoint >= 1)
                {
                    StartCoroutine(FlickerTorch());
                    lastChargePoint = currentPoint;
                }

                if (currentCharge <= 0f)
                {
                    remainingCharges--;

                    if (remainingCharges > 0)
                    {
                        currentCharge = torchDuration;
                        lastChargePoint = maxCharges;
                    }
                    else
                    {
                        TurnOffTorch();
                        rechargeTimer = rechargeTime;
                    }
                }
            }

            if (remainingCharges < maxCharges)
            {
                rechargeTimer -= Time.deltaTime;
                if (rechargeTimer <= 0)
                {
                    remainingCharges++;
                    rechargeTimer = rechargeTime;

                    if (!isLit && remainingCharges > 0)
                    {
                        currentCharge = torchDuration;
                        lastChargePoint = maxCharges;
                    }
                }
            }

            UpdateChargeStateImage();
        }

        if (!isPicked && isPlayerNearby && Input.GetKeyDown(KeyCode.E))
        {
            PickTorch();
        }
    }

    private void DetectTorch()
    {
        isPlayerNearby = false;
        Vector3 directionToTorch = transform.position - playerCamera.transform.position;
        float distance = directionToTorch.magnitude;
        float angle = Vector3.Angle(playerCamera.transform.forward, directionToTorch);

        if (distance <= detectionRange && angle <= torchVisibleAngle)
        {
            Ray ray = new Ray(playerCamera.transform.position, directionToTorch.normalized);
            if (Physics.Raycast(ray, out RaycastHit hit, detectionRange + 1f))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    isPlayerNearby = true;
                    pickButton.SetActive(true);
                    return;
                }
            }
        }

        pickButton.SetActive(false);
    }

    private void UpdateOutlineState()
    {
        if (outline == null || playerCamera == null || isPicked) return;

        Vector3 viewportPos = playerCamera.WorldToViewportPoint(transform.position);
        bool isVisible = viewportPos.x >= 0 && viewportPos.x <= 1 &&
                         viewportPos.y >= 0 && viewportPos.y <= 1 &&
                         viewportPos.z > 0;

        outline.enabled = isVisible;

        if (!isVisible) return;

        Vector3 direction = transform.position - playerCamera.transform.position;
        float angle = Vector3.Angle(direction, playerCamera.transform.forward);

        Ray ray = new Ray(playerCamera.transform.position, direction.normalized);
        if (Physics.Raycast(ray, out RaycastHit hit, detectionRange + 1f))
        {
            if (hit.collider.gameObject == gameObject && angle <= torchVisibleAngle)
            {
                outline.OutlineColor = detectedColor;
                return;
            }
        }

        outline.OutlineColor = defaultColor;
    }

    public void PickTorch()
    {
        if (isPicked) return;

        isPicked = true;
        pickButton.SetActive(false);
        toggleTorchButton.SetActive(true);

        transform.SetParent(handPosition);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;

        originalLocalPosition = transform.localPosition;
        originalLocalRotation = transform.localRotation;

        if (outline != null)
            outline.enabled = false;

        if (chargeStateImage != null)
            chargeStateImage.gameObject.SetActive(true);

        PlaySound(pickSound);
    }

    public void ToggleTorch()
    {
        if (isLit)
            TurnOffTorch();
        else if (remainingCharges > 0)
            TurnOnTorch();
    }

    private void TurnOnTorch()
    {
        isLit = true;
        torchLight.enabled = true;
        currentCharge = torchDuration;
        lastChargePoint = maxCharges;
        PlaySound(torchOnSound);
    }

    public void TurnOffTorch()
    {
        isLit = false;
        torchLight.enabled = false;
        PlaySound(torchOffSound);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
            Debug.Log("Playing sound: " + clip.name);
        }
        else
        {
            Debug.LogWarning("Missing audio clip or AudioSource on: " + gameObject.name);
        }
    }

    private void UpdateChargeStateImage()
    {
        if (chargeStateImage != null && chargeStateSprites != null && chargeStateSprites.Length > 0)
        {
            int index = Mathf.Clamp(remainingCharges, 0, chargeStateSprites.Length - 1);
            chargeStateImage.sprite = chargeStateSprites[index];
        }
    }

    private void HandleTorchMovement()
    {
        if (!isPicked) return;

        float verticalMove = Mathf.Clamp(playerCamera.transform.eulerAngles.x, -torchMoveLimit, torchMoveLimit);
        float horizontalMove = Mathf.Clamp(playerCamera.transform.eulerAngles.y, -torchMoveLimit, torchMoveLimit);

        float jitterX = Mathf.PerlinNoise(Time.time * 2f, 0) * 0.01f;
        float jitterY = Mathf.PerlinNoise(0, Time.time * 2f) * 0.01f;

        Vector3 targetPosition = originalLocalPosition + new Vector3(horizontalMove * 0.01f + jitterX, verticalMove * 0.01f + jitterY, 0);
        Quaternion targetRotation = originalLocalRotation * Quaternion.Euler(verticalMove * 2f, horizontalMove * 2f, 0);

        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * smoothSpeed);
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * smoothSpeed);
    }

    public IEnumerator FlickerTorch(float totalDuration = 1f, float interval = 0.05f)
    {
        if (torchLight == null || isFlickering) yield break;

        isFlickering = true;

        float endTime = Time.time + totalDuration;
        while (Time.time < endTime)
        {
            torchLight.enabled = !torchLight.enabled;
            PlaySound(flickerSound);
            yield return new WaitForSeconds(interval);
        }

        torchLight.enabled = true;
        isFlickering = false;
    }
}
