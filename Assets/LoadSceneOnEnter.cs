using UnityEngine;
using System.Collections;

public class LoadSceneOnEnter : MonoBehaviour
{

    public string Scene;
    
	// Update is called once per frame
	void Update ()
    {
	    if(Input.GetKeyDown(KeyCode.Return))
        {
            Application.LoadLevel(Scene);
        }
	}
}
