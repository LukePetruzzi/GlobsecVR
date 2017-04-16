using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using VRTK;

public class CustomSceneTeleportController : MonoBehaviour {

    
    public VRTK_DestinationMarker pointer;
    public VRTK_ControllerEvents controllerEvents;

    public int canvasScalingFrames = 10;

    private void OnEnable()
    {
        pointer.DestinationMarkerEnter += CheckSceneTeleport;
        pointer.DestinationMarkerExit += DisableCanvas;
    }

    private void OnDisable()
    {
        pointer.DestinationMarkerEnter -= CheckSceneTeleport;
        pointer.DestinationMarkerExit -= DisableCanvas;

    }

    // Use this for initialization
    void Start () { 
     
	}
	
	// Update is called once per frame
	void Update () {

	}

    void CheckSceneTeleport(object sender, DestinationMarkerEventArgs e)
    {
        // check the target
        if (e.target.gameObject.tag == "SceneTeleportDestination")
        {
            // scale up the canvas
            Transform t = e.target.gameObject.GetComponentInChildren<Canvas>().transform;
            StartCoroutine(LerpScale(t, t.localScale, Vector3.one));

            if (e.target.gameObject.name == "BuildingC")
            {
                // user pull sthe trigger to teleport
                if (controllerEvents.triggerPressed)
                {
                    SceneManager.LoadScene("IBXScene");
                    GameObject.Find("NetworkManager").GetComponent<NetworkManager>().ServerChangeScene("IBXScene");
                }
            }
        }
    }

    void DisableCanvas(object sender, DestinationMarkerEventArgs e)
    {
        if (e.target.gameObject.tag == "SceneTeleportDestination")
        {
            // scale down the canvas
            Transform t = e.target.gameObject.GetComponentInChildren<Canvas>().transform;
            StartCoroutine(LerpScale(t, t.localScale, Vector3.zero));
        }
    }

    IEnumerator LerpScale(Transform t, Vector3 InitialScale, Vector3 FinalScale)
    {
        float progress = 0;

        while (progress <= 1)
        {
            t.localScale = Vector3.Lerp(InitialScale, FinalScale, progress);
            progress += Time.deltaTime * 5;
            yield return null;
        }

        t.localScale = FinalScale;
    }
}
