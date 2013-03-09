using System;
using System.IO;
using UnityEngine;

[Serializable]
public class AudioItem : IEquatable<AudioItem>
{
    public AudioClip Clip;

    public string NameOfSyncSource;

    public bool SyncWithOtherAudioClip;

    public bool PlayOnAwake;

    public bool Loop;

    public float Volume = 1;

    public float Pitch = 1;

    public float Pan2D;

    public string Name;

    public bool Equals(AudioItem other)
    {
        return other.Clip == Clip &&
        other.NameOfSyncSource == NameOfSyncSource &&
        other.PlayOnAwake == PlayOnAwake &&
        other.Loop == Loop &&
        Mathf.Approximately(other.Volume, Volume) &&
        Mathf.Approximately(other.Pitch, Pitch) &&
        Mathf.Approximately(other.Pan2D, Pan2D) &&
        other.Name == Name;
    }
}

