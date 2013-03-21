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
    
    public AudioClip[] Clips;

    public SoundType Type;

    public string NameOfSyncSource;

    public bool SyncWithOtherAudioClip;

    public bool PlayOnAwake;

    public bool Loop;

    public bool DontDestroyOnLoad;

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

