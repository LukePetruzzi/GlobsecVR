using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MyNetworkedPlayerController : NetworkBehaviour {

	// Use this for initialization
	void Start () {
		if (isLocalPlayer)
        {
            return;
        }
        else
        {
            this.transform.rotation = Quaternion.Euler(new Vector3(12, 12, 12));
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
