using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchmakingCamera : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        // get rid of this camera when a game is joined
		if (GameObject.Find("Player(Clone)") != null)
        {
            Destroy(this.gameObject);
        }
	}
}
