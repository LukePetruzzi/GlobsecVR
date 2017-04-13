using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MyNetworkedPlayerController : NetworkBehaviour {

    public override void OnStartLocalPlayer()
    {
        //Renderer[] rens = GetComponentsInChildren<Renderer>();
        //foreach (Renderer ren in rens)
        //{
        //    ren.enabled = false;
        //}

        GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
    }

    public override void PreStartClient()
    {
        GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
    }
    // Use this for initialization
    void Start () {

        // enable the animator if client or server alike
        this.gameObject.GetComponent<Animator>().enabled = true;
        

        // turn on everything the local player needs
        if (isLocalPlayer)
        {
            this.gameObject.GetComponent<IKControl>().enabled = true;
            this.gameObject.GetComponent<PlayerController>().enabled = true;
            this.gameObject.GetComponent<UpdateBodyPosition>().enabled = true;

            // turn off the head
            getChildGameObject(this.gameObject, "Body").GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            getChildGameObject(this.gameObject, "default").GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            getChildGameObject(this.gameObject, "Eyelashes").GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            getChildGameObject(this.gameObject, "Eyewear").GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            getChildGameObject(this.gameObject, "Hats").GetComponent<SkinnedMeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

            // change name so it's obvious whos local in the inspector
            gameObject.name = gameObject.name.Replace("(Clone)", "") + "_localPlayer";

        }
        else // is NOT the local player... just leave
        {

            gameObject.name = gameObject.name.Replace("(Clone)", "") + "_clientPlayer";

            return;
        }
	}
	
	// Update is called once per frame
	void Update () {
        
        
	}

    static public GameObject getChildGameObject(GameObject fromGameObject, string withName)
    {
        //Author: Isaac Dart, June-13.
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
        return null;
    }
}


