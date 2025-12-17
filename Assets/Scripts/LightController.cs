/*using UnityEngine;

public class LightControllerManager : MonoBehaviour
{
    public Transform player;
    public float triggerDistance = 80f;

    public AudioSource sharedFlickerSource;
    public AudioClip sharedFlickerClip;
    public float minVolume = 0.1f;
    public float maxVolume = 1f;

    private SpotlightWithGhost[] allLights;

    void Start()
    {
        allLights = FindObjectsOfType<SpotlightWithGhost>();
        foreach (var light in allLights)
        {
            light.manager = this;
        }
    }

    void Update()
    {
        SpotlightWithGhost nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var light in allLights)
        {
            float dist = Vector3.Distance(player.position, light.transform.position);
            bool inRange = dist <= triggerDistance;
            light.SetActiveState(inRange);

            if (inRange && dist < minDist)
            {
                nearest = light;
                minDist = dist;
            }
        }

        if (nearest != null && Random.value < nearest.flickerProbability * Time.deltaTime)
        {
            nearest.TriggerFlicker();
        }
    }

    public void PlaySharedFlickerSound(Vector3 position, float distance)
    {
        if (!sharedFlickerSource || !sharedFlickerClip) return;

        sharedFlickerSource.transform.position = position;
        sharedFlickerSource.volume = Mathf.Lerp(maxVolume, minVolume, distance / triggerDistance);
        sharedFlickerSource.PlayOneShot(sharedFlickerClip);
    }
}*/
