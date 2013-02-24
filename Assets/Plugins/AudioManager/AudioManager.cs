using UnityEngine;
using System.Collections;

public partial class AudioManager : MonoBehaviour
{
    public AudioItem[] AudioItems;

	// Use this for initialization
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
	
	// Update is called once per frame
	static void PlaySound (int id)
    {
	
	}
}
