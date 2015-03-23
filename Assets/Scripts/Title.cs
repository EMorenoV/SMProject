using UnityEngine;
using System.Collections;

public class Title : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Time.timeScale = 0;		// At the beginning of the game nothing moves. We are just waiting for a key press
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.anyKey)
		{
			Time.timeScale = 1f;		// Start moving stuff :)
			Destroy (this.gameObject);	// It's a simple game. If we are killed we reload the level
										// that's why is ok to destroy the UI component
		}
	}
}
