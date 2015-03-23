/**************

State Machines Assigment
by Esteban Moreno Valdes
March 2015

Game Name : PIGS AGUS BOMBS

DESCRIPTION
-----------

Pigs agus Bombs is an arcade game. The action takes place in a green field where a number
of pigs and bombs has been placed. The player controls the Red pig.
The goal of the game is to blow up the grey pigs throwing bombs at them. But watch! There is
a timer on the bombs and they might explode before you release it.

Controls
--------

Cursor Keys - Move
Space - Take, Release a bomb (in the direction the player is moving)
ESC - Restart the Level

Comments
--------

The actual State Machines are implemented in the Enemy and Bomb classes
A PDF file with the State Machines Graphs is located in the directory /SM Chart

**************/

using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	float speed = 1f;		// Player's speed
	Animator anim_ref;		// Reference to the Animator Component
	bool bomb_taken;	    // Keep track if the player is holding a bomb
	Collider2D Bomb_ref;	// A reference to the taken bomb
	Vector3 direction;		// The direction the player is looking at. We need this to throw bombs to enemies
							// in the proper direction
	bool freeze;			// Once the player is hit he won't be able to move
	
	int maxEnemies = 5;		// Max enemies in this particular static level. This should be done with a level
							// generator manager keeping track of the number of enemies etc... But this is
							// an alpha I guess :)
	
	int enemies_killed;		// Keep track of the enemies killed to see if we won the game
	
	public AudioClip sfx_shoot, sfx_take;	// Sound effects
	
	// Use this for initialization
	void Start () {
		anim_ref = this.GetComponent<Animator>();
	}	

	void Update () {
	
		direction = Vector3.zero;		// Have a fresh start every frame. This is the walking
										// direction that will be used to attack enemies
		
		// Move the player
		if(!freeze)		// If we are moving, see what keys we pressed
		{	
			if(Input.GetKey(KeyCode.UpArrow))
			{
				// See if we are inside the screen boundaries first
				if(transform.position.y < Screen_Info.SCR_MAX_UP - .1f)
				{
					// Update direction Vector
					direction += new Vector3(0, Time.deltaTime * speed, 0);
					// Update the position
					transform.position += new Vector3(0, Time.deltaTime * speed, 0);
					// Trigger the walking animation
					anim_ref.SetBool("walk", true);	
				}
			}
			
			// The same structure is followed in the next IF statements
			
			else if(Input.GetKey(KeyCode.DownArrow))
			{	
				if(transform.position.y > Screen_Info.SCR_MAX_BOTTOM + .1f)
				{
					direction += new Vector3(0, -Time.deltaTime * speed, 0);
					transform.position += new Vector3(0, -Time.deltaTime * speed, 0);
					anim_ref.SetBool("walk", true);
				}
			}
			
			if(Input.GetKey(KeyCode.LeftArrow))
			{
				if(transform.position.x > Screen_Info.SCR_MAX_LEFT + .1f)
				{
					direction += new Vector3(-Time.deltaTime * speed, 0, 0);
					transform.position += new Vector3(-Time.deltaTime * speed, 0, 0);
					anim_ref.SetBool("walk", true);
				}
			}
			
			else if(Input.GetKey(KeyCode.RightArrow))
			{
				if(transform.position.x < Screen_Info.SCR_MAX_RIGHT - .1f)
				{
					direction += new Vector3(Time.deltaTime * speed, 0, 0);
					transform.position += new Vector3(Time.deltaTime * speed, 0, 0);
					anim_ref.SetBool("walk", true);
				}
			}
			
			// If all keys are up stop walking
			
			if (!Input.GetKey(KeyCode.UpArrow) && !Input.GetKey(KeyCode.DownArrow)
			    && !Input.GetKey(KeyCode.LeftArrow) && !Input.GetKey(KeyCode.RightArrow))
			{
				anim_ref.SetBool("walk", false);  
			}
			
			// If we press Space key two things may happen :
			// 1) Take a bomb if we are not holding one
			// 2) Release a bomb if the player took one already
			
			if (Input.GetKeyDown(KeyCode.Space))
			{	
				// If the player didn't take a bomb see if we can take one now
				if(!bomb_taken)
				{
					Bomb_ref = Physics2D.OverlapCircle(this.transform.position, .2f);
					
					// Are we colliding with a bomb object?
					if(Bomb_ref != null)
					{
						// YEs! We are...
						bomb_taken = true;
						audio.PlayOneShot(sfx_take);
						// The bomb becomes a child gameobject to simulate it was taken by the player
						Bomb_ref.gameObject.transform.parent = this.transform;
					}
				}
				
				// If we have a bomb release it in the direction the player is moving
				
				else
				{
					// Set the bomb free
					Bomb_ref.gameObject.transform.parent = null;
					// And throw it in the direction we are looking at
					Bomb_ref.GetComponent<Bomb>().Throw_Bomb_Direction(direction);
					audio.PlayOneShot(sfx_shoot); // Do some noise
					bomb_taken = false;
					Bomb_ref = null;	// Avoid dealing with old references the next time
				}
			}
			
			if(Input.GetKey(KeyCode.Escape))
			{
				Application.LoadLevel("Main");
			}
		}
	}
	
	// This is called from the class Bomb
	public void Freeze()
	{
		freeze = true;
		this.GetComponent<SpriteRenderer>().color = Color.black;
		Invoke ("Restart_Level", 3f);
	}
	
	public void Update_Stats()	// Did we beat the game?
	{
		enemies_killed++;
		
		if(enemies_killed >= maxEnemies)
		{
			// Display the winning message
			GameObject.Find("Win_Canvas").GetComponent<Canvas>().enabled = true;
			// Stop!
			Time.timeScale = 0;
		}
	}
	
	// Reload everything...
	void Restart_Level()
	{
		Application.LoadLevel("Main");
	}
}
