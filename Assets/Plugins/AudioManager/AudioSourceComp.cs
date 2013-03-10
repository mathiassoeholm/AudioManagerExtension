using UnityEngine;
using System.Collections;

public class AudioSourceComp : MonoBehaviour
{
    private bool doDestroyOnLoad;
    
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
}
