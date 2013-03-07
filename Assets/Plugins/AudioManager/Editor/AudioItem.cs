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

    private string filePath;

    public string FilePath
    {
        get { return filePath; }
        set
        {
            // Load and assign audio clip
            Clip = (AudioClip)Resources.LoadAssetAtPath(value, typeof(AudioClip));
           
            // Assign file path
            filePath = value;
        }
    }
}

