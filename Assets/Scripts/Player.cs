using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour {

	float speed = 1f;
	Animator anim_ref;	// Reference to the Animator Component
	public bool bomb_taken;
	Collider2D Bomb_ref;	// A reference to the taken bomb
	public Vector3 direction;	// The direction the player is looking at. We need this to throw bombs to enemies
	// in the proper direction
	public bool freeze;
	
	public AudioClip sfx_shoot, sfx_take;
	int maxEnemies = 5;	// Max enemies in this particular static level. This should be done by a level
								// generator manager keeping track of the number of enemies etc... But this is
								// an alpha I guess :)
	
	int enemies_killed;
	
	// Use this for initialization
	void Start () {
		anim_ref = this.GetComponent<Animator>();
	}	

	
	// Update is called once per frame
	void Update () {
	
		direction = Vector3.zero;
		
		// Move the player
	if(!freeze)
	{	
		if(Input.GetKey(KeyCode.UpArrow))
		{
			//transform.Translate(new Vector3(0, Time.deltaTime, 0));
			if(transform.position.y < Screen_Info.SCR_MAX_UP - .1f)
			{
				direction += new Vector3(0, Time.deltaTime * speed, 0);
				transform.position += new Vector3(0, Time.deltaTime * speed, 0);
				anim_ref.SetBool("walk", true);
			}
		}
		
		else if(Input.GetKey(KeyCode.DownArrow))
		{
			//transform.Translate(new Vector3(0, Time.deltaTime, 0));
			
			if(transform.position.y > Screen_Info.SCR_MAX_BOTTOM + .1f)
			{
				direction += new Vector3(0, -Time.deltaTime * speed, 0);
				transform.position += new Vector3(0, -Time.deltaTime * speed, 0);
				anim_ref.SetBool("walk", true);
			}
		}
		
		if(Input.GetKey(KeyCode.LeftArrow))
		{
			//transform.Translate(new Vector3(0, Time.deltaTime, 0));
			
			if(transform.position.x > Screen_Info.SCR_MAX_LEFT + .1f)
			{
				direction += new Vector3(-Time.deltaTime * speed, 0, 0);
				transform.position += new Vector3(-Time.deltaTime * speed, 0, 0);
				anim_ref.SetBool("walk", true);
			}
		}
		
		else if(Input.GetKey(KeyCode.RightArrow))
		{
			//transform.Translate(new Vector3(0, Time.deltaTime, 0));
			
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
		
		if (Input.GetKeyDown(KeyCode.Space))
		{	
			// If the player didnt take a bomb see if we can take one now
			if(!bomb_taken)
			{
				Bomb_ref = Physics2D.OverlapCircle(this.transform.position, .2f);
				
				if(Bomb_ref != null)
				{
					//temp.collider.gameObject.transform.parent = this.transform;
					bomb_taken = true;
					audio.PlayOneShot(sfx_take);
					Bomb_ref.gameObject.transform.parent = this.transform;
				}
			}
			
			// If we have a bomb release it in the direction the player is moving
			
			else
			{
				Bomb_ref.gameObject.transform.parent = null;
				Bomb_ref.GetComponent<Bomb>().Throw_Bomb_Direction(direction);
				audio.PlayOneShot(sfx_shoot);
				bomb_taken = false;
				Bomb_ref = null;
			}
		}
		
		if(Input.GetKey(KeyCode.Escape))
		{
			Application.LoadLevel("Main");
		}
	}
	}
	
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
			GameObject.Find("Win_Canvas").GetComponent<Canvas>().enabled = true;
			Time.timeScale = 0;
		}
	}
	
	void Restart_Level()
	{
		Application.LoadLevel("Main");
	}
}
