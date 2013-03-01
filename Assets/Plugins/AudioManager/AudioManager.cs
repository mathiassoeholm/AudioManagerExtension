using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;

public partial class AudioManager : MonoBehaviour
{
    public AudioItem[] AudioItems;

    // List keeping track of all audio sources in the scene, used to play sound effects
    private List<AudioSource> audioSources = new List<AudioSource>();

    private static AudioManager instance;

    public static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                var objects = FindObjectsOfType(typeof(AudioManager));

                if (objects.Length == 0)
                {
                    Debug.Log("No AudioManager was found in the scene, spawning one...");
                    instance = new GameObject("AudioManage").AddComponent<AudioManager>();
                }
                else if (objects.Length > 1)
                {
                    Debug.Log("Several Audio Managers were found in scene! You can only have one.. Destroying others");

                    // Destroy remaining audio managers
                    for (int i = 1; i < objects.Length; i++)
                    {
                        GameObject gameObjectToDestroy = (objects[i] as AudioManager).gameObject;
                        
                        if (Application.isPlaying)
                        {
                            // Destroy in game
                            Destroy(gameObjectToDestroy);
                        }
                        else
                        {
                            // Destroy in editor
                            DestroyImmediate(gameObjectToDestroy);
                        }
                    }
                }
                else
                {
                    instance = (AudioManager)objects[0];
                }
            }

            return instance;
        }
    }

    public static void CreateNewInstance()
    {
        var objects = FindObjectsOfType(typeof(AudioManager));

        // Destroy old audio managers
        for (int i = 0; i < objects.Length; i++)
        {
            GameObject gameObjectToDestroy = (objects[i] as AudioManager).gameObject;
            
            if (Application.isPlaying)
            {
                // Destroy in game
                Destroy(gameObjectToDestroy);
            }
            else
            {
                // Destroy in editor
                DestroyImmediate(gameObjectToDestroy);
            }
        }

        instance = new GameObject("AudioManage").AddComponent<AudioManager>();
    }

    void Awake()
    {
        Object[] audioSorcesInScene = FindSceneObjectsOfType(typeof (AudioSource));
        
        // Destroy any previous audio sources used to play sounds in the editor
        for (int i = audioSorcesInScene.Length - 1; i >= 0; i--)
        {
            Destroy((audioSorcesInScene[i] as AudioSource).gameObject);
        }

        // Reset audio sources list
        audioSources = new List<AudioSource>();
    }

	void Start ()
    {
        // Play start on awake sounds
	    for (int i = 0; i < AudioItems.Length; i++)
	    {
	        if (AudioItems[i].PlayOnAwake)
	        {
                PlaySound(i); 
	        }
	    }
	}

    void OnApplicationQuit()
    {
        // Reset audio sources list
        audioSources = new List<AudioSource>();
    }
	
	static void PlaySound (int id)
    {
	    // Use the audio manager instance to play a sound
        Instance.PlaySound(Instance.AudioItems[id]);
	}

    public bool IsAudioItemPlaying(AudioItem item)
    {
        return audioSources.Any(audioSource => audioSource.clip == item.Clip && audioSource.isPlaying);
    }

    public void StopAudioItem(AudioItem item)
    {
        // Destroy any audio sources with this sound
        for (int i = audioSources.Count - 1; i >= 0; i--)
        {
            if (audioSources[i].clip == item.Clip)
            {
                DestroyImmediate(audioSources[i].gameObject);
                audioSources.RemoveAt(i);
            }
        }
    }

    public void StopAllSounds()
    {
        // Destroy all audio sources
        for (int i = audioSources.Count - 1; i >= 0; i--)
        {
            DestroyImmediate(audioSources[i].gameObject);
        }
        
        // Reset the list
        audioSources = new List<AudioSource>();
    }

    public void PlaySound(AudioItem audioItem, Action onComplete = null)
    {
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
            audioSource = new GameObject("AudioSource").AddComponent<AudioSource>();

            //audioSource.gameObject.hideFlags = HideFlags.HideAndDontSave;

            // Add new audio source to our list
            audioSources.Add(audioSource);
        }

        audioSource.loop = audioItem.Loop;

        // Assign the clip to the selected audio source
        audioSource.clip = audioItem.Clip;

        // Set volume
        audioSource.volume = audioItem.Volume;

        // Play the clip with the selected audio source
        audioSource.Play();
    }
}
