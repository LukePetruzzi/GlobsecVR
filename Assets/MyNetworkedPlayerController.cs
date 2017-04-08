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
            this.gameObject.GetComponent<Animator>().enabled = true;
            this.gameObject.GetComponent<IKControl>().enabled = true;
            this.gameObject.GetComponent<PlayerController>().enabled = true;
            this.gameObject.GetComponent<UpdateBodyPosition>().enabled = true;
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
