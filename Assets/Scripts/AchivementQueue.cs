using System.Collections.Generic;
using UnityEngine;

public static class AchievementQueue
{
    private const string Key = "QueuedAchievements";

    public static void QueueAchievement(string id)
    {
        string queued = PlayerPrefs.GetString(Key, "");
        if (!queued.Contains(id))
        {
            queued += id + ";";
            PlayerPrefs.SetString(Key, queued);
            PlayerPrefs.Save();
        }
    }

    public static List<string> GetAndClearQueuedAchievements()
    {
        string data = PlayerPrefs.GetString(Key, "");
        PlayerPrefs.DeleteKey(Key);
        PlayerPrefs.Save();

        return new List<string>(data.Split(new[] { ';' }, System.StringSplitOptions.RemoveEmptyEntries));
    }
}
