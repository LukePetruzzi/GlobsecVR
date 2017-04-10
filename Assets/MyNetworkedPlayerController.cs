using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MyNetworkedPlayerController : NetworkBehaviour {

	// Use this for initialization
	void Start () {
        // turn on everything the local player needs
		if (isLocalPlayer)
        {
            this.gameObject.GetComponentInChildren<Animator>().enabled = true;
            this.gameObject.GetComponentInChildren<IKControl>().enabled = true;
            this.gameObject.GetComponentInChildren<PlayerController>().enabled = true;
            this.gameObject.GetComponentInChildren<UpdateBodyPosition>().enabled = true;
        }
        else // is NOT the local player... just leave
        {
            return;
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
