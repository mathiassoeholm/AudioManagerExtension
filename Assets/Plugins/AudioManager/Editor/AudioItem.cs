using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using UnityEditor;
using UnityEngine;

[Serializable]
public class AudioItem : ISerializable
{
    private string filePath;

    public AudioClip Clip;

    public bool PlayOnAwake;

    public bool Loop;

    public float Volume;

    public string FilePath
    {
        get { return filePath; }
        set
        {
            // Load audio clip
            Clip = (AudioClip)Resources.LoadAssetAtPath(value, typeof(AudioClip));
           
            filePath = value;
        }
    }

    public void LoadItem(string dataPath)
    {
        if (!File.Exists(dataPath))
        {
            Debug.Log("Tried loading audio item but no file exists");

            return;
        }

        using (TextReader reader = File.OpenText(dataPath))
        {
            FilePath = reader.ReadLine().Split('=')[1];

            Debug.Log("loaded FilePath was " + FilePath);

            PlayOnAwake = Convert.ToBoolean(reader.ReadLine().Split('=')[1]);
            Loop = Convert.ToBoolean(reader.ReadLine().Split('=')[1]);
            Volume = (float)Convert.ToDouble(reader.ReadLine().Split('=')[1]);
        }
    }

    public void DeleteSaveData()
    {
        
    }

    public void SaveItem(int index)
    {
        Debug.Log("Saved item");

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
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("PlayOnAwake", PlayOnAwake);
        info.AddValue("Loop", Loop);
        info.AddValue("Volume", Volume);
        info.AddValue("Volume", Volume);
    }
}

