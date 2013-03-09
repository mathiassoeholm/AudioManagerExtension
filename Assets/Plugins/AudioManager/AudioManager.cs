using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class AudioManager : MonoBehaviour
{
    public AudioItem[] AudioItems;

    public AudioManagerSettings Settings;

    // List keeping track of all audio sources in the scene, used to play sound effects
    private static List<AudioSource> audioSources = new List<AudioSource>();

    private static AudioManager instance;

    void Awake()
    {
        instance = this;
    }

	void Start ()
    {
        Debug.Log("AudioManager .Start()");
        
        // Play start on awake sounds
	    for (int i = 0; i < AudioItems.Length; i++)
	    {
	        if (AudioItems[i].PlayOnAwake)
	        {
                PlaySound(i, AudioItems[i].Volume); 
	        }
	    }
	}

    void OnApplicationQuit()
    {
        // Reset audio sources list
        audioSources = new List<AudioSource>();
    }

    private static void PlaySound (int id, float? volume)
    {
        // Use the audio manager instance to play a sound
        instance.PlaySound(instance.AudioItems[id], volume == null ? instance.AudioItems[id].Volume : (float)volume);
    }

    private static void StopSound(int id)
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource.clip == instance.AudioItems[id].Clip)
            {
                audioSource.Stop();
            }
        }
    }

    public bool IsAudioItemPlaying(AudioItem item)
    {
        // Remove destroyed audio sources from the list
        RemoveAllMissingSources();
        
        return audioSources.Any(audioSource => audioSource.clip == item.Clip && audioSource.isPlaying);
    }

    public void AddLeakedAudioSources()
    {
        foreach (AudioSource audioSource in FindSceneObjectsOfType(typeof(AudioSource)).OfType<AudioSource>())
        {
            if (!audioSources.Contains(audioSource))
            {
                audioSources.Add(audioSource);
            }
        }
    }

    public void RemoveAllAudioSourcesWithClip(AudioClip clip)
    {
        AddLeakedAudioSources();

        for (int i = audioSources.Count - 1; i >= 0; i--)
        {
            if (audioSources[i].clip == clip)
            {
                DestroyImmediate(audioSources[i].gameObject);
                audioSources.RemoveAt(i);
            }
        }
    }

    public void UpdateAudioSourcesWithNewSettings(AudioItem itemToUpdate)
    {
        foreach (AudioSource audioSource in audioSources)
        {
            if (audioSource.clip == itemToUpdate.Clip)
            {
                ApplySettingsToAudioSource(audioSource, itemToUpdate);
            }
        }
    }

    private void ApplySettingsToAudioSource(AudioSource audioSource, AudioItem audioSettings, float? volume = null)
    {
        audioSource.volume = (volume == null ? audioSettings.Volume : (float)volume) * Settings.MasterVolume;

        audioSource.pitch = audioSettings.Pitch;

        audioSource.loop = audioSettings.Loop;

        audioSource.playOnAwake = audioSettings.PlayOnAwake;

        audioSource.pan = audioSettings.Pan2D;
    }

    private void RemoveAllMissingSources()
    {
        // Remove destroyed audio sources from the list
        for (int i = audioSources.Count - 1; i >= 0; i--)
        {
            if (audioSources[i] == null)
            {
                audioSources.RemoveAt(i);
            }
        }
    }

    public void StopAudioItem(AudioItem item)
    {
        // Destroy any audio sources with this sound
        for (int i = audioSources.Count - 1; i >= 0; i--)
        {
            if (audioSources[i].clip == item.Clip)
            {
                audioSources[i].Stop();
            }
        }
    }

    public void StopAllSounds()
    {
        // Stop all audio sources
        for (int i = audioSources.Count - 1; i >= 0; i--)
        {
            audioSources[i].Stop();
        }
    }

    public void PlaySound(AudioItem audioItem)
    {
        PlaySound(audioItem, audioItem.Volume);
    }

    public void PlaySound(AudioItem audioItem, float volume)
    {
        RemoveAllMissingSources();
        
        // We need an audio source to play a sound
        var audioSource = new AudioSource();
        bool didFindAudioSource = false;

        // Loops through all audio sources we've created so far
        foreach (AudioSource source in audioSources)
        {
            // If an existing audio source is not playing any sound, select that one
            if (!source.isPlaying)
            {
                audioSource = source;
                didFindAudioSource = true;
                break;
            }
        }

        // If we didn't find a usable audiosource in the scene, create a new one
        if (!didFindAudioSource)
        {
            // Create audio source
            audioSource = new GameObject("AudioSource").AddComponent<AudioSourceComp>().gameObject.AddComponent<AudioSource>();

           // audioSource.gameObject.hideFlags = HideFlags.DontSave;

            // Add new audio source to our list
            audioSources.Add(audioSource);
        }

        // Assign the clip to the selected audio source
        audioSource.clip = audioItem.Clip;

        // Apply settings to audio source
        ApplySettingsToAudioSource(audioSource, audioItem, volume);

        audioSource.GetComponent<AudioSourceComp>().DoDestroyOnLoad = audioItem.DontDestroyOnLoad;

        // Play the clip with the selected audio source
        audioSource.Play();
    }
}
