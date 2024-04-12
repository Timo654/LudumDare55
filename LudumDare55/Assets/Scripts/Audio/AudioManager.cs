using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    [Header("Volume")]
    [Range(0, 1)]

    public float MusicVolume = 1;
    [Range(0, 1)]

    public float SFXVolume = 1;
    [Range(0, 1)]

    public float UIVolume = 1;
    [Range(0, 1)]

    private Bus musicBus;

    private Bus sfxBus;
    private Bus reverbBus;
    private Bus uiBus;
    const string sid = "00000000-0000-0000-0000-000000000000";
    static readonly Guid nullGuid = new Guid(sid);

    private List<EventInstance> eventInstances = new();
    class TimelineInfo
    {
        public FMOD.StringWrapper LastMarker = new();
    }
    TimelineInfo timelineInfo;
    GCHandle timelineHandle;

    EVENT_CALLBACK beatCallback;

    private static AudioManager _instance { get; set; }
    public static event Action<string> OnNewBGMMarker;
    // public static AudioManager instance;
    private static EventInstance musicEventInstance;


    // public access for the Singleton
    // and lazy instantiation if not exists
    public static AudioManager Instance
    {
        get
        {
            // if exists directly return
            if (_instance) return _instance;

            // otherwise search it in the scene
            _instance = FindObjectOfType<AudioManager>();

            // found it?
            if (_instance) return _instance;

            // otherwise create and initialize it
            CreateInstance();

            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void CreateInstance()
    {
        // skip if already exists
        if (_instance) return;
        if (SceneManager.GetActiveScene().name == "BankLoader") return;
        InitializeInstance(new GameObject(nameof(AudioManager)).AddComponent<AudioManager>());
    }

    private static void InitializeInstance(AudioManager instance)
    {
        _instance = instance;
        DontDestroyOnLoad(_instance.gameObject);
        _instance.eventInstances = new List<EventInstance>();
        _instance.musicBus = RuntimeManager.GetBus("bus:/Music");
        _instance.sfxBus = RuntimeManager.GetBus("bus:/SFX");
        _instance.uiBus = RuntimeManager.GetBus("bus:/UI");
        _instance.SFXVolume = SaveManager.Instance.systemData.SFXVolume / 100f;
        _instance.MusicVolume = SaveManager.Instance.systemData.MusicVolume / 100f;
        _instance.UIVolume = SaveManager.Instance.systemData.UIVolume / 100f;

    }

    private void Awake()
    {
        if (_instance && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        InitializeInstance(this);
    }

    public EventInstance CreateInstance(EventReference eventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(eventReference);
        eventInstances.Add(eventInstance);
        return eventInstance;
    }

    private void Update()
    {
        musicBus.setVolume(MusicVolume);
        sfxBus.setVolume(SFXVolume);
        uiBus.setVolume(UIVolume);
    }

    public static bool IsEventReferenceValid(EventReference eventReference)
    {
        return eventReference.Guid != nullGuid;
    }
    public static void PlayOneShot(EventReference eventReference)
    {
        if (eventReference.Guid != nullGuid)
            RuntimeManager.PlayOneShot(eventReference);
        else
        {
            Debug.LogWarning("EventReference is null, ignoring...");
        }
    }
    public void InitializeMusic(EventReference musicEventReference)
    {
        if (musicEventReference.Guid == nullGuid)
        {
            Debug.LogWarning("EventReference is null, ignoring.");
            return;
        }
        musicEventInstance = CreateInstance(musicEventReference);
        timelineInfo = new TimelineInfo();

        // Explicitly create the delegate object and assign it to a member so it doesn't get freed
        // by the garbage collected while it's being used
        beatCallback = new EVENT_CALLBACK(BeatEventCallback);

        // Pin the class that will store the data modified during the callback
        timelineHandle = GCHandle.Alloc(timelineInfo);
        // Pass the object through the userdata of the instance
        musicEventInstance.setUserData(GCHandle.ToIntPtr(timelineHandle));
        musicEventInstance.setCallback(beatCallback, EVENT_CALLBACK_TYPE.TIMELINE_BEAT | EVENT_CALLBACK_TYPE.TIMELINE_MARKER);
    }

    public void SetMusicParameter(string parameter, float value)
    {
        musicEventInstance.setParameterByName(parameter, value);
    }
    public void StartMusic()
    {
        if (!musicEventInstance.isValid())
        {
            Debug.LogWarning("Music is not initialized yet, ignoring.");
            return;
        }
        musicEventInstance.start();
    }

    public int GetMusicPosition()
    {
        musicEventInstance.getTimelinePosition(out int position);
        return position;
    }
    public void StopSFX()
    {
        sfxBus.stopAllEvents(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    public void FadeOutMusic()
    {
        musicEventInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicEventInstance.release();

    }

    public static bool IsPlaying()
    {
        musicEventInstance.getPlaybackState(out PLAYBACK_STATE state);
        return state != PLAYBACK_STATE.STOPPED;
    }

    public static bool IsPlaying(EventInstance instance)
    {
        instance.getPlaybackState(out PLAYBACK_STATE state);
        return state != PLAYBACK_STATE.STOPPED;
    }
    public static void Unpause()
    {
        musicEventInstance.setPaused(false);
    }

    public static void Unpause(EventInstance instance)
    {
        instance.setPaused(false);
    }

    public static void Pause()
    {
        musicEventInstance.setPaused(true);
    }

    public static void Pause(EventInstance instance)
    {
        instance.setPaused(true);
    }

    private void CleanUp()
    {
        if (eventInstances != null)
        {
            foreach (EventInstance eventInstance in eventInstances)
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                eventInstance.release();
            }
        }

    }

    // taken from https://www.fmod.com/docs/2.02/unity/examples-timeline-callbacks.html
    [AOT.MonoPInvokeCallback(typeof(EVENT_CALLBACK))]
    static FMOD.RESULT BeatEventCallback(EVENT_CALLBACK_TYPE type, IntPtr instancePtr, IntPtr parameterPtr)
    {
        EventInstance instance = new(instancePtr);

        // Retrieve the user data
        FMOD.RESULT result = instance.getUserData(out IntPtr timelineInfoPtr);
        if (result != FMOD.RESULT.OK)
        {
            Debug.LogError("Timeline Callback error: " + result);
        }
        else if (timelineInfoPtr != IntPtr.Zero)
        {
            // Get the object to store beat and marker details
            GCHandle timelineHandle = GCHandle.FromIntPtr(timelineInfoPtr);
            TimelineInfo timelineInfo = (TimelineInfo)timelineHandle.Target;

            switch (type)
            {
                case EVENT_CALLBACK_TYPE.TIMELINE_MARKER:
                    {
                        var parameter = (TIMELINE_MARKER_PROPERTIES)Marshal.PtrToStructure(parameterPtr, typeof(TIMELINE_MARKER_PROPERTIES));
                        timelineInfo.LastMarker = parameter.name;
                        OnNewBGMMarker?.Invoke(parameter.name);
                        break;
                    }
                case EVENT_CALLBACK_TYPE.DESTROYED:
                    {
                        // Now the event has been destroyed, unpin the timeline memory so it can be garbage collected
                        timelineHandle.Free();
                        break;
                    }
            }
        }
        return FMOD.RESULT.OK;
    }

    private void OnDestroy()
    {
        CleanUp();
    }

}
