using System;
using System.IO;
using UnityEngine;

[Serializable]
public class AudioItem
{
    public AudioClip Clip;

    public string NameOfSyncSource;

    public bool SyncWithOtherAudioClip;

    public bool PlayOnAwake;

    public bool Loop;

    public bool DontDestroyOnLoad;

    public float Volume = 1;

    public float Pitch = 1;

    public float Pan2D;

    public string Name;
}

