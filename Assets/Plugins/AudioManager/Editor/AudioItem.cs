using System;
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

    public bool PlayOnAwake
    {
        get { return playOnAwake; }
        set { playOnAwake = value; }
    }

    public bool Loop
    {
        get { return loop; }
        set
        {
            if (loop != value)
            {
                // Register change
                AudioWindow.ChangesNeedToBeSaved = true;
            }
            
            loop = value;
        }
    }

    public bool Is3D
    {
        get { return is3D; }
        set
        {
            if (is3D != value)
            {
                // Register change
                AudioWindow.ChangesNeedToBeSaved = true;
            }

            is3D = value;
        }
    }

    public string Path
    {
        get { return path; }
        set
        {
            // Load audio clip
            Clip = (AudioClip)Resources.LoadAssetAtPath(value, typeof(AudioClip));
            
            if (path != value)
            {
                // Register change
                AudioWindow.ChangesNeedToBeSaved = true;
            }

            path = value;
        }
    }

    public float Volume
    {
        get { return volume; }
        set
        {
            if (volume != value)
            {
                // Register change
                AudioWindow.ChangesNeedToBeSaved = true;
            }

            volume = value;
        }
    }

    public void SaveItem(int index)
    {
        // Save in editor prefs
        EditorPrefs.SetString(index + "_Path", Path);
        EditorPrefs.SetBool(index + "_PlayOnAwake", PlayOnAwake);
        EditorPrefs.SetBool(index + "_Loop", Loop);
        EditorPrefs.SetBool(index + "_Is3D", Is3D);
        EditorPrefs.SetFloat(index + "_Volume", Volume);
    }
}

