using UnityEngine;
using System.Collections;

public class AudioTest : MonoBehaviour
{

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void OnGUI ()
    {
        if (GUILayout.Button("Play sound"))
        {
            AudioManager.PlayTheme_4_Dubstep();
        }

        if (GUILayout.Button("Stop sound"))
        {
            AudioManager.StopTheme_4_Dubstep();
        }
	}
}
