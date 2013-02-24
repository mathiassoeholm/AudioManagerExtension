using UnityEngine;
using System.Collections;

public class AudioManagerTest : MonoBehaviour
{

	// Update is called once per frame
	void OnGUI ()
    {
	    if (GUILayout.Button("Play a sound"))
	    {
	        AudioManager.PlayFuel_Pickup();
	    }
	}
}
