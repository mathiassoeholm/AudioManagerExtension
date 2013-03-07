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

    public string Name;

    public string FilePath;

    public void LoadAudioClipFromPath()
    {
        Name = Path.GetFileName(FilePath);
        
        Clip = (AudioClip)Resources.LoadAssetAtPath(FilePath, typeof(AudioClip));
    }
}

