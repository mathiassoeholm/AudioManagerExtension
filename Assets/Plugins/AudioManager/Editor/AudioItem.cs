using System;
using System.IO;
using UnityEngine;

[Serializable]
public class AudioItem
{
    public AudioClip Clip;

    public bool PlayOnAwake;

    public bool Loop;

    public float Volume;

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

    public void LoadItem(string dataPath)
    {
        // Check if the data file actually exists before we load it
        if (!File.Exists(dataPath))
        {
            Debug.Log("Tried loading audio item but no file exists");

            return;
        }

        using (TextReader reader = File.OpenText(dataPath))
        {
            FilePath = reader.ReadLine().Split('=')[1];

            Debug.Log("Loaded FilePath was " + FilePath);

            PlayOnAwake = Convert.ToBoolean(reader.ReadLine().Split('=')[1]);
            Loop = Convert.ToBoolean(reader.ReadLine().Split('=')[1]);
            Volume = (float)Convert.ToDouble(reader.ReadLine().Split('=')[1]);
        }
    }

    public void DeleteSaveData()
    {
        File.Delete(@"ProjectSettings/AudioItems/audioitem_" + Path.GetFileName(FilePath) + ".txt");

        Debug.Log("Deleted " + @"ProjectSettings/AudioItems/audioitem_" + Path.GetFileName(FilePath) + ".txt");
    }

    public void SaveItem()
    {
        // If the clip doesn't exist, remove this item
        if (Clip == null)
        {
            Debug.Log("Clip was null, deleting item");
            
            AudioWindow.RemoveAudioItem(this);

            return;
        }
        
        if (!Directory.Exists(@"ProjectSettings/AudioItems"))
        {
            Directory.CreateDirectory(@"ProjectSettings/AudioItems");
        }

        using (TextWriter writer = File.CreateText(@"ProjectSettings/AudioItems/audioitem_" + Path.GetFileName(FilePath) + ".txt"))
        {
            writer.WriteLine("FilePath=" + FilePath);
            writer.WriteLine("PlayOnAwake=" + PlayOnAwake);
            writer.WriteLine("Loop=" + Loop);
            writer.WriteLine("Volume=" + Volume);
        }

        Debug.Log("Saved item");
    }
}

