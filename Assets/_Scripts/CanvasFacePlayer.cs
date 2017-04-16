using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasFacePlayer : MonoBehaviour {

    public GameObject localPlayer;

	// Use this for initialization
	void Start () {
        localPlayer = GameObject.Find("Camera (eye)");
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 camRotation = Camera.main.transform.rotation.eulerAngles;

        //this.transform.rotation.eulerAngles = Vector3(camRotation.x, 90, 90);
        this.transform.localRotation = Quaternion.Euler(new Vector3(-camRotation.y + 90, 90, 90));
    }
}
