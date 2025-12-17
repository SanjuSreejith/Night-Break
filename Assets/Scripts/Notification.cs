using UnityEngine;
using Unity.Notifications.Android;
using System.Collections.Generic;
using System;

public class HorrorNotificationManager : MonoBehaviour
{
    private static HorrorNotificationManager _instance;
    public static HorrorNotificationManager Instance => _instance;

    private const string CHANNEL_ID = "horror_channel";
    private const string LAST_RESET_KEY = "last_reset_date";
    private const string FIRST_NOTIFICATION_KEY = "first_notification_sent";

    // Notification times (hours): 10AM, 3PM, 7PM, 11PM
    private readonly int[] _primaryNotificationTimes = { 10, 15, 19, 23 };
    private readonly string[] _timeSlotKeys = { "morning", "afternoon", "evening", "night" };
    private readonly string[] _timeSlotTitles = {
        "The Darkness Calls...",
        "They Miss You...",
        "It's Getting Dark...",
        "The Witching Hour..."
    };

    private const int FOLLOW_UP_DELAY_MINUTES = 10;

    private List<string> _horrorMessages = new List<string>()
    {
        // Classic Horror
        "The darkness still remembers you...",
        "A whisper calls your name in the empty halls...",
        "Not all who enter escape... Will you?",
        "Did you hear that? Something's waiting for you...",
        "Your footsteps are still echoing... Come back...",
        "The walls are closing in... It's getting harder to leave...",
        "What you left behind is still here... watching...",
        "The exit is getting further away...",
        "Do you really think you're safe outside?",
        "The nightmare hasn't ended yet...",

        // Psychological Horror
        "There's something lurking in the shadows... Come back...",
        "They say no one escapes twice...",
        "The spirits are growing restless... Answer their call...",
        "You're being watched, even now...",
        "Time is running out for you to return...",
        "The whispers are growing louder... They want you back...",
        "A cold hand just brushed your shoulder...",
        "Something moved in the darkness... It knows you're gone...",
        "The halls are silent... too silent...",
        "You left a door open... it's still waiting...",

        // Supernatural Elements
        "The shadows have grown... they're reaching for you...",
        "Your heartbeat is still echoing in the walls...",
        "The last one who tried to escape... never did...",
        "Someone is calling for you... but it's not human...",
        "Run? Hide? No one ever truly leaves...",
        "The door creaked open again... who opened it?",
        "It's been waiting for you... patiently...",
        "They're getting closer... you can still return...",
        "You can hear them if you listen closely... shhh...",
        "A candle just blew out... and you weren't there...",

        // Mirror/Reflection Horror
        "Your reflection just blinked... and you weren't there...",
        "You left something behind... and it wants you back...",
        "The temperature just dropped... can you feel it?",
        "That wasn't there before... was it?",
        "The painting's eyes follow you... even now...",
        "The clock stopped at the hour you left...",
        "Your name was just whispered... from inside...",
        "The floorboards creak with invisible steps...",
        "The mirror shows something behind you...",
        "Your breath fogs the glass... but you're not cold...",

        // Haunted Object Horror
        "The toys move when you're not looking...",
        "The music box plays by itself...",
        "The doll's head turned... just slightly...",
        "The phone rings... but no one's there...",
        "The TV turns on by itself... to static...",
        "The pages of the book turn... with no wind...",
        "The rocking chair moves... with no one in it...",
        "The lights flicker... but the power's fine...",
        "The door unlocks itself... again...",
        "The window is open... you swear you closed it...",

        // Body Horror
        "The temperature drops when you enter the room...",
        "Your shadow moves... but you don't...",
        "The writing appears on the wall... in red...",
        "The child's laughter comes from... nowhere...",
        "The footsteps follow you... but no one's there...",
        "The breathing isn't yours... but it's close...",
        "The handprint appears... on the foggy glass...",
        "The blood drips upward... defying gravity...",
        "The face in the photograph... changed...",
        "The voice on the recording... isn't human...",

        // Paranormal Phenomena
        "The walls are bleeding again...",
        "Your name appears in the dust...",
        "The mannequin turned its head...",
        "The basement door is ajar...",
        "The radio plays your thoughts...",
        "The man in the corner isn't blinking...",
        "The water runs red when you turn the faucet...",
        "The attic stairs creak under invisible weight...",
        "The calendar shows the wrong month...",
        "The closet door won't stay closed...",

        // Shadow Figures
        "The shadow in the hallway has too many teeth...",
        "The nursery rhyme is playing backwards...",
        "The fog outside forms faces...",
        "The man in the mirror isn't you anymore...",
        "The cellar door is pounding from the inside...",
        "The writing in your journal isn't yours...",
        "The man at the foot of your bed isn't sleeping...",
        "The child's drawing shows what's behind you...",
        "The cemetery gates are open... just for you...",
        "The man without a face is waiting in the foyer...",

        // Supernatural Occurrences
        "The piano plays your childhood home's address...",
        "The man in the photograph just winked...",
        "The shadow children are holding hands...",
        "The basement smells like your childhood home...",
        "The mannequin has your mother's face...",
        "The elevator stops at floors that don't exist...",
        "The fog has fingers that reach for you...",
        "The man in the rain is always facing away...",
        "The hospital bed is still warm...",
        "The asylum walls whisper your secrets...",

        // Ominous Warnings
        "The funeral procession has no faces...",
        "The gravestone bears your birthdate...",
        "The children's laughter comes from the well...",
        "The shadow in the window isn't yours...",
        "The man at the door has been dead for years...",
        "The phone call is just breathing... your breathing...",
        "The footsteps in the snow lead to your bed...",
        "The reflection in the puddle shows the noose...",
        "The hand in yours isn't attached to anyone...",
        "The voice mail is from your future self...",

        // Disturbing Visions
        "The figure in the photo is closer each time...",
        "The writing on your skin isn't in your handwriting...",
        "The child at the window only appears at midnight...",
        "The man in the coat has no face under his hat...",
        "The footsteps stop when you stop... but start before you do...",
        "The breathing in your ear smells like rot...",
        "The figure in the distance is always the same height... no matter how far...",
        "The hand on your shoulder has too many fingers...",
        "The voice in the static says your full name...",
        "The figure in the mirror only appears when you blink...",

        // New Additions (50 more)
        "The whispering starts when the lights go out...",
        "The door you locked is now open...",
        "The temperature drops exactly at 3:07 AM...",
        "The shadow in the corner is breathing...",
        "The hands coming from under the bed are cold...",
        "The writing on the wall wasn't there yesterday...",
        "The face in the window disappears when you look...",
        "The voice calling your name is coming from underground...",
        "The man standing in the rain isn't getting wet...",
        "The child at the end of the hall isn't blinking...",
        "The footsteps match yours... but faster...",
        "The reflection shows someone standing behind you...",
        "The phone keeps dialing your own number...",
        "The television turns on to show your empty room...",
        "The breathing gets louder when you close your eyes...",
        "The door handle turns by itself at night...",
        "The shadow under the door isn't cast by anything...",
        "The whispering comes from inside the walls...",
        "The figure in the photo wasn't there when it was taken...",
        "The cold spot follows you through the house...",
        "The writing appears in steam on the mirror...",
        "The music plays from rooms with no devices...",
        "The face in the crowd is always the same...",
        "The knocking comes from inside the closet...",
        "The shadow people watch you sleep...",
        "The voice knows what you're thinking...",
        "The figure is always just out of sight...",
        "The child's laughter comes from the attic...",
        "The phone displays calls from your own number...",
        "The footsteps pace outside your door at night...",
        "The shadow moves when you're not looking...",
        "The whispering stops when you enter the room...",
        "The figure in the mirror smiles when you don't...",
        "The cold breath on your neck has no source...",
        "The door you just closed is now open...",
        "The handprint on the window is too large...",
        "The shadow figure watches from across the street...",
        "The whispering voices know your secrets...",
        "The figure is always one step closer each time you look...",
        "The child at the foot of your bed isn't yours...",
        "The lights flicker to reveal something standing behind you...",
        "The footsteps follow you down the stairs...",
        "The shadow in the corner just waved at you...",
        "The voice in the static says your name...",
        "The figure vanishes when you blink...",
        "The temperature drops as the figure approaches...",
        "The whispering is getting louder... closer...",
        "The door creaks open by itself...",
        "The shadowy figure beckons you to follow...",
        "The cold fingers brush your arm...",
        "The room is darker than it should be...",
        "The laughter echoes long after the room is empty...",
        "The eyes in the darkness are watching you..."
    };

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeNotificationChannel();
        ResetDailyNotificationFlags();
        ScheduleAllNotifications();
    }

    void InitializeNotificationChannel()
    {
#if UNITY_ANDROID
        var channel = new AndroidNotificationChannel()
        {
            Id = CHANNEL_ID,
            Name = "Horror Notifications",
            Importance = Importance.High,
            Description = "Notifications to keep you on edge...",
        };
        AndroidNotificationCenter.RegisterNotificationChannel(channel);
#endif
    }

    void ResetDailyNotificationFlags()
    {
        string today = DateTime.Now.ToString("yyyyMMdd");
        if (PlayerPrefs.GetString(LAST_RESET_KEY, "") != today)
        {
            // Reset flags daily to allow new notifications
            foreach (string timeKey in _timeSlotKeys)
            {
                PlayerPrefs.DeleteKey($"{timeKey}_primary_sent");
                PlayerPrefs.DeleteKey($"{timeKey}_followup_sent");
            }
            PlayerPrefs.SetString(LAST_RESET_KEY, today);
            PlayerPrefs.Save();
        }
    }

    void ScheduleAllNotifications()
    {
        for (int i = 0; i < _primaryNotificationTimes.Length; i++)
        {
            ScheduleNotificationPair(_timeSlotKeys[i], _timeSlotTitles[i], _primaryNotificationTimes[i]);
        }
    }

    void ScheduleNotificationPair(string timeKey, string title, int hour)
    {
        DateTime targetTime = CalculateTargetTime(hour);
        int delaySeconds = (int)(targetTime - DateTime.Now).TotalSeconds;

        if (delaySeconds <= 0)
            return; // Time passed already, skip scheduling

        // Check if primary notification already sent today
        if (!PlayerPrefs.HasKey($"{timeKey}_primary_sent"))
        {
            SendHorrorNotification(
                title,
                GetRandomHorrorMessage(),
                delaySeconds,
                $"{timeKey}_primary_sent",
                true
            );

            // Schedule follow-up notification 10 minutes later
            SendHorrorNotification(
                "Still Here...",
                "You can't hide forever...",
                delaySeconds + FOLLOW_UP_DELAY_MINUTES * 60,
                $"{timeKey}_followup_sent",
                true
            );
        }
    }

    DateTime CalculateTargetTime(int targetHour)
    {
        DateTime now = DateTime.Now;
        DateTime targetTime = new DateTime(now.Year, now.Month, now.Day, targetHour, 0, 0);

        // If target time already passed today, schedule for next day
        if (targetTime <= now)
            targetTime = targetTime.AddDays(1);

        return targetTime;
    }

    string GetRandomHorrorMessage()
    {
        if (_horrorMessages.Count == 0)
            return "The darkness awaits...";
        int index = UnityEngine.Random.Range(0, _horrorMessages.Count);
        return _horrorMessages[index];
    }

    void SendHorrorNotification(string title, string message, int delaySeconds, string playerPrefKey, bool saveFlag)
    {
#if UNITY_ANDROID
        var notification = new AndroidNotification();
        notification.Title = title;
        notification.Text = message;
        notification.FireTime = DateTime.Now.AddSeconds(delaySeconds);
       

        int id = AndroidNotificationCenter.SendNotification(notification, CHANNEL_ID);
        Debug.Log($"Scheduled notification '{title}' with id {id} in {delaySeconds} seconds.");
#elif UNITY_IOS
        // iOS Notification code can be added here
#endif

        if (saveFlag)
        {
            PlayerPrefs.SetInt(playerPrefKey, 1);
            PlayerPrefs.Save();
        }
    }
}
