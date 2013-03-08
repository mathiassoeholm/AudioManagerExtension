using System;
using System.IO;
using UnityEngine;

[Serializable]
public class AudioItem
{
    public AudioClip Clip;

    public bool PlayOnAwake;

    public bool Loop;

    public float Volume = 1;

    public float Pitch = 1;

    public string Name;
}

