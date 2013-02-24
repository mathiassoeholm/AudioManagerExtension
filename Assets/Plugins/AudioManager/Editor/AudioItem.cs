﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

[Serializable]
public class AudioItem
{
    private bool playOnAwake;
    private bool loop;
    private bool is3D;
    private string path;
    private float volume;

    public AudioClip Clip;

    public bool PlayOnAwake;

    public bool Loop;

    public float Volume;

    public string Path
    {
        get { return path; }
        set
        {
            // Load audio clip
            Clip = (AudioClip)Resources.LoadAssetAtPath(value, typeof(AudioClip));
           
            path = value;
        }
    }

    public void SaveItem(int index)
    {
        // Save in editor prefs
        EditorPrefs.SetString(index + "_Path", Path);
        EditorPrefs.SetBool(index + "_PlayOnAwake", PlayOnAwake);
        EditorPrefs.SetBool(index + "_Loop", Loop);
        EditorPrefs.SetFloat(index + "_Volume", Volume);
    }
}

