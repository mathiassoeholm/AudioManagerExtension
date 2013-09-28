using UnityEngine;

public class AudioSourceComp : MonoBehaviour
{
    private bool _doDestroyOnLoad;

    private AudioSource _source;

    public float Volume;
    public float OriginVolume;

    public bool DoDestroyOnLoad
    {
        get { return _doDestroyOnLoad; }
        set
        {
            if (!value)
            {
                DontDestroyOnLoad(gameObject);
            }

            _doDestroyOnLoad = value;
        }
    }

	void Awake ()
    {
        if (_doDestroyOnLoad)
	    {
	        Destroy(gameObject);
	    }
	}

    public void PreInitialize()
    {
        _source = audio;
        OriginVolume = _source.volume;
    }

    public void Initialize()
    {
        Volume = _source.volume;
    }

    public void UpdateVolume(float masterVolume, float otherFactor)
    {
        Volume = OriginVolume * masterVolume * otherFactor;
        _source.volume = Volume;
    }

    public void Mute()
    {
        _source.volume = 0;
    }

    public void UnMute()
    {
        _source.volume = Volume;
    }
}
