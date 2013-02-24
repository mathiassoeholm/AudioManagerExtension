using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public partial class AudioManager : MonoBehaviour
{
    public AudioItem[] AudioItems;

    // List keeping track of all audio sources in the scene, used to play sound effects
    private List<AudioSource> audioSources = new List<AudioSource>();

    private static AudioManager instance;

    private static AudioManager Instance
    {
        get
        {
            if (instance == null)
            {
                var objects = FindObjectsOfType(typeof(AudioManager));

                if (objects.Length == 0)
                {
                    Debug.Log("There's no AudioManager in the scene, attempting to spawn one..");

                    // Spawn the audio manager prefab
                    instance =
                        ((GameObject)
                         Instantiate(Resources.LoadAssetAtPath("Assets/Plugins/AudioManager/AudioManager.prefab",
                                                               typeof (GameObject)))).GetComponent<AudioManager>();
                }
                else if (objects.Length > 1)
                {
                    Debug.LogError("AudioManager is a Singleton but several (" + objects.Length + ") were found in scene!");
                    instance = (AudioManager)objects[0];
                }
                else
                {
                    instance = (AudioManager)objects[0];
                }
            }

            return instance;
        }
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
	
	static void PlaySound (int id)
    {
	    // Use the audio manager instance to play a sound
        Instance.PlaySound(Instance.AudioItems[id]);
	}

    private void PlaySound(AudioItem audioItem)
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
