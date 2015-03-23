using UnityEngine;
using System.Collections;

/**************************************************

This is where the first SM is implemented. 
The other one is in Bomb.cs
For extra info about the game please see Player.cs

***************************************************/

public class Enemy : MonoBehaviour {

	float speed = 1.5f;			// Enemy speed. Make it slighter faster than the player
	float rotation_speed = 2f;	// This is the speed of the search vector rotation in SEARCH State.
								// In other words the speed of the radar :) You can see it in action
								// in the Scene View
	float maxBombTime = 4;		// The enemy won't get bombs where the timer is less than this number	
	
	float decision_time;		// How many seconds before the enemy attacks the player?
	float walking_time;			// Number of seconds before the enemy goes to search state again
								// If the enemy is lost because the player took the bomb they were going
								// for or for other reason, it will search a new target again after this
								// walking_time variable has elapsed
	
	float rot_speed = 750f;		// Rotation speed used in the Dead sequence
	
	bool exploding;				// Is the enemy alive or performing a Dead sequence (rotating basically)
	
	Animator anim_ref;	// Reference to the Animator Component
	
	bool bomb_taken = false;	// Are we carrying a bomb already?
	
	enum States {
		Init,			// Initialise some of the enemy variables
		Search,			// Radar mode on = Look for a bomb with a timer >= 5
		Walk,			// Go and get the bomb following the search_direction vector
		Take,			// Take a bomb
		Attack,			// Throw the bomb to the player
		Dead,			// Do a little animation
	}
	
	public RaycastHit2D hit;
	public AudioClip sfx_explosion;
	public Vector3 search_direction;
	
	States current_state = States.Init;

	// Screen boundaries
	
	public float SCR_MAX_RIGHT = 2.64f, SCR_MAX_LEFT = -2.64f, SCR_MAX_UP = 1.95f, SCR_MAX_BOTTOM = -1.95f;
	
	void Update () {
	
		// Process the different states
		switch(current_state)
		{
			case States.Init:
				Initialize();
				break;
			case States.Search:
				Search();
				break;	
			case States.Walk:
				Walk();
				break;
			case States.Take:
				Take();
				break;
			case States.Attack:
				Attack ();
				break;
			case States.Dead:
				Dead();
				break;
		}
		
		// Any state!
		
		if(this.transform.localScale.x > 3f)	// Did we reach the Max exploding size
		{
			Destroy(this.gameObject);
		}
		
		Debug.DrawRay(transform.position, search_direction, Color.red, .2f);
	}
	
	void Initialize()
	{
		anim_ref = this.GetComponent<Animator>();
		search_direction = Vector2.up;
		current_state = States.Search;
	}
	
	void Search()
	{
		hit = Physics2D.Raycast(this.transform.position, search_direction, 100f);
		
		// If the ray hits something and is a bomb that is not moving (shot_by_enemy) go for it
		
		if ( hit.collider != null && !hit.collider.GetComponent<Bomb>().shot_by_enemy) 
		{
		
			//Debug.Log ("Something hitted" + hit.collider.name);
			
			// If we hit a bomb and its timer is above, equal to maxBombTime then
			// the enemy will pick it up. That gives enough time to hold it for few seconds and 
			// throw it to the player
			
			if(hit.collider.tag != "Player" && hit.collider.GetComponent<Bomb>().timer >= maxBombTime)
			{
			 current_state = States.Walk;
			}
		}
		
		// keep searching ...
		// Rotate the vector and try to find a bomb in a different direction
		
		search_direction = Quaternion.Euler(0, 0, -1 * rotation_speed) * search_direction;
	}
	
	void Walk()
	{
		if(hit.collider != null)	// If a bomb was detected walk towards it
		{
			anim_ref.SetBool("walk", true);
			transform.Translate(search_direction.normalized * Time.deltaTime * speed);
			
			//Debug.Log ("Bomb distance: " + Vector2.Distance(hit.collider.transform.position, this.transform.position));
		
			if(!bomb_taken && Vector2.Distance(hit.collider.transform.position, this.transform.position) <=0.1f)
			{
				// If it's not taken by another enemy then take it. Avoid stealing
				
				if(hit.collider.transform.parent == null)
					current_state = States.Take;
			}
			
			// If the enemy goes beyond the screen boundaries change the direction of the translation vector
			
			if (transform.position.y > SCR_MAX_UP || transform.position.y < SCR_MAX_BOTTOM)
			{
				search_direction = new Vector3(search_direction.x, -search_direction.y, search_direction.z);
			}
			
			else if (transform.position.x > SCR_MAX_RIGHT || transform.position.x < SCR_MAX_LEFT)
			{
				search_direction = new Vector3(-search_direction.x, search_direction.y, search_direction.z);
			}
			
			// Is it time to throw the bomb?
			
			if(bomb_taken && decision_time < 0)
			{
				current_state = States.Attack;
			}
			else if (bomb_taken && decision_time > 0) {
				decision_time -= Time.deltaTime;	// Keep decrementing
			}
		}
		
		walking_time += Time.deltaTime;
		if(walking_time > 10f)		// If enemy lost, force a search again
		{
			walking_time = 0;
			current_state = States.Search;
		}
		
	}
	
	void Take()
	{
		// Make the bomb a child of the enemy to represent it was taken and being carried
		hit.collider.transform.parent = this.transform;
		bomb_taken = true;
		
		// When a bomb is taken see how much time is left and assign the decision_time
		
		decision_time = 2f;
		
		current_state = States.Walk;
	}
	
	void Attack()
	{
	
		walking_time = 0;
		
		// Throw the bomb
		
		// That means first detach the enemy from the bomb
		
		hit.collider.transform.parent = null;
		hit.collider.GetComponent<Bomb>().Throw_Bomb();
		search_direction = Vector2.up;
		
		bomb_taken = false;
		
		current_state = States.Search;
	}
	
	void Dead()
	{
			// Did the enemy enter the elimination sequence?
			if(!exploding)
			{
				// No, then play the sound this time only
				
				exploding = true;
			}
	
			// Scale and Rotate to simulate the explosion
			this.transform.localScale += new Vector3(Time.deltaTime, Time.deltaTime, 1f);
			this.transform.Rotate(new Vector3(0, 0, Time.deltaTime * rot_speed));	
	}
	
	public void Explode()
	{
		audio.PlayOneShot(sfx_explosion);
		current_state = States.Dead;
	}
}
