using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AchievementItem : MonoBehaviour
{
    public string achievementKey; // Unique Key for the achievement
    public TMP_Text achievementText;
    public GameObject tickMark; // Green Tick

    [HideInInspector] public bool isUnlocked = false;

    public void Unlock(bool unlocked)
    {
        isUnlocked = unlocked;
        tickMark.SetActive(unlocked);
    }
}
