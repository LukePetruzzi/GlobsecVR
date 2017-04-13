using UnityEngine;
using System.Collections;

public class SideScroll : MonoBehaviour {

	public float Speed = 1.0f;
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetKey(KeyCode.LeftArrow)== true)
		{
			transform.position = transform.position + new Vector3((Speed * -1.0f),0.0f,0.0f);
		}

		if(Input.GetKey(KeyCode.RightArrow)== true)
		{
		transform.position = transform.position + new Vector3(Speed,0.0f,0.0f);
		}
	}
}
