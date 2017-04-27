using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IBXDisplayer : MonoBehaviour {

	// Use this for initialization
	void Start () {
        if (GameObject.Find("IBX") != null)
        {
            // turn off the meshes
            foreach (Renderer rend in GameObject.Find("IBX").GetComponentsInChildren<Renderer>())
            {
                rend.enabled = true;
            }
            foreach (Collider coll in GameObject.Find("IBX").GetComponentsInChildren<Collider>())
            {
                coll.enabled = true;
            }
        }
    }

}
