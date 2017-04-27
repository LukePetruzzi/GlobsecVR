using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRTK;

public class CustomSceneTeleportController : MonoBehaviour {

    
    public VRTK_DestinationMarker pointer;
    public VRTK_ControllerEvents controllerEvents;
    public MyNetworkedPlayerController networkedController;

    private void OnEnable()
    {
        pointer.DestinationMarkerEnter += CheckSceneTeleport;
        pointer.DestinationMarkerExit += DisableCanvas;
        //SceneManager.sceneLoaded += WaitForControllerInput;
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
        if (networkedController == null && GameObject.Find("PlayerBody_localPlayer") != null)
        {
            networkedController = GameObject.Find("PlayerBody_localPlayer").GetComponent<MyNetworkedPlayerController>();
        }
	}

    void WaitForControllerInput(Scene s, LoadSceneMode y)
    {
        StartCoroutine(Wait());
    }
    IEnumerator Wait()
    {
        pointer.enabled = false;
        yield return new WaitForSeconds(2);
        pointer.enabled = true;
    }

    void CheckSceneTeleport(object sender, DestinationMarkerEventArgs e)
    {
        // check the target
        if (e.target.gameObject.tag == "SceneTeleportDestination")
        {
            // scale up the canvas
            Transform t = e.target.gameObject.GetComponentInChildren<Canvas>().transform;
            StartCoroutine(LerpScale(t, t.localScale, Vector3.one));

            if (e.target.gameObject.name == "BuildingA" || e.target.gameObject.name == "BuildingB" || e.target.gameObject.name == "BuildingC")
            {
                // user pull sthe trigger to teleport
                if (controllerEvents.triggerClicked)
                {
                    pointer.enabled = false;

                    // choose a scene at random out of the 3 bunkers and go to it.
                    System.Random rnd = new System.Random();
                    // generate random number. 1 inclusive 4 exclusive
                    int bunk = rnd.Next(1, 4);

                    if (bunk == 1)
                    {
                        networkedController.ChangeScene("Bunker_A");
                    }
                    else if (bunk == 2)
                    {
                        networkedController.ChangeScene("Bunker_B");
                    }
                    else if (bunk == 3)
                    {
                        networkedController.ChangeScene("Bunker_C");
                    }
                    pointer.enabled = true;
                }
            }
            //if (e.target.gameObject.name == "BuildingB")
            //{
            //    // user pull sthe trigger to teleport
            //    if (controllerEvents.triggerClicked)
            //    {
            //        pointer.enabled = false;
            //        // use the network controller to change the scene
            //        networkedController.ChangeScene("Bunker_B");
            //        pointer.enabled = true;
            //    }
            //}
            //if (e.target.gameObject.name == "BuildingA")
            //{
            //    // user pull sthe trigger to teleport
            //    if (controllerEvents.triggerClicked)
            //    {
            //        pointer.enabled = false;
            //        // use the network controller to change the scene
            //        networkedController.ChangeScene("Bunker_A");
            //        pointer.enabled = true;
            //    }
            //}

            if (e.target.gameObject.name == "DoorToMainRoom")
            {
                // user pull sthe trigger to teleport
                if (controllerEvents.triggerClicked)
                {
                    pointer.enabled = false;
                    // use the network controller to change the scene
                    networkedController.ChangeScene("MainSceneReturn");
                    pointer.enabled = true;
                }
            }
            if (e.target.gameObject.name == "WeaponToInspect")
            {
                if (controllerEvents.triggerClicked)
                {
                    pointer.enabled = false;
                    // use the network controller to change the scene
                    networkedController.ChangeScene("IBXScene");
                    pointer.enabled = true;
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

    static public GameObject getChildGameObject(GameObject fromGameObject, string withName)
    {
        Transform[] ts = fromGameObject.transform.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in ts) if (t.gameObject.name == withName) return t.gameObject;
        return null;
    }
}
