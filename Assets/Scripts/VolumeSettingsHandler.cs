using UnityEngine;
using UnityEngine.UI;

public class VolumeSettingsHandler : MonoBehaviour
{
    public Slider volumeSlider;
    public Image volumeIcon; // 🔊 Reference to the UI image
    public Sprite volumeSprite; // Speaker icon
    public Sprite noVolumeSprite; // Muted icon

    private void Start()
    {
        float savedVolume = PlayerPrefs.GetFloat("Volume", 0.5f);
        AudioListener.volume = savedVolume;
        volumeSlider.value = savedVolume;
        UpdateVolumeIcon(savedVolume);

        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void SetVolume(float value)
    {
        AudioListener.volume = value;
        PlayerPrefs.SetFloat("Volume", value);
        PlayerPrefs.Save();

        UpdateVolumeIcon(value);
    }

    private void UpdateVolumeIcon(float volume)
    {
        if (volumeIcon == null || volumeSprite == null || noVolumeSprite == null)
            return;

        volumeIcon.sprite = volume <= 0.01f ? noVolumeSprite : volumeSprite;
    }
}
