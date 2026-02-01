using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Serializable]
    public class SoundDef
    {
        public int id;               // e.g., 30x
        public AudioClip clip;       // mp3 imported as AudioClip
    }

    [Header("Sound Library (ID -> Clip)")]
    [SerializeField] private List<SoundDef> sounds = new List<SoundDef>();

    [Header("Audio Source Pool")]
    [SerializeField] private int prewarmSources = 8;
    [SerializeField] private AudioMixerGroup outputMixer;
    [SerializeField] private bool dontDestroyOnLoad = true;

    [Header("Defaults")]
    [SerializeField] private float defaultVolume = 1f;
    [SerializeField] private bool default2D = true; // SpatialBlend = 0

    // Dict of all possible sounds: (ID -> AudioClip)
    private readonly Dictionary<int, AudioClip> soundDict = new Dictionary<int, AudioClip>();

    // Dict of all going-on audio sources:
    // (id playing -> entry { source, isLooping, paused, onCompleted })
    private class PlayingEntry
    {
        public int id;
        public AudioSource source;
        public bool looping;
        public bool paused;

        public Action onCompleted; // NEW: optional callback when finished (non-looping natural completion)
    }
    private readonly Dictionary<int, PlayingEntry> playingDict = new Dictionary<int, PlayingEntry>();

    // Pool of reusable AudioSources (free sources not currently assigned)
    private readonly List<AudioSource> freeSources = new List<AudioSource>();
    private readonly List<Action> tempCallbacks = new List<Action>(); 
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        BuildSoundDict();
        PrewarmPool(prewarmSources);

        PlaySound(308, true, null, false,0.3f);
    }

    private void BuildSoundDict()
    {
        soundDict.Clear();
        for (int i = 0; i < sounds.Count; i++)
        {
            var def = sounds[i];
            if (def == null || def.clip == null) continue;
            soundDict[def.id] = def.clip;
        }
    }

    private void PrewarmPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            freeSources.Add(CreateNewSource());
        }
    }

    private AudioSource CreateNewSource()
    {
        var go = new GameObject($"AudioSource_{freeSources.Count + playingDict.Count}");
        go.transform.SetParent(transform, false);

        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = false;
        src.volume = defaultVolume;
        src.outputAudioMixerGroup = outputMixer;

        if (default2D)
        {
            src.spatialBlend = 0f; // 2D
        }

        return src;
    }

    private AudioSource GetFreeSource()
    {
        for (int i = 0; i < freeSources.Count; i++)
        {
            var s = freeSources[i];
            if (s != null)
            {
                freeSources.RemoveAt(i);
                return s;
            }
        }
        return CreateNewSource();
    }

    private void ReleaseSource(AudioSource src)
    {
        if (src == null) return;

        src.Stop();
        src.clip = null;
        src.loop = false;

        freeSources.Add(src);
    }

    // public playSound(int id, bool loop)
    // If same id already playing:
    // - restartIfAlreadyPlaying = true  => replay (default)
    // - restartIfAlreadyPlaying = false => do nothing
    // NEW: optional onCompleted callback invoked ONLY when non-looping sound finishes naturally.
    public void PlaySound(
        int id,
        bool loop,
        Action onCompleted = null,
        bool restartIfAlreadyPlaying = true,
        float? volume = null
    )
    {
        if (!soundDict.TryGetValue(id, out var clip) || clip == null)
        {
            Debug.LogWarning($"SoundManager: No clip found for id={id}");
            return;
        }

        float vol = Mathf.Clamp01(volume ?? defaultVolume);

        // Same id already playing
        if (playingDict.TryGetValue(id, out var existing) && existing != null && existing.source != null)
        {
            if (!restartIfAlreadyPlaying)
                return;

            existing.looping = loop;
            existing.paused = false;
            existing.onCompleted = onCompleted;

            existing.source.loop = loop;
            existing.source.clip = clip;
            existing.source.volume = vol;

            existing.source.Stop();
            existing.source.Play();
            return;
        }

        // New id: allocate a source
        var src = GetFreeSource();
        src.clip = clip;
        src.loop = loop;
        src.volume = vol;
        if (outputMixer != null) src.outputAudioMixerGroup = outputMixer;

        src.Play();

        playingDict[id] = new PlayingEntry
        {
            id = id,
            source = src,
            looping = loop,
            paused = false,
            onCompleted = onCompleted
        };
    }

    // public stopSound(int id)
    // Find audio source with matching id, stop and empty the sound inside
    public void StopSound(int id)
    {
        if (!playingDict.TryGetValue(id, out var entry) || entry == null)
            return;

        playingDict.Remove(id);

        if (entry.source != null)
        {
            ReleaseSource(entry.source);
        }
    }

    public void PauseSound(int id)
    {
        if (!playingDict.TryGetValue(id, out var entry) || entry == null || entry.source == null)
            return;

        // Pause only if currently playing
        entry.source.Pause();
        entry.paused = true;
    }

    public void UnpauseSound(int id)
    {
        if (!playingDict.TryGetValue(id, out var entry) || entry == null || entry.source == null)
            return;

        // UnPause works even if the source is not currently playing but paused
        entry.source.UnPause();
        entry.paused = false;
    }

    // Public update()
    // Loop through all audio sources:
    // If a sound finished playing and not looping: stop and empty it
    /*private void Update()
    {
        if (playingDict.Count == 0) return;

        // Avoid modifying dictionary while iterating
        tempRemoveIds.Clear();

        foreach (var kv in playingDict)
        {
            var entry = kv.Value;
            if (entry == null || entry.source == null)
            {
                tempRemoveIds.Add(kv.Key);
                continue;
            }

            // If paused, do not auto-release
            if (entry.paused) continue;

            // Non-looping finished: isPlaying becomes false after completion
            if (!entry.looping && !entry.source.isPlaying)
            {
                // NEW: invoke callback on natural completion (non-looping)
                if (entry.onCompleted != null)
                {
                    try
                    {
                        entry.onCompleted.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                ReleaseSource(entry.source);
                tempRemoveIds.Add(kv.Key);
            }
        }

        for (int i = 0; i < tempRemoveIds.Count; i++)
        {
            playingDict.Remove(tempRemoveIds[i]);
        }
    }*/

    private void Update()
    {
        if (playingDict.Count == 0) return;

        tempRemoveIds.Clear();
        tempCallbacks.Clear();

        foreach (var kv in playingDict)
        {
            var entry = kv.Value;
            if (entry == null || entry.source == null)
            {
                tempRemoveIds.Add(kv.Key);
                continue;
            }

            if (entry.paused) continue;

            if (!entry.looping && !entry.source.isPlaying)
            {
                // 先缓存回调，别在 foreach 里调用
                if (entry.onCompleted != null)
                {
                    tempCallbacks.Add(entry.onCompleted);
                    entry.onCompleted = null; // 防止重复调用
                }

                ReleaseSource(entry.source);
                tempRemoveIds.Add(kv.Key);
            }
        }

        for (int i = 0; i < tempRemoveIds.Count; i++)
            playingDict.Remove(tempRemoveIds[i]);

        // 现在才执行回调：回调里 PlaySound 修改字典也安全
        for (int i = 0; i < tempCallbacks.Count; i++)
        {
            try { tempCallbacks[i]?.Invoke(); }
            catch (Exception ex) { Debug.LogException(ex); }
        }
    }

    private readonly List<int> tempRemoveIds = new List<int>();

    // Optional helpers
    public bool IsPlaying(int id)
    {
        return playingDict.TryGetValue(id, out var e) && e != null && e.source != null && e.source.isPlaying;
    }

    public bool HasClip(int id) => soundDict.ContainsKey(id);

#if UNITY_EDITOR
    // In Editor: rebuild dict when values change (optional convenience)
    private void OnValidate()
    {
        if (!Application.isPlaying)
            BuildSoundDict();
    }
#endif
}
