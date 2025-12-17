using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EyeAnimation : MonoBehaviour
{
    public Image topEyelidImage;  // Reference to the top eyelid Image component
    public Image bottomEyelidImage; // Reference to the bottom eyelid Image component
    public float animationSpeed = 2f;  // Speed of the animation

    private bool eyesOpen = false;  // Start with the eyes closed by default

    void Start()
    {
        CloseEyes(); // Start with the eyes closed by default
    }

    void Update()
    {
        // Toggle eye animation when the player presses the space bar
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (eyesOpen)
            {
                CloseEyes();
            }
            else
            {
                OpenEyes();
            }
        }
    }

    public void OpenEyes()
    {
        eyesOpen = true;
        StartCoroutine(AnimateEyelid(topEyelidImage, 0f));  // Move top eyelid up
        StartCoroutine(AnimateEyelid(bottomEyelidImage, 0f));  // Move bottom eyelid down
    }

    public void CloseEyes()
    {
        eyesOpen = false;
        StartCoroutine(AnimateEyelid(topEyelidImage, 1f));  // Move top eyelid down
        StartCoroutine(AnimateEyelid(bottomEyelidImage, 1f));  // Move bottom eyelid up
    }

    private IEnumerator AnimateEyelid(Image eyelid, float targetPosition)
    {
        float currentPosition = eyelid.rectTransform.localPosition.y;
        float timeElapsed = 0f;

        while (timeElapsed < animationSpeed)
        {
            timeElapsed += Time.deltaTime;
            float newPosition = Mathf.Lerp(currentPosition, targetPosition, timeElapsed / animationSpeed);
            eyelid.rectTransform.localPosition = new Vector3(eyelid.rectTransform.localPosition.x, newPosition, eyelid.rectTransform.localPosition.z); // Move vertically
            yield return null;
        }

        eyelid.rectTransform.localPosition = new Vector3(eyelid.rectTransform.localPosition.x, targetPosition, eyelid.rectTransform.localPosition.z); // Ensure exact position after animation
    }
}
