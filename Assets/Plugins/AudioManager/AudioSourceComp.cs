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

                Debug.Log("Nope");
            }
            else
            {
                Debug.Log("Yeah");
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
