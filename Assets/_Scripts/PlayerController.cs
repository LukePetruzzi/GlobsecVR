using UnityEngine;
using System.Collections;

//Controls the rotation and walking animations of the player
public class PlayerController : MonoBehaviour 
{
	public Transform target;				//The target height object for the player
	public Transform player;				//The player object
	public Animator animator;				//The animator, used in changing the walk speed
	public float minThreshold;				//Minimum threshold for movement speed
	public float maxThreshold;				//Minimum threshold for movement speed
	public float transitionSpeed = 1.0f;	//Smoothing value for movement speed

	private float currentSpeed;				//The current walking speed, moves smoothly towards target speed
	private float targetSpeed;				//The walking speed calculated during CheckCharacterLocation
	private Vector3 previousPosition;		//The position the player was at last time we checked. Used in checking the offset for finding movement speed

	void Start () 
	{
		
	}

	void Update ()
	{
        if (this.target == null)
        {
            // Find the head
            this.target = GameObject.Find("Camera (eye)").transform;
            this.player = target;
            //At the start, reset the height of the player
            //ResetScaling();

            //Initialize the previousPosition variable
            previousPosition = player.position;

            //Start the character movement speed check routine
            StartCoroutine(CheckCharacterLocation());
        }
        else {
            //Smoothly move the current speed towards the one we calculated
            currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, Time.deltaTime * transitionSpeed);

            //Set the animation speed of the walking to that value
            animator.SetFloat("WalkSpeed", currentSpeed);
        }

	}

	//Scale character to player height
	public void ResetScaling()
	{
		float scale = Mathf.Clamp (Map.map (target.transform.localPosition.y, 0, 1.6f, 0, 1), 0.9f, 1.25f);
		transform.localScale = new Vector3 (scale, scale, scale);
	}

	//Check the offset of the player every 0.1 seconds, to figure out the walking speed
	IEnumerator CheckCharacterLocation()
	{
		while (true)
		{
			//Exclude changes in height from this check
			Vector2 flatPlayerPos = new Vector2 (player.position.x, player.position.z);
			Vector2 flatPrevPos = new Vector2 (previousPosition.x, previousPosition.z);

			//Check the distance from previous location to current
			float distanceMoved = Vector2.Distance (flatPlayerPos, flatPrevPos);

			if (distanceMoved > minThreshold)
			{
				//Map the walking animation speed to the distance moved based on min/max values set in inspector
				targetSpeed = Map.map (distanceMoved, minThreshold, maxThreshold, 0.5f, 1);
			}
			else
			{
				targetSpeed = 0;
			}

			//Set the previous position value, for use in the next loop iteration
			previousPosition = player.position;

			yield return new WaitForSeconds (0.1f);
		}
	}
}
