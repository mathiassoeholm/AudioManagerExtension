using System;
using System.IO;
using UnityEngine;

[Serializable]
public class AudioItem
{
    public enum SoundType
    {
        SoundEffect,
        Music
    }

    public enum PlayMode
    {
        RandomNoRepeat,
        Random,
        Sequential,
        Reverse
    }
    
    public AudioClip[] Clips;

    public SoundType Type;

    public PlayMode Mode;

    public string NameOfSyncSource;

    public bool SyncWithOtherAudioClip;

    public bool PlayOnAwake;

    public bool Loop;

    public bool DontDestroyOnLoad;

    public bool IsCollection;

    public float RandomPitch;

    public float RandomVolume;

    public float Volume = 1;

    public float Pitch = 1;

    public float Pan2D;

    public string Name;

    public AudioClip GetClip()
    {
        return Clips[0];
    }
}

