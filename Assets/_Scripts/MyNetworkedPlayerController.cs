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

            // turn off the head
            getChildGameObject(this.gameObject, "Body").GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            getChildGameObject(this.gameObject, "default").GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            getChildGameObject(this.gameObject, "Eyelashes").GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            getChildGameObject(this.gameObject, "Eyewear").GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            getChildGameObject(this.gameObject, "Hats").GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

        }
        else // is NOT the local player... just leave
        {
            return;
        }
	}
	
	// Update is called once per frame
	void Update () {
        GameObject bodyToTrack = getChildGameObject(this.gameObject, "PlayerBody");
        this.transform.position = bodyToTrack.transform.position;
        this.transform.rotation = bodyToTrack.transform.rotation;
	}

    static public GameObject getChildGameObject(GameObject fromGameObject, string withName)
    {
        //Author: Isaac Dart, June-13.
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
        return null;
    }
}


