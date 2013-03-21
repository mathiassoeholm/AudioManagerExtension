using UnityEngine;
using System.Collections;

public class AudioSourceComp : MonoBehaviour
{
    private bool doDestroyOnLoad;

    private AudioSource source;

    public float startVolume;

    public bool DoDestroyOnLoad
    {
        get { return doDestroyOnLoad; }
        set
        {
            if (!value)
            {
                DontDestroyOnLoad(gameObject);
            }

            doDestroyOnLoad = value;
        }
    }

	void Awake ()
    {
        if (doDestroyOnLoad)
	    {
	        Destroy(gameObject);
	    }
	}

    public void Initialize()
    {
        source = audio;
        startVolume = source.volume;
    }

    public void Mute()
    {
        source.volume = 0;
    }

    public void UnMute()
    {
        source.volume = startVolume;
    }
}
