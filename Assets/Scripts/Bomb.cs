using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/**************************************************

This is where the second StateMachine is implemented. 
The other one is in Enemy.cs
For extra info about the game please see Player.cs

***************************************************/

public class Bomb : MonoBehaviour {

	public float timer;
	public Text text_ref; 	// A ref to the bomb text label (=timer)
	float rot_speed = 750f; // Rotation speed in the Explosion sequence
	float speed = 2f;		// The moving speed
	Vector3 target;			// Where is the bomb going to?
	public bool shot_by_enemy, shot_by_player;	// Who shot the bomb?
	float maxDistance = 1f;   // The maximum distance the bomb can move before stopping. Applies when is thrown
							  // by the player. If it was thrown by the enemy it will stop where the Player
							  // target was
	float distance;			  // This is the actual distance variable
	float maxSize = 3f;		  // Size limit when the bomb is exploding

	
	public AudioClip sfx_explosion;
	bool exploding;			  // Is it already exploding?
	
	// Screen boundaries
	
	float SCR_MAX_RIGHT = 2.75f, SCR_MAX_LEFT = -2.75f, SCR_MAX_UP = 2.1f, SCR_MAX_BOTTOM = -2.1f;
	
	enum States {
		Init,			// Initialise some of the bomb variables
		Idle,			// Waiting for a player/enemy to pick it up and start the timer
		Timer_on,		// Bomb activated and timer going down!
		Moving,			// The bomb is moving towards the target
		Explode,		// If timer has a value of 0 detonate the bomb
	}
	
	States current_state = States.Init;
	
	// Update is called once per frame
	void Update () {
		// Process the different states
		switch(current_state)
		{
			case States.Init:
				Initialize();
				break;
			case States.Idle:
				shot_by_enemy = false;
				shot_by_player = false;
				Idle();
				break;
			case States.Timer_on:
				Timer_On();
				break;
			case States.Explode:
				Explode();
				break;
			case States.Moving:	// Move towards player
				Move();
				break;
		}
		
		// If the bomb is outside the screen and its not being hold by a player or enemy then destroy it
		
		if(this.transform.parent==null && (this.transform.position.x > SCR_MAX_RIGHT || this.transform.position.x < SCR_MAX_LEFT ||
		   this.transform.position.y > SCR_MAX_UP || this.transform.position.y < SCR_MAX_BOTTOM))
		   {
		   
				Destroy(this.gameObject);
		   }
	}
	
	void Initialize()
	{
		timer = Random.Range(1, 20);	// Randomize things a little bit
		text_ref.text = timer.ToString();
		current_state = States.Idle;
	}
	
	void Idle()
	{
		if(this.transform.parent!=null) // If the bomb was taken activate the timer
		{
			current_state = States.Timer_on;
		}
	}
	
	void Timer_On()
	{
		int temp_int;
	
		timer -= Time.deltaTime;
		temp_int = (int)timer;
		text_ref.text = temp_int.ToString();		// Update the UI
		//Debug.Log ("Bomb activated!!");
		
		if(timer <= 1)	// See if we have to make the bomb explode
		{
			current_state = States.Explode;
		}
	}
	
	void Explode()
	{
		if (!exploding)
		{
			exploding = true;
			audio.PlayOneShot(sfx_explosion, .5f);
		}
	
		// Scale and Rotate to simulate the explosion
		this.transform.localScale += new Vector3(Time.deltaTime, Time.deltaTime, 1f);
		this.transform.Rotate(new Vector3(0, 0, Time.deltaTime * rot_speed));
		
		if(this.transform.localScale.x > maxSize)   // Did we reach the explosion size limit?
		{
			Destroy(this.gameObject);	
		}

		// If the player is holding the bomb freeze him

		if(this.transform.parent!=null && this.transform.parent.tag == "Player")
		{
			this.transform.parent.GetComponent<Player>().Freeze();
		}
	}
	
	void Move()
	{	
		// Move towards the target
		this.transform.position += target * Time.deltaTime;
		
		if(!shot_by_player && Vector3.Distance(this.transform.position, target) < 1)
		{
			current_state = States.Idle;
		}
		
		if(shot_by_player)
		{
			distance += Time.deltaTime;	// Limit the amount of units the bomb can move to make the game
										// a bit more interesting
		}
		
		if(distance > maxDistance)
		{
			distance = 0;
			current_state = States.Idle;
		}
	}
	
	// Called from Enemy.cs
	public void Throw_Bomb()
	{
		
		// We can only throw a bomb only if is not already exploding
		if(current_state != States.Explode)
		{
			// Set a target. In our case the Player is the target
			target = GameObject.Find ("Player").transform.position - this.transform.position;
			//target.Normalize();
			//timer = 4;
			shot_by_enemy = true;
			current_state = States.Moving;
		}
	}
	
	// Called from Player.cs after pressing the SPACE key
	public void Throw_Bomb_Direction(Vector3 target_in)
	{	
		// We can only throw a bomb only if is not already exploding
		if(current_state != States.Explode)
		{
			target = target_in;
			target.Normalize();
			target = target * speed;
			shot_by_player = true;
			current_state = States.Moving;
		}
	}
	
	void OnTriggerEnter2D(Collider2D other) {
		//Debug.Log ("Trigger entered!!: " + other.collider.name);
		
		if(other.tag == "Player" && shot_by_enemy)  // If bomb collides with the player and it was shot by an enemy then its game over
		{
			other.GetComponent<Player>().Freeze();
			current_state = States.Explode;
		}
		
		if(other.tag == "Enemy" && shot_by_player)	// If the bomb collides with an enemy and if was shot by the player kill him :)
		{
			//Destroy(other.gameObject);
			other.GetComponent<Enemy>().Explode();
			GameObject.Find("Player").GetComponent<Player>().Update_Stats();
			this.GetComponent<Collider2D>().enabled = false;
			current_state = States.Explode;
		}
	}
}
