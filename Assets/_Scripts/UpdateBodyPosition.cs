using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateBodyPosition : MonoBehaviour {

    public Transform cam;

    private void Start()
    {
        
    }

    // Update is called once per frame
    void Update () {

        if (this.cam == null)
        {
            this.cam = GameObject.Find("Camera (eye)").transform;
        }
        else
        {
            this.transform.position = new Vector3(cam.position.x, this.transform.position.y, cam.position.z) - (this.transform.forward * .1f);
            this.transform.rotation = Quaternion.Euler(new Vector3(this.transform.eulerAngles.x, cam.eulerAngles.y, this.transform.eulerAngles.z));
        }

        
    }
}
